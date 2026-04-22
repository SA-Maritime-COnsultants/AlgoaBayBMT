using AlgoaBayBMT.Data;
using AlgoaBayBMT.Services.Interfaces;
using AlgoaBayBMT.Services.Models;
using AlgoaBayBMT.Shared.Models;
using AlgoaBayBMT.Shared.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AlgoaBayBMT.Services;

public sealed class LearnerTrainingService(
    IDbContextFactory<ApplicationDbContext> dbContextFactory,
    UserManager<ApplicationUser> userManager,
    IWebHostEnvironment environment) : ILearnerTrainingService
{
    private static readonly string[] FullAccessRoles = [RoleNames.Admin, RoleNames.Dffe, RoleNames.Samsa];

    public async Task<MyTrainingDashboardModel> GetDashboardAsync(string userId, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var learner = await LoadLearnerContextAsync(dbContext, userId, cancellationToken);
        if (learner is null)
        {
            return new MyTrainingDashboardModel();
        }

        var accessContext = await LoadCourseAccessContextAsync(dbContext, learner, cancellationToken);
        var requiredCourses = accessContext.RequiredCourses
            .OrderByDescending(x => x.Status == TrainingAccessStatus.Overdue || x.Status == TrainingAccessStatus.Expired)
            .ThenBy(x => x.Title)
            .ToList();
        var additionalCourses = learner.HasFullAccess
            ? accessContext.AdditionalCourses.OrderBy(x => x.Title).ToList()
            : new List<MyTrainingCourseListItemModel>();

        var completedRequired = requiredCourses.Count(x => x.Status == TrainingAccessStatus.Compliant);
        var inProgress = requiredCourses.Count(x => x.Status == TrainingAccessStatus.InProgress);
        var overdueOrExpired = requiredCourses.Count(x => x.Status is TrainingAccessStatus.Overdue or TrainingAccessStatus.Expired);
        var compliancePercent = requiredCourses.Count == 0
            ? 100m
            : Math.Round((decimal)completedRequired / requiredCourses.Count * 100m, 2);

        return new MyTrainingDashboardModel
        {
            LearnerName = learner.DisplayName,
            HasFullAccess = learner.HasFullAccess,
            TotalRequiredCourses = requiredCourses.Count,
            CompletedCourses = completedRequired,
            InProgressCourses = inProgress,
            OverdueOrExpiredCourses = overdueOrExpired,
            CompliancePercent = compliancePercent,
            RequiredCourses = requiredCourses,
            AdditionalCourses = additionalCourses,
            Certificates = accessContext.Certificates
        };
    }

    public async Task<TrainingCoursePlayerModel?> GetCoursePlayerAsync(string userId, Guid courseId, Guid? lessonId = null, Guid? blockId = null, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var learner = await LoadLearnerContextAsync(dbContext, userId, cancellationToken);
        if (learner is null)
        {
            return null;
        }

        var accessContext = await LoadCourseAccessContextAsync(dbContext, learner, cancellationToken);
        var canAccessCourse = learner.HasFullAccess
            || accessContext.RequiredCourses.Any(x => x.CourseId == courseId)
            || accessContext.AdditionalCourses.Any(x => x.CourseId == courseId);
        if (!canAccessCourse)
        {
            return null;
        }

        var courseProjection = await dbContext.Courses.AsNoTracking()
            .Where(x => x.CourseId == courseId && x.IsActive && x.CurrentVersionId.HasValue)
            .Select(x => new CourseProjection
            {
                CourseId = x.CourseId,
                Code = x.Code,
                Title = x.Title,
                Summary = x.Summary,
                RegulatoryReference = x.RegulatoryReference,
                PassMarkPercent = x.PassMarkPercent,
                ValidityMonths = x.ValidityMonths,
                CurrentVersionId = x.CurrentVersionId!.Value
            })
            .FirstOrDefaultAsync(cancellationToken);
        if (courseProjection is null)
        {
            return null;
        }

        var modules = await dbContext.Modules.AsNoTracking()
            .Where(x => x.CourseVersionId == courseProjection.CurrentVersionId && x.IsActive)
            .OrderBy(x => x.OrderIndex)
            .Select(x => new ModuleProjection
            {
                ModuleId = x.ModuleId,
                Title = x.Title,
                OrderIndex = x.OrderIndex
            })
            .ToListAsync(cancellationToken);

        var moduleIds = modules.Select(x => x.ModuleId).ToList();
        var lessons = await dbContext.Lessons.AsNoTracking()
            .Where(x => moduleIds.Contains(x.ModuleId) && x.IsActive)
            .OrderBy(x => x.OrderIndex)
            .Select(x => new LessonProjection
            {
                LessonId = x.LessonId,
                ModuleId = x.ModuleId,
                Title = x.Title,
                Summary = x.Summary,
                OrderIndex = x.OrderIndex
            })
            .ToListAsync(cancellationToken);

        var lessonIds = lessons.Select(x => x.LessonId).ToList();
        var blocks = await dbContext.LessonBlocks.AsNoTracking()
            .Where(x => lessonIds.Contains(x.LessonId) && x.IsActive)
            .OrderBy(x => x.OrderIndex)
            .Select(x => new BlockProjection
            {
                BlockId = x.LessonBlockId,
                LessonId = x.LessonId,
                BlockType = x.BlockType,
                Title = x.Title,
                Subtitle = x.Subtitle,
                OrderIndex = x.OrderIndex,
                ContentHtml = x.MarkdownBody,
                ThumbnailUrl = x.ThumbnailUrl,
                FileUrl = x.FileUrl,
                ExternalUrl = x.ExternalUrl,
                MimeType = x.MimeType,
                DurationSeconds = x.DurationSeconds,
                MetadataJson = x.MetadataJson,
                IsRequired = x.IsRequired
            })
            .ToListAsync(cancellationToken);

        var lessonProgressList = await dbContext.UserLessonProgress
            .Where(x => x.UserId == userId && lessonIds.Contains(x.LessonId))
            .ToListAsync(cancellationToken);
        var lessonProgressLookup = lessonProgressList.ToDictionary(x => x.LessonId);

        var courseProgress = await dbContext.UserCourseProgress
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId && x.CourseId == courseId, cancellationToken);

        var completion = await dbContext.CourseCompletionRecords
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.CourseId == courseId)
            .OrderByDescending(x => x.CompletedOnUtc)
            .Select(x => new CompletionProjection
            {
                CompletionId = x.CourseCompletionRecordId,
                CompletedOnUtc = x.CompletedOnUtc,
                ExpiryDateUtc = x.ExpiryDateUtc,
                FinalScorePercent = x.FinalScorePercent,
                CertificateNumber = x.CertificateNumber,
                CourseVersionId = x.CourseVersionId
            })
            .FirstOrDefaultAsync(cancellationToken);

        var certificate = completion is null
            ? null
            : await dbContext.TrainingCertificates.AsNoTracking()
                .Where(x => x.CourseCompletionRecordId == completion.CompletionId)
                .Select(x => new CertificateProjection
                {
                    CertificateId = x.TrainingCertificateId,
                    VerificationCode = x.VerificationCode,
                    FilePath = x.FilePath,
                    ExpiresOnUtc = x.ExpiresOnUtc,
                    IssuedOnUtc = x.IssuedOnUtc
                })
                .FirstOrDefaultAsync(cancellationToken);

        var metadataLookup = blocks.ToDictionary(x => x.BlockId, x => DeserializeMetadata(x.MetadataJson));
        var linkedAssessmentIds = metadataLookup.Values
            .Where(x => x.LinkedAssessmentId.HasValue)
            .Select(x => x.LinkedAssessmentId!.Value)
            .Distinct()
            .ToList();

        var assessmentQuestions = linkedAssessmentIds.Count == 0
            ? new List<AssessmentQuestionProjection>()
            : await dbContext.TrainingQuestionBankQuestions.AsNoTracking()
                .Where(x => linkedAssessmentIds.Contains(x.TrainingCourseAssessmentId) && x.IsActive)
                .OrderBy(x => x.Prompt)
                .Select(x => new AssessmentQuestionProjection
                {
                    AssessmentId = x.TrainingCourseAssessmentId,
                    QuestionId = x.TrainingQuestionBankQuestionId,
                    Prompt = x.Prompt,
                    ScenarioText = x.ScenarioText,
                    Points = x.Points
                })
                .ToListAsync(cancellationToken);

        var assessmentQuestionIds = assessmentQuestions.Select(x => x.QuestionId).ToList();
        var assessmentOptions = assessmentQuestionIds.Count == 0
            ? new List<AssessmentOptionProjection>()
            : await dbContext.TrainingQuestionBankOptions.AsNoTracking()
                .Where(x => assessmentQuestionIds.Contains(x.TrainingQuestionBankQuestionId))
                .OrderBy(x => x.OrderIndex)
                .Select(x => new AssessmentOptionProjection
                {
                    QuestionId = x.TrainingQuestionBankQuestionId,
                    OptionId = x.TrainingQuestionBankOptionId,
                    OptionText = x.OptionText,
                    IsCorrect = x.IsCorrect
                })
                .ToListAsync(cancellationToken);

        var latestAssessmentScores = linkedAssessmentIds.Count == 0
            ? new Dictionary<Guid, decimal?>()
            : await dbContext.UserAssessmentAttempts.AsNoTracking()
                .Where(x => x.UserId == userId && linkedAssessmentIds.Contains(x.TrainingCourseAssessmentId))
                .GroupBy(x => x.TrainingCourseAssessmentId)
                .Select(x => new
                {
                    AssessmentId = x.Key,
                    ScorePercent = x.OrderByDescending(attempt => attempt.SubmittedOnUtc ?? attempt.StartedOnUtc)
                        .Select(attempt => attempt.ScorePercent)
                        .FirstOrDefault()
                })
                .ToDictionaryAsync(x => x.AssessmentId, x => x.ScorePercent, cancellationToken);

        var flattenedLessons = modules
            .OrderBy(x => x.OrderIndex)
            .SelectMany(module => lessons.Where(lesson => lesson.ModuleId == module.ModuleId).OrderBy(lesson => lesson.OrderIndex))
            .ToList();

        var lockedLessonReached = false;
        var lessonSummaries = new Dictionary<Guid, TrainingPlayerLessonSummaryModel>();
        foreach (var lesson in flattenedLessons)
        {
            var progress = lessonProgressLookup.GetValueOrDefault(lesson.LessonId);
            var isCompleted = progress?.Status == ProgressStatus.Completed;
            var isLocked = lockedLessonReached;
            lessonSummaries[lesson.LessonId] = new TrainingPlayerLessonSummaryModel
            {
                LessonId = lesson.LessonId,
                Title = lesson.Title,
                OrderIndex = lesson.OrderIndex,
                IsCompleted = isCompleted,
                IsLocked = isLocked,
                ProgressPercent = progress?.PercentComplete ?? 0m
            };

            if (!isCompleted)
            {
                lockedLessonReached = true;
            }
        }

        var selectedLesson = SelectLesson(flattenedLessons, lessonSummaries, lessonId);
        if (selectedLesson is null)
        {
            return null;
        }

        var selectedLessonProgress = lessonProgressLookup.GetValueOrDefault(selectedLesson.LessonId);
        var evidence = DeserializeEvidence(selectedLessonProgress?.CompletionEvidenceJson);
        var lessonBlocks = blocks.Where(x => x.LessonId == selectedLesson.LessonId).OrderBy(x => x.OrderIndex).ToList();
        var blockModels = BuildBlockModels(lessonBlocks, metadataLookup, evidence, assessmentQuestions, assessmentOptions, latestAssessmentScores, environment);
        foreach (var bm in blockModels.Where(b => b.BlockType == LessonBlockType.Video))
        {
            if (evidence.VideoSecondsByBlock.TryGetValue(bm.BlockId, out var savedSeconds))
            {
                bm.SavedVideoSeconds = savedSeconds;
            }
        }

        // Backward-compatible fallback for older records that only stored a single lesson-level video position.
        if (selectedLessonProgress?.VideoSecondsWatched.HasValue == true
            && !blockModels.Any(b => b.BlockType == LessonBlockType.Video && b.SavedVideoSeconds.HasValue))
        {
            var lessonVideoBlocks = blockModels.Where(b => b.BlockType == LessonBlockType.Video).ToList();
            if (lessonVideoBlocks.Count == 1)
            {
                lessonVideoBlocks[0].SavedVideoSeconds = selectedLessonProgress.VideoSecondsWatched;
            }
        }
        ApplyBlockAvailability(blockModels);

        var currentBlock = SelectBlock(blockModels, blockId);
        if (currentBlock is null)
        {
            return null;
        }

        var currentIndex = blockModels.FindIndex(x => x.BlockId == currentBlock.BlockId);
        var moduleModels = modules
            .OrderBy(x => x.OrderIndex)
            .Select(module => new TrainingPlayerModuleModel
            {
                ModuleId = module.ModuleId,
                Title = module.Title,
                OrderIndex = module.OrderIndex,
                TotalLessons = lessons.Count(x => x.ModuleId == module.ModuleId),
                CompletedLessons = lessons.Count(x => x.ModuleId == module.ModuleId && lessonSummaries.GetValueOrDefault(x.LessonId)?.IsCompleted == true),
                Lessons = lessons.Where(x => x.ModuleId == module.ModuleId)
                    .OrderBy(x => x.OrderIndex)
                    .Select(x =>
                    {
                        var summary = lessonSummaries[x.LessonId];
                        summary.IsCurrent = x.LessonId == selectedLesson.LessonId;
                        return summary;
                    })
                    .ToList()
            })
            .ToList();

        var courseProgressPercent = courseProgress?.PercentComplete ?? CalculateCourseProgress(flattenedLessons, lessonSummaries);
        var totalLessons = flattenedLessons.Count;
        var completedLessons = lessonSummaries.Values.Count(x => x.IsCompleted);
        var currentBlockRequiredComplete = !currentBlock.IsRequired || currentBlock.IsCompleted;

        return new TrainingCoursePlayerModel
        {
            CourseId = courseProjection.CourseId,
            Code = courseProjection.Code,
            Title = courseProjection.Title,
            Summary = courseProjection.Summary,
            RegulatoryReference = courseProjection.RegulatoryReference,
            PassMarkPercent = courseProjection.PassMarkPercent,
            ValidityMonths = courseProjection.ValidityMonths,
            CourseProgressPercent = courseProgressPercent,
            CompletedLessons = completedLessons,
            TotalLessons = totalLessons,
            HasFullAccess = learner.HasFullAccess,
            IsCompleted = courseProgress?.Status == ProgressStatus.Completed || (completion is not null && completion.ExpiryDateUtc >= DateTime.UtcNow),
            CertificateId = certificate?.CertificateId,
            CertificateUrl = certificate is null ? null : $"/my-training/certificates/{certificate.CertificateId}",
            CurrentLessonId = selectedLesson.LessonId,
            CurrentBlockId = currentBlock.BlockId,
            CanGoPrevious = currentIndex > 0,
            CanGoNext = currentIndex >= 0 && currentIndex < blockModels.Count - 1 && currentBlockRequiredComplete,
            CanMarkCurrentBlockComplete = currentBlock.BlockType is not LessonBlockType.Quiz and not LessonBlockType.Assessment && !currentBlock.IsCompleted,
            CurrentBlockActionText = GetBlockActionText(currentBlock.BlockType),
            Modules = moduleModels,
            CurrentLesson = new TrainingPlayerLessonModel
            {
                LessonId = selectedLesson.LessonId,
                Title = selectedLesson.Title,
                Summary = selectedLesson.Summary,
                OrderIndex = selectedLesson.OrderIndex,
                ProgressPercent = selectedLessonProgress?.PercentComplete ?? 0m,
                IsCompleted = lessonSummaries[selectedLesson.LessonId].IsCompleted,
                IsLocked = lessonSummaries[selectedLesson.LessonId].IsLocked,
                Blocks = blockModels.Select(x =>
                {
                    x.IsCurrent = x.BlockId == currentBlock.BlockId;
                    return x;
                }).ToList()
            },
            CurrentBlock = currentBlock
        };
    }

    public async Task<OperationResult> CompleteBlockAsync(string userId, Guid courseId, Guid lessonId, Guid blockId, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var block = await dbContext.LessonBlocks.FirstOrDefaultAsync(x => x.LessonBlockId == blockId && x.LessonId == lessonId, cancellationToken);
        if (block is null)
        {
            return OperationResult.Failure("Training block not found.");
        }

        await MarkBlockCompletedInternalAsync(dbContext, userId, courseId, lessonId, blockId, null, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("Block completed.");
    }

    public async Task<OperationResult> CompleteVideoBlockAsync(string userId, Guid courseId, Guid lessonId, Guid blockId, int secondsWatched, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var block = await dbContext.LessonBlocks.FirstOrDefaultAsync(x => x.LessonBlockId == blockId && x.LessonId == lessonId, cancellationToken);
        if (block is null)
        {
            return OperationResult.Failure("Training block not found.");
        }

        var lessonProgress = await dbContext.UserLessonProgress.FirstOrDefaultAsync(x => x.UserId == userId && x.LessonId == lessonId, cancellationToken);
        if (lessonProgress is null)
        {
            lessonProgress = new UserLessonProgress
            {
                UserLessonProgressId = Guid.NewGuid(),
                UserId = userId,
                LessonId = lessonId,
                StartedOnUtc = DateTime.UtcNow,
                LastAccessedOnUtc = DateTime.UtcNow,
                Status = ProgressStatus.Started
            };
            dbContext.UserLessonProgress.Add(lessonProgress);
        }

        var evidence = DeserializeEvidence(lessonProgress.CompletionEvidenceJson);
        evidence.VideoSecondsByBlock[blockId] = Math.Max(0, secondsWatched);
        lessonProgress.VideoSecondsWatched = Math.Max(0, secondsWatched);
        lessonProgress.CompletionEvidenceJson = JsonSerializer.Serialize(evidence);
        lessonProgress.LastAccessedOnUtc = DateTime.UtcNow;
        lessonProgress.StartedOnUtc ??= DateTime.UtcNow;

        await MarkBlockCompletedInternalAsync(dbContext, userId, courseId, lessonId, blockId, null, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("Video block completed.");
    }

    public async Task<OperationResult<TrainingQuizSubmissionResultModel>> SubmitQuizBlockAsync(string userId, Guid courseId, Guid lessonId, Guid blockId, Dictionary<int, List<int>> answers, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var block = await dbContext.LessonBlocks.AsNoTracking().FirstOrDefaultAsync(x => x.LessonBlockId == blockId && x.LessonId == lessonId, cancellationToken);
        if (block is null)
        {
            return OperationResult<TrainingQuizSubmissionResultModel>.Failure("Quiz block not found.");
        }

        var metadata = DeserializeMetadata(block.MetadataJson);
        if (metadata.QuizQuestions.Count == 0)
        {
            return OperationResult<TrainingQuizSubmissionResultModel>.Failure("This block does not contain quiz questions.");
        }

        var correctCount = 0;
        for (var index = 0; index < metadata.QuizQuestions.Count; index++)
        {
            var question = metadata.QuizQuestions[index];
            var submitted = answers.TryGetValue(index, out var selected)
                ? selected.OrderBy(x => x).ToList()
                : new List<int>();
            var correct = question.Options
                .Where(x => x.IsCorrect)
                .OrderBy(x => x.OrderIndex)
                .Select((x, optionIndex) => optionIndex)
                .ToList();
            if (submitted.SequenceEqual(correct))
            {
                correctCount++;
            }
        }

        var scorePercent = metadata.QuizQuestions.Count == 0
            ? 0m
            : Math.Round((decimal)correctCount / metadata.QuizQuestions.Count * 100m, 2);
        var passed = scorePercent >= 80m;
        if (passed)
        {
            await MarkBlockCompletedInternalAsync(dbContext, userId, courseId, lessonId, blockId, scorePercent, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var result = new TrainingQuizSubmissionResultModel
        {
            Passed = passed,
            ScorePercent = scorePercent,
            Message = passed
                ? $"Quiz passed with {scorePercent:0.##}%."
                : $"Quiz not yet passed. Current score: {scorePercent:0.##}%."
        };

        return OperationResult<TrainingQuizSubmissionResultModel>.Success(result, result.Message);
    }

    public async Task<OperationResult<TrainingAssessmentSubmissionResultModel>> SubmitAssessmentBlockAsync(string userId, Guid courseId, Guid lessonId, Guid blockId, Dictionary<Guid, Guid?> answers, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var block = await dbContext.LessonBlocks.AsNoTracking().FirstOrDefaultAsync(x => x.LessonBlockId == blockId && x.LessonId == lessonId, cancellationToken);
        if (block is null)
        {
            return OperationResult<TrainingAssessmentSubmissionResultModel>.Failure("Assessment block not found.");
        }

        var metadata = DeserializeMetadata(block.MetadataJson);
        if (!metadata.LinkedAssessmentId.HasValue)
        {
            return OperationResult<TrainingAssessmentSubmissionResultModel>.Failure("No assessment is linked to this block.");
        }

        var assessment = await dbContext.TrainingCourseAssessments.FirstOrDefaultAsync(x => x.TrainingCourseAssessmentId == metadata.LinkedAssessmentId.Value, cancellationToken);
        if (assessment is null)
        {
            return OperationResult<TrainingAssessmentSubmissionResultModel>.Failure("Assessment could not be loaded.");
        }

        var questions = await dbContext.TrainingQuestionBankQuestions
            .Include(x => x.Options)
            .Where(x => x.TrainingCourseAssessmentId == assessment.TrainingCourseAssessmentId && x.IsActive)
            .OrderBy(x => x.Prompt)
            .ToListAsync(cancellationToken);
        if (questions.Count == 0)
        {
            return OperationResult<TrainingAssessmentSubmissionResultModel>.Failure("No assessment questions are configured yet.");
        }

        var attemptNumber = await dbContext.UserAssessmentAttempts
            .Where(x => x.UserId == userId && x.TrainingCourseAssessmentId == assessment.TrainingCourseAssessmentId)
            .MaxAsync(x => (int?)x.AttemptNumber, cancellationToken) ?? 0;
        attemptNumber++;

        var attempt = new UserAssessmentAttempt
        {
            UserAssessmentAttemptId = Guid.NewGuid(),
            TrainingCourseAssessmentId = assessment.TrainingCourseAssessmentId,
            UserId = userId,
            AttemptNumber = attemptNumber,
            StartedOnUtc = DateTime.UtcNow,
            SubmittedOnUtc = DateTime.UtcNow
        };

        decimal availablePoints = 0m;
        decimal earnedPoints = 0m;
        foreach (var question in questions)
        {
            availablePoints += question.Points;
            answers.TryGetValue(question.TrainingQuestionBankQuestionId, out var selectedOptionId);
            var selectedOption = question.Options.FirstOrDefault(x => x.TrainingQuestionBankOptionId == selectedOptionId);
            var isCorrect = selectedOption?.IsCorrect == true;
            var awardedPoints = isCorrect ? question.Points : 0m;
            earnedPoints += awardedPoints;

            attempt.Responses.Add(new UserAssessmentResponse
            {
                UserAssessmentResponseId = Guid.NewGuid(),
                UserAssessmentAttemptId = attempt.UserAssessmentAttemptId,
                TrainingQuestionBankQuestionId = question.TrainingQuestionBankQuestionId,
                SelectedOptionId = selectedOptionId,
                IsCorrect = isCorrect,
                AwardedPoints = awardedPoints
            });
        }

        attempt.ScorePercent = availablePoints == 0m ? 0m : Math.Round(earnedPoints / availablePoints * 100m, 2);
        attempt.Passed = attempt.ScorePercent >= assessment.PassMarkPercent;

        dbContext.UserAssessmentAttempts.Add(attempt);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (attempt.Passed)
        {
            await MarkBlockCompletedInternalAsync(dbContext, userId, courseId, lessonId, blockId, attempt.ScorePercent, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var result = new TrainingAssessmentSubmissionResultModel
        {
            Passed = attempt.Passed,
            ScorePercent = attempt.ScorePercent ?? 0m,
            EarnedPoints = earnedPoints,
            AvailablePoints = availablePoints,
            Message = attempt.Passed
                ? $"Assessment passed with {attempt.ScorePercent:0.##}%."
                : $"Assessment submitted. A minimum of {assessment.PassMarkPercent:0.##}% is required."
        };

        return OperationResult<TrainingAssessmentSubmissionResultModel>.Success(result, result.Message);
    }

    public async Task<TrainingCertificateViewModel?> GetCertificateAsync(string userId, Guid certificateId, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var learner = await LoadLearnerContextAsync(dbContext, userId, cancellationToken);
        if (learner is null)
        {
            return null;
        }

        var certificate = await dbContext.TrainingCertificates.AsNoTracking()
            .Where(x => x.TrainingCertificateId == certificateId)
            .Join(dbContext.CourseCompletionRecords.AsNoTracking(),
                certificateEntity => certificateEntity.CourseCompletionRecordId,
                completion => completion.CourseCompletionRecordId,
                (certificateEntity, completion) => new { certificateEntity, completion })
            .Join(dbContext.Courses.AsNoTracking(),
                joined => joined.completion.CourseId,
                course => course.CourseId,
                (joined, course) => new { joined.certificateEntity, joined.completion, course })
            .Join(dbContext.CourseVersions.AsNoTracking(),
                joined => joined.completion.CourseVersionId,
                version => version.CourseVersionId,
                (joined, version) => new { joined.certificateEntity, joined.completion, joined.course, version })
            .FirstOrDefaultAsync(cancellationToken);
        if (certificate is null)
        {
            return null;
        }

        if (!learner.HasFullAccess && !string.Equals(certificate.completion.UserId, userId, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return new TrainingCertificateViewModel
        {
            CertificateId = certificate.certificateEntity.TrainingCertificateId,
            LearnerFullName = learner.DisplayName,
            CourseTitle = certificate.course.Title,
            CourseCode = certificate.course.Code,
            CertificateNumber = certificate.certificateEntity.CertificateNumber,
            VerificationCode = certificate.certificateEntity.VerificationCode,
            CompletedOnUtc = certificate.completion.CompletedOnUtc,
            ExpiresOnUtc = certificate.completion.ExpiryDateUtc,
            VersionLabel = string.IsNullOrWhiteSpace(certificate.version.VersionLabel)
                ? $"v{certificate.version.VersionNumber}"
                : certificate.version.VersionLabel
        };
    }

    public async Task<OperationResult> SaveVideoProgressAsync(string userId, Guid lessonId, Guid blockId, int secondsWatched, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var lessonProgress = await dbContext.UserLessonProgress.FirstOrDefaultAsync(x => x.UserId == userId && x.LessonId == lessonId, cancellationToken);
        var isNewLessonProgress = lessonProgress is null;
        if (lessonProgress is null)
        {
            lessonProgress = new UserLessonProgress
            {
                UserLessonProgressId = Guid.NewGuid(),
                UserId = userId,
                LessonId = lessonId,
                StartedOnUtc = DateTime.UtcNow,
                LastAccessedOnUtc = DateTime.UtcNow,
                Status = ProgressStatus.Started
            };
            dbContext.UserLessonProgress.Add(lessonProgress);
        }

        if (!isNewLessonProgress)
        {
            await dbContext.Entry(lessonProgress).ReloadAsync(cancellationToken);
        }

        var normalizedSeconds = Math.Max(0, secondsWatched);
        var evidence = DeserializeEvidence(lessonProgress.CompletionEvidenceJson);
        evidence.VideoSecondsByBlock[blockId] = normalizedSeconds;

        lessonProgress.VideoSecondsWatched = normalizedSeconds;
        lessonProgress.CompletionEvidenceJson = JsonSerializer.Serialize(evidence);
        lessonProgress.LastAccessedOnUtc = DateTime.UtcNow;
        lessonProgress.StartedOnUtc ??= DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("Progress saved.");
    }

    public async Task<OperationResult> SaveFlashCardProgressAsync(string userId, Guid courseId, Guid lessonId, Guid blockId, int currentCardIndex, int maxViewedCardIndex, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var block = await dbContext.LessonBlocks.AsNoTracking().FirstOrDefaultAsync(x => x.LessonBlockId == blockId && x.LessonId == lessonId, cancellationToken);
        if (block is null)
        {
            return OperationResult.Failure("Training block not found.");
        }

        var lessonProgress = await dbContext.UserLessonProgress.FirstOrDefaultAsync(x => x.UserId == userId && x.LessonId == lessonId, cancellationToken);
        if (lessonProgress is null)
        {
            lessonProgress = new UserLessonProgress
            {
                UserLessonProgressId = Guid.NewGuid(),
                UserId = userId,
                LessonId = lessonId,
                StartedOnUtc = DateTime.UtcNow,
                LastAccessedOnUtc = DateTime.UtcNow,
                Status = ProgressStatus.Started
            };
            dbContext.UserLessonProgress.Add(lessonProgress);
        }

        var evidence = DeserializeEvidence(lessonProgress.CompletionEvidenceJson);
        var normalizedCurrentIndex = Math.Max(0, currentCardIndex);
        var normalizedMaxViewedIndex = Math.Max(normalizedCurrentIndex, maxViewedCardIndex);
        evidence.FlashCardCurrentIndexByBlock[blockId] = normalizedCurrentIndex;
        evidence.FlashCardMaxViewedIndexByBlock[blockId] = normalizedMaxViewedIndex;

        lessonProgress.CompletionEvidenceJson = JsonSerializer.Serialize(evidence);
        lessonProgress.LastAccessedOnUtc = DateTime.UtcNow;
        lessonProgress.StartedOnUtc ??= DateTime.UtcNow;

        var metadata = DeserializeMetadata(block.MetadataJson);
        var totalCards = metadata.QuizQuestions.Count > 0 ? metadata.QuizQuestions.Count : 1;
        var allViewed = normalizedMaxViewedIndex >= totalCards - 1;
        if (allViewed)
        {
            await MarkBlockCompletedInternalAsync(dbContext, userId, courseId, lessonId, blockId, null, cancellationToken);
        }
        else
        {
            await UpdateLessonProgressAsync(dbContext, userId, courseId, lessonId, lessonProgress, await EnsureCourseProgressAsync(dbContext, userId, courseId, cancellationToken), await dbContext.Courses.FirstAsync(x => x.CourseId == courseId, cancellationToken), cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return OperationResult.Success(allViewed ? "Flash cards completed." : "Flash card progress saved.");
    }

    private async Task MarkBlockCompletedInternalAsync(ApplicationDbContext dbContext, string userId, Guid courseId, Guid lessonId, Guid blockId, decimal? scorePercent, CancellationToken cancellationToken)
    {
        var course = await dbContext.Courses.FirstAsync(x => x.CourseId == courseId, cancellationToken);
        var lessonProgress = await dbContext.UserLessonProgress.FirstOrDefaultAsync(x => x.UserId == userId && x.LessonId == lessonId, cancellationToken);
        if (lessonProgress is null)
        {
            lessonProgress = new UserLessonProgress
            {
                UserLessonProgressId = Guid.NewGuid(),
                UserId = userId,
                LessonId = lessonId,
                StartedOnUtc = DateTime.UtcNow,
                LastAccessedOnUtc = DateTime.UtcNow,
                Status = ProgressStatus.Started
            };
            dbContext.UserLessonProgress.Add(lessonProgress);
        }

        var courseProgress = await EnsureCourseProgressAsync(dbContext, userId, courseId, cancellationToken);

        var evidence = DeserializeEvidence(lessonProgress.CompletionEvidenceJson);
        evidence.CompletedBlockIds.Add(blockId);
        if (scorePercent.HasValue)
        {
            evidence.BlockScores[blockId] = scorePercent.Value;
        }

        lessonProgress.CompletionEvidenceJson = JsonSerializer.Serialize(evidence);
        lessonProgress.LastAccessedOnUtc = DateTime.UtcNow;
        lessonProgress.StartedOnUtc ??= DateTime.UtcNow;
        courseProgress.LastAccessedOnUtc = DateTime.UtcNow;
        courseProgress.StartedOnUtc ??= DateTime.UtcNow;

        await UpdateLessonProgressAsync(dbContext, userId, courseId, lessonId, lessonProgress, courseProgress, course, cancellationToken);
    }

    private async Task UpdateLessonProgressAsync(ApplicationDbContext dbContext, string userId, Guid courseId, Guid lessonId, UserLessonProgress lessonProgress, UserCourseProgress courseProgress, Course course, CancellationToken cancellationToken)
    {
        var lessonBlocks = await dbContext.LessonBlocks.AsNoTracking()
            .Where(x => x.LessonId == lessonId && x.IsActive)
            .OrderBy(x => x.OrderIndex)
            .ToListAsync(cancellationToken);
        var evidence = DeserializeEvidence(lessonProgress.CompletionEvidenceJson);
        var requiredBlockIds = lessonBlocks.Where(x => x.IsRequired).Select(x => x.LessonBlockId).ToList();
        var completedRequiredBlocks = requiredBlockIds.Count == 0
            ? 0
            : requiredBlockIds.Count(x => evidence.CompletedBlockIds.Contains(x));

        lessonProgress.PercentComplete = requiredBlockIds.Count == 0
            ? 100m
            : Math.Round((decimal)completedRequiredBlocks / requiredBlockIds.Count * 100m, 2);
        lessonProgress.Status = lessonProgress.PercentComplete >= 100m ? ProgressStatus.Completed : ProgressStatus.Started;
        lessonProgress.CompletedOnUtc = lessonProgress.Status == ProgressStatus.Completed ? DateTime.UtcNow : null;
        lessonProgress.KnowledgeCheckPassed = lessonBlocks
            .Where(x => x.BlockType is LessonBlockType.Quiz or LessonBlockType.Assessment && x.IsRequired)
            .All(x => evidence.CompletedBlockIds.Contains(x.LessonBlockId));

        var currentVersionId = course.CurrentVersionId;
        if (!currentVersionId.HasValue)
        {
            return;
        }

        var requiredLessons = await dbContext.Lessons.AsNoTracking()
            .Join(dbContext.Modules.AsNoTracking().Where(x => x.CourseVersionId == currentVersionId.Value),
                lesson => lesson.ModuleId,
                module => module.ModuleId,
                (lesson, module) => lesson)
            .Where(x => x.IsActive && x.IsRequired)
            .OrderBy(x => x.OrderIndex)
            .ToListAsync(cancellationToken);

        var requiredLessonIds = requiredLessons.Select(x => x.LessonId).ToList();
        var lessonProgressRecords = await dbContext.UserLessonProgress
            .Where(x => x.UserId == userId && requiredLessonIds.Contains(x.LessonId))
            .ToListAsync(cancellationToken);
        var completedRequiredLessons = lessonProgressRecords.Count(x => x.Status == ProgressStatus.Completed);

        courseProgress.PercentComplete = requiredLessonIds.Count == 0
            ? 100m
            : Math.Round((decimal)completedRequiredLessons / requiredLessonIds.Count * 100m, 2);
        courseProgress.Status = courseProgress.PercentComplete >= 100m ? ProgressStatus.Completed : ProgressStatus.Started;
        courseProgress.CompletedOnUtc = courseProgress.Status == ProgressStatus.Completed ? DateTime.UtcNow : null;
        var nextLessonId = requiredLessons
            .Select(x => x.LessonId)
            .FirstOrDefault(id => !lessonProgressRecords.Any(progress => progress.LessonId == id && progress.Status == ProgressStatus.Completed));

        courseProgress.CurrentLessonId = nextLessonId == Guid.Empty
            ? null
            : await dbContext.Lessons.AsNoTracking().AnyAsync(x => x.LessonId == nextLessonId, cancellationToken)
                ? nextLessonId
                : null;
        courseProgress.ExpiryDateUtc = courseProgress.Status == ProgressStatus.Completed ? DateTime.UtcNow.AddMonths(course.ValidityMonths) : courseProgress.ExpiryDateUtc;

        if (courseProgress.Status == ProgressStatus.Completed)
        {
            await EnsureCompletionRecordAsync(dbContext, userId, course, courseProgress, cancellationToken);
        }
    }

    private async Task<UserCourseProgress> EnsureCourseProgressAsync(ApplicationDbContext dbContext, string userId, Guid courseId, CancellationToken cancellationToken)
    {
        var courseProgress = await dbContext.UserCourseProgress.FirstOrDefaultAsync(x => x.UserId == userId && x.CourseId == courseId, cancellationToken);
        if (courseProgress is not null)
        {
            return courseProgress;
        }

        courseProgress = new UserCourseProgress
        {
            UserCourseProgressId = Guid.NewGuid(),
            UserId = userId,
            CourseId = courseId,
            AssignedOnUtc = DateTime.UtcNow,
            StartedOnUtc = DateTime.UtcNow,
            LastAccessedOnUtc = DateTime.UtcNow,
            Status = ProgressStatus.Started
        };
        dbContext.UserCourseProgress.Add(courseProgress);
        return courseProgress;
    }

    private async Task EnsureCompletionRecordAsync(ApplicationDbContext dbContext, string userId, Course course, UserCourseProgress courseProgress, CancellationToken cancellationToken)
    {
        if (!course.CurrentVersionId.HasValue)
        {
            return;
        }

        var existingCompletion = await dbContext.CourseCompletionRecords
            .FirstOrDefaultAsync(x => x.UserId == userId && x.CourseId == course.CourseId && x.CourseVersionId == course.CurrentVersionId.Value, cancellationToken);
        if (existingCompletion is not null)
        {
            return;
        }

        var completion = new CourseCompletionRecord
        {
            CourseCompletionRecordId = Guid.NewGuid(),
            UserId = userId,
            CourseId = course.CourseId,
            CourseVersionId = course.CurrentVersionId.Value,
            CompletedOnUtc = courseProgress.CompletedOnUtc ?? DateTime.UtcNow,
            ExpiryDateUtc = (courseProgress.CompletedOnUtc ?? DateTime.UtcNow).AddMonths(course.ValidityMonths),
            FinalScorePercent = courseProgress.PercentComplete,
            CertificateNumber = $"TRN-{course.Code}-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}"
        };

        var certificate = new TrainingCertificate
        {
            TrainingCertificateId = Guid.NewGuid(),
            CourseCompletionRecordId = completion.CourseCompletionRecordId,
            CertificateNumber = completion.CertificateNumber,
            VerificationCode = Guid.NewGuid().ToString("N")[..10].ToUpperInvariant(),
            IssuedOnUtc = completion.CompletedOnUtc,
            ExpiresOnUtc = completion.ExpiryDateUtc,
            FilePath = $"/my-training/certificates/{Guid.Empty}"
        };
        certificate.FilePath = $"/my-training/certificates/{certificate.TrainingCertificateId}";

        dbContext.CourseCompletionRecords.Add(completion);
        dbContext.TrainingCertificates.Add(certificate);
    }

    private static List<TrainingPlayerBlockModel> BuildBlockModels(
        List<BlockProjection> lessonBlocks,
        Dictionary<Guid, TrainingLessonBlockMetadataModel> metadataLookup,
        LessonProgressEvidenceModel evidence,
        List<AssessmentQuestionProjection> assessmentQuestions,
        List<AssessmentOptionProjection> assessmentOptions,
        Dictionary<Guid, decimal?> latestAssessmentScores,
        IWebHostEnvironment environment)
    {
        return lessonBlocks.Select(block =>
        {
            var metadata = metadataLookup[block.BlockId];
            return new TrainingPlayerBlockModel
            {
                BlockId = block.BlockId,
                BlockType = NormalizeBlockType(block.BlockType),
                DisplayType = GetDisplayType(block.BlockType),
                Title = block.Title,
                Subtitle = block.Subtitle,
                ContentHtml = NormalizeRichHtml(block.ContentHtml),
                SecondaryContentHtml = NormalizeRichHtml(metadata.SecondaryContentHtml),
                IntroTextHtml = NormalizeRichHtml(metadata.IntroTextHtml),
                ThumbnailUrl = block.ThumbnailUrl,
                FileUrl = NormalizeVideoUrl(block.FileUrl, environment),
                ExternalUrl = block.ExternalUrl,
                MimeType = block.MimeType,
                DurationSeconds = block.DurationSeconds,
                SavedFlashCardIndex = evidence.FlashCardCurrentIndexByBlock.GetValueOrDefault(block.BlockId),
                SavedFlashCardMaxViewedIndex = evidence.FlashCardMaxViewedIndexByBlock.GetValueOrDefault(block.BlockId),
                IsCompleted = evidence.CompletedBlockIds.Contains(block.BlockId),
                CompletionLabel = evidence.CompletedBlockIds.Contains(block.BlockId) ? "Completed" : "Required",
                LinkedAssessmentId = metadata.LinkedAssessmentId,
                LinkedAssessmentName = metadata.LinkedAssessmentName,
                LatestScorePercent = metadata.LinkedAssessmentId.HasValue
                    ? latestAssessmentScores.GetValueOrDefault(metadata.LinkedAssessmentId.Value)
                    : evidence.BlockScores.GetValueOrDefault(block.BlockId),
                QuizQuestions = metadata.QuizQuestions.Select((question, questionIndex) => new TrainingPlayerQuizQuestionModel
                {
                    QuestionIndex = questionIndex,
                    Prompt = NormalizeRichHtml(question.Prompt) ?? string.Empty,
                    QuestionType = question.QuestionType,
                    ScenarioText = NormalizeRichHtml(question.ScenarioText),
                    Options = question.Options.Select((option, optionIndex) => new TrainingPlayerQuizOptionModel
                    {
                        OptionIndex = optionIndex,
                        OptionText = option.OptionText
                    }).ToList()
                }).ToList(),
                AssessmentQuestions = metadata.LinkedAssessmentId.HasValue
                    ? assessmentQuestions.Where(x => x.AssessmentId == metadata.LinkedAssessmentId.Value)
                        .Select(question => new TrainingPlayerAssessmentQuestionModel
                        {
                            QuestionId = question.QuestionId,
                            Prompt = question.Prompt,
                            ScenarioText = question.ScenarioText,
                            Points = question.Points,
                            Options = assessmentOptions.Where(x => x.QuestionId == question.QuestionId)
                                .Select(option => new TrainingPlayerAssessmentOptionModel
                                {
                                    OptionId = option.OptionId,
                                    OptionText = option.OptionText
                                })
                                .ToList()
                        })
                        .ToList()
                    : new List<TrainingPlayerAssessmentQuestionModel>()
            };
        }).ToList();
    }

    private static void ApplyBlockAvailability(List<TrainingPlayerBlockModel> blockModels)
    {
        var lockReached = false;
        foreach (var block in blockModels)
        {
            block.IsLocked = lockReached;
            if (block.IsRequired && !block.IsCompleted)
            {
                lockReached = true;
            }
        }
    }

    private static LessonProjection? SelectLesson(List<LessonProjection> flattenedLessons, Dictionary<Guid, TrainingPlayerLessonSummaryModel> lessonSummaries, Guid? requestedLessonId)
    {
        if (requestedLessonId.HasValue && lessonSummaries.TryGetValue(requestedLessonId.Value, out var requestedSummary) && !requestedSummary.IsLocked)
        {
            return flattenedLessons.FirstOrDefault(x => x.LessonId == requestedLessonId.Value);
        }

        return flattenedLessons.FirstOrDefault(x => !lessonSummaries[x.LessonId].IsLocked && !lessonSummaries[x.LessonId].IsCompleted)
            ?? flattenedLessons.FirstOrDefault(x => !lessonSummaries[x.LessonId].IsLocked)
            ?? flattenedLessons.FirstOrDefault();
    }

    private static TrainingPlayerBlockModel? SelectBlock(List<TrainingPlayerBlockModel> blockModels, Guid? requestedBlockId)
    {
        if (requestedBlockId.HasValue)
        {
            var requested = blockModels.FirstOrDefault(x => x.BlockId == requestedBlockId.Value && !x.IsLocked);
            if (requested is not null)
            {
                return requested;
            }
        }

        return blockModels.FirstOrDefault(x => !x.IsLocked && !x.IsCompleted)
            ?? blockModels.FirstOrDefault(x => !x.IsLocked)
            ?? blockModels.FirstOrDefault();
    }

    private static decimal CalculateCourseProgress(List<LessonProjection> lessons, Dictionary<Guid, TrainingPlayerLessonSummaryModel> summaries)
    {
        var requiredLessons = lessons.ToList();
        if (requiredLessons.Count == 0)
        {
            return 100m;
        }

        var completed = requiredLessons.Count(x => summaries.GetValueOrDefault(x.LessonId)?.IsCompleted == true);
        return Math.Round((decimal)completed / requiredLessons.Count * 100m, 2);
    }

    private async Task<LearnerContext?> LoadLearnerContextAsync(ApplicationDbContext dbContext, string userId, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var identityUser = await userManager.FindByIdAsync(userId);
        if (identityUser is null)
        {
            return null;
        }

        var roles = await userManager.GetRolesAsync(identityUser);
        var onboardRole = await dbContext.CrewDeployments.AsNoTracking()
            .Where(x => x.UserId == userId && x.Status == DeploymentStatus.Active)
            .OrderByDescending(x => x.StartedOnUtc)
            .Select(x => x.OnboardRoleName)
            .FirstOrDefaultAsync(cancellationToken);

        return new LearnerContext
        {
            UserId = userId,
            DisplayName = string.IsNullOrWhiteSpace(user.FullName) ? user.Email ?? user.UserName ?? "Learner" : user.FullName,
            IsCrew = user.IsCrew,
            Roles = roles.ToHashSet(StringComparer.OrdinalIgnoreCase),
            Qualification = user.Qualification.GetDisplayName(),
            OnboardRole = !string.IsNullOrWhiteSpace(onboardRole) ? onboardRole : user.CrewRank.GetDisplayName(),
            HasFullAccess = roles.Any(role => FullAccessRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
        };
    }

    private async Task<CourseAccessContext> LoadCourseAccessContextAsync(ApplicationDbContext dbContext, LearnerContext learner, CancellationToken cancellationToken)
    {
        var courses = await dbContext.Courses.AsNoTracking()
            .Where(x => x.IsActive && x.CurrentVersionId.HasValue)
            .Select(x => new CourseAccessProjection
            {
                CourseId = x.CourseId,
                Code = x.Code,
                Title = x.Title,
                Summary = x.Summary,
                ThumbnailUrl = x.ThumbnailUrl,
                AudienceSummary = x.TargetAudienceSummary,
                PassMarkPercent = x.PassMarkPercent,
                DurationMinutes = x.EstimatedDurationMinutes,
                ValidityMonths = x.ValidityMonths,
                IsMandatory = x.IsMandatory,
                CurrentVersionId = x.CurrentVersionId!.Value
            })
            .ToListAsync(cancellationToken);

        var courseIds = courses.Select(x => x.CourseId).ToList();
        var rules = await dbContext.CourseAudienceRules.AsNoTracking()
            .Where(x => courseIds.Contains(x.CourseId))
            .ToListAsync(cancellationToken);
        var assignments = await dbContext.UserTrainingAssignments.AsNoTracking()
            .Where(x => x.UserId == learner.UserId && courseIds.Contains(x.CourseId))
            .ToListAsync(cancellationToken);
        var progressRecords = await dbContext.UserCourseProgress.AsNoTracking()
            .Where(x => x.UserId == learner.UserId && courseIds.Contains(x.CourseId))
            .ToListAsync(cancellationToken);
        var completions = await dbContext.CourseCompletionRecords.AsNoTracking()
            .Where(x => x.UserId == learner.UserId && courseIds.Contains(x.CourseId))
            .Select(x => new CompletionProjection
            {
                CompletionId = x.CourseCompletionRecordId,
                CourseId = x.CourseId,
                CompletedOnUtc = x.CompletedOnUtc,
                ExpiryDateUtc = x.ExpiryDateUtc,
                FinalScorePercent = x.FinalScorePercent,
                CertificateNumber = x.CertificateNumber,
                CourseVersionId = x.CourseVersionId
            })
            .ToListAsync(cancellationToken);
        var completionIds = completions.Select(x => x.CompletionId).ToList();
        var certificates = await dbContext.TrainingCertificates.AsNoTracking()
            .Where(x => completionIds.Contains(x.CourseCompletionRecordId))
            .Select(x => new CertificateProjection
            {
                CertificateId = x.TrainingCertificateId,
                CompletionId = x.CourseCompletionRecordId,
                VerificationCode = x.VerificationCode,
                FilePath = x.FilePath,
                ExpiresOnUtc = x.ExpiresOnUtc,
                IssuedOnUtc = x.IssuedOnUtc
            })
            .ToListAsync(cancellationToken);

        var requiredCourses = new List<MyTrainingCourseListItemModel>();
        var additionalCourses = new List<MyTrainingCourseListItemModel>();

        foreach (var course in courses)
        {
            var courseRules = rules.Where(x => x.CourseId == course.CourseId).ToList();
            var assignment = assignments.FirstOrDefault(x => x.CourseId == course.CourseId);
            var progress = progressRecords.FirstOrDefault(x => x.CourseId == course.CourseId);
            var completion = completions.Where(x => x.CourseId == course.CourseId).OrderByDescending(x => x.CompletedOnUtc).FirstOrDefault();
            var certificate = completion is null ? null : certificates.FirstOrDefault(x => x.CompletionId == completion.CompletionId);
            var applicable = learner.HasFullAccess || IsCourseApplicable(course, courseRules, assignment, learner);
            var item = BuildDashboardCourseItem(course, assignment, progress, completion, certificate);

            if (applicable)
            {
                requiredCourses.Add(item);
            }
            else if (learner.HasFullAccess)
            {
                additionalCourses.Add(item);
            }
        }

        var certificateItems = completions
            .Join(certificates,
                completion => completion.CompletionId,
                certificate => certificate.CompletionId,
                (completion, certificate) => new MyTrainingCertificateListItemModel
                {
                    CertificateId = certificate.CertificateId,
                    CourseId = completion.CourseId,
                    CourseCode = courses.First(x => x.CourseId == completion.CourseId).Code,
                    CourseTitle = courses.First(x => x.CourseId == completion.CourseId).Title,
                    CertificateNumber = completion.CertificateNumber,
                    VerificationCode = certificate.VerificationCode,
                    CompletedOnUtc = completion.CompletedOnUtc,
                    ExpiresOnUtc = completion.ExpiryDateUtc,
                    FinalScorePercent = completion.FinalScorePercent,
                    PassMarkPercent = courses.First(x => x.CourseId == completion.CourseId).PassMarkPercent,
                    CompletionStatus = completion.ExpiryDateUtc < DateTime.UtcNow ? "Expired" : "Completed",
                    ViewUrl = $"/my-training/certificates/{certificate.CertificateId}",
                    DownloadUrl = $"/my-training/certificates/{certificate.CertificateId}?download=true"
                })
            .OrderByDescending(x => x.CompletedOnUtc)
            .ToList();

        return new CourseAccessContext
        {
            RequiredCourses = requiredCourses,
            AdditionalCourses = additionalCourses,
            Certificates = certificateItems
        };
    }

    private static MyTrainingCourseListItemModel BuildDashboardCourseItem(
        CourseAccessProjection course,
        UserTrainingAssignment? assignment,
        UserCourseProgress? progress,
        CompletionProjection? completion,
        CertificateProjection? certificate)
    {
        var now = DateTime.UtcNow;
        var status = TrainingAccessStatus.NotStarted;
        if (completion is not null && completion.ExpiryDateUtc < now)
        {
            status = TrainingAccessStatus.Expired;
        }
        else if (completion is not null)
        {
            status = TrainingAccessStatus.Compliant;
        }
        else if (assignment?.DueDateUtc is DateTime dueDate && dueDate < now)
        {
            status = TrainingAccessStatus.Overdue;
        }
        else if (progress?.Status == ProgressStatus.Started)
        {
            status = TrainingAccessStatus.InProgress;
        }

        return new MyTrainingCourseListItemModel
        {
            CourseId = course.CourseId,
            Code = course.Code,
            Title = course.Title,
            Summary = course.Summary,
            ThumbnailUrl = course.ThumbnailUrl,
            AudienceSummary = course.AudienceSummary,
            PassMarkPercent = course.PassMarkPercent,
            DurationMinutes = course.DurationMinutes,
            ValidityMonths = course.ValidityMonths,
            ProgressPercent = completion is not null && completion.ExpiryDateUtc >= now ? 100m : progress?.PercentComplete ?? 0m,
            DueDateUtc = assignment?.DueDateUtc,
            ExpiryDateUtc = completion?.ExpiryDateUtc,
            Status = status,
            ActionText = status switch
            {
                TrainingAccessStatus.Compliant => "Review",
                TrainingAccessStatus.InProgress => "Resume",
                TrainingAccessStatus.Expired => "Restart",
                _ => "Start"
            },
            IsMandatory = course.IsMandatory
        };
    }

    private static bool IsCourseApplicable(CourseAccessProjection course, List<CourseAudienceRule> rules, UserTrainingAssignment? assignment, LearnerContext learner)
    {
        if (assignment is not null)
        {
            return true;
        }

        if (rules.Count == 0)
        {
            return course.IsMandatory && learner.IsCrew;
        }

        foreach (var rule in rules)
        {
            switch (rule.RuleType)
            {
                case CourseAudienceRuleType.AllCrew when learner.IsCrew:
                    return true;
                case CourseAudienceRuleType.ApplicationRole when !string.IsNullOrWhiteSpace(rule.ApplicationRoleName)
                    && learner.Roles.Contains(rule.ApplicationRoleName):
                    return true;
                case CourseAudienceRuleType.OnBoardRole when !string.IsNullOrWhiteSpace(rule.OnBoardRole)
                    && string.Equals(rule.OnBoardRole, learner.OnboardRole, StringComparison.OrdinalIgnoreCase):
                    return true;
                case CourseAudienceRuleType.Qualification when !string.IsNullOrWhiteSpace(rule.Qualification)
                    && string.Equals(rule.Qualification, learner.Qualification, StringComparison.OrdinalIgnoreCase):
                    return true;
                case CourseAudienceRuleType.ManualAssignment:
                    return true;
            }
        }

        return false;
    }

    private static LessonProgressEvidenceModel DeserializeEvidence(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new LessonProgressEvidenceModel();
        }

        try
        {
            return JsonSerializer.Deserialize<LessonProgressEvidenceModel>(json) ?? new LessonProgressEvidenceModel();
        }
        catch
        {
            return new LessonProgressEvidenceModel();
        }
    }

    private static TrainingLessonBlockMetadataModel DeserializeMetadata(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new TrainingLessonBlockMetadataModel();
        }

        try
        {
            return JsonSerializer.Deserialize<TrainingLessonBlockMetadataModel>(json) ?? new TrainingLessonBlockMetadataModel();
        }
        catch
        {
            return new TrainingLessonBlockMetadataModel();
        }
    }

    private static LessonBlockType NormalizeBlockType(LessonBlockType blockType) => blockType;

    private static string? NormalizeVideoUrl(string? url, IWebHostEnvironment environment)
    {
        if (TrainingAssetStorageService.TryMapToStreamEndpoint(url, environment, out var streamUrl))
        {
            return streamUrl;
        }

        return url;
    }

    private static string GetDisplayType(LessonBlockType blockType) => blockType switch
    {
        LessonBlockType.TextNarrative => "Narrative",
        LessonBlockType.Card => "Card",
        LessonBlockType.Flashcard => "Flash Cards",
        LessonBlockType.Video => "Video",
        LessonBlockType.Quiz => "Quiz",
        LessonBlockType.Assessment => "Assessment",
        LessonBlockType.Download => "Download",
        _ => blockType.ToString()
    };

    private static string GetBlockActionText(LessonBlockType blockType) => blockType switch
    {
        LessonBlockType.Video => "Confirm Video Watched",
        LessonBlockType.Flashcard => "Confirm Cards Reviewed",
        LessonBlockType.Download => "Confirm Download Reviewed",
        _ => "Mark Block Complete"
    };

    private sealed class LearnerContext
    {
        public string UserId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool IsCrew { get; set; }
        public HashSet<string> Roles { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public string? Qualification { get; set; }
        public string? OnboardRole { get; set; }
        public bool HasFullAccess { get; set; }
    }

    private sealed class CourseAccessContext
    {
        public List<MyTrainingCourseListItemModel> RequiredCourses { get; set; } = new();
        public List<MyTrainingCourseListItemModel> AdditionalCourses { get; set; } = new();
        public List<MyTrainingCertificateListItemModel> Certificates { get; set; } = new();
    }

    private sealed class LessonProgressEvidenceModel
    {
        public HashSet<Guid> CompletedBlockIds { get; set; } = new();
        public Dictionary<Guid, decimal> BlockScores { get; set; } = new();
        public Dictionary<Guid, int> VideoSecondsByBlock { get; set; } = new();
        public Dictionary<Guid, int> FlashCardCurrentIndexByBlock { get; set; } = new();
        public Dictionary<Guid, int> FlashCardMaxViewedIndexByBlock { get; set; } = new();
    }

    private static string? NormalizeRichHtml(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return html;
        }

        return Regex.Replace(
            html,
            "(<img[^>]*\\ssrc=[\"'])(?!https?:|/|data:)([^\"']+)([\"'])",
            match =>
            {
                var prefix = match.Groups[1].Value;
                var path = match.Groups[2].Value.TrimStart('~').TrimStart('/');
                var suffix = match.Groups[3].Value;
                return $"{prefix}/{path}{suffix}";
            },
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    private sealed class CourseProjection
    {
        public Guid CourseId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public string? RegulatoryReference { get; set; }
        public decimal PassMarkPercent { get; set; }
        public int ValidityMonths { get; set; }
        public Guid CurrentVersionId { get; set; }
    }

    private sealed class CourseAccessProjection
    {
        public Guid CourseId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? AudienceSummary { get; set; }
        public decimal PassMarkPercent { get; set; }
        public int? DurationMinutes { get; set; }
        public int ValidityMonths { get; set; }
        public bool IsMandatory { get; set; }
        public Guid CurrentVersionId { get; set; }
    }

    private sealed class ModuleProjection
    {
        public Guid ModuleId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
    }

    private sealed class LessonProjection
    {
        public Guid LessonId { get; set; }
        public Guid ModuleId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public int OrderIndex { get; set; }
    }

    private sealed class BlockProjection
    {
        public Guid BlockId { get; set; }
        public Guid LessonId { get; set; }
        public LessonBlockType BlockType { get; set; }
        public string? Title { get; set; }
        public string? Subtitle { get; set; }
        public int OrderIndex { get; set; }
        public string? ContentHtml { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? FileUrl { get; set; }
        public string? ExternalUrl { get; set; }
        public string? MimeType { get; set; }
        public int? DurationSeconds { get; set; }
        public string? MetadataJson { get; set; }
        public bool IsRequired { get; set; }
    }

    private sealed class AssessmentQuestionProjection
    {
        public Guid AssessmentId { get; set; }
        public Guid QuestionId { get; set; }
        public string Prompt { get; set; } = string.Empty;
        public string? ScenarioText { get; set; }
        public decimal Points { get; set; }
    }

    private sealed class AssessmentOptionProjection
    {
        public Guid QuestionId { get; set; }
        public Guid OptionId { get; set; }
        public string OptionText { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
    }

    private sealed class CompletionProjection
    {
        public Guid CompletionId { get; set; }
        public Guid CourseId { get; set; }
        public Guid CourseVersionId { get; set; }
        public DateTime CompletedOnUtc { get; set; }
        public DateTime ExpiryDateUtc { get; set; }
        public decimal? FinalScorePercent { get; set; }
        public string CertificateNumber { get; set; } = string.Empty;
    }

    private sealed class CertificateProjection
    {
        public Guid CertificateId { get; set; }
        public Guid CompletionId { get; set; }
        public string VerificationCode { get; set; } = string.Empty;
        public string? FilePath { get; set; }
        public DateTime ExpiresOnUtc { get; set; }
        public DateTime IssuedOnUtc { get; set; }
    }
}
