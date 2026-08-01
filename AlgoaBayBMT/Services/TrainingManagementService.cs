using AlgoaBayBMT.Data;
using AlgoaBayBMT.Services.Interfaces;
using AlgoaBayBMT.Services.Models;
using AlgoaBayBMT.Shared.Models;
using AlgoaBayBMT.Shared.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AlgoaBayBMT.Services
{
    public class TrainingManagementService(
        IDbContextFactory<ApplicationDbContext> dbContextFactory,
        RoleManager<IdentityRole> roleManager,
        ITrainingAssetStorageService trainingAssetStorageService,
        ILogger<TrainingManagementService> logger) : ITrainingManagementService
    {
        public async Task<TrainingDashboardModel> GetDashboardAsync(CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var assignedLearnerIds = await dbContext.UserTrainingAssignments.AsNoTracking()
                .Select(x => x.UserId)
                .Distinct()
                .ToListAsync(cancellationToken);
            var startedLearnerIds = await dbContext.UserCourseProgress.AsNoTracking()
                .Where(x => x.StartedOnUtc.HasValue || x.Status != ProgressStatus.NotStarted)
                .Select(x => x.UserId)
                .Distinct()
                .ToListAsync(cancellationToken);
            var completedLearnerIds = await dbContext.CourseCompletionRecords.AsNoTracking()
                .Select(x => x.UserId)
                .Distinct()
                .ToListAsync(cancellationToken);
            var enrolledLearnerCount = assignedLearnerIds
                .Concat(startedLearnerIds)
                .Concat(completedLearnerIds)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();

            var dashboard = new TrainingDashboardModel
            {
                TotalCourses = await dbContext.Courses.AsNoTracking().CountAsync(x => !x.IsDeleted, cancellationToken),
                ActiveCourses = await dbContext.Courses.AsNoTracking().CountAsync(x => x.IsActive && !x.IsDeleted, cancellationToken),
                PublishedCourses = await dbContext.CourseVersions.AsNoTracking().CountAsync(x => x.Status == CourseVersionStatus.Published, cancellationToken),
                DraftCourses = await dbContext.CourseVersions.AsNoTracking().CountAsync(x => x.Status == CourseVersionStatus.Draft, cancellationToken),
                TotalModules = await dbContext.TrainingModules.AsNoTracking().CountAsync(x => !x.IsArchived, cancellationToken),
                TotalLessons = await dbContext.Lessons.AsNoTracking().CountAsync(cancellationToken),
                TotalContentBlocks = await dbContext.LessonBlocks.AsNoTracking().CountAsync(cancellationToken),
                TotalQuestionBankQuestions = await dbContext.TrainingQuestionBankQuestions.AsNoTracking().CountAsync(cancellationToken),
                ActiveLearners = await dbContext.UserCourseProgress.AsNoTracking().Select(x => x.UserId).Distinct().CountAsync(cancellationToken),
                EnrolledLearners = enrolledLearnerCount,
                StartedLearners = startedLearnerIds.Concat(completedLearnerIds).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
                CompletedLearners = completedLearnerIds.Count
            };

            dashboard.RecentCourses = (await GetCoursesAsync(cancellationToken))
                .OrderByDescending(x => x.CreatedOnUtc)
                .Take(6)
                .ToList();

            return dashboard;
        }

        public async Task<OperationResult> ResetTrainingDataAsync(string? changedByUserId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var trainingFileUrls = new List<string>();
            trainingFileUrls.AddRange(await dbContext.Courses.AsNoTracking()
                .Where(x => x.ThumbnailUrl != null)
                .Select(x => x.ThumbnailUrl!)
                .ToListAsync(cancellationToken));

            var blockAssets = await dbContext.LessonBlocks.AsNoTracking()
                .Select(x => new { x.ThumbnailUrl, x.FileUrl, x.ExternalUrl })
                .ToListAsync(cancellationToken);
            trainingFileUrls.AddRange(blockAssets.SelectMany(x => new[] { x.ThumbnailUrl, x.FileUrl, x.ExternalUrl }).Where(x => !string.IsNullOrWhiteSpace(x))!);

            trainingFileUrls.AddRange(await dbContext.MediaAssets.AsNoTracking()
                .Where(x => x.RelativePath.StartsWith("uploads/training"))
                .Select(x => "/" + x.RelativePath)
                .ToListAsync(cancellationToken));

            var deletedCourseCount = await dbContext.Courses.AsNoTracking().CountAsync(cancellationToken);

            await TransactionalExecution.ExecuteAsync(dbContext, cancellationToken, async transaction =>
            {
            await dbContext.UserAssessmentResponses.ExecuteDeleteAsync(cancellationToken);
            await dbContext.UserAssessmentAttempts.ExecuteDeleteAsync(cancellationToken);
            await dbContext.TrainingQuestionBankOptions.ExecuteDeleteAsync(cancellationToken);
            await dbContext.TrainingQuestionBankQuestions.ExecuteDeleteAsync(cancellationToken);
            // ModuleVersions -> TrainingCourseAssessments is NoAction (two cascade paths reach the
            // same tables), so the back-reference must be cleared before the assessments go.
            await dbContext.ModuleVersions.ExecuteUpdateAsync(
                setters => setters.SetProperty(x => x.AssessmentId, (Guid?)null), cancellationToken);
            await dbContext.TrainingCourseAssessments.ExecuteDeleteAsync(cancellationToken);
            await dbContext.TrainingCertificates.ExecuteDeleteAsync(cancellationToken);
            await dbContext.CourseCompletionRecords.ExecuteDeleteAsync(cancellationToken);
            await dbContext.UserLessonProgress.ExecuteDeleteAsync(cancellationToken);
            await dbContext.UserCourseProgress.ExecuteDeleteAsync(cancellationToken);
            await dbContext.UserTrainingAssignments.ExecuteDeleteAsync(cancellationToken);
            await dbContext.CourseAudienceRules.ExecuteDeleteAsync(cancellationToken);
            // Rank configuration and course-module references come out before the course versions
            // they hang off, and before the module versions they point at (both FKs are Restrict).
            await dbContext.CourseRankModules.ExecuteDeleteAsync(cancellationToken);
            await dbContext.CourseRankProfiles.ExecuteDeleteAsync(cancellationToken);
            await dbContext.CourseModules.ExecuteDeleteAsync(cancellationToken);
            await dbContext.LessonBlocks.ExecuteDeleteAsync(cancellationToken);
            await dbContext.Lessons.ExecuteDeleteAsync(cancellationToken);
            // Clear the identity -> version pointer before deleting versions (Restrict FK).
            await dbContext.TrainingModules.ExecuteUpdateAsync(
                setters => setters.SetProperty(x => x.CurrentVersionId, (Guid?)null), cancellationToken);
            await dbContext.ModuleVersions.ExecuteDeleteAsync(cancellationToken);
            await dbContext.TrainingModules.ExecuteDeleteAsync(cancellationToken);
            await dbContext.CourseVersions.ExecuteDeleteAsync(cancellationToken);
            await dbContext.TrainingAuditLogs.ExecuteDeleteAsync(cancellationToken);
            await dbContext.Courses.ExecuteDeleteAsync(cancellationToken);
            await dbContext.MediaAssets
                .Where(x => x.RelativePath.StartsWith("uploads/training"))
                .ExecuteDeleteAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);
                return true;
            });

            await trainingAssetStorageService.DeleteFilesAsync(trainingFileUrls, cancellationToken);

            return OperationResult.Success(deletedCourseCount == 0
                ? "Training data was already empty."
                : $"Training reset completed. {deletedCourseCount} course record(s) and related training data were removed.");
        }

        public async Task<List<TrainingCourseListItemModel>> GetCoursesAsync(CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var courses = await dbContext.Courses
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Title)
                .Select(x => new TrainingCourseListItemModel
                {
                    CourseId = x.CourseId,
                    Code = x.Code,
                    Title = x.Title,
                    Summary = x.Summary,
                    Description = x.Description,
                    TargetAudienceSummary = x.TargetAudienceSummary,
                    PassMarkPercent = x.PassMarkPercent,
                    DurationMinutes = x.EstimatedDurationMinutes,
                    ValidityMonths = x.ValidityMonths,
                    IsMandatory = x.IsMandatory,
                    IsActive = x.IsActive,
                    ThumbnailUrl = x.ThumbnailUrl,
                    CurrentVersionId = x.CurrentVersionId,
                    CreatedOnUtc = x.CreatedOnUtc,
                    Cost = x.Cost
                })
                .ToListAsync(cancellationToken);

            var courseIds = courses.Select(x => x.CourseId).ToList();
            var audienceRules = await dbContext.CourseAudienceRules
                .AsNoTracking()
                .Where(x => courseIds.Contains(x.CourseId))
                .ToListAsync(cancellationToken);
            var audienceLookup = audienceRules
                .GroupBy(x => x.CourseId)
                .ToDictionary(x => x.Key, x => ResolveAudienceType(x));

            var currentVersionIds = courses
                .Where(x => x.CurrentVersionId.HasValue)
                .Select(x => x.CurrentVersionId!.Value)
                .Distinct()
                .ToList();

            var versionLookup = await dbContext.CourseVersions
                .AsNoTracking()
                .Where(x => currentVersionIds.Contains(x.CourseVersionId))
                .Select(x => new
                {
                    x.CourseVersionId,
                    x.VersionNumber,
                    x.VersionLabel,
                    x.Status
                })
                .ToDictionaryAsync(x => x.CourseVersionId, cancellationToken);

            var moduleCounts = await dbContext.CourseModules
                .AsNoTracking()
                .Where(x => currentVersionIds.Contains(x.CourseVersionId))
                .GroupBy(x => x.CourseVersionId)
                .Select(x => new { CourseVersionId = x.Key, Count = x.Count() })
                .ToDictionaryAsync(x => x.CourseVersionId, x => x.Count, cancellationToken);

            var lessonCounts = await dbContext.CourseModules
                .AsNoTracking()
                .Where(x => currentVersionIds.Contains(x.CourseVersionId))
                .Join(dbContext.Lessons.AsNoTracking(),
                    courseModule => courseModule.ModuleVersionId,
                    lesson => lesson.ModuleVersionId,
                    (courseModule, lesson) => new { courseModule.CourseVersionId, lesson.LessonId })
                .GroupBy(x => x.CourseVersionId)
                .Select(x => new { CourseVersionId = x.Key, Count = x.Count() })
                .ToDictionaryAsync(x => x.CourseVersionId, x => x.Count, cancellationToken);

            var courseAssignments = await dbContext.UserTrainingAssignments
                .AsNoTracking()
                .Where(x => courseIds.Contains(x.CourseId))
                .Select(x => new { x.CourseId, x.UserId })
                .ToListAsync(cancellationToken);
            var courseProgressRecords = await dbContext.UserCourseProgress
                .AsNoTracking()
                .Where(x => courseIds.Contains(x.CourseId))
                .Select(x => new { x.CourseId, x.UserId, x.StartedOnUtc, x.Status })
                .ToListAsync(cancellationToken);
            var courseCompletions = await dbContext.CourseCompletionRecords
                .AsNoTracking()
                .Where(x => courseIds.Contains(x.CourseId))
                .Select(x => new { x.CourseId, x.UserId })
                .ToListAsync(cancellationToken);

            foreach (var course in courses)
            {
                course.AudienceType = audienceLookup.GetValueOrDefault(course.CourseId, TrainingAudienceType.All);
                course.TargetAudienceSummary = GetAudienceSummary(course.AudienceType);

                if (course.CurrentVersionId.HasValue && versionLookup.TryGetValue(course.CurrentVersionId.Value, out var version))
                {
                    course.CurrentVersionNumber = version.VersionNumber;
                    course.CurrentVersionLabel = version.VersionLabel;
                    course.CurrentVersionStatus = version.Status;
                    course.ModuleCount = moduleCounts.GetValueOrDefault(course.CurrentVersionId.Value);
                    course.LessonCount = lessonCounts.GetValueOrDefault(course.CurrentVersionId.Value);
                }

                course.EnrolledLearnerCount = courseAssignments.Where(x => x.CourseId == course.CourseId).Select(x => x.UserId)
                    .Concat(courseProgressRecords.Where(x => x.CourseId == course.CourseId).Select(x => x.UserId))
                    .Concat(courseCompletions.Where(x => x.CourseId == course.CourseId).Select(x => x.UserId))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count();
                course.StartedLearnerCount = courseProgressRecords.Where(x => x.CourseId == course.CourseId && (x.StartedOnUtc.HasValue || x.Status != ProgressStatus.NotStarted)).Select(x => x.UserId)
                    .Concat(courseCompletions.Where(x => x.CourseId == course.CourseId).Select(x => x.UserId))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count();
                course.CompletedLearnerCount = courseCompletions.Where(x => x.CourseId == course.CourseId).Select(x => x.UserId)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count();
            }

            return courses;
        }

        public async Task<TrainingCourseStudentStatusPageModel?> GetCourseStudentStatusPageAsync(Guid courseId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var course = await dbContext.Courses.AsNoTracking()
                .Where(x => x.CourseId == courseId)
                .Select(x => new TrainingCourseStudentStatusPageModel
                {
                    CourseId = x.CourseId,
                    CourseCode = x.Code,
                    CourseTitle = x.Title,
                    Summary = x.Summary,
                    PassMarkPercent = x.PassMarkPercent,
                    ValidityMonths = x.ValidityMonths
                })
                .FirstOrDefaultAsync(cancellationToken);
            if (course is null)
            {
                return null;
            }

            var assignments = await dbContext.UserTrainingAssignments.AsNoTracking()
                .Where(x => x.CourseId == courseId)
                .Select(x => new { x.UserId, x.DueDateUtc })
                .ToListAsync(cancellationToken);
            var progressRecords = await dbContext.UserCourseProgress.AsNoTracking()
                .Where(x => x.CourseId == courseId)
                .Select(x => new { x.UserId, x.Status, x.StartedOnUtc, x.CompletedOnUtc, x.ExpiryDateUtc })
                .ToListAsync(cancellationToken);
            var completionRecords = await dbContext.CourseCompletionRecords.AsNoTracking()
                .Where(x => x.CourseId == courseId)
                .Select(x => new { x.CourseCompletionRecordId, x.UserId, x.CompletedOnUtc, x.ExpiryDateUtc, x.FinalScorePercent, x.CertificateNumber })
                .ToListAsync(cancellationToken);

            var latestCompletions = completionRecords
                .GroupBy(x => x.UserId, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => group.OrderByDescending(x => x.CompletedOnUtc).First(),
                    StringComparer.OrdinalIgnoreCase);

            var completionIds = latestCompletions.Values.Select(x => x.CourseCompletionRecordId).ToList();
            var certificates = completionIds.Count == 0
                ? new Dictionary<Guid, (Guid CertificateId, string CertificateNumber)>()
                : await dbContext.TrainingCertificates.AsNoTracking()
                    .Where(x => completionIds.Contains(x.CourseCompletionRecordId))
                    .Select(x => new { x.CourseCompletionRecordId, x.TrainingCertificateId, x.CertificateNumber })
                    .ToDictionaryAsync(
                        x => x.CourseCompletionRecordId,
                        x => (CertificateId: x.TrainingCertificateId, CertificateNumber: x.CertificateNumber),
                        cancellationToken);

            var userIds = assignments.Select(x => x.UserId)
                .Concat(progressRecords.Select(x => x.UserId))
                .Concat(latestCompletions.Keys)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var users = userIds.Count == 0
                ? new List<ApplicationUser>()
                : await dbContext.Users.AsNoTracking()
                    .Where(x => userIds.Contains(x.Id))
                    .ToListAsync(cancellationToken);

            var assignmentLookup = assignments
                .GroupBy(x => x.UserId, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.OrderByDescending(x => x.DueDateUtc).FirstOrDefault(), StringComparer.OrdinalIgnoreCase);
            var progressLookup = progressRecords
                .GroupBy(x => x.UserId, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.OrderByDescending(x => x.CompletedOnUtc ?? x.StartedOnUtc).FirstOrDefault(), StringComparer.OrdinalIgnoreCase);
            var now = DateTime.UtcNow;

            course.Students = users
                .Select(user =>
                {
                    assignmentLookup.TryGetValue(user.Id, out var assignment);
                    progressLookup.TryGetValue(user.Id, out var progress);
                    latestCompletions.TryGetValue(user.Id, out var completion);

                    var status = completion is not null
                        ? completion.ExpiryDateUtc < now ? TrainingCourseStudentStatus.Expired : TrainingCourseStudentStatus.Completed
                        : progress is not null && (progress.Status == ProgressStatus.Completed || progress.CompletedOnUtc.HasValue)
                            ? TrainingCourseStudentStatus.Completed
                            : progress is not null && (progress.StartedOnUtc.HasValue || progress.Status != ProgressStatus.NotStarted)
                                ? TrainingCourseStudentStatus.InProgress
                                : TrainingCourseStudentStatus.NotStarted;

                    string? certificateViewUrl = null;
                    string? certificateDownloadUrl = null;
                    string? certificateNumber = completion?.CertificateNumber;
                    if (completion is not null && certificates.TryGetValue(completion.CourseCompletionRecordId, out var certificate))
                    {
                        certificateViewUrl = $"/my-training/certificates/{certificate.CertificateId}";
                        certificateDownloadUrl = $"/my-training/certificates/{certificate.CertificateId}?download=true";
                        certificateNumber = certificate.CertificateNumber;
                    }

                    return new TrainingCourseStudentStatusModel
                    {
                        UserId = user.Id,
                        StudentName = string.IsNullOrWhiteSpace(user.FullName) ? user.Email ?? user.UserName ?? "Learner" : user.FullName,
                        Email = user.Email,
                        UserName = user.UserName,
                        Status = status,
                        StartedOnUtc = progress?.StartedOnUtc,
                        CompletedOnUtc = completion?.CompletedOnUtc ?? progress?.CompletedOnUtc,
                        ExpiryDateUtc = completion?.ExpiryDateUtc ?? progress?.ExpiryDateUtc,
                        DueDateUtc = assignment?.DueDateUtc,
                        ResultPercent = completion?.FinalScorePercent,
                        CertificateNumber = certificateNumber,
                        CertificateViewUrl = certificateViewUrl,
                        CertificateDownloadUrl = certificateDownloadUrl
                    };
                })
                .OrderBy(x => x.StudentName)
                .ToList();

            course.EnrolledLearnerCount = course.Students.Count;
            course.StartedLearnerCount = course.Students.Count(x => x.Status is TrainingCourseStudentStatus.InProgress or TrainingCourseStudentStatus.Completed or TrainingCourseStudentStatus.Expired);
            course.CompletedLearnerCount = course.Students.Count(x => x.Status is TrainingCourseStudentStatus.Completed or TrainingCourseStudentStatus.Expired);

            return course;
        }

        public async Task<TrainingCourseEditModel?> GetCourseAsync(Guid courseId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var course = await dbContext.Courses
                .AsNoTracking()
                .Where(x => x.CourseId == courseId)
                .Select(x => new TrainingCourseEditModel
                {
                    CourseId = x.CourseId,
                    Code = x.Code,
                    Name = x.Title,
                    Title = x.Title,
                    Summary = x.Summary,
                    Description = x.Description,
                    ThumbnailUrl = x.ThumbnailUrl,
                    TargetAudienceSummary = x.TargetAudienceSummary,
                    RegulatoryReference = x.RegulatoryReference,
                    LearningObjectives = x.LearningObjectives,
                    PassMarkPercent = x.PassMarkPercent,
                    ValidityMonths = x.ValidityMonths,
                    DurationMinutes = x.EstimatedDurationMinutes,
                    EstimatedDurationMinutes = x.EstimatedDurationMinutes,
                    IsMandatory = x.IsMandatory,
                    IsActive = x.IsActive,
                    Cost = x.Cost
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (course is null)
            {
                return null;
            }

            var rules = await dbContext.CourseAudienceRules
                .AsNoTracking()
                .Where(x => x.CourseId == courseId)
                .ToListAsync(cancellationToken);
            course.AudienceType = ResolveAudienceType(rules);
            course.TargetAudienceSummary = GetAudienceSummary(course.AudienceType);

            return course;
        }

        public async Task<OperationResult<TrainingCourseEditModel>> SaveCourseAsync(TrainingCourseEditModel model, string? changedByUserId, CancellationToken cancellationToken = default)
        {
            await EnsureTrainingRolesAsync();
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var normalizedCode = model.Code.Trim().ToUpperInvariant();
            if (await dbContext.Courses.AnyAsync(x => x.Code == normalizedCode && x.CourseId != (model.CourseId ?? Guid.Empty), cancellationToken))
            {
                return OperationResult<TrainingCourseEditModel>.Failure("A course with this code already exists.");
            }

            Course course;
            var isNewCourse = !model.CourseId.HasValue;
            if (model.CourseId.HasValue)
            {
                var existingCourse = await dbContext.Courses.FirstOrDefaultAsync(x => x.CourseId == model.CourseId.Value, cancellationToken);
                if (existingCourse is null)
                {
                    return OperationResult<TrainingCourseEditModel>.Failure("Course not found. It may have been deleted.");
                }
                course = existingCourse;
                course.UpdatedByUserId = changedByUserId;
                course.UpdatedOnUtc = DateTime.UtcNow;
            }
            else
            {
                course = new Course
                {
                    CourseId = Guid.NewGuid(),
                    CreatedByUserId = changedByUserId,
                    CreatedOnUtc = DateTime.UtcNow
                };
                dbContext.Courses.Add(course);
            }

            course.Code = normalizedCode;
            var normalizedTitle = string.IsNullOrWhiteSpace(model.Title) ? model.Name.Trim() : model.Title.Trim();
            course.Title = normalizedTitle;
            course.Summary = model.Summary?.Trim();
            course.Description = model.Description?.Trim();
            course.ThumbnailUrl = model.ThumbnailUrl?.Trim();
            course.TargetAudienceSummary = GetAudienceSummary(model.AudienceType);
            course.RegulatoryReference = model.RegulatoryReference?.Trim();
            course.LearningObjectives = model.LearningObjectives?.Trim();
            course.PassMarkPercent = model.PassMarkPercent;
            course.ValidityMonths = model.ValidityMonths;
            course.EstimatedDurationMinutes = model.DurationMinutes ?? model.EstimatedDurationMinutes;
            course.IsMandatory = model.IsMandatory;
            course.IsActive = model.IsActive;
            course.Cost = model.Cost;

            var existingAudienceRules = await dbContext.CourseAudienceRules
                .Where(x => x.CourseId == course.CourseId)
                .ToListAsync(cancellationToken);
            if (existingAudienceRules.Count > 0)
            {
                dbContext.CourseAudienceRules.RemoveRange(existingAudienceRules);
            }

            foreach (var audienceRule in CreateAudienceRules(course.CourseId, model.AudienceType))
            {
                dbContext.CourseAudienceRules.Add(audienceRule);
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            if (!course.CurrentVersionId.HasValue)
            {
                var initialVersion = new CourseVersion
                {
                    CourseVersionId = Guid.NewGuid(),
                    CourseId = course.CourseId,
                    VersionNumber = 1,
                    VersionLabel = "v1",
                    Status = CourseVersionStatus.Draft,
                    CreatedOnUtc = DateTime.UtcNow
                };

                dbContext.CourseVersions.Add(initialVersion);
                await dbContext.SaveChangesAsync(cancellationToken);

                course.CurrentVersionId = initialVersion.CourseVersionId;
                course.UpdatedByUserId = changedByUserId;
                course.UpdatedOnUtc = DateTime.UtcNow;
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            model.CourseId = course.CourseId;
            model.Title = course.Title;
            model.Name = course.Title;
            model.TargetAudienceSummary = course.TargetAudienceSummary;
            model.EstimatedDurationMinutes = course.EstimatedDurationMinutes;
            model.DurationMinutes = course.EstimatedDurationMinutes;
            await WriteAuditLogAsync(dbContext, "Course", course.CourseId.ToString(), isNewCourse ? "Create" : "Save", changedByUserId, notes: course.Title, cancellationToken: cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            return OperationResult<TrainingCourseEditModel>.Success(model, "Course saved.");
        }

        public async Task<OperationResult> ToggleCourseActiveAsync(Guid courseId, bool isActive, string? changedByUserId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var course = await dbContext.Courses.FirstOrDefaultAsync(x => x.CourseId == courseId, cancellationToken);
            if (course is null)
            {
                return OperationResult.Failure("Course not found.");
            }

            course.IsActive = isActive;
            course.UpdatedByUserId = changedByUserId;
            course.UpdatedOnUtc = DateTime.UtcNow;
            await WriteAuditLogAsync(dbContext, "Course", course.CourseId.ToString(), isActive ? "Activate" : "Deactivate", changedByUserId, notes: course.Title, cancellationToken: cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            return OperationResult.Success(isActive ? "Course activated." : "Course deactivated.");
        }

        public async Task<OperationResult> SoftDeleteCourseAsync(Guid courseId, string? changedByUserId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var course = await dbContext.Courses.FirstOrDefaultAsync(x => x.CourseId == courseId, cancellationToken);
            if (course is null)
            {
                return OperationResult.Failure("Course not found.");
            }

            if (course.IsDeleted)
            {
                return OperationResult.Success("Course was already removed.");
            }

            // Soft delete: retain the row (and all its versions/modules/lessons/progress history)
            // but hide it from the management and editing lists.
            course.IsDeleted = true;
            course.DeletedByUserId = changedByUserId;
            course.DeletedOnUtc = DateTime.UtcNow;
            course.UpdatedByUserId = changedByUserId;
            course.UpdatedOnUtc = DateTime.UtcNow;

            await WriteAuditLogAsync(dbContext, "Course", course.CourseId.ToString(), "Delete", changedByUserId, notes: course.Title, cancellationToken: cancellationToken);
            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex, "Failed to soft-delete course {CourseId}", courseId);
                return OperationResult.Failure("The course could not be removed. Please try again.");
            }
            return OperationResult.Success("Course removed.");
        }

        public async Task<OperationResult<CourseVersion>> CreateNextCourseVersionAsync(Guid courseId, string? changedByUserId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var course = await dbContext.Courses.FirstOrDefaultAsync(x => x.CourseId == courseId, cancellationToken);
            if (course is null)
            {
                return OperationResult<CourseVersion>.Failure("Course not found.");
            }

            var nextVersionNumber = await dbContext.CourseVersions
                .Where(x => x.CourseId == courseId)
                .MaxAsync(x => (int?)x.VersionNumber, cancellationToken) ?? 0;
            nextVersionNumber++;

            var nextVersion = new CourseVersion
            {
                CourseVersionId = Guid.NewGuid(),
                CourseId = courseId,
                VersionNumber = nextVersionNumber,
                VersionLabel = $"v{nextVersionNumber}",
                Status = CourseVersionStatus.Draft,
                ChangeSummary = "Created from current training content.",
                CreatedOnUtc = DateTime.UtcNow
            };

            dbContext.CourseVersions.Add(nextVersion);
            await dbContext.SaveChangesAsync(cancellationToken);

            if (course.CurrentVersionId.HasValue)
            {
                // Copy REFERENCES only. Modules are shared authored records, so a new course
                // version re-points at the same ModuleVersion rows — no lesson or content block
                // is duplicated, and existing learner progress against those lessons stays valid.
                var sourceCourseModules = await dbContext.CourseModules.AsNoTracking()
                    .Where(x => x.CourseVersionId == course.CurrentVersionId.Value)
                    .OrderBy(x => x.OrderIndex)
                    .ToListAsync(cancellationToken);

                var courseModuleMap = new Dictionary<Guid, Guid>();
                foreach (var sourceCourseModule in sourceCourseModules)
                {
                    var newCourseModuleId = Guid.NewGuid();
                    courseModuleMap[sourceCourseModule.CourseModuleId] = newCourseModuleId;
                    dbContext.CourseModules.Add(new CourseModule
                    {
                        CourseModuleId = newCourseModuleId,
                        CourseVersionId = nextVersion.CourseVersionId,
                        ModuleVersionId = sourceCourseModule.ModuleVersionId,
                        OrderIndex = sourceCourseModule.OrderIndex,
                        IsRequired = sourceCourseModule.IsRequired
                    });
                }

                // Carry the rank configuration forward so the draft starts where the author left off.
                var sourceRankProfiles = await dbContext.CourseRankProfiles.AsNoTracking()
                    .Where(x => x.CourseVersionId == course.CurrentVersionId.Value)
                    .OrderBy(x => x.OrderIndex)
                    .ToListAsync(cancellationToken);

                var sourceRankProfileIds = sourceRankProfiles.Select(x => x.CourseRankProfileId).ToList();
                var sourceRankModules = await dbContext.CourseRankModules.AsNoTracking()
                    .Where(x => sourceRankProfileIds.Contains(x.CourseRankProfileId))
                    .ToListAsync(cancellationToken);

                var rankProfileMap = new Dictionary<Guid, Guid>();
                foreach (var sourceRankProfile in sourceRankProfiles)
                {
                    var newCourseRankProfileId = Guid.NewGuid();
                    rankProfileMap[sourceRankProfile.CourseRankProfileId] = newCourseRankProfileId;
                    dbContext.CourseRankProfiles.Add(new CourseRankProfile
                    {
                        CourseRankProfileId = newCourseRankProfileId,
                        CourseVersionId = nextVersion.CourseVersionId,
                        RankProfileId = sourceRankProfile.RankProfileId,
                        OrderIndex = sourceRankProfile.OrderIndex,
                        IsActive = sourceRankProfile.IsActive
                    });
                }

                foreach (var sourceRankModule in sourceRankModules)
                {
                    if (!rankProfileMap.TryGetValue(sourceRankModule.CourseRankProfileId, out var newRankProfileId) ||
                        !courseModuleMap.TryGetValue(sourceRankModule.CourseModuleId, out var newCourseModuleId))
                    {
                        continue;
                    }

                    dbContext.CourseRankModules.Add(new CourseRankModule
                    {
                        CourseRankModuleId = Guid.NewGuid(),
                        CourseRankProfileId = newRankProfileId,
                        CourseModuleId = newCourseModuleId,
                        IsIncluded = sourceRankModule.IsIncluded
                    });
                }
            }

            course.CurrentVersionId = nextVersion.CourseVersionId;
            course.UpdatedByUserId = changedByUserId;
            course.UpdatedOnUtc = DateTime.UtcNow;
            await WriteAuditLogAsync(dbContext, "CourseVersion", nextVersion.CourseVersionId.ToString(), "CreateVersion", changedByUserId, notes: course.Title, cancellationToken: cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            return OperationResult<CourseVersion>.Success(nextVersion, $"Created {nextVersion.VersionLabel}.");
        }

        public async Task<OperationResult> PublishCourseVersionAsync(Guid courseId, Guid courseVersionId, string? changedByUserId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var course = await dbContext.Courses.FirstOrDefaultAsync(x => x.CourseId == courseId, cancellationToken);
            if (course is null)
            {
                return OperationResult.Failure("Course not found.");
            }

            var version = await dbContext.CourseVersions.FirstOrDefaultAsync(x => x.CourseVersionId == courseVersionId && x.CourseId == courseId, cancellationToken);
            if (version is null)
            {
                return OperationResult.Failure("Course version not found.");
            }

            var existingPublishedVersions = await dbContext.CourseVersions
                .Where(x => x.CourseId == courseId && x.Status == CourseVersionStatus.Published && x.CourseVersionId != courseVersionId)
                .ToListAsync(cancellationToken);

            foreach (var publishedVersion in existingPublishedVersions)
            {
                publishedVersion.Status = CourseVersionStatus.Archived;
                publishedVersion.EffectiveToUtc = DateTime.UtcNow;
                publishedVersion.UpdatedOnUtc = DateTime.UtcNow;
            }

            version.Status = CourseVersionStatus.Published;
            version.EffectiveFromUtc = version.EffectiveFromUtc ?? DateTime.UtcNow;
            version.ApprovedByUserId = changedByUserId;
            version.ApprovedOnUtc = DateTime.UtcNow;
            version.UpdatedOnUtc = DateTime.UtcNow;

            course.CurrentVersionId = version.CourseVersionId;
            course.UpdatedByUserId = changedByUserId;
            course.UpdatedOnUtc = DateTime.UtcNow;

            await WriteAuditLogAsync(dbContext, "CourseVersion", version.CourseVersionId.ToString(), "Publish", changedByUserId, notes: course.Title, cancellationToken: cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            return OperationResult.Success("Course version published.");
        }

        public async Task<TrainingCourseBuilderModel?> GetCourseBuilderAsync(Guid courseId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var course = await dbContext.Courses.AsNoTracking().FirstOrDefaultAsync(x => x.CourseId == courseId, cancellationToken);
            if (course is null || !course.CurrentVersionId.HasValue)
            {
                return null;
            }

            var version = await dbContext.CourseVersions.AsNoTracking()
                .FirstOrDefaultAsync(x => x.CourseVersionId == course.CurrentVersionId.Value, cancellationToken);
            if (version is null)
            {
                return null;
            }

            // The course's modules are references, resolved through CourseModules in course order.
            var modules = await dbContext.CourseModules.AsNoTracking()
                .Where(x => x.CourseVersionId == version.CourseVersionId)
                .OrderBy(x => x.OrderIndex)
                .Join(dbContext.ModuleVersions.AsNoTracking(),
                    courseModule => courseModule.ModuleVersionId,
                    moduleVersion => moduleVersion.ModuleVersionId,
                    (courseModule, moduleVersion) => new { courseModule, moduleVersion })
                .ToListAsync(cancellationToken);

            var moduleIds = modules.Select(x => x.moduleVersion.ModuleVersionId).ToList();
            var lessons = await dbContext.Lessons.AsNoTracking()
                .Where(x => moduleIds.Contains(x.ModuleVersionId))
                .OrderBy(x => x.OrderIndex)
                .ToListAsync(cancellationToken);

            var lessonIds = lessons.Select(x => x.LessonId).ToList();
            var blocks = await dbContext.LessonBlocks.AsNoTracking()
                .Where(x => lessonIds.Contains(x.LessonId))
                .OrderBy(x => x.OrderIndex)
                .ToListAsync(cancellationToken);

            var assessments = await dbContext.TrainingCourseAssessments.AsNoTracking()
                .Where(x => x.TrainingCourseId == course.CourseId)
                .OrderBy(x => x.Name)
                .ToListAsync(cancellationToken);
            var assessmentLookup = assessments.ToDictionary(x => x.TrainingCourseAssessmentId, x => x.Name);

            var assessmentIds = assessments.Select(x => x.TrainingCourseAssessmentId).ToList();
            var questionCounts = await dbContext.TrainingQuestionBankQuestions.AsNoTracking()
                .Where(x => assessmentIds.Contains(x.TrainingCourseAssessmentId))
                .GroupBy(x => x.TrainingCourseAssessmentId)
                .Select(x => new { AssessmentId = x.Key, Count = x.Count() })
                .ToDictionaryAsync(x => x.AssessmentId, x => x.Count, cancellationToken);

            return new TrainingCourseBuilderModel
            {
                CourseId = course.CourseId,
                Code = course.Code,
                Title = course.Title,
                Summary = course.Summary,
                Description = course.Description,
                PassMarkPercent = course.PassMarkPercent,
                CurrentVersionId = version.CourseVersionId,
                CurrentVersionNumber = version.VersionNumber,
                CurrentVersionLabel = version.VersionLabel,
                CurrentVersionStatus = version.Status,
                PublishedOnUtc = version.ApprovedOnUtc,
                Modules = modules.Select(entry => new TrainingModuleEditModel
                {
                    ModuleVersionId = entry.moduleVersion.ModuleVersionId,
                    ModuleId = entry.moduleVersion.ModuleId,
                    CourseModuleId = entry.courseModule.CourseModuleId,
                    VersionNumber = entry.moduleVersion.VersionNumber,
                    Status = entry.moduleVersion.Status,
                    Title = entry.moduleVersion.Title,
                    Description = entry.moduleVersion.Description,
                    // Sequence position belongs to the course, not to the shared module.
                    OrderIndex = entry.courseModule.OrderIndex,
                    EstimatedMinutes = entry.moduleVersion.EstimatedMinutes,
                    IsActive = entry.moduleVersion.IsActive,
                    HasModuleAssessment = entry.moduleVersion.HasModuleAssessment,
                    AssessmentId = entry.moduleVersion.AssessmentId,
                    AssessmentName = entry.moduleVersion.AssessmentId.HasValue ? assessmentLookup.GetValueOrDefault(entry.moduleVersion.AssessmentId.Value) : null,
                    AssessmentPassMarkPercent = entry.moduleVersion.AssessmentPassMarkPercent,
                    AssessmentMaxAttempts = entry.moduleVersion.AssessmentMaxAttempts,
                    Lessons = lessons
                        .Where(x => x.ModuleVersionId == entry.moduleVersion.ModuleVersionId)
                        .OrderBy(x => x.OrderIndex)
                        .Select(lesson => new TrainingLessonEditModel
                        {
                            LessonId = lesson.LessonId,
                            ModuleVersionId = lesson.ModuleVersionId,
                            Title = lesson.Title,
                            Summary = lesson.Summary,
                            OrderIndex = lesson.OrderIndex,
                            EstimatedMinutes = lesson.EstimatedMinutes,
                            IsPreview = lesson.IsPreview,
                            IsActive = lesson.IsActive,
                            Blocks = blocks
                                .Where(x => x.LessonId == lesson.LessonId)
                                .OrderBy(x => x.OrderIndex)
                                 .Select(block =>
                                 {
                                     var metadata = DeserializeBlockMetadata(block.MetadataJson);
                                     return new TrainingLessonBlockEditModel
                                     {
                                          UiKey = block.LessonBlockId,
                                         LessonBlockId = block.LessonBlockId,
                                         LessonId = block.LessonId,
                                         BlockType = block.BlockType,
                                         Title = block.Title,
                                         Subtitle = block.Subtitle,
                                         OrderIndex = block.OrderIndex,
                                         ContentHtml = block.MarkdownBody,
                                         SecondaryContentHtml = metadata.SecondaryContentHtml,
                                         IntroTextHtml = metadata.IntroTextHtml,
                                          ThumbnailUrl = NormalizeStoredTrainingUrl(block.ThumbnailUrl),
                                          FileUrl = NormalizeStoredTrainingUrl(block.FileUrl),
                                         ExternalUrl = block.ExternalUrl,
                                         MimeType = block.MimeType,
                                         DurationSeconds = block.DurationSeconds,
                                         MetadataJson = block.MetadataJson,
                                         MediaAssetId = block.MediaAssetId,
                                         LinkedAssessmentId = metadata.LinkedAssessmentId,
                                         LinkedAssessmentName = metadata.LinkedAssessmentName ?? (metadata.LinkedAssessmentId.HasValue ? assessmentLookup.GetValueOrDefault(metadata.LinkedAssessmentId.Value) : null),
                                         QuizQuestions = NormalizeQuizQuestions(metadata.QuizQuestions),
                                         IsRequired = block.IsRequired,
                                         IsActive = block.IsActive
                                     };
                                 })
                                .ToList()
                        })
                        .ToList()
                }).ToList(),
                Assessments = assessments.Select(x => new TrainingCourseAssessmentEditModel
                {
                    TrainingCourseAssessmentId = x.TrainingCourseAssessmentId,
                    TrainingCourseId = x.TrainingCourseId,
                    Name = x.Name,
                    Instructions = x.Instructions,
                    PassMarkPercent = x.PassMarkPercent,
                    RandomQuestionCount = x.RandomQuestionCount,
                    MaxAttempts = x.MaxAttempts,
                    TimeLimitMinutes = x.TimeLimitMinutes,
                    IsActive = x.IsActive,
                    QuestionBankCount = questionCounts.GetValueOrDefault(x.TrainingCourseAssessmentId)
                }).ToList()
            };
        }

        public async Task<List<TrainingLessonEditModel>> GetLessonsForModuleVersionAsync(Guid moduleVersionId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var lessons = await dbContext.Lessons.AsNoTracking()
                .Where(x => x.ModuleVersionId == moduleVersionId)
                .OrderBy(x => x.OrderIndex)
                .ToListAsync(cancellationToken);

            var lessonIds = lessons.Select(x => x.LessonId).ToList();
            var blocks = await dbContext.LessonBlocks.AsNoTracking()
                .Where(x => lessonIds.Contains(x.LessonId))
                .OrderBy(x => x.OrderIndex)
                .ToListAsync(cancellationToken);

            var assessmentIds = blocks
                .Select(x => DeserializeBlockMetadata(x.MetadataJson).LinkedAssessmentId)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Distinct()
                .ToList();
            var assessmentLookup = assessmentIds.Count == 0
                ? new Dictionary<Guid, string>()
                : await dbContext.TrainingCourseAssessments.AsNoTracking()
                    .Where(x => assessmentIds.Contains(x.TrainingCourseAssessmentId))
                    .ToDictionaryAsync(x => x.TrainingCourseAssessmentId, x => x.Name, cancellationToken);

            return lessons.Select(lesson => new TrainingLessonEditModel
            {
                LessonId = lesson.LessonId,
                ModuleVersionId = lesson.ModuleVersionId,
                Title = lesson.Title,
                Summary = lesson.Summary,
                OrderIndex = lesson.OrderIndex,
                EstimatedMinutes = lesson.EstimatedMinutes,
                IsPreview = lesson.IsPreview,
                IsActive = lesson.IsActive,
                Blocks = blocks
                    .Where(x => x.LessonId == lesson.LessonId)
                    .OrderBy(x => x.OrderIndex)
                    .Select(block =>
                    {
                        var metadata = DeserializeBlockMetadata(block.MetadataJson);
                        return new TrainingLessonBlockEditModel
                        {
                            UiKey = block.LessonBlockId,
                            LessonBlockId = block.LessonBlockId,
                            LessonId = block.LessonId,
                            BlockType = block.BlockType,
                            Title = block.Title,
                            Subtitle = block.Subtitle,
                            OrderIndex = block.OrderIndex,
                            ContentHtml = block.MarkdownBody,
                            SecondaryContentHtml = metadata.SecondaryContentHtml,
                            IntroTextHtml = metadata.IntroTextHtml,
                            ThumbnailUrl = NormalizeStoredTrainingUrl(block.ThumbnailUrl),
                            FileUrl = NormalizeStoredTrainingUrl(block.FileUrl),
                            ExternalUrl = block.ExternalUrl,
                            MimeType = block.MimeType,
                            DurationSeconds = block.DurationSeconds,
                            MetadataJson = block.MetadataJson,
                            MediaAssetId = block.MediaAssetId,
                            LinkedAssessmentId = metadata.LinkedAssessmentId,
                            LinkedAssessmentName = metadata.LinkedAssessmentName ?? (metadata.LinkedAssessmentId.HasValue ? assessmentLookup.GetValueOrDefault(metadata.LinkedAssessmentId.Value) : null),
                            QuizQuestions = NormalizeQuizQuestions(metadata.QuizQuestions),
                            IsRequired = block.IsRequired,
                            IsActive = block.IsActive
                        };
                    })
                    .ToList()
            }).ToList();
        }

        public async Task<OperationResult<TrainingModuleEditModel>> SaveModuleAsync(TrainingModuleEditModel model, string? changedByUserId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            TrainingModuleVersion moduleVersion;
            if (model.ModuleVersionId.HasValue)
            {
                var existingVersion = await dbContext.ModuleVersions.FirstOrDefaultAsync(x => x.ModuleVersionId == model.ModuleVersionId.Value, cancellationToken);
                if (existingVersion is null)
                {
                    return OperationResult<TrainingModuleEditModel>.Failure("Module not found. It may have been deleted.");
                }
                if (existingVersion.Status != ModuleVersionStatus.Draft)
                {
                    return OperationResult<TrainingModuleEditModel>.Failure("This module version is published. Create a new version to make changes.");
                }
                moduleVersion = existingVersion;
            }
            else
            {
                // New module: a stable identity plus its first draft version.
                var moduleId = Guid.NewGuid();
                var moduleVersionId = Guid.NewGuid();

                // TrainingModule.CurrentVersionId and ModuleVersion.ModuleId reference each other,
                // so they cannot be inserted in a single SaveChanges. Identity first with a null
                // pointer, then the version, then link.
                var newModule = new TrainingModule
                {
                    ModuleId = moduleId,
                    Code = await GenerateModuleCodeAsync(dbContext, cancellationToken),
                    Title = model.Title.Trim(),
                    Description = model.Description?.Trim(),
                    CurrentVersionId = null,
                    CreatedByUserId = changedByUserId,
                    CreatedOnUtc = DateTime.UtcNow
                };
                dbContext.TrainingModules.Add(newModule);
                await dbContext.SaveChangesAsync(cancellationToken);

                moduleVersion = new TrainingModuleVersion
                {
                    ModuleVersionId = moduleVersionId,
                    ModuleId = moduleId,
                    VersionNumber = 1,
                    VersionLabel = "v1",
                    Status = ModuleVersionStatus.Draft,
                    Title = model.Title.Trim(),
                    CreatedOnUtc = DateTime.UtcNow
                };
                dbContext.ModuleVersions.Add(moduleVersion);
                await dbContext.SaveChangesAsync(cancellationToken);

                newModule.CurrentVersionId = moduleVersionId;

                // Attach to the course that requested the creation, as a reference.
                if (model.CourseVersionId.HasValue)
                {
                    var nextOrder = await dbContext.CourseModules
                        .Where(x => x.CourseVersionId == model.CourseVersionId.Value)
                        .MaxAsync(x => (int?)x.OrderIndex, cancellationToken) ?? 0;

                    var courseModule = new CourseModule
                    {
                        CourseModuleId = Guid.NewGuid(),
                        CourseVersionId = model.CourseVersionId.Value,
                        ModuleVersionId = moduleVersionId,
                        OrderIndex = nextOrder + 1
                    };
                    dbContext.CourseModules.Add(courseModule);
                    model.CourseModuleId = courseModule.CourseModuleId;
                    model.OrderIndex = courseModule.OrderIndex;
                }
            }

            moduleVersion.Title = model.Title.Trim();
            moduleVersion.Description = model.Description?.Trim();
            moduleVersion.EstimatedMinutes = model.EstimatedMinutes;
            moduleVersion.IsActive = model.IsActive;
            moduleVersion.HasModuleAssessment = model.HasModuleAssessment;
            moduleVersion.AssessmentId = model.HasModuleAssessment ? model.AssessmentId : null;
            moduleVersion.AssessmentPassMarkPercent = model.HasModuleAssessment ? model.AssessmentPassMarkPercent : null;
            moduleVersion.AssessmentMaxAttempts = model.AssessmentMaxAttempts > 0 ? model.AssessmentMaxAttempts : 3;
            moduleVersion.UpdatedOnUtc = DateTime.UtcNow;

            await dbContext.SaveChangesAsync(cancellationToken);
            if (model.CourseVersionId.HasValue)
            {
                await ReindexCourseModulesAsync(dbContext, model.CourseVersionId.Value, cancellationToken);
            }
            await WriteAuditLogAsync(dbContext, "ModuleVersion", moduleVersion.ModuleVersionId.ToString(), "Save", changedByUserId, notes: moduleVersion.Title, cancellationToken: cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            model.ModuleVersionId = moduleVersion.ModuleVersionId;
            model.ModuleId = moduleVersion.ModuleId;
            model.VersionNumber = moduleVersion.VersionNumber;
            model.Status = moduleVersion.Status;
            return OperationResult<TrainingModuleEditModel>.Success(model, "Module saved.");
        }

        /// <summary>
        /// Removes a module from a course. The shared module version itself is untouched — other
        /// courses that include it are unaffected, and no lesson or content block is deleted.
        /// </summary>
        public async Task<OperationResult> DeleteModuleAsync(Guid courseModuleId, string? changedByUserId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var courseModule = await dbContext.CourseModules.FirstOrDefaultAsync(x => x.CourseModuleId == courseModuleId, cancellationToken);
            if (courseModule is null)
            {
                return OperationResult.Failure("Module not found in this course.");
            }

            var versionId = courseModule.CourseVersionId;
            await dbContext.CourseRankModules
                .Where(x => x.CourseModuleId == courseModuleId)
                .ExecuteDeleteAsync(cancellationToken);
            dbContext.CourseModules.Remove(courseModule);
            await WriteAuditLogAsync(dbContext, "CourseModule", courseModuleId.ToString(), "Remove", changedByUserId, notes: courseModule.ModuleVersionId.ToString(), cancellationToken: cancellationToken);
            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
                await ReindexCourseModulesAsync(dbContext, versionId, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex, "Failed to remove course module {CourseModuleId}", courseModuleId);
                return OperationResult.Failure("The module could not be removed. Please try again.");
            }
            return OperationResult.Success("Module removed from course.");
        }

        /// <summary>Reorders a module within one course. Other courses keep their own ordering.</summary>
        public async Task<OperationResult> MoveModuleAsync(Guid courseModuleId, int direction, string? changedByUserId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var courseModule = await dbContext.CourseModules.FirstOrDefaultAsync(x => x.CourseModuleId == courseModuleId, cancellationToken);
            if (courseModule is null)
            {
                return OperationResult.Failure("Module not found in this course.");
            }

            var courseModules = await dbContext.CourseModules
                .Where(x => x.CourseVersionId == courseModule.CourseVersionId)
                .OrderBy(x => x.OrderIndex)
                .ToListAsync(cancellationToken);

            var index = courseModules.FindIndex(x => x.CourseModuleId == courseModuleId);
            var targetIndex = index + direction;
            if (index < 0 || targetIndex < 0 || targetIndex >= courseModules.Count)
            {
                return OperationResult.Failure("Module cannot be moved further.");
            }

            (courseModules[index].OrderIndex, courseModules[targetIndex].OrderIndex) = (courseModules[targetIndex].OrderIndex, courseModules[index].OrderIndex);
            await WriteAuditLogAsync(dbContext, "CourseModule", courseModuleId.ToString(), "Reorder", changedByUserId, notes: null, cancellationToken: cancellationToken);
            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
                await ReindexCourseModulesAsync(dbContext, courseModule.CourseVersionId, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex, "Failed to reorder course module {CourseModuleId}", courseModuleId);
                return OperationResult.Failure("The module order could not be updated. Please try again.");
            }
            return OperationResult.Success("Module order updated.");
        }

        /// <summary>Allocates the next sequential module code (MOD-0001, MOD-0002, ...).</summary>
        private static async Task<string> GenerateModuleCodeAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
        {
            var existingCodes = await dbContext.TrainingModules
                .AsNoTracking()
                .Where(x => x.Code.StartsWith("MOD-"))
                .Select(x => x.Code)
                .ToListAsync(cancellationToken);

            var highest = existingCodes
                .Select(code => int.TryParse(code.AsSpan(4), out var value) ? value : 0)
                .DefaultIfEmpty(0)
                .Max();

            return $"MOD-{highest + 1:D4}";
        }

        /// <summary>
        /// Guards content edits against the immutability rule: once a module version is published
        /// or archived, its lessons and content blocks are frozen, because learner progress,
        /// completion records and certificates all reference them. Authors create a new version.
        /// </summary>
        private static async Task<OperationResult> EnsureModuleVersionEditableAsync(
            ApplicationDbContext dbContext, Guid moduleVersionId, CancellationToken cancellationToken)
        {
            var status = await dbContext.ModuleVersions
                .AsNoTracking()
                .Where(x => x.ModuleVersionId == moduleVersionId)
                .Select(x => (ModuleVersionStatus?)x.Status)
                .FirstOrDefaultAsync(cancellationToken);

            if (status is null)
            {
                return OperationResult.Failure("Module not found. It may have been deleted.");
            }

            return status == ModuleVersionStatus.Draft
                ? OperationResult.Success()
                : OperationResult.Failure("This module version is published. Create a new version to make changes.");
        }

        /// <summary>Same guard, resolved from a lesson rather than a module version.</summary>
        private static async Task<OperationResult> EnsureLessonEditableAsync(
            ApplicationDbContext dbContext, Guid lessonId, CancellationToken cancellationToken)
        {
            var moduleVersionId = await dbContext.Lessons
                .AsNoTracking()
                .Where(x => x.LessonId == lessonId)
                .Select(x => (Guid?)x.ModuleVersionId)
                .FirstOrDefaultAsync(cancellationToken);

            return moduleVersionId is null
                ? OperationResult.Failure("Lesson not found. It may have been deleted.")
                : await EnsureModuleVersionEditableAsync(dbContext, moduleVersionId.Value, cancellationToken);
        }

        public async Task<OperationResult<TrainingLessonEditModel>> SaveLessonAsync(TrainingLessonEditModel model, string? changedByUserId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var editable = await EnsureModuleVersionEditableAsync(dbContext, model.ModuleVersionId, cancellationToken);
            if (!editable.Succeeded)
            {
                return OperationResult<TrainingLessonEditModel>.Failure(editable.Message ?? "This module version cannot be edited.");
            }

            TrainingLesson lesson;
            if (model.LessonId.HasValue)
            {
                var existingLesson = await dbContext.Lessons.FirstOrDefaultAsync(x => x.LessonId == model.LessonId.Value, cancellationToken);
                if (existingLesson is null)
                {
                    return OperationResult<TrainingLessonEditModel>.Failure("Lesson not found. It may have been deleted.");
                }
                lesson = existingLesson;
            }
            else
            {
                var nextOrder = await dbContext.Lessons
                    .Where(x => x.ModuleVersionId == model.ModuleVersionId)
                    .MaxAsync(x => (int?)x.OrderIndex, cancellationToken) ?? 0;

                lesson = new TrainingLesson
                {
                    LessonId = Guid.NewGuid(),
                    ModuleVersionId = model.ModuleVersionId,
                    OrderIndex = nextOrder + 1
                };
                dbContext.Lessons.Add(lesson);
            }

            lesson.Title = model.Title.Trim();
            lesson.Summary = model.Summary?.Trim();
            lesson.EstimatedMinutes = model.EstimatedMinutes;
            lesson.IsPreview = model.IsPreview;
            lesson.IsActive = model.IsActive;

            await dbContext.SaveChangesAsync(cancellationToken);
            await ReindexLessonsAsync(dbContext, lesson.ModuleVersionId, cancellationToken);
            await WriteAuditLogAsync(dbContext, "Lesson", lesson.LessonId.ToString(), "Save", changedByUserId, notes: lesson.Title, cancellationToken: cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            model.LessonId = lesson.LessonId;
            model.OrderIndex = lesson.OrderIndex;
            return OperationResult<TrainingLessonEditModel>.Success(model, "Lesson saved.");
        }

        public async Task<OperationResult> DeleteLessonAsync(Guid lessonId, string? changedByUserId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var editable = await EnsureLessonEditableAsync(dbContext, lessonId, cancellationToken);
            if (!editable.Succeeded)
            {
                return editable;
            }

            var lesson = await dbContext.Lessons.FirstOrDefaultAsync(x => x.LessonId == lessonId, cancellationToken);
            if (lesson is null)
            {
                return OperationResult.Failure("Lesson not found.");
            }

            var moduleId = lesson.ModuleVersionId;
            dbContext.Lessons.Remove(lesson);
            await WriteAuditLogAsync(dbContext, "Lesson", lessonId.ToString(), "Delete", changedByUserId, notes: lesson.Title, cancellationToken: cancellationToken);
            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
                await ReindexLessonsAsync(dbContext, moduleId, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex, "Failed to delete lesson {LessonId}", lessonId);
                return OperationResult.Failure("The lesson could not be deleted. Please try again.");
            }
            return OperationResult.Success("Lesson deleted.");
        }

        public async Task<OperationResult> MoveLessonAsync(Guid lessonId, int direction, string? changedByUserId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var editable = await EnsureLessonEditableAsync(dbContext, lessonId, cancellationToken);
            if (!editable.Succeeded)
            {
                return editable;
            }

            var lesson = await dbContext.Lessons.FirstOrDefaultAsync(x => x.LessonId == lessonId, cancellationToken);
            if (lesson is null)
            {
                return OperationResult.Failure("Lesson not found.");
            }

            var lessons = await dbContext.Lessons
                .Where(x => x.ModuleVersionId == lesson.ModuleVersionId)
                .OrderBy(x => x.OrderIndex)
                .ToListAsync(cancellationToken);

            var index = lessons.FindIndex(x => x.LessonId == lessonId);
            var targetIndex = index + direction;
            if (index < 0 || targetIndex < 0 || targetIndex >= lessons.Count)
            {
                return OperationResult.Failure("Lesson cannot be moved further.");
            }

            (lessons[index].OrderIndex, lessons[targetIndex].OrderIndex) = (lessons[targetIndex].OrderIndex, lessons[index].OrderIndex);
            await WriteAuditLogAsync(dbContext, "Lesson", lessonId.ToString(), "Reorder", changedByUserId, notes: lesson.Title, cancellationToken: cancellationToken);
            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
                await ReindexLessonsAsync(dbContext, lesson.ModuleVersionId, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex, "Failed to reorder lesson {LessonId}", lessonId);
                return OperationResult.Failure("The lesson order could not be updated. Please try again.");
            }
            return OperationResult.Success("Lesson order updated.");
        }

        public async Task<OperationResult<TrainingLessonBlockEditModel>> SaveLessonBlockAsync(TrainingLessonBlockEditModel model, string? changedByUserId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var editable = await EnsureLessonEditableAsync(dbContext, model.LessonId, cancellationToken);
            if (!editable.Succeeded)
            {
                return OperationResult<TrainingLessonBlockEditModel>.Failure(editable.Message ?? "This module version cannot be edited.");
            }

            // An Assessment block may be saved as a draft without a linked assessment; the learner
            // runtime guards against a missing pool, so blocking the save here only stranded new blocks.
            if (model.LinkedAssessmentId.HasValue)
            {
                model.LinkedAssessmentName = await dbContext.TrainingCourseAssessments
                    .Where(x => x.TrainingCourseAssessmentId == model.LinkedAssessmentId.Value)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            if (model.BlockType == LessonBlockType.Flashcard)
            {
                model.QuizQuestions = NormalizeQuizQuestions(model.QuizQuestions);
            }

            LessonBlock block;
            if (model.LessonBlockId.HasValue)
            {
                var existingBlock = await dbContext.LessonBlocks.FirstOrDefaultAsync(x => x.LessonBlockId == model.LessonBlockId.Value, cancellationToken);
                if (existingBlock is null)
                {
                    return OperationResult<TrainingLessonBlockEditModel>.Failure("Lesson content block not found. It may have been deleted.");
                }
                block = existingBlock;
            }
            else
            {
                var nextOrder = await dbContext.LessonBlocks
                    .Where(x => x.LessonId == model.LessonId)
                    .MaxAsync(x => (int?)x.OrderIndex, cancellationToken) ?? 0;

                block = new LessonBlock
                {
                    LessonBlockId = Guid.NewGuid(),
                    LessonId = model.LessonId,
                    OrderIndex = nextOrder + 1
                };
                dbContext.LessonBlocks.Add(block);
            }

            block.BlockType = model.BlockType;
            block.Title = model.Title?.Trim();
            block.Subtitle = model.Subtitle?.Trim();
            block.MarkdownBody = model.ContentHtml?.Trim();
            block.ThumbnailUrl = NormalizeStoredTrainingUrl(model.ThumbnailUrl);
            block.FileUrl = NormalizeStoredTrainingUrl(model.FileUrl);
            block.ExternalUrl = model.ExternalUrl?.Trim();
            block.MimeType = model.MimeType?.Trim();
            block.DurationSeconds = model.DurationSeconds;
            block.MetadataJson = SerializeBlockMetadata(model);
            block.MediaAssetId = model.MediaAssetId;
            block.IsRequired = model.IsRequired;
            block.IsActive = model.IsActive;

            await dbContext.SaveChangesAsync(cancellationToken);
            await ReindexBlocksAsync(dbContext, block.LessonId, cancellationToken);
            await WriteAuditLogAsync(dbContext, "LessonBlock", block.LessonBlockId.ToString(), "Save", changedByUserId, notes: block.Title, cancellationToken: cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            model.LessonBlockId = block.LessonBlockId;
            model.OrderIndex = block.OrderIndex;
            return OperationResult<TrainingLessonBlockEditModel>.Success(model, "Lesson content saved.");
        }

        public async Task<TrainingQuestionBankPageModel?> GetQuestionBankPageAsync(Guid courseId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var course = await dbContext.Courses
                .AsNoTracking()
                .Where(x => x.CourseId == courseId)
                .Select(x => new
                {
                    x.CourseId,
                    x.Code,
                    x.Title,
                    x.PassMarkPercent,
                    x.ValidityMonths,
                    x.CurrentVersionId
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (course is null)
            {
                return null;
            }

            var modules = course.CurrentVersionId.HasValue
                ? await dbContext.CourseModules.AsNoTracking()
                    .Where(x => x.CourseVersionId == course.CurrentVersionId.Value)
                    .OrderBy(x => x.OrderIndex)
                    .Join(dbContext.ModuleVersions.AsNoTracking(),
                        courseModule => courseModule.ModuleVersionId,
                        moduleVersion => moduleVersion.ModuleVersionId,
                        (courseModule, moduleVersion) => new TrainingModuleLookupModel
                        {
                            ModuleVersionId = moduleVersion.ModuleVersionId,
                            Name = moduleVersion.Title
                        })
                    .ToListAsync(cancellationToken)
                : new List<TrainingModuleLookupModel>();

            var moduleIdList = modules.Select(x => x.ModuleVersionId).ToList();
            var lessons = moduleIdList.Count == 0
                ? new List<TrainingLessonLookupModel>()
                : await dbContext.Lessons.AsNoTracking()
                    .Where(x => moduleIdList.Contains(x.ModuleVersionId))
                    .OrderBy(x => x.OrderIndex)
                    .Select(x => new TrainingLessonLookupModel
                    {
                        LessonId = x.LessonId,
                        ModuleVersionId = x.ModuleVersionId,
                        Name = x.Title
                    })
                    .ToListAsync(cancellationToken);

            var assessments = await dbContext.TrainingCourseAssessments.AsNoTracking()
                .Where(x => x.TrainingCourseId == courseId)
                .OrderBy(x => x.Name)
                .ToListAsync(cancellationToken);

            var assessmentIds = assessments.Select(x => x.TrainingCourseAssessmentId).ToList();
            var questions = await dbContext.TrainingQuestionBankQuestions.AsNoTracking()
                .Where(x => assessmentIds.Contains(x.TrainingCourseAssessmentId))
                .OrderBy(x => x.TrainingModuleVersionId)
                .ThenBy(x => x.Prompt)
                .ToListAsync(cancellationToken);

            var questionIds = questions.Select(x => x.TrainingQuestionBankQuestionId).ToList();
            var options = await dbContext.TrainingQuestionBankOptions.AsNoTracking()
                .Where(x => questionIds.Contains(x.TrainingQuestionBankQuestionId))
                .OrderBy(x => x.OrderIndex)
                .ToListAsync(cancellationToken);

            var questionCounts = questions
                .GroupBy(x => x.TrainingCourseAssessmentId)
                .ToDictionary(x => x.Key, x => x.Count());

            return new TrainingQuestionBankPageModel
            {
                CourseId = course.CourseId,
                CourseCode = course.Code,
                CourseTitle = course.Title,
                PassMarkPercent = course.PassMarkPercent,
                ValidityMonths = course.ValidityMonths,
                Modules = modules,
                Lessons = lessons,
                Assessments = assessments.Select(x => new TrainingCourseAssessmentEditModel
                {
                    TrainingCourseAssessmentId = x.TrainingCourseAssessmentId,
                    TrainingCourseId = x.TrainingCourseId,
                    Name = x.Name,
                    Instructions = x.Instructions,
                    PassMarkPercent = x.PassMarkPercent,
                    RandomQuestionCount = x.RandomQuestionCount,
                    MaxAttempts = x.MaxAttempts,
                    TimeLimitMinutes = x.TimeLimitMinutes,
                    IsActive = x.IsActive,
                    QuestionBankCount = questionCounts.GetValueOrDefault(x.TrainingCourseAssessmentId)
                }).ToList(),
                Questions = questions.Select(x => new TrainingQuestionBankQuestionEditModel
                {
                    TrainingQuestionBankQuestionId = x.TrainingQuestionBankQuestionId,
                    TrainingCourseAssessmentId = x.TrainingCourseAssessmentId,
                    TrainingModuleVersionId = x.TrainingModuleVersionId,
                    QuestionType = x.QuestionType,
                    TrainingLessonId = x.TrainingLessonId,
                    Prompt = x.Prompt,
                    ScenarioText = x.ScenarioText,
                    Explanation = x.Explanation,
                    DifficultyLevel = x.DifficultyLevel,
                    Points = x.Points,
                    IsActive = x.IsActive,
                    Options = options
                        .Where(opt => opt.TrainingQuestionBankQuestionId == x.TrainingQuestionBankQuestionId)
                        .Select(opt => new TrainingQuestionBankOptionEditModel
                        {
                            TrainingQuestionBankOptionId = opt.TrainingQuestionBankOptionId,
                            OptionText = opt.OptionText,
                            IsCorrect = opt.IsCorrect,
                            OrderIndex = opt.OrderIndex
                        })
                        .ToList()
                }).ToList()
            };
        }

        public async Task<OperationResult<TrainingCourseAssessmentEditModel>> SaveCourseAssessmentAsync(TrainingCourseAssessmentEditModel model, string? changedByUserId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            TrainingCourseAssessment assessment;
            if (model.TrainingCourseAssessmentId.HasValue)
            {
                var existingAssessment = await dbContext.TrainingCourseAssessments.FirstOrDefaultAsync(x => x.TrainingCourseAssessmentId == model.TrainingCourseAssessmentId.Value, cancellationToken);
                if (existingAssessment is null)
                {
                    return OperationResult<TrainingCourseAssessmentEditModel>.Failure("Assessment not found. It may have been deleted.");
                }
                assessment = existingAssessment;
            }
            else
            {
                assessment = new TrainingCourseAssessment
                {
                    TrainingCourseAssessmentId = Guid.NewGuid(),
                    TrainingCourseId = model.TrainingCourseId
                };
                dbContext.TrainingCourseAssessments.Add(assessment);
            }

            assessment.Name = model.Name.Trim();
            assessment.Instructions = model.Instructions?.Trim();
            assessment.PassMarkPercent = model.PassMarkPercent;
            assessment.RandomQuestionCount = model.RandomQuestionCount;
            assessment.MaxAttempts = model.MaxAttempts;
            assessment.TimeLimitMinutes = model.TimeLimitMinutes;
            assessment.IsActive = model.IsActive;

            await WriteAuditLogAsync(dbContext, "TrainingCourseAssessment", assessment.TrainingCourseAssessmentId.ToString(), "Save", changedByUserId, notes: assessment.Name, cancellationToken: cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            model.TrainingCourseAssessmentId = assessment.TrainingCourseAssessmentId;
            return OperationResult<TrainingCourseAssessmentEditModel>.Success(model, "Assessment saved.");
        }

        public async Task<OperationResult<TrainingQuestionBankQuestionEditModel>> SaveQuestionBankQuestionAsync(TrainingQuestionBankQuestionEditModel model, string? changedByUserId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            TrainingQuestionBankQuestion question;
            if (model.TrainingQuestionBankQuestionId.HasValue)
            {
                var existingQuestion = await dbContext.TrainingQuestionBankQuestions
                    .Include(x => x.Options)
                    .FirstOrDefaultAsync(x => x.TrainingQuestionBankQuestionId == model.TrainingQuestionBankQuestionId.Value, cancellationToken);
                if (existingQuestion is null)
                {
                    return OperationResult<TrainingQuestionBankQuestionEditModel>.Failure("Question bank item not found. It may have been deleted.");
                }
                question = existingQuestion;
            }
            else
            {
                question = new TrainingQuestionBankQuestion
                {
                    TrainingQuestionBankQuestionId = Guid.NewGuid(),
                    TrainingCourseAssessmentId = model.TrainingCourseAssessmentId
                };
                dbContext.TrainingQuestionBankQuestions.Add(question);
            }

            question.TrainingCourseAssessmentId = model.TrainingCourseAssessmentId;
            question.TrainingModuleVersionId = model.TrainingModuleVersionId;
            question.TrainingLessonId = model.TrainingLessonId;
            question.QuestionType = model.QuestionType;
            question.Prompt = model.Prompt.Trim();
            question.ScenarioText = model.ScenarioText?.Trim();
            question.Explanation = model.Explanation?.Trim();
            question.DifficultyLevel = model.DifficultyLevel;
            question.Points = model.Points;
            question.IsActive = model.IsActive;

            if (model.TrainingQuestionBankQuestionId.HasValue)
            {
                dbContext.TrainingQuestionBankOptions.RemoveRange(question.Options);
            }

            question.Options = model.Options
                .Where(x => !string.IsNullOrWhiteSpace(x.OptionText))
                .OrderBy(x => x.OrderIndex)
                .Select(option => new TrainingQuestionBankOption
                {
                    TrainingQuestionBankOptionId = option.TrainingQuestionBankOptionId ?? Guid.NewGuid(),
                    TrainingQuestionBankQuestionId = question.TrainingQuestionBankQuestionId,
                    OptionText = option.OptionText.Trim(),
                    IsCorrect = option.IsCorrect,
                    OrderIndex = option.OrderIndex
                })
                .ToList();

            await WriteAuditLogAsync(dbContext, "TrainingQuestionBankQuestion", question.TrainingQuestionBankQuestionId.ToString(), "Save", changedByUserId, notes: question.Prompt, cancellationToken: cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            model.TrainingQuestionBankQuestionId = question.TrainingQuestionBankQuestionId;
            return OperationResult<TrainingQuestionBankQuestionEditModel>.Success(model, "Question bank item saved.");
        }

        public async Task<OperationResult> DeleteQuestionBankQuestionAsync(Guid trainingQuestionBankQuestionId, string? changedByUserId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var question = await dbContext.TrainingQuestionBankQuestions.FirstOrDefaultAsync(x => x.TrainingQuestionBankQuestionId == trainingQuestionBankQuestionId, cancellationToken);
            if (question is null)
            {
                return OperationResult.Failure("Question bank item not found.");
            }

            dbContext.TrainingQuestionBankQuestions.Remove(question);
            await WriteAuditLogAsync(dbContext, "TrainingQuestionBankQuestion", trainingQuestionBankQuestionId.ToString(), "Delete", changedByUserId, notes: question.Prompt, cancellationToken: cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            return OperationResult.Success("Question bank item deleted.");
        }

        public async Task<OperationResult> DeleteLessonBlockAsync(Guid lessonBlockId, string? changedByUserId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var block = await dbContext.LessonBlocks.FirstOrDefaultAsync(x => x.LessonBlockId == lessonBlockId, cancellationToken);
            if (block is null)
            {
                return OperationResult.Failure("Lesson content block not found.");
            }

            var editable = await EnsureLessonEditableAsync(dbContext, block.LessonId, cancellationToken);
            if (!editable.Succeeded)
            {
                return editable;
            }

            var lessonId = block.LessonId;
            dbContext.LessonBlocks.Remove(block);
            await WriteAuditLogAsync(dbContext, "LessonBlock", lessonBlockId.ToString(), "Delete", changedByUserId, notes: block.Title, cancellationToken: cancellationToken);
            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
                await ReindexBlocksAsync(dbContext, lessonId, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex, "Failed to delete lesson block {LessonBlockId}", lessonBlockId);
                return OperationResult.Failure("The content block could not be deleted. Please try again.");
            }
            return OperationResult.Success("Lesson content block deleted.");
        }

        public async Task<OperationResult> MoveLessonBlockAsync(Guid lessonBlockId, int direction, string? changedByUserId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var block = await dbContext.LessonBlocks.FirstOrDefaultAsync(x => x.LessonBlockId == lessonBlockId, cancellationToken);
            if (block is null)
            {
                return OperationResult.Failure("Lesson content block not found.");
            }

            var editable = await EnsureLessonEditableAsync(dbContext, block.LessonId, cancellationToken);
            if (!editable.Succeeded)
            {
                return editable;
            }

            var blocks = await dbContext.LessonBlocks
                .Where(x => x.LessonId == block.LessonId)
                .OrderBy(x => x.OrderIndex)
                .ToListAsync(cancellationToken);

            var index = blocks.FindIndex(x => x.LessonBlockId == lessonBlockId);
            var targetIndex = index + direction;
            if (index < 0 || targetIndex < 0 || targetIndex >= blocks.Count)
            {
                return OperationResult.Failure("Lesson content block cannot be moved further.");
            }

            (blocks[index].OrderIndex, blocks[targetIndex].OrderIndex) = (blocks[targetIndex].OrderIndex, blocks[index].OrderIndex);
            await WriteAuditLogAsync(dbContext, "LessonBlock", lessonBlockId.ToString(), "Reorder", changedByUserId, notes: block.Title, cancellationToken: cancellationToken);
            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
                await ReindexBlocksAsync(dbContext, block.LessonId, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex, "Failed to reorder lesson block {LessonBlockId}", lessonBlockId);
                return OperationResult.Failure("The content block order could not be updated. Please try again.");
            }
            return OperationResult.Success("Lesson content order updated.");
        }

        private static async Task ReindexCourseModulesAsync(ApplicationDbContext dbContext, Guid courseVersionId, CancellationToken cancellationToken)
        {
            var courseModules = await dbContext.CourseModules
                .Where(x => x.CourseVersionId == courseVersionId)
                .OrderBy(x => x.OrderIndex)
                .ToListAsync(cancellationToken);

            for (var index = 0; index < courseModules.Count; index++)
            {
                courseModules[index].OrderIndex = index + 1;
            }
        }

        private static async Task ReindexLessonsAsync(ApplicationDbContext dbContext, Guid moduleVersionId, CancellationToken cancellationToken)
        {
            var lessons = await dbContext.Lessons
                .Where(x => x.ModuleVersionId == moduleVersionId)
                .OrderBy(x => x.OrderIndex)
                .ThenBy(x => x.Title)
                .ToListAsync(cancellationToken);

            for (var index = 0; index < lessons.Count; index++)
            {
                lessons[index].OrderIndex = index + 1;
            }
        }

        private static async Task ReindexBlocksAsync(ApplicationDbContext dbContext, Guid lessonId, CancellationToken cancellationToken)
        {
            var blocks = await dbContext.LessonBlocks
                .Where(x => x.LessonId == lessonId)
                .OrderBy(x => x.OrderIndex)
                .ThenBy(x => x.Title)
                .ToListAsync(cancellationToken);

            for (var index = 0; index < blocks.Count; index++)
            {
                blocks[index].OrderIndex = index + 1;
            }
        }

        private static async Task WriteAuditLogAsync(
            ApplicationDbContext dbContext,
            string entityName,
            string entityId,
            string actionType,
            string? changedByUserId,
            string? notes,
            CancellationToken cancellationToken)
        {
            await dbContext.TrainingAuditLogs.AddAsync(new TrainingAuditLog
            {
                TrainingAuditLogId = Guid.NewGuid(),
                EntityName = entityName,
                EntityId = entityId,
                ActionType = actionType,
                ChangedByUserId = changedByUserId,
                ChangedOnUtc = DateTime.UtcNow,
                Notes = notes
            }, cancellationToken);
        }

        private async Task EnsureTrainingRolesAsync()
        {
            foreach (var roleName in new[] { RoleNames.Crew, RoleNames.Officer, RoleNames.Responder })
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        private static TrainingAudienceType ResolveAudienceType(IEnumerable<CourseAudienceRule> rules)
        {
            var applicableRules = rules.ToList();
            if (applicableRules.Count == 0)
            {
                return TrainingAudienceType.All;
            }

            if (applicableRules.Any(x => x.RuleType == CourseAudienceRuleType.AllCrew))
            {
                return TrainingAudienceType.All;
            }

            var appRole = applicableRules
                .FirstOrDefault(x => x.RuleType == CourseAudienceRuleType.ApplicationRole)
                ?.ApplicationRoleName;

            return appRole switch
            {
                RoleNames.Crew => TrainingAudienceType.Crew,
                RoleNames.Officer => TrainingAudienceType.Officers,
                RoleNames.Responder => TrainingAudienceType.Responders,
                _ => TrainingAudienceType.All
            };
        }

        private static string GetAudienceSummary(TrainingAudienceType audienceType) => audienceType switch
        {
            TrainingAudienceType.Crew => "Crew",
            TrainingAudienceType.Officers => "Officers",
            TrainingAudienceType.Responders => "Responders",
            _ => "All"
        };

        private static IEnumerable<CourseAudienceRule> CreateAudienceRules(Guid courseId, TrainingAudienceType audienceType)
        {
            if (audienceType == TrainingAudienceType.All)
            {
                yield return new CourseAudienceRule
                {
                    CourseAudienceRuleId = Guid.NewGuid(),
                    CourseId = courseId,
                    RuleType = CourseAudienceRuleType.AllCrew,
                    IsMandatory = true,
                    Notes = "Visible to all training audiences."
                };

                yield break;
            }

            yield return new CourseAudienceRule
            {
                CourseAudienceRuleId = Guid.NewGuid(),
                CourseId = courseId,
                RuleType = CourseAudienceRuleType.ApplicationRole,
                ApplicationRoleName = audienceType switch
                {
                    TrainingAudienceType.Crew => RoleNames.Crew,
                    TrainingAudienceType.Officers => RoleNames.Officer,
                    TrainingAudienceType.Responders => RoleNames.Responder,
                    _ => null
                },
                IsMandatory = true,
                Notes = $"Assigned to {GetAudienceSummary(audienceType)}."
            };
        }

        private static TrainingLessonBlockMetadataModel DeserializeBlockMetadata(string? metadataJson)
        {
            if (string.IsNullOrWhiteSpace(metadataJson))
            {
                return new TrainingLessonBlockMetadataModel();
            }

            try
            {
                return JsonSerializer.Deserialize<TrainingLessonBlockMetadataModel>(metadataJson) ?? new TrainingLessonBlockMetadataModel();
            }
            catch
            {
                return new TrainingLessonBlockMetadataModel();
            }
        }

        private static string? SerializeBlockMetadata(TrainingLessonBlockEditModel model)
        {
            var metadata = new TrainingLessonBlockMetadataModel
            {
                SecondaryContentHtml = string.IsNullOrWhiteSpace(model.SecondaryContentHtml) ? null : model.SecondaryContentHtml.Trim(),
                IntroTextHtml = string.IsNullOrWhiteSpace(model.IntroTextHtml) ? null : model.IntroTextHtml.Trim(),
                LinkedAssessmentId = model.LinkedAssessmentId,
                LinkedAssessmentName = string.IsNullOrWhiteSpace(model.LinkedAssessmentName) ? null : model.LinkedAssessmentName.Trim(),
                QuizQuestions = NormalizeQuizQuestions(model.QuizQuestions)
            };

            if (string.IsNullOrWhiteSpace(metadata.SecondaryContentHtml)
                && string.IsNullOrWhiteSpace(metadata.IntroTextHtml)
                && !metadata.LinkedAssessmentId.HasValue
                && metadata.QuizQuestions.Count == 0)
            {
                return null;
            }

            return JsonSerializer.Serialize(metadata);
        }

        private static List<TrainingLessonQuizQuestionEditModel> NormalizeQuizQuestions(IEnumerable<TrainingLessonQuizQuestionEditModel>? questions)
        {
            return questions?
                .Where(x => !string.IsNullOrWhiteSpace(x.FrontText) || !string.IsNullOrWhiteSpace(x.Prompt))
                .OrderBy(x => x.OrderIndex)
                .Select(question => new TrainingLessonQuizQuestionEditModel
                {
                    UiKey = question.UiKey == Guid.Empty ? Guid.NewGuid() : question.UiKey,
                    OrderIndex = question.OrderIndex > 0 ? question.OrderIndex : 0,
                    Prompt = string.IsNullOrWhiteSpace(question.FrontText)
                        ? question.Prompt.Trim()
                        : question.FrontText.Trim(),
                    FrontText = string.IsNullOrWhiteSpace(question.FrontText) ? question.Prompt.Trim() : question.FrontText.Trim(),
                    QuestionType = question.QuestionType,
                    ScenarioText = string.IsNullOrWhiteSpace(question.ScenarioText) ? null : question.ScenarioText.Trim(),
                    BackText = string.IsNullOrWhiteSpace(question.BackText) ? (string.IsNullOrWhiteSpace(question.ScenarioText) ? null : question.ScenarioText.Trim()) : question.BackText.Trim(),
                    IncludeFrontImage = question.IncludeFrontImage && !string.IsNullOrWhiteSpace(question.FrontImageUrl),
                    FrontImageUrl = string.IsNullOrWhiteSpace(question.FrontImageUrl) ? null : question.FrontImageUrl.Trim(),
                    FrontImageFileName = string.IsNullOrWhiteSpace(question.FrontImageFileName) ? null : question.FrontImageFileName.Trim(),
                    Options = NormalizeQuizOptions(question.Options)
                })
                .ToList() ?? new List<TrainingLessonQuizQuestionEditModel>();
        }

        private static List<TrainingLessonQuizOptionEditModel> NormalizeQuizOptions(IEnumerable<TrainingLessonQuizOptionEditModel>? options)
        {
            var normalizedOptions = options?
                .Where(x => !string.IsNullOrWhiteSpace(x.OptionText))
                .OrderBy(x => x.OrderIndex)
                .Select((option, index) => new TrainingLessonQuizOptionEditModel
                {
                    OptionText = option.OptionText.Trim(),
                    IsCorrect = option.IsCorrect,
                    OrderIndex = index + 1
                })
                .ToList() ?? new List<TrainingLessonQuizOptionEditModel>();

            if (normalizedOptions.Count >= 2)
            {
                return normalizedOptions;
            }

            while (normalizedOptions.Count < 2)
            {
                normalizedOptions.Add(new TrainingLessonQuizOptionEditModel { OrderIndex = normalizedOptions.Count + 1 });
            }

            return normalizedOptions;
        }

        private static string? NormalizeStoredTrainingUrl(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var trimmed = value.Trim();
            if (trimmed.StartsWith("blob:", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return trimmed;
        }

        // ── Training Requirements ──────────────────────────────────────────────

        public async Task<List<TrainingRequirementRowModel>> GetTrainingRequirementsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await using var ctx = await dbContextFactory.CreateDbContextAsync(cancellationToken);

                // Load courses with their audience rules in two separate queries to avoid
                // EF Core translation issues with .ToList() inside a Select projection.
                var courses = await ctx.Courses
                    .AsNoTracking()
                    .OrderBy(c => c.Title)
                    .ToListAsync(cancellationToken);

                var courseIds = courses.Select(c => c.CourseId).ToList();

                var rules = await ctx.CourseAudienceRules
                    .AsNoTracking()
                    .Where(r => courseIds.Contains(r.CourseId) && r.IsMandatory)
                    .ToListAsync(cancellationToken);

                var rulesByCourse = rules.GroupBy(r => r.CourseId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var allRankOptions = Enum.GetValues<CrewRank>().ToList();

                return courses.Select(c =>
                {
                    var courseRules = rulesByCourse.TryGetValue(c.CourseId, out var r) ? r : [];

                    var requiredRanks = courseRules
                        .Where(r => r.RuleType == CourseAudienceRuleType.OnBoardRole && r.OnBoardRole != null)
                        .Select(r => allRankOptions.Cast<CrewRank?>()
                            .FirstOrDefault(rank => string.Equals(rank!.Value.GetDisplayName(), r.OnBoardRole, StringComparison.OrdinalIgnoreCase)))
                        .Where(rank => rank.HasValue)
                        .Select(rank => rank!.Value)
                        .ToList();

                    return new TrainingRequirementRowModel
                    {
                        CourseId = c.CourseId,
                        Code = c.Code,
                        Title = c.Title,
                        IsActive = c.IsActive,
                        RequiredForAllCrew = courseRules.Any(r => r.RuleType == CourseAudienceRuleType.AllCrew),
                        RequiredForRanks = requiredRanks
                    };
                }).ToList();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error loading training requirements");
                return [];
            }
        }

        public async Task<OperationResult> SetCourseAllCrewRequirementAsync(Guid courseId, bool required, string? changedByUserId, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var ctx = await dbContextFactory.CreateDbContextAsync(cancellationToken);

                var existing = await ctx.CourseAudienceRules
                    .Where(r => r.CourseId == courseId && r.RuleType == CourseAudienceRuleType.AllCrew && r.IsMandatory)
                    .FirstOrDefaultAsync(cancellationToken);

                if (required && existing == null)
                {
                    ctx.CourseAudienceRules.Add(new CourseAudienceRule
                    {
                        CourseAudienceRuleId = Guid.NewGuid(),
                        CourseId = courseId,
                        RuleType = CourseAudienceRuleType.AllCrew,
                        IsMandatory = true,
                        Notes = $"Set by admin on {DateTime.UtcNow:dd MMM yyyy}"
                    });
                    await ctx.SaveChangesAsync(cancellationToken);
                }
                else if (!required && existing != null)
                {
                    ctx.CourseAudienceRules.Remove(existing);
                    await ctx.SaveChangesAsync(cancellationToken);
                }

                return OperationResult.Success();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error setting all-crew requirement for course {CourseId}", courseId);
                return OperationResult.Failure(ex.Message);
            }
        }

        public async Task<OperationResult> SetCourseRankRequirementsAsync(Guid courseId, IEnumerable<CrewRank>? ranks, string? changedByUserId, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var ctx = await dbContextFactory.CreateDbContextAsync(cancellationToken);

                var existing = await ctx.CourseAudienceRules
                    .Where(r => r.CourseId == courseId && r.RuleType == CourseAudienceRuleType.OnBoardRole && r.IsMandatory)
                    .ToListAsync(cancellationToken);

                ctx.CourseAudienceRules.RemoveRange(existing);

                foreach (var rank in ranks ?? [])
                {
                    ctx.CourseAudienceRules.Add(new CourseAudienceRule
                    {
                        CourseAudienceRuleId = Guid.NewGuid(),
                        CourseId = courseId,
                        RuleType = CourseAudienceRuleType.OnBoardRole,
                        OnBoardRole = rank.GetDisplayName(),
                        IsMandatory = true,
                        Notes = $"Set by admin on {DateTime.UtcNow:dd MMM yyyy}"
                    });
                }

                await ctx.SaveChangesAsync(cancellationToken);
                return OperationResult.Success();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error setting rank requirements for course {CourseId}", courseId);
                return OperationResult.Failure(ex.Message);
            }
        }
    }
}
