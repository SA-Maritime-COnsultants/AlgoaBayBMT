using AlgoaBayBMT.Data;
using AlgoaBayBMT.Services.Interfaces;
using AlgoaBayBMT.Services.Models;
using AlgoaBayBMT.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace AlgoaBayBMT.Services
{
    /// <inheritdoc />
    public sealed class ModuleLibraryService(
        IDbContextFactory<ApplicationDbContext> dbContextFactory,
        ILogger<ModuleLibraryService> logger) : IModuleLibraryService
    {
        public async Task<List<ModuleLibraryItemModel>> SearchModulesAsync(
            string? query = null,
            string? category = null,
            bool includeArchived = false,
            CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var modules = dbContext.TrainingModules.AsNoTracking().AsQueryable();
            if (!includeArchived)
            {
                modules = modules.Where(x => !x.IsArchived);
            }

            if (!string.IsNullOrWhiteSpace(query))
            {
                var term = query.Trim();
                modules = modules.Where(x => EF.Functions.Like(x.Title, $"%{term}%")
                    || EF.Functions.Like(x.Code, $"%{term}%")
                    || (x.Description != null && EF.Functions.Like(x.Description, $"%{term}%")));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                modules = modules.Where(x => x.Category == category);
            }

            var results = await modules
                .OrderBy(x => x.Code)
                .Select(module => new
                {
                    module,
                    version = dbContext.ModuleVersions.AsNoTracking()
                        .FirstOrDefault(v => v.ModuleVersionId == module.CurrentVersionId)
                })
                .ToListAsync(cancellationToken);

            var versionIds = results.Where(x => x.version != null).Select(x => x.version!.ModuleVersionId).ToList();

            var lessonCounts = await dbContext.Lessons.AsNoTracking()
                .Where(x => versionIds.Contains(x.ModuleVersionId) && x.IsActive)
                .GroupBy(x => x.ModuleVersionId)
                .Select(x => new { ModuleVersionId = x.Key, Count = x.Count() })
                .ToDictionaryAsync(x => x.ModuleVersionId, x => x.Count, cancellationToken);

            // Usage counts every course that references ANY version of the module, which is what
            // "used in 3 courses" should mean to an author.
            var moduleIds = results.Select(x => x.module.ModuleId).ToList();
            var usage = await dbContext.CourseModules.AsNoTracking()
                .Join(dbContext.ModuleVersions.AsNoTracking().Where(v => moduleIds.Contains(v.ModuleId)),
                    courseModule => courseModule.ModuleVersionId,
                    moduleVersion => moduleVersion.ModuleVersionId,
                    (courseModule, moduleVersion) => new { moduleVersion.ModuleId, courseModule.CourseVersionId })
                .Join(dbContext.CourseVersions.AsNoTracking(),
                    entry => entry.CourseVersionId,
                    courseVersion => courseVersion.CourseVersionId,
                    (entry, courseVersion) => new { entry.ModuleId, courseVersion.CourseId })
                .Distinct()
                .ToListAsync(cancellationToken);

            var usageByModule = usage
                .GroupBy(x => x.ModuleId)
                .ToDictionary(x => x.Key, x => x.Select(u => u.CourseId).Distinct().Count());

            return results.Select(entry => new ModuleLibraryItemModel
            {
                ModuleId = entry.module.ModuleId,
                CurrentVersionId = entry.module.CurrentVersionId,
                Code = entry.module.Code,
                Title = entry.version?.Title ?? entry.module.Title,
                Description = entry.version?.Description ?? entry.module.Description,
                Category = entry.module.Category,
                VersionNumber = entry.version?.VersionNumber ?? 0,
                Status = entry.version?.Status ?? ModuleVersionStatus.Draft,
                IsActive = entry.module.IsActive,
                IsArchived = entry.module.IsArchived,
                EstimatedMinutes = entry.version?.EstimatedMinutes,
                LessonCount = entry.version is null ? 0 : lessonCounts.GetValueOrDefault(entry.version.ModuleVersionId),
                UsageCourseCount = usageByModule.GetValueOrDefault(entry.module.ModuleId),
                CreatedOnUtc = entry.module.CreatedOnUtc
            }).ToList();
        }

        public async Task<List<string>> GetCategoriesAsync(CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            return await dbContext.TrainingModules.AsNoTracking()
                .Where(x => x.Category != null && x.Category != "")
                .Select(x => x.Category!)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync(cancellationToken);
        }

        public async Task<ModuleLibraryItemModel?> GetModuleAsync(Guid moduleId, CancellationToken cancellationToken = default)
        {
            var all = await SearchModulesAsync(includeArchived: true, cancellationToken: cancellationToken);
            return all.FirstOrDefault(x => x.ModuleId == moduleId);
        }

        public async Task<TrainingModuleEditModel?> GetModuleVersionAsync(Guid moduleVersionId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var version = await dbContext.ModuleVersions.AsNoTracking()
                .FirstOrDefaultAsync(x => x.ModuleVersionId == moduleVersionId, cancellationToken);
            if (version is null)
            {
                return null;
            }

            return new TrainingModuleEditModel
            {
                ModuleVersionId = version.ModuleVersionId,
                ModuleId = version.ModuleId,
                VersionNumber = version.VersionNumber,
                Status = version.Status,
                Title = version.Title,
                Description = version.Description,
                EstimatedMinutes = version.EstimatedMinutes,
                IsActive = version.IsActive,
                HasModuleAssessment = version.HasModuleAssessment,
                AssessmentId = version.AssessmentId,
                AssessmentPassMarkPercent = version.AssessmentPassMarkPercent,
                AssessmentMaxAttempts = version.AssessmentMaxAttempts
            };
        }

        public async Task<OperationResult<ModuleLibraryItemModel>> SaveModuleAsync(ModuleLibraryItemModel model, string? changedByUserId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(model.Title))
            {
                return OperationResult<ModuleLibraryItemModel>.Failure("A module title is required.");
            }

            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            return await TransactionalExecution.ExecuteAsync(dbContext, cancellationToken, async transaction =>
            {
                TrainingModule module;
                TrainingModuleVersion? version = null;

                if (model.ModuleId != Guid.Empty)
                {
                    var existing = await dbContext.TrainingModules.FirstOrDefaultAsync(x => x.ModuleId == model.ModuleId, cancellationToken);
                    if (existing is null)
                    {
                        return OperationResult<ModuleLibraryItemModel>.Failure("Module not found. It may have been deleted.");
                    }
                    module = existing;
                    module.UpdatedByUserId = changedByUserId;
                    module.UpdatedOnUtc = DateTime.UtcNow;

                    if (module.CurrentVersionId is Guid currentVersionId)
                    {
                        version = await dbContext.ModuleVersions.FirstOrDefaultAsync(x => x.ModuleVersionId == currentVersionId, cancellationToken);
                    }
                }
                else
                {
                    var moduleId = Guid.NewGuid();
                    var moduleVersionId = Guid.NewGuid();

                    // TrainingModule.CurrentVersionId and ModuleVersion.ModuleId point at each
                    // other, so inserting both in one SaveChanges is a circular dependency EF
                    // cannot order. Insert the identity with a null pointer first, then the
                    // version, then link them.
                    module = new TrainingModule
                    {
                        ModuleId = moduleId,
                        Code = string.IsNullOrWhiteSpace(model.Code)
                            ? await GenerateModuleCodeAsync(dbContext, cancellationToken)
                            : model.Code.Trim().ToUpperInvariant(),
                        Title = model.Title.Trim(),
                        CurrentVersionId = null,
                        CreatedByUserId = changedByUserId,
                        CreatedOnUtc = DateTime.UtcNow
                    };
                    dbContext.TrainingModules.Add(module);
                    await dbContext.SaveChangesAsync(cancellationToken);

                    version = new TrainingModuleVersion
                    {
                        ModuleVersionId = moduleVersionId,
                        ModuleId = moduleId,
                        VersionNumber = 1,
                        VersionLabel = "v1",
                        Status = ModuleVersionStatus.Draft,
                        Title = model.Title.Trim(),
                        CreatedOnUtc = DateTime.UtcNow
                    };
                    dbContext.ModuleVersions.Add(version);
                    await dbContext.SaveChangesAsync(cancellationToken);

                    module.CurrentVersionId = moduleVersionId;
                }

                module.Title = model.Title.Trim();
                module.Description = model.Description?.Trim();
                module.Category = string.IsNullOrWhiteSpace(model.Category) ? null : model.Category.Trim();
                module.IsActive = model.IsActive;

                // Title and description only propagate to a DRAFT version. A published version is
                // frozen evidence and must not be edited through the library screen.
                if (version is not null && version.Status == ModuleVersionStatus.Draft)
                {
                    version.Title = module.Title;
                    version.Description = module.Description;
                    version.EstimatedMinutes = model.EstimatedMinutes;
                    version.UpdatedOnUtc = DateTime.UtcNow;
                }

                await WriteAuditAsync(dbContext, "TrainingModule", module.ModuleId, "Save", changedByUserId, afterJson: new
                {
                    module.Code,
                    module.Title,
                    module.Category,
                    module.IsActive
                }, notes: module.Title);

                try
                {
                    await dbContext.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                }
                catch (DbUpdateException ex)
                {
                    logger.LogError(ex, "Failed to save module {ModuleId}", module.ModuleId);
                    return OperationResult<ModuleLibraryItemModel>.Failure("The module could not be saved. The code may already be in use.");
                }

                model.ModuleId = module.ModuleId;
                model.Code = module.Code;
                model.CurrentVersionId = module.CurrentVersionId;
                return OperationResult<ModuleLibraryItemModel>.Success(model, "Module saved.");
            });
        }

        public async Task<OperationResult<TrainingModuleEditModel>> CreateModuleVersionAsync(Guid moduleId, string? changedByUserId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var opResult = await TransactionalExecution.ExecuteAsync(dbContext, cancellationToken, async transaction =>
            {
                var module = await dbContext.TrainingModules.FirstOrDefaultAsync(x => x.ModuleId == moduleId, cancellationToken);
                if (module is null)
                {
                    return OperationResult<Guid?>.Failure("Module not found.");
                }

                var nextVersionNumber = (await dbContext.ModuleVersions
                    .Where(x => x.ModuleId == moduleId)
                    .MaxAsync(x => (int?)x.VersionNumber, cancellationToken) ?? 0) + 1;

                var newVersionId = Guid.NewGuid();
                var newVersion = new TrainingModuleVersion
                {
                    ModuleVersionId = newVersionId,
                    ModuleId = moduleId,
                    VersionNumber = nextVersionNumber,
                    VersionLabel = $"v{nextVersionNumber}",
                    Status = ModuleVersionStatus.Draft,
                    Title = module.Title,
                    Description = module.Description,
                    CreatedOnUtc = DateTime.UtcNow,
                    ChangeSummary = "Created from the current version."
                };

                // Deep-copy the source content. New identifiers are correct here because this is
                // NEW content: the source version keeps the lesson and block ids that existing
                // learner progress, completion records and certificates reference.
                if (module.CurrentVersionId is Guid sourceVersionId)
                {
                    var source = await dbContext.ModuleVersions.AsNoTracking()
                        .FirstOrDefaultAsync(x => x.ModuleVersionId == sourceVersionId, cancellationToken);
                    if (source is not null)
                    {
                        newVersion.Title = source.Title;
                        newVersion.Description = source.Description;
                        newVersion.EstimatedMinutes = source.EstimatedMinutes;
                        newVersion.IsActive = source.IsActive;
                        newVersion.HasModuleAssessment = source.HasModuleAssessment;
                        newVersion.AssessmentId = source.AssessmentId;
                        newVersion.AssessmentPassMarkPercent = source.AssessmentPassMarkPercent;
                        newVersion.AssessmentMaxAttempts = source.AssessmentMaxAttempts;
                    }

                    dbContext.ModuleVersions.Add(newVersion);

                    var sourceLessons = await dbContext.Lessons.AsNoTracking()
                        .Where(x => x.ModuleVersionId == sourceVersionId)
                        .OrderBy(x => x.OrderIndex)
                        .ToListAsync(cancellationToken);

                    var sourceLessonIds = sourceLessons.Select(x => x.LessonId).ToList();
                    var sourceBlocks = await dbContext.LessonBlocks.AsNoTracking()
                        .Where(x => sourceLessonIds.Contains(x.LessonId))
                        .OrderBy(x => x.OrderIndex)
                        .ToListAsync(cancellationToken);

                    var lessonMap = new Dictionary<Guid, Guid>();
                    foreach (var sourceLesson in sourceLessons)
                    {
                        var newLessonId = Guid.NewGuid();
                        lessonMap[sourceLesson.LessonId] = newLessonId;
                        dbContext.Lessons.Add(new TrainingLesson
                        {
                            LessonId = newLessonId,
                            ModuleVersionId = newVersionId,
                            Title = sourceLesson.Title,
                            Summary = sourceLesson.Summary,
                            OrderIndex = sourceLesson.OrderIndex,
                            EstimatedMinutes = sourceLesson.EstimatedMinutes,
                            IsPreview = sourceLesson.IsPreview,
                            IsRequired = sourceLesson.IsRequired,
                            IsActive = sourceLesson.IsActive
                        });
                    }

                    foreach (var sourceBlock in sourceBlocks)
                    {
                        dbContext.LessonBlocks.Add(new LessonBlock
                        {
                            LessonBlockId = Guid.NewGuid(),
                            LessonId = lessonMap[sourceBlock.LessonId],
                            BlockType = sourceBlock.BlockType,
                            Title = sourceBlock.Title,
                            Subtitle = sourceBlock.Subtitle,
                            OrderIndex = sourceBlock.OrderIndex,
                            MarkdownBody = sourceBlock.MarkdownBody,
                            ThumbnailUrl = sourceBlock.ThumbnailUrl,
                            FileUrl = sourceBlock.FileUrl,
                            ExternalUrl = sourceBlock.ExternalUrl,
                            MimeType = sourceBlock.MimeType,
                            DurationSeconds = sourceBlock.DurationSeconds,
                            MetadataJson = sourceBlock.MetadataJson,
                            MediaAssetId = sourceBlock.MediaAssetId,
                            IsRequired = sourceBlock.IsRequired,
                            IsActive = sourceBlock.IsActive
                        });
                    }
                }
                else
                {
                    dbContext.ModuleVersions.Add(newVersion);
                }

                module.CurrentVersionId = newVersionId;
                module.UpdatedByUserId = changedByUserId;
                module.UpdatedOnUtc = DateTime.UtcNow;

                await WriteAuditAsync(dbContext, "ModuleVersion", newVersionId, "CreateVersion", changedByUserId,
                    afterJson: new { newVersion.ModuleId, newVersion.VersionNumber, newVersion.Status },
                    notes: $"{module.Code} v{nextVersionNumber}");

                try
                {
                    await dbContext.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                }
                catch (DbUpdateException ex)
                {
                    logger.LogError(ex, "Failed to create a new version of module {ModuleId}", moduleId);
                    return OperationResult<Guid?>.Failure("The new module version could not be created. Please try again.");
                }

                return OperationResult<Guid?>.Success(newVersionId, $"Created v{nextVersionNumber}.");
            });

            if (!opResult.Succeeded || opResult.Data is not Guid createdVersionId)
            {
                return OperationResult<TrainingModuleEditModel>.Failure(opResult.Message ?? string.Join(", ", opResult.Errors));
            }

            var result = await GetModuleVersionAsync(createdVersionId, cancellationToken);
            return OperationResult<TrainingModuleEditModel>.Success(result!, opResult.Message ?? "Version created.");
        }

        public async Task<OperationResult> PublishModuleVersionAsync(Guid moduleVersionId, string? changedByUserId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            return await TransactionalExecution.ExecuteAsync(dbContext, cancellationToken, async transaction =>
            {
                var version = await dbContext.ModuleVersions.FirstOrDefaultAsync(x => x.ModuleVersionId == moduleVersionId, cancellationToken);
                if (version is null)
                {
                    return OperationResult.Failure("Module version not found.");
                }

                if (version.Status != ModuleVersionStatus.Draft)
                {
                    return OperationResult.Failure("Only a draft version can be published.");
                }

                var lessonCount = await dbContext.Lessons.CountAsync(x => x.ModuleVersionId == moduleVersionId && x.IsActive, cancellationToken);
                if (lessonCount == 0)
                {
                    return OperationResult.Failure("Add at least one active lesson before publishing this module.");
                }

                var beforeStatus = version.Status;
                version.Status = ModuleVersionStatus.Published;
                version.PublishedOnUtc = DateTime.UtcNow;
                version.PublishedByUserId = changedByUserId;
                version.UpdatedOnUtc = DateTime.UtcNow;

                await WriteAuditAsync(dbContext, "ModuleVersion", moduleVersionId, "Publish", changedByUserId,
                    beforeJson: new { Status = beforeStatus },
                    afterJson: new { version.Status, version.PublishedOnUtc },
                    notes: version.Title);

                try
                {
                    await dbContext.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                }
                catch (DbUpdateConcurrencyException)
                {
                    return OperationResult.Failure("This module was changed by someone else. Reload and try again.");
                }

                return OperationResult.Success("Module version published. Its content is now locked.");
            });
        }

        public async Task<OperationResult> ArchiveModuleAsync(Guid moduleId, bool archived, string? changedByUserId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var module = await dbContext.TrainingModules.FirstOrDefaultAsync(x => x.ModuleId == moduleId, cancellationToken);
            if (module is null)
            {
                return OperationResult.Failure("Module not found.");
            }

            module.IsArchived = archived;
            module.ArchivedByUserId = archived ? changedByUserId : null;
            module.ArchivedOnUtc = archived ? DateTime.UtcNow : null;
            module.UpdatedByUserId = changedByUserId;
            module.UpdatedOnUtc = DateTime.UtcNow;

            await WriteAuditAsync(dbContext, "TrainingModule", moduleId, archived ? "Archive" : "Restore", changedByUserId,
                afterJson: new { module.IsArchived }, notes: module.Title);

            await dbContext.SaveChangesAsync(cancellationToken);
            return OperationResult.Success(archived ? "Module archived." : "Module restored.");
        }

        public async Task<OperationResult> DeleteModuleAsync(Guid moduleId, string? changedByUserId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var module = await dbContext.TrainingModules.FirstOrDefaultAsync(x => x.ModuleId == moduleId, cancellationToken);
            if (module is null)
            {
                return OperationResult.Failure("Module not found.");
            }

            var usage = await GetModuleUsageAsync(moduleId, cancellationToken);
            if (!usage.CanDelete)
            {
                return usage.Courses.Count > 0
                    ? OperationResult.Failure($"This module is used by {usage.Courses.Count} course version(s). Remove it from those courses first, or archive it instead.")
                    : OperationResult.Failure("Learners have progress against this module. Archive it instead of deleting it.");
            }

            return await TransactionalExecution.ExecuteAsync(dbContext, cancellationToken, async transaction =>
            {
                var versionIds = await dbContext.ModuleVersions
                    .Where(x => x.ModuleId == moduleId)
                    .Select(x => x.ModuleVersionId)
                    .ToListAsync(cancellationToken);

                var lessonIds = await dbContext.Lessons
                    .Where(x => versionIds.Contains(x.ModuleVersionId))
                    .Select(x => x.LessonId)
                    .ToListAsync(cancellationToken);

                await dbContext.LessonBlocks.Where(x => lessonIds.Contains(x.LessonId)).ExecuteDeleteAsync(cancellationToken);
                await dbContext.Lessons.Where(x => versionIds.Contains(x.ModuleVersionId)).ExecuteDeleteAsync(cancellationToken);

                // Clear the identity -> version pointer before the versions go (Restrict FK).
                module.CurrentVersionId = null;
                await dbContext.SaveChangesAsync(cancellationToken);

                // Both directions of the module/assessment link are NoAction, so detach them by hand.
                await dbContext.ModuleVersions
                    .Where(x => x.ModuleId == moduleId)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.AssessmentId, (Guid?)null), cancellationToken);
                await dbContext.TrainingCourseAssessments
                    .Where(x => x.ModuleVersionId != null && versionIds.Contains(x.ModuleVersionId!.Value))
                    .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.ModuleVersionId, (Guid?)null), cancellationToken);
                await dbContext.TrainingQuestionBankQuestions
                    .Where(x => x.TrainingModuleVersionId != null && versionIds.Contains(x.TrainingModuleVersionId!.Value))
                    .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.TrainingModuleVersionId, (Guid?)null), cancellationToken);

                await dbContext.ModuleVersions.Where(x => x.ModuleId == moduleId).ExecuteDeleteAsync(cancellationToken);

                dbContext.TrainingModules.Remove(module);
                await WriteAuditAsync(dbContext, "TrainingModule", moduleId, "Delete", changedByUserId, notes: module.Title);

                try
                {
                    await dbContext.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                }
                catch (DbUpdateException ex)
                {
                    logger.LogError(ex, "Failed to delete module {ModuleId}", moduleId);
                    return OperationResult.Failure("The module could not be deleted. Please try again.");
                }

                return OperationResult.Success("Module deleted.");
            });
        }

        public async Task<ModuleUsageModel> GetModuleUsageAsync(Guid moduleId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var courses = await dbContext.CourseModules.AsNoTracking()
                .Join(dbContext.ModuleVersions.AsNoTracking().Where(x => x.ModuleId == moduleId),
                    courseModule => courseModule.ModuleVersionId,
                    moduleVersion => moduleVersion.ModuleVersionId,
                    (courseModule, moduleVersion) => new { courseModule, moduleVersion })
                .Join(dbContext.CourseVersions.AsNoTracking(),
                    entry => entry.courseModule.CourseVersionId,
                    courseVersion => courseVersion.CourseVersionId,
                    (entry, courseVersion) => new { entry.moduleVersion, courseVersion })
                .Join(dbContext.Courses.AsNoTracking(),
                    entry => entry.courseVersion.CourseId,
                    course => course.CourseId,
                    (entry, course) => new ModuleUsageCourseModel
                    {
                        CourseId = course.CourseId,
                        CourseVersionId = entry.courseVersion.CourseVersionId,
                        CourseCode = course.Code,
                        CourseTitle = course.Title,
                        CourseVersionNumber = entry.courseVersion.VersionNumber,
                        CourseVersionStatus = entry.courseVersion.Status,
                        ModuleVersionNumber = entry.moduleVersion.VersionNumber
                    })
                .ToListAsync(cancellationToken);

            var versionIds = await dbContext.ModuleVersions.AsNoTracking()
                .Where(x => x.ModuleId == moduleId)
                .Select(x => x.ModuleVersionId)
                .ToListAsync(cancellationToken);

            var lessonIds = await dbContext.Lessons.AsNoTracking()
                .Where(x => versionIds.Contains(x.ModuleVersionId))
                .Select(x => x.LessonId)
                .ToListAsync(cancellationToken);

            var hasEvidence = lessonIds.Count > 0
                && await dbContext.UserLessonProgress.AsNoTracking().AnyAsync(x => lessonIds.Contains(x.LessonId), cancellationToken);

            return new ModuleUsageModel
            {
                ModuleId = moduleId,
                Courses = courses,
                HasLearnerEvidence = hasEvidence
            };
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

        private static Task WriteAuditAsync(
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
                BeforeJson = beforeJson is null ? null : System.Text.Json.JsonSerializer.Serialize(beforeJson),
                AfterJson = afterJson is null ? null : System.Text.Json.JsonSerializer.Serialize(afterJson),
                Notes = notes
            });
            return Task.CompletedTask;
        }
    }
}
