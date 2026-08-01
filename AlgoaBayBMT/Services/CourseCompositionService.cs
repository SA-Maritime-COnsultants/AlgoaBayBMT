using System.Text.Json;
using AlgoaBayBMT.Data;
using AlgoaBayBMT.Services.Interfaces;
using AlgoaBayBMT.Services.Models;
using AlgoaBayBMT.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace AlgoaBayBMT.Services
{
    /// <inheritdoc />
    public sealed class CourseCompositionService(
        IDbContextFactory<ApplicationDbContext> dbContextFactory,
        ITrainingResolutionService resolutionService,
        ILogger<CourseCompositionService> logger) : ICourseCompositionService
    {
        public async Task<CourseCompositionModel?> GetCompositionAsync(Guid courseId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var course = await dbContext.Courses.AsNoTracking()
                .FirstOrDefaultAsync(x => x.CourseId == courseId && !x.IsDeleted, cancellationToken);
            if (course?.CurrentVersionId is not Guid courseVersionId)
            {
                return null;
            }

            var version = await dbContext.CourseVersions.AsNoTracking()
                .FirstOrDefaultAsync(x => x.CourseVersionId == courseVersionId, cancellationToken);
            if (version is null)
            {
                return null;
            }

            var model = new CourseCompositionModel
            {
                CourseId = course.CourseId,
                CourseVersionId = courseVersionId,
                Code = course.Code,
                Title = course.Title,
                VersionNumber = version.VersionNumber,
                VersionLabel = version.VersionLabel,
                Status = version.Status,
                RowVersion = version.RowVersion,
                Modules = await LoadCourseModulesAsync(dbContext, courseVersionId, cancellationToken)
            };

            var allProfiles = await dbContext.RankProfiles.AsNoTracking()
                .Include(x => x.Ranks)
                .Where(x => x.IsActive)
                .OrderBy(x => x.OrderIndex)
                .ToListAsync(cancellationToken);

            var selected = await dbContext.CourseRankProfiles.AsNoTracking()
                .Where(x => x.CourseVersionId == courseVersionId)
                .ToListAsync(cancellationToken);

            var selectedIds = selected.Select(x => x.CourseRankProfileId).ToList();
            var inclusions = await dbContext.CourseRankModules.AsNoTracking()
                .Where(x => selectedIds.Contains(x.CourseRankProfileId))
                .ToListAsync(cancellationToken);

            foreach (var profile in allProfiles)
            {
                var courseRankProfile = selected.FirstOrDefault(x => x.RankProfileId == profile.RankProfileId);
                var inclusionMap = courseRankProfile is null
                    ? new Dictionary<Guid, bool>()
                    : inclusions
                        .Where(x => x.CourseRankProfileId == courseRankProfile.CourseRankProfileId)
                        .ToDictionary(x => x.CourseModuleId, x => x.IsIncluded);

                // A module with no stored row is included, so count exclusions rather than ticks.
                var includedCount = courseRankProfile is null
                    ? 0
                    : model.Modules.Count(m => inclusionMap.GetValueOrDefault(m.CourseModuleId, true));

                model.RankProfiles.Add(new CourseRankProfileItemModel
                {
                    CourseRankProfileId = courseRankProfile?.CourseRankProfileId,
                    RankProfileId = profile.RankProfileId,
                    Code = profile.Code,
                    Name = profile.Name,
                    OrderIndex = courseRankProfile?.OrderIndex ?? profile.OrderIndex,
                    IsSelected = courseRankProfile is not null && courseRankProfile.IsActive,
                    TotalDurationMinutes = courseRankProfile?.TotalDurationMinutes,
                    Ranks = profile.Ranks.Select(x => x.CrewRank).ToList(),
                    ModuleInclusion = inclusionMap,
                    IncludedModuleCount = includedCount
                });
            }

            return model;
        }

        public async Task<OperationResult> AddModuleToCourseAsync(Guid courseVersionId, Guid moduleVersionId, string? changedByUserId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            return await TransactionalExecution.ExecuteAsync(dbContext, cancellationToken, async transaction =>
            {
                var editable = await EnsureCourseVersionEditableAsync(dbContext, courseVersionId, cancellationToken);
                if (!editable.Succeeded)
                {
                    return editable;
                }

                var moduleVersion = await dbContext.ModuleVersions.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.ModuleVersionId == moduleVersionId, cancellationToken);
                if (moduleVersion is null)
                {
                    return OperationResult.Failure("Module version not found.");
                }

                var alreadyAdded = await dbContext.CourseModules
                    .AnyAsync(x => x.CourseVersionId == courseVersionId && x.ModuleVersionId == moduleVersionId, cancellationToken);
                if (alreadyAdded)
                {
                    return OperationResult.Failure("That module is already in this course.");
                }

                // Two versions of the same module in one course would give a learner the same
                // content twice, so block it here rather than only at publish.
                var siblingVersionInCourse = await dbContext.CourseModules.AsNoTracking()
                    .Where(x => x.CourseVersionId == courseVersionId)
                    .Join(dbContext.ModuleVersions.AsNoTracking().Where(x => x.ModuleId == moduleVersion.ModuleId),
                        courseModule => courseModule.ModuleVersionId,
                        other => other.ModuleVersionId,
                        (courseModule, other) => other.VersionNumber)
                    .FirstOrDefaultAsync(cancellationToken);
                if (siblingVersionInCourse != 0)
                {
                    return OperationResult.Failure($"This course already includes v{siblingVersionInCourse} of that module. Remove it before adding a different version.");
                }

                var nextOrder = await dbContext.CourseModules
                    .Where(x => x.CourseVersionId == courseVersionId)
                    .MaxAsync(x => (int?)x.OrderIndex, cancellationToken) ?? 0;

                dbContext.CourseModules.Add(new CourseModule
                {
                    CourseModuleId = Guid.NewGuid(),
                    CourseVersionId = courseVersionId,
                    ModuleVersionId = moduleVersionId,
                    OrderIndex = nextOrder + 1,
                    IsRequired = true
                });

                WriteAudit(dbContext, "CourseModule", courseVersionId, "AddModule", changedByUserId,
                    afterJson: new { moduleVersionId, moduleVersion.Title }, notes: moduleVersion.Title);

                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return OperationResult.Success($"Added {moduleVersion.Title}.");
            });
        }

        public async Task<OperationResult> RemoveModuleFromCourseAsync(Guid courseModuleId, string? changedByUserId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            return await TransactionalExecution.ExecuteAsync(dbContext, cancellationToken, async transaction =>
            {
                var courseModule = await dbContext.CourseModules.FirstOrDefaultAsync(x => x.CourseModuleId == courseModuleId, cancellationToken);
                if (courseModule is null)
                {
                    return OperationResult.Failure("Module not found in this course.");
                }

                var editable = await EnsureCourseVersionEditableAsync(dbContext, courseModule.CourseVersionId, cancellationToken);
                if (!editable.Succeeded)
                {
                    return editable;
                }

                // Removing the reference leaves the shared module and its content untouched; other
                // courses that include it are unaffected.
                await dbContext.CourseRankModules.Where(x => x.CourseModuleId == courseModuleId).ExecuteDeleteAsync(cancellationToken);
                dbContext.CourseModules.Remove(courseModule);

                WriteAudit(dbContext, "CourseModule", courseModuleId, "RemoveModule", changedByUserId,
                    beforeJson: new { courseModule.ModuleVersionId, courseModule.OrderIndex });

                await dbContext.SaveChangesAsync(cancellationToken);
                await ReindexAsync(dbContext, courseModule.CourseVersionId, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return OperationResult.Success("Module removed from course.");
            });
        }

        public async Task<OperationResult> ReorderCourseModulesAsync(Guid courseVersionId, IReadOnlyList<Guid> orderedCourseModuleIds, string? changedByUserId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            return await TransactionalExecution.ExecuteAsync(dbContext, cancellationToken, async transaction =>
            {
                var editable = await EnsureCourseVersionEditableAsync(dbContext, courseVersionId, cancellationToken);
                if (!editable.Succeeded)
                {
                    return editable;
                }

                var courseModules = await dbContext.CourseModules
                    .Where(x => x.CourseVersionId == courseVersionId)
                    .ToListAsync(cancellationToken);

                // Guard against a stale drag: the posted list must describe exactly this course.
                if (orderedCourseModuleIds.Count != courseModules.Count
                    || orderedCourseModuleIds.Distinct().Count() != orderedCourseModuleIds.Count
                    || orderedCourseModuleIds.Any(id => courseModules.All(x => x.CourseModuleId != id)))
                {
                    return OperationResult.Failure("The module order is out of date. Reload the course and try again.");
                }

                for (var index = 0; index < orderedCourseModuleIds.Count; index++)
                {
                    var courseModule = courseModules.First(x => x.CourseModuleId == orderedCourseModuleIds[index]);
                    courseModule.OrderIndex = index + 1;
                }

                WriteAudit(dbContext, "CourseVersion", courseVersionId, "ReorderModules", changedByUserId,
                    afterJson: new { Order = orderedCourseModuleIds });

                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return OperationResult.Success("Module order updated.");
            });
        }

        public async Task<OperationResult> SetCourseRankProfilesAsync(Guid courseVersionId, IReadOnlyList<Guid> rankProfileIds, string? changedByUserId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            return await TransactionalExecution.ExecuteAsync(dbContext, cancellationToken, async transaction =>
            {
                var editable = await EnsureCourseVersionEditableAsync(dbContext, courseVersionId, cancellationToken);
                if (!editable.Succeeded)
                {
                    return editable;
                }

                var existing = await dbContext.CourseRankProfiles
                    .Where(x => x.CourseVersionId == courseVersionId)
                    .ToListAsync(cancellationToken);

                var before = existing.Select(x => x.RankProfileId).ToList();

                var removed = existing.Where(x => !rankProfileIds.Contains(x.RankProfileId)).ToList();
                if (removed.Count > 0)
                {
                    var removedIds = removed.Select(x => x.CourseRankProfileId).ToList();
                    await dbContext.CourseRankModules.Where(x => removedIds.Contains(x.CourseRankProfileId)).ExecuteDeleteAsync(cancellationToken);
                    dbContext.CourseRankProfiles.RemoveRange(removed);
                }

                // Ordering follows the global level order, so resolution priority stays junior-to-senior.
                var profileOrder = await dbContext.RankProfiles.AsNoTracking()
                    .Where(x => rankProfileIds.Contains(x.RankProfileId))
                    .ToDictionaryAsync(x => x.RankProfileId, x => x.OrderIndex, cancellationToken);

                foreach (var rankProfileId in rankProfileIds)
                {
                    var current = existing.FirstOrDefault(x => x.RankProfileId == rankProfileId);
                    if (current is null)
                    {
                        dbContext.CourseRankProfiles.Add(new CourseRankProfile
                        {
                            CourseRankProfileId = Guid.NewGuid(),
                            CourseVersionId = courseVersionId,
                            RankProfileId = rankProfileId,
                            OrderIndex = profileOrder.GetValueOrDefault(rankProfileId),
                            IsActive = true
                        });
                    }
                    else
                    {
                        current.IsActive = true;
                        current.OrderIndex = profileOrder.GetValueOrDefault(rankProfileId);
                    }
                }

                WriteAudit(dbContext, "CourseRankProfile", courseVersionId, "Configure", changedByUserId,
                    beforeJson: new { RankProfileIds = before },
                    afterJson: new { RankProfileIds = rankProfileIds });

                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return OperationResult.Success("Rank levels updated.");
            });
        }

        public async Task<OperationResult> SetRankModuleInclusionAsync(Guid courseRankProfileId, Guid courseModuleId, bool isIncluded, string? changedByUserId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var courseRankProfile = await dbContext.CourseRankProfiles.AsNoTracking()
                .FirstOrDefaultAsync(x => x.CourseRankProfileId == courseRankProfileId, cancellationToken);
            if (courseRankProfile is null)
            {
                return OperationResult.Failure("Rank level not found on this course.");
            }

            var editable = await EnsureCourseVersionEditableAsync(dbContext, courseRankProfile.CourseVersionId, cancellationToken);
            if (!editable.Succeeded)
            {
                return editable;
            }

            var row = await dbContext.CourseRankModules
                .FirstOrDefaultAsync(x => x.CourseRankProfileId == courseRankProfileId && x.CourseModuleId == courseModuleId, cancellationToken);

            if (row is null)
            {
                dbContext.CourseRankModules.Add(new CourseRankModule
                {
                    CourseRankModuleId = Guid.NewGuid(),
                    CourseRankProfileId = courseRankProfileId,
                    CourseModuleId = courseModuleId,
                    IsIncluded = isIncluded
                });
            }
            else
            {
                row.IsIncluded = isIncluded;
            }

            WriteAudit(dbContext, "CourseRankModule", courseRankProfileId, "Configure", changedByUserId,
                afterJson: new { courseModuleId, isIncluded });

            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                return OperationResult.Failure("This rank configuration was changed by someone else. Reload and try again.");
            }

            return OperationResult.Success(isIncluded ? "Module included." : "Module excluded.");
        }

        public async Task<PublishValidationResultModel> ValidateForPublishAsync(Guid courseVersionId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var result = new PublishValidationResultModel();

            var version = await dbContext.CourseVersions.AsNoTracking()
                .FirstOrDefaultAsync(x => x.CourseVersionId == courseVersionId, cancellationToken);
            if (version is null)
            {
                result.Issues.Add(Error(CourseBuilderStep.CourseSetup, "Course version not found."));
                return result;
            }

            var course = await dbContext.Courses.AsNoTracking()
                .FirstOrDefaultAsync(x => x.CourseId == version.CourseId, cancellationToken);
            if (course is null)
            {
                result.Issues.Add(Error(CourseBuilderStep.CourseSetup, "Course not found."));
                return result;
            }

            // ---- Rule 1: course metadata is complete ------------------------------------------
            if (string.IsNullOrWhiteSpace(course.Code))
            {
                result.Issues.Add(Error(CourseBuilderStep.CourseSetup, "A course code is required."));
            }
            if (string.IsNullOrWhiteSpace(course.Title))
            {
                result.Issues.Add(Error(CourseBuilderStep.CourseSetup, "A course title is required."));
            }
            if (string.IsNullOrWhiteSpace(course.Description))
            {
                result.Issues.Add(Error(CourseBuilderStep.CourseSetup, "A course description is required."));
            }
            if (course.ValidityMonths < 1)
            {
                result.Issues.Add(Error(CourseBuilderStep.CourseSetup, "Certificate validity must be at least one month."));
            }
            if (course.PassMarkPercent is < 1 or > 100)
            {
                result.Issues.Add(Error(CourseBuilderStep.CourseSetup, "The pass mark must be between 1% and 100%."));
            }

            var courseModules = await LoadCourseModulesAsync(dbContext, courseVersionId, cancellationToken);
            if (courseModules.Count == 0)
            {
                result.Issues.Add(Error(CourseBuilderStep.AddModules, "Add at least one module to this course."));
            }

            // ---- Rule 4: every referenced module version is publishable -----------------------
            var moduleVersionIds = courseModules.Select(x => x.ModuleVersionId).ToList();
            var lessonsByModule = await dbContext.Lessons.AsNoTracking()
                .Where(x => moduleVersionIds.Contains(x.ModuleVersionId) && x.IsActive)
                .Select(x => new { x.LessonId, x.ModuleVersionId })
                .ToListAsync(cancellationToken);

            var lessonIds = lessonsByModule.Select(x => x.LessonId).ToList();
            var blockCountsByLesson = await dbContext.LessonBlocks.AsNoTracking()
                .Where(x => lessonIds.Contains(x.LessonId) && x.IsActive)
                .GroupBy(x => x.LessonId)
                .Select(x => new { LessonId = x.Key, Count = x.Count() })
                .ToDictionaryAsync(x => x.LessonId, x => x.Count, cancellationToken);

            foreach (var module in courseModules)
            {
                if (module.Status == ModuleVersionStatus.Archived)
                {
                    result.Issues.Add(Error(CourseBuilderStep.AddModules, $"'{module.Title}' ({module.VersionReference}) is archived and cannot be published in a course."));
                }

                var moduleLessons = lessonsByModule.Where(x => x.ModuleVersionId == module.ModuleVersionId).ToList();
                if (moduleLessons.Count == 0)
                {
                    result.Issues.Add(Error(CourseBuilderStep.AddModules, $"'{module.Title}' has no active lessons."));
                }
                else if (moduleLessons.All(x => blockCountsByLesson.GetValueOrDefault(x.LessonId) == 0))
                {
                    result.Issues.Add(Error(CourseBuilderStep.AddModules, $"'{module.Title}' has no lesson content."));
                }
            }

            // ---- Rule 6: assessments must be reachable from this course -----------------------
            await ValidateAssessmentReachabilityAsync(dbContext, course.CourseId, courseModules, lessonsByModule.Select(x => x.LessonId).ToList(), result, cancellationToken);

            // ---- Rule 2: at least one active rank profile -------------------------------------
            var selectedProfiles = await dbContext.CourseRankProfiles.AsNoTracking()
                .Where(x => x.CourseVersionId == courseVersionId && x.IsActive)
                .OrderBy(x => x.OrderIndex)
                .Join(dbContext.RankProfiles.AsNoTracking(),
                    courseRankProfile => courseRankProfile.RankProfileId,
                    profile => profile.RankProfileId,
                    (courseRankProfile, profile) => new { courseRankProfile, profile })
                .ToListAsync(cancellationToken);

            if (selectedProfiles.Count == 0)
            {
                result.Issues.Add(Error(CourseBuilderStep.RankConfiguration, "Select at least one rank level for this course."));
                return result;
            }

            var courseRankProfileIds = selectedProfiles.Select(x => x.courseRankProfile.CourseRankProfileId).ToList();
            var exclusions = await dbContext.CourseRankModules.AsNoTracking()
                .Where(x => courseRankProfileIds.Contains(x.CourseRankProfileId) && !x.IsIncluded)
                .ToListAsync(cancellationToken);

            foreach (var entry in selectedProfiles)
            {
                var excludedForProfile = exclusions
                    .Where(x => x.CourseRankProfileId == entry.courseRankProfile.CourseRankProfileId)
                    .Select(x => x.CourseModuleId)
                    .ToHashSet();

                var included = courseModules.Where(x => !excludedForProfile.Contains(x.CourseModuleId)).ToList();

                // ---- Rule 3: every selected rank has at least one module ----------------------
                if (included.Count == 0)
                {
                    result.Issues.Add(Error(CourseBuilderStep.RankConfiguration,
                        $"'{entry.profile.Name}' has no modules selected.", entry.profile.Name));
                    continue;
                }

                // ---- Rule 5: no duplicate module within one rank sequence ---------------------
                var duplicateModule = included
                    .GroupBy(x => x.ModuleId)
                    .FirstOrDefault(x => x.Count() > 1);
                if (duplicateModule is not null)
                {
                    var titles = string.Join(", ", duplicateModule.Select(x => x.VersionReference));
                    result.Issues.Add(Error(CourseBuilderStep.RankConfiguration,
                        $"'{entry.profile.Name}' includes the same module more than once ({titles}).", entry.profile.Name));
                }

                // ---- Rule 7: total duration per rank ------------------------------------------
                var duration = included.Sum(x => x.EstimatedMinutes ?? 0);
                result.DurationByRankProfile[entry.profile.RankProfileId] = duration;
                if (duration == 0)
                {
                    result.Issues.Add(new PublishValidationIssue
                    {
                        Severity = PublishValidationSeverity.Warning,
                        Step = CourseBuilderStep.ReviewPublish,
                        Message = $"'{entry.profile.Name}' has no estimated duration. Set module durations to show learners a time estimate.",
                        RankProfileName = entry.profile.Name
                    });
                }
            }

            return result;
        }

        public async Task<OperationResult> PublishAsync(Guid courseId, Guid courseVersionId, string? changedByUserId, CancellationToken cancellationToken = default)
        {
            var validation = await ValidateForPublishAsync(courseVersionId, cancellationToken);
            if (!validation.CanPublish)
            {
                var first = validation.Issues.First(x => x.Severity == PublishValidationSeverity.Error);
                return OperationResult.Failure($"This course cannot be published yet: {first.Message}");
            }

            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            return await TransactionalExecution.ExecuteAsync(dbContext, cancellationToken, async transaction =>
            {
                var course = await dbContext.Courses.FirstOrDefaultAsync(x => x.CourseId == courseId, cancellationToken);
                var version = await dbContext.CourseVersions.FirstOrDefaultAsync(x => x.CourseVersionId == courseVersionId && x.CourseId == courseId, cancellationToken);
                if (course is null || version is null)
                {
                    return OperationResult.Failure("Course version not found.");
                }

                var beforeStatus = version.Status;

                // Archive any previously published version of the same course.
                var previouslyPublished = await dbContext.CourseVersions
                    .Where(x => x.CourseId == courseId && x.Status == CourseVersionStatus.Published && x.CourseVersionId != courseVersionId)
                    .ToListAsync(cancellationToken);

                foreach (var archived in previouslyPublished)
                {
                    archived.Status = CourseVersionStatus.Archived;
                    archived.EffectiveToUtc = DateTime.UtcNow;
                    archived.UpdatedOnUtc = DateTime.UtcNow;
                }

                version.Status = CourseVersionStatus.Published;
                version.EffectiveFromUtc ??= DateTime.UtcNow;
                version.ApprovedByUserId = changedByUserId;
                version.ApprovedOnUtc = DateTime.UtcNow;
                version.UpdatedOnUtc = DateTime.UtcNow;

                // Freeze the per-rank duration computed during validation.
                var courseRankProfiles = await dbContext.CourseRankProfiles
                    .Where(x => x.CourseVersionId == courseVersionId)
                    .ToListAsync(cancellationToken);
                foreach (var courseRankProfile in courseRankProfiles)
                {
                    if (validation.DurationByRankProfile.TryGetValue(courseRankProfile.RankProfileId, out var duration))
                    {
                        courseRankProfile.TotalDurationMinutes = duration;
                    }
                }

                course.CurrentVersionId = version.CourseVersionId;
                course.UpdatedByUserId = changedByUserId;
                course.UpdatedOnUtc = DateTime.UtcNow;

                WriteAudit(dbContext, "CourseVersion", courseVersionId, "Publish", changedByUserId,
                    beforeJson: new { Status = beforeStatus },
                    afterJson: new
                    {
                        version.Status,
                        version.EffectiveFromUtc,
                        DurationByRankProfile = validation.DurationByRankProfile
                    },
                    notes: course.Title);

                try
                {
                    await dbContext.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                }
                catch (DbUpdateConcurrencyException)
                {
                    return OperationResult.Failure("This course was changed by someone else while you were publishing. Reload and try again.");
                }
                catch (DbUpdateException ex)
                {
                    logger.LogError(ex, "Failed to publish course version {CourseVersionId}", courseVersionId);
                    return OperationResult.Failure("The course could not be published. Please try again.");
                }

                return OperationResult.Success("Course published.");
            });
        }

        public async Task<List<ResolvedTrainingSequenceModel>> GetRankSequencesAsync(Guid courseVersionId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var profiles = await dbContext.CourseRankProfiles.AsNoTracking()
                .Where(x => x.CourseVersionId == courseVersionId && x.IsActive)
                .OrderBy(x => x.OrderIndex)
                .Join(dbContext.RankProfiles.AsNoTracking().Include(x => x.Ranks),
                    courseRankProfile => courseRankProfile.RankProfileId,
                    profile => profile.RankProfileId,
                    (courseRankProfile, profile) => profile)
                .ToListAsync(cancellationToken);

            var sequences = new List<ResolvedTrainingSequenceModel>();
            foreach (var profile in profiles)
            {
                // Resolve through any one of the profile's ranks - they all map to this profile.
                var representativeRank = profile.Ranks.Select(x => (CrewRank?)x.CrewRank).FirstOrDefault();
                if (representativeRank is null)
                {
                    continue;
                }

                var sequence = await resolutionService.ResolveAsync(courseVersionId, representativeRank, forceUnfiltered: false, cancellationToken);
                if (sequence is not null)
                {
                    sequences.Add(sequence);
                }
            }

            return sequences;
        }

        /// <summary>
        /// Rule 6. A module-scoped assessment travels with a shared module into every course.
        /// A course-scoped assessment only belongs to its owning course, so including a module
        /// that points at another course's assessment would silently break the learner's exam.
        /// Both the module-level AssessmentId and each Assessment block's LinkedAssessmentId in
        /// LessonBlock.MetadataJson are checked - the JSON link has no foreign key behind it.
        /// </summary>
        private static async Task ValidateAssessmentReachabilityAsync(
            ApplicationDbContext dbContext,
            Guid courseId,
            List<CourseModuleItemModel> courseModules,
            List<Guid> lessonIds,
            PublishValidationResultModel result,
            CancellationToken cancellationToken)
        {
            var moduleVersionIds = courseModules.Select(x => x.ModuleVersionId).ToList();

            var moduleAssessments = await dbContext.ModuleVersions.AsNoTracking()
                .Where(x => moduleVersionIds.Contains(x.ModuleVersionId) && x.HasModuleAssessment)
                .Select(x => new { x.ModuleVersionId, x.Title, x.AssessmentId })
                .ToListAsync(cancellationToken);

            var linkedAssessmentIds = new List<(Guid ModuleVersionId, Guid AssessmentId)>();

            foreach (var entry in moduleAssessments)
            {
                if (entry.AssessmentId is not Guid assessmentId)
                {
                    result.Issues.Add(Error(CourseBuilderStep.AddModules,
                        $"'{entry.Title}' is marked as having an assessment but none is selected."));
                    continue;
                }
                linkedAssessmentIds.Add((entry.ModuleVersionId, assessmentId));
            }

            // Assessment blocks store their assessment id inside MetadataJson, not as a column.
            var assessmentBlocks = await dbContext.LessonBlocks.AsNoTracking()
                .Where(x => lessonIds.Contains(x.LessonId) && x.IsActive && x.BlockType == LessonBlockType.Assessment && x.MetadataJson != null)
                .Select(x => new { x.LessonId, x.MetadataJson })
                .ToListAsync(cancellationToken);

            var lessonToModule = await dbContext.Lessons.AsNoTracking()
                .Where(x => lessonIds.Contains(x.LessonId))
                .ToDictionaryAsync(x => x.LessonId, x => x.ModuleVersionId, cancellationToken);

            foreach (var block in assessmentBlocks)
            {
                Guid? linkedId = null;
                try
                {
                    var metadata = JsonSerializer.Deserialize<TrainingLessonBlockMetadataModel>(block.MetadataJson!);
                    linkedId = metadata?.LinkedAssessmentId;
                }
                catch (JsonException)
                {
                    // Unreadable metadata is reported as a content problem, not a crash.
                    result.Issues.Add(Error(CourseBuilderStep.AddModules, "An assessment block has unreadable configuration and must be re-saved."));
                    continue;
                }

                if (linkedId is Guid id && lessonToModule.TryGetValue(block.LessonId, out var moduleVersionId))
                {
                    linkedAssessmentIds.Add((moduleVersionId, id));
                }
            }

            if (linkedAssessmentIds.Count == 0)
            {
                return;
            }

            var assessmentIds = linkedAssessmentIds.Select(x => x.AssessmentId).Distinct().ToList();
            var assessments = await dbContext.TrainingCourseAssessments.AsNoTracking()
                .Where(x => assessmentIds.Contains(x.TrainingCourseAssessmentId))
                .Select(x => new { x.TrainingCourseAssessmentId, x.TrainingCourseId, x.ModuleVersionId, x.Name })
                .ToListAsync(cancellationToken);

            foreach (var (moduleVersionId, assessmentId) in linkedAssessmentIds.Distinct())
            {
                var assessment = assessments.FirstOrDefault(x => x.TrainingCourseAssessmentId == assessmentId);
                var moduleTitle = courseModules.FirstOrDefault(x => x.ModuleVersionId == moduleVersionId)?.Title ?? "A module";

                if (assessment is null)
                {
                    result.Issues.Add(Error(CourseBuilderStep.AddModules, $"'{moduleTitle}' references an assessment that no longer exists."));
                    continue;
                }

                var reachable = assessment.ModuleVersionId == moduleVersionId || assessment.TrainingCourseId == courseId;
                if (!reachable)
                {
                    result.Issues.Add(Error(CourseBuilderStep.AddModules,
                        $"'{moduleTitle}' uses assessment '{assessment.Name}', which belongs to a different course. Re-scope it to the module in the Module Library so it can be shared."));
                }
            }
        }

        private static async Task<List<CourseModuleItemModel>> LoadCourseModulesAsync(
            ApplicationDbContext dbContext,
            Guid courseVersionId,
            CancellationToken cancellationToken)
        {
            var entries = await dbContext.CourseModules.AsNoTracking()
                .Where(x => x.CourseVersionId == courseVersionId)
                .OrderBy(x => x.OrderIndex)
                .Join(dbContext.ModuleVersions.AsNoTracking(),
                    courseModule => courseModule.ModuleVersionId,
                    moduleVersion => moduleVersion.ModuleVersionId,
                    (courseModule, moduleVersion) => new { courseModule, moduleVersion })
                .Join(dbContext.TrainingModules.AsNoTracking(),
                    entry => entry.moduleVersion.ModuleId,
                    module => module.ModuleId,
                    (entry, module) => new { entry.courseModule, entry.moduleVersion, module })
                .ToListAsync(cancellationToken);

            var moduleVersionIds = entries.Select(x => x.moduleVersion.ModuleVersionId).ToList();
            var lessonCounts = await dbContext.Lessons.AsNoTracking()
                .Where(x => moduleVersionIds.Contains(x.ModuleVersionId) && x.IsActive)
                .GroupBy(x => x.ModuleVersionId)
                .Select(x => new { ModuleVersionId = x.Key, Count = x.Count() })
                .ToDictionaryAsync(x => x.ModuleVersionId, x => x.Count, cancellationToken);

            return entries.Select(entry => new CourseModuleItemModel
            {
                CourseModuleId = entry.courseModule.CourseModuleId,
                ModuleVersionId = entry.moduleVersion.ModuleVersionId,
                ModuleId = entry.moduleVersion.ModuleId,
                Code = entry.module.Code,
                Title = entry.moduleVersion.Title,
                Category = entry.module.Category,
                OrderIndex = entry.courseModule.OrderIndex,
                IsRequired = entry.courseModule.IsRequired,
                VersionNumber = entry.moduleVersion.VersionNumber,
                Status = entry.moduleVersion.Status,
                EstimatedMinutes = entry.moduleVersion.EstimatedMinutes,
                LessonCount = lessonCounts.GetValueOrDefault(entry.moduleVersion.ModuleVersionId)
            }).ToList();
        }

        /// <summary>A published or archived course version is immutable; edits need a new version.</summary>
        private static async Task<OperationResult> EnsureCourseVersionEditableAsync(
            ApplicationDbContext dbContext,
            Guid courseVersionId,
            CancellationToken cancellationToken)
        {
            var status = await dbContext.CourseVersions.AsNoTracking()
                .Where(x => x.CourseVersionId == courseVersionId)
                .Select(x => (CourseVersionStatus?)x.Status)
                .FirstOrDefaultAsync(cancellationToken);

            if (status is null)
            {
                return OperationResult.Failure("Course version not found.");
            }

            return status == CourseVersionStatus.Draft
                ? OperationResult.Success()
                : OperationResult.Failure("This course version is published. Create a new version to make changes.");
        }

        private static async Task ReindexAsync(ApplicationDbContext dbContext, Guid courseVersionId, CancellationToken cancellationToken)
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

        private static PublishValidationIssue Error(CourseBuilderStep step, string message, string? rankProfileName = null) => new()
        {
            Severity = PublishValidationSeverity.Error,
            Step = step,
            Message = message,
            RankProfileName = rankProfileName
        };

        private static void WriteAudit(
            ApplicationDbContext dbContext,
            string entityName,
            Guid entityId,
            string actionType,
            string? changedByUserId,
            object? beforeJson = null,
            object? afterJson = null,
            string? notes = null)
        {
            dbContext.TrainingAuditLogs.Add(new TrainingAuditLog
            {
                TrainingAuditLogId = Guid.NewGuid(),
                EntityName = entityName,
                EntityId = entityId.ToString(),
                ActionType = actionType,
                ChangedByUserId = changedByUserId,
                ChangedOnUtc = DateTime.UtcNow,
                BeforeJson = beforeJson is null ? null : JsonSerializer.Serialize(beforeJson),
                AfterJson = afterJson is null ? null : JsonSerializer.Serialize(afterJson),
                Notes = notes
            });
        }
    }
}
