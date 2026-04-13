using AlgoaBayBMT.Data;
using AlgoaBayBMT.Services.Interfaces;
using AlgoaBayBMT.Services.Models;
using AlgoaBayBMT.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace AlgoaBayBMT.Services
{
    public class TrainingManagementService(IDbContextFactory<ApplicationDbContext> dbContextFactory) : ITrainingManagementService
    {
        public async Task<TrainingDashboardModel> GetDashboardAsync(CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var dashboard = new TrainingDashboardModel
            {
                TotalCourses = await dbContext.Courses.AsNoTracking().CountAsync(cancellationToken),
                PublishedCourses = await dbContext.CourseVersions.AsNoTracking().CountAsync(x => x.Status == CourseVersionStatus.Published, cancellationToken),
                DraftCourses = await dbContext.CourseVersions.AsNoTracking().CountAsync(x => x.Status == CourseVersionStatus.Draft, cancellationToken),
                TotalModules = await dbContext.Modules.AsNoTracking().CountAsync(cancellationToken),
                TotalLessons = await dbContext.Lessons.AsNoTracking().CountAsync(cancellationToken)
            };

            dashboard.RecentCourses = (await GetCoursesAsync(cancellationToken))
                .OrderByDescending(x => x.CreatedOnUtc)
                .Take(6)
                .ToList();

            return dashboard;
        }

        public async Task<List<TrainingCourseListItemModel>> GetCoursesAsync(CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var courses = await dbContext.Courses
                .AsNoTracking()
                .OrderBy(x => x.Title)
                .Select(x => new TrainingCourseListItemModel
                {
                    CourseId = x.CourseId,
                    Code = x.Code,
                    Title = x.Title,
                    Description = x.Description,
                    TargetAudienceSummary = x.TargetAudienceSummary,
                    ValidityMonths = x.ValidityMonths,
                    IsMandatory = x.IsMandatory,
                    IsActive = x.IsActive,
                    CurrentVersionId = x.CurrentVersionId,
                    CreatedOnUtc = x.CreatedOnUtc
                })
                .ToListAsync(cancellationToken);

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

            var moduleCounts = await dbContext.Modules
                .AsNoTracking()
                .Where(x => currentVersionIds.Contains(x.CourseVersionId))
                .GroupBy(x => x.CourseVersionId)
                .Select(x => new { CourseVersionId = x.Key, Count = x.Count() })
                .ToDictionaryAsync(x => x.CourseVersionId, x => x.Count, cancellationToken);

            var lessonCounts = await dbContext.Modules
                .AsNoTracking()
                .Where(x => currentVersionIds.Contains(x.CourseVersionId))
                .Join(dbContext.Lessons.AsNoTracking(),
                    module => module.ModuleId,
                    lesson => lesson.ModuleId,
                    (module, lesson) => new { module.CourseVersionId, lesson.LessonId })
                .GroupBy(x => x.CourseVersionId)
                .Select(x => new { CourseVersionId = x.Key, Count = x.Count() })
                .ToDictionaryAsync(x => x.CourseVersionId, x => x.Count, cancellationToken);

            foreach (var course in courses)
            {
                if (course.CurrentVersionId.HasValue && versionLookup.TryGetValue(course.CurrentVersionId.Value, out var version))
                {
                    course.CurrentVersionNumber = version.VersionNumber;
                    course.CurrentVersionLabel = version.VersionLabel;
                    course.CurrentVersionStatus = version.Status;
                    course.ModuleCount = moduleCounts.GetValueOrDefault(course.CurrentVersionId.Value);
                    course.LessonCount = lessonCounts.GetValueOrDefault(course.CurrentVersionId.Value);
                }
            }

            return courses;
        }

        public async Task<TrainingCourseEditModel?> GetCourseAsync(Guid courseId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            return await dbContext.Courses
                .AsNoTracking()
                .Where(x => x.CourseId == courseId)
                .Select(x => new TrainingCourseEditModel
                {
                    CourseId = x.CourseId,
                    Code = x.Code,
                    Title = x.Title,
                    Description = x.Description,
                    ThumbnailUrl = x.ThumbnailUrl,
                    TargetAudienceSummary = x.TargetAudienceSummary,
                    RegulatoryReference = x.RegulatoryReference,
                    LearningObjectives = x.LearningObjectives,
                    ValidityMonths = x.ValidityMonths,
                    EstimatedDurationMinutes = x.EstimatedDurationMinutes,
                    IsMandatory = x.IsMandatory,
                    IsActive = x.IsActive
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<OperationResult<TrainingCourseEditModel>> SaveCourseAsync(TrainingCourseEditModel model, string? changedByUserId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var normalizedCode = model.Code.Trim().ToUpperInvariant();
            if (await dbContext.Courses.AnyAsync(x => x.Code == normalizedCode && x.CourseId != (model.CourseId ?? Guid.Empty), cancellationToken))
            {
                return OperationResult<TrainingCourseEditModel>.Failure("A course with this code already exists.");
            }

            Course course;
            if (model.CourseId.HasValue)
            {
                course = await dbContext.Courses.FirstOrDefaultAsync(x => x.CourseId == model.CourseId.Value, cancellationToken)
                    ?? throw new InvalidOperationException("Course not found.");
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
            course.Title = model.Title.Trim();
            course.Description = model.Description?.Trim();
            course.ThumbnailUrl = model.ThumbnailUrl?.Trim();
            course.TargetAudienceSummary = model.TargetAudienceSummary?.Trim();
            course.RegulatoryReference = model.RegulatoryReference?.Trim();
            course.LearningObjectives = model.LearningObjectives?.Trim();
            course.ValidityMonths = model.ValidityMonths;
            course.EstimatedDurationMinutes = model.EstimatedDurationMinutes;
            course.IsMandatory = model.IsMandatory;
            course.IsActive = model.IsActive;

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
            await WriteAuditLogAsync(dbContext, "Course", course.CourseId.ToString(), model.CourseId.HasValue ? "Save" : "Create", changedByUserId, notes: course.Title, cancellationToken: cancellationToken);
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
                var sourceModules = await dbContext.Modules.AsNoTracking()
                    .Where(x => x.CourseVersionId == course.CurrentVersionId.Value)
                    .OrderBy(x => x.OrderIndex)
                    .ToListAsync(cancellationToken);

                var sourceModuleIds = sourceModules.Select(x => x.ModuleId).ToList();
                var sourceLessons = await dbContext.Lessons.AsNoTracking()
                    .Where(x => sourceModuleIds.Contains(x.ModuleId))
                    .OrderBy(x => x.OrderIndex)
                    .ToListAsync(cancellationToken);

                var sourceLessonIds = sourceLessons.Select(x => x.LessonId).ToList();
                var sourceBlocks = await dbContext.LessonBlocks.AsNoTracking()
                    .Where(x => sourceLessonIds.Contains(x.LessonId))
                    .OrderBy(x => x.OrderIndex)
                    .ToListAsync(cancellationToken);

                var moduleMap = new Dictionary<Guid, Guid>();
                foreach (var sourceModule in sourceModules)
                {
                    var newModuleId = Guid.NewGuid();
                    moduleMap[sourceModule.ModuleId] = newModuleId;
                    dbContext.Modules.Add(new TrainingModule
                    {
                        ModuleId = newModuleId,
                        CourseVersionId = nextVersion.CourseVersionId,
                        Title = sourceModule.Title,
                        Description = sourceModule.Description,
                        OrderIndex = sourceModule.OrderIndex,
                        EstimatedMinutes = sourceModule.EstimatedMinutes,
                        IsActive = sourceModule.IsActive
                    });
                }

                var lessonMap = new Dictionary<Guid, Guid>();
                foreach (var sourceLesson in sourceLessons)
                {
                    var newLessonId = Guid.NewGuid();
                    lessonMap[sourceLesson.LessonId] = newLessonId;
                    dbContext.Lessons.Add(new TrainingLesson
                    {
                        LessonId = newLessonId,
                        ModuleId = moduleMap[sourceLesson.ModuleId],
                        Title = sourceLesson.Title,
                        Summary = sourceLesson.Summary,
                        LessonType = sourceLesson.LessonType,
                        CompletionRule = sourceLesson.CompletionRule,
                        OrderIndex = sourceLesson.OrderIndex,
                        EstimatedMinutes = sourceLesson.EstimatedMinutes,
                        IsRequired = sourceLesson.IsRequired,
                        IsPreview = sourceLesson.IsPreview,
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
                        OrderIndex = sourceBlock.OrderIndex,
                        MarkdownBody = sourceBlock.MarkdownBody,
                        FileUrl = sourceBlock.FileUrl,
                        ExternalUrl = sourceBlock.ExternalUrl,
                        MimeType = sourceBlock.MimeType,
                        DurationSeconds = sourceBlock.DurationSeconds,
                        MetadataJson = sourceBlock.MetadataJson,
                        MediaAssetId = sourceBlock.MediaAssetId,
                        IsRequired = sourceBlock.IsRequired
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

            var modules = await dbContext.Modules.AsNoTracking()
                .Where(x => x.CourseVersionId == version.CourseVersionId)
                .OrderBy(x => x.OrderIndex)
                .ToListAsync(cancellationToken);

            var moduleIds = modules.Select(x => x.ModuleId).ToList();
            var lessons = await dbContext.Lessons.AsNoTracking()
                .Where(x => moduleIds.Contains(x.ModuleId))
                .OrderBy(x => x.OrderIndex)
                .ToListAsync(cancellationToken);

            var lessonIds = lessons.Select(x => x.LessonId).ToList();
            var blocks = await dbContext.LessonBlocks.AsNoTracking()
                .Where(x => lessonIds.Contains(x.LessonId))
                .OrderBy(x => x.OrderIndex)
                .ToListAsync(cancellationToken);

            return new TrainingCourseBuilderModel
            {
                CourseId = course.CourseId,
                Code = course.Code,
                Title = course.Title,
                Description = course.Description,
                CurrentVersionId = version.CourseVersionId,
                CurrentVersionNumber = version.VersionNumber,
                CurrentVersionLabel = version.VersionLabel,
                CurrentVersionStatus = version.Status,
                PublishedOnUtc = version.ApprovedOnUtc,
                Modules = modules.Select(module => new TrainingModuleEditModel
                {
                    ModuleId = module.ModuleId,
                    CourseVersionId = module.CourseVersionId,
                    Title = module.Title,
                    Description = module.Description,
                    OrderIndex = module.OrderIndex,
                    EstimatedMinutes = module.EstimatedMinutes,
                    IsActive = module.IsActive,
                    Lessons = lessons
                        .Where(x => x.ModuleId == module.ModuleId)
                        .OrderBy(x => x.OrderIndex)
                        .Select(lesson => new TrainingLessonEditModel
                        {
                            LessonId = lesson.LessonId,
                            ModuleId = lesson.ModuleId,
                            Title = lesson.Title,
                            Summary = lesson.Summary,
                            LessonType = lesson.LessonType,
                            CompletionRule = lesson.CompletionRule,
                            OrderIndex = lesson.OrderIndex,
                            EstimatedMinutes = lesson.EstimatedMinutes,
                            IsRequired = lesson.IsRequired,
                            IsPreview = lesson.IsPreview,
                            IsActive = lesson.IsActive,
                            Blocks = blocks
                                .Where(x => x.LessonId == lesson.LessonId)
                                .OrderBy(x => x.OrderIndex)
                                .Select(block => new TrainingLessonBlockEditModel
                                {
                                    LessonBlockId = block.LessonBlockId,
                                    LessonId = block.LessonId,
                                    BlockType = block.BlockType,
                                    Title = block.Title,
                                    OrderIndex = block.OrderIndex,
                                    MarkdownBody = block.MarkdownBody,
                                    FileUrl = block.FileUrl,
                                    ExternalUrl = block.ExternalUrl,
                                    MimeType = block.MimeType,
                                    DurationSeconds = block.DurationSeconds,
                                    MetadataJson = block.MetadataJson,
                                    MediaAssetId = block.MediaAssetId,
                                    IsRequired = block.IsRequired
                                })
                                .ToList()
                        })
                        .ToList()
                }).ToList()
            };
        }

        public async Task<OperationResult<TrainingModuleEditModel>> SaveModuleAsync(TrainingModuleEditModel model, string? changedByUserId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            TrainingModule module;
            if (model.ModuleId.HasValue)
            {
                module = await dbContext.Modules.FirstOrDefaultAsync(x => x.ModuleId == model.ModuleId.Value, cancellationToken)
                    ?? throw new InvalidOperationException("Module not found.");
            }
            else
            {
                var nextOrder = await dbContext.Modules
                    .Where(x => x.CourseVersionId == model.CourseVersionId)
                    .MaxAsync(x => (int?)x.OrderIndex, cancellationToken) ?? 0;

                module = new TrainingModule
                {
                    ModuleId = Guid.NewGuid(),
                    CourseVersionId = model.CourseVersionId,
                    OrderIndex = nextOrder + 1
                };
                dbContext.Modules.Add(module);
            }

            module.Title = model.Title.Trim();
            module.Description = model.Description?.Trim();
            module.EstimatedMinutes = model.EstimatedMinutes;
            module.IsActive = model.IsActive;

            await dbContext.SaveChangesAsync(cancellationToken);
            await ReindexModulesAsync(dbContext, module.CourseVersionId, cancellationToken);
            await WriteAuditLogAsync(dbContext, "Module", module.ModuleId.ToString(), "Save", changedByUserId, notes: module.Title, cancellationToken: cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            model.ModuleId = module.ModuleId;
            model.OrderIndex = module.OrderIndex;
            return OperationResult<TrainingModuleEditModel>.Success(model, "Module saved.");
        }

        public async Task<OperationResult> DeleteModuleAsync(Guid moduleId, string? changedByUserId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var module = await dbContext.Modules.FirstOrDefaultAsync(x => x.ModuleId == moduleId, cancellationToken);
            if (module is null)
            {
                return OperationResult.Failure("Module not found.");
            }

            var versionId = module.CourseVersionId;
            dbContext.Modules.Remove(module);
            await WriteAuditLogAsync(dbContext, "Module", moduleId.ToString(), "Delete", changedByUserId, notes: module.Title, cancellationToken: cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            await ReindexModulesAsync(dbContext, versionId, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            return OperationResult.Success("Module deleted.");
        }

        public async Task<OperationResult> MoveModuleAsync(Guid moduleId, int direction, string? changedByUserId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var module = await dbContext.Modules.FirstOrDefaultAsync(x => x.ModuleId == moduleId, cancellationToken);
            if (module is null)
            {
                return OperationResult.Failure("Module not found.");
            }

            var modules = await dbContext.Modules
                .Where(x => x.CourseVersionId == module.CourseVersionId)
                .OrderBy(x => x.OrderIndex)
                .ToListAsync(cancellationToken);

            var index = modules.FindIndex(x => x.ModuleId == moduleId);
            var targetIndex = index + direction;
            if (index < 0 || targetIndex < 0 || targetIndex >= modules.Count)
            {
                return OperationResult.Failure("Module cannot be moved further.");
            }

            (modules[index].OrderIndex, modules[targetIndex].OrderIndex) = (modules[targetIndex].OrderIndex, modules[index].OrderIndex);
            await WriteAuditLogAsync(dbContext, "Module", moduleId.ToString(), "Reorder", changedByUserId, notes: module.Title, cancellationToken: cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            await ReindexModulesAsync(dbContext, module.CourseVersionId, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            return OperationResult.Success("Module order updated.");
        }

        public async Task<OperationResult<TrainingLessonEditModel>> SaveLessonAsync(TrainingLessonEditModel model, string? changedByUserId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            TrainingLesson lesson;
            if (model.LessonId.HasValue)
            {
                lesson = await dbContext.Lessons.FirstOrDefaultAsync(x => x.LessonId == model.LessonId.Value, cancellationToken)
                    ?? throw new InvalidOperationException("Lesson not found.");
            }
            else
            {
                var nextOrder = await dbContext.Lessons
                    .Where(x => x.ModuleId == model.ModuleId)
                    .MaxAsync(x => (int?)x.OrderIndex, cancellationToken) ?? 0;

                lesson = new TrainingLesson
                {
                    LessonId = Guid.NewGuid(),
                    ModuleId = model.ModuleId,
                    OrderIndex = nextOrder + 1
                };
                dbContext.Lessons.Add(lesson);
            }

            lesson.Title = model.Title.Trim();
            lesson.Summary = model.Summary?.Trim();
            lesson.LessonType = model.LessonType;
            lesson.CompletionRule = model.CompletionRule;
            lesson.EstimatedMinutes = model.EstimatedMinutes;
            lesson.IsRequired = model.IsRequired;
            lesson.IsPreview = model.IsPreview;
            lesson.IsActive = model.IsActive;

            await dbContext.SaveChangesAsync(cancellationToken);
            await ReindexLessonsAsync(dbContext, lesson.ModuleId, cancellationToken);
            await WriteAuditLogAsync(dbContext, "Lesson", lesson.LessonId.ToString(), "Save", changedByUserId, notes: lesson.Title, cancellationToken: cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            model.LessonId = lesson.LessonId;
            model.OrderIndex = lesson.OrderIndex;
            return OperationResult<TrainingLessonEditModel>.Success(model, "Lesson saved.");
        }

        public async Task<OperationResult> DeleteLessonAsync(Guid lessonId, string? changedByUserId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var lesson = await dbContext.Lessons.FirstOrDefaultAsync(x => x.LessonId == lessonId, cancellationToken);
            if (lesson is null)
            {
                return OperationResult.Failure("Lesson not found.");
            }

            var moduleId = lesson.ModuleId;
            dbContext.Lessons.Remove(lesson);
            await WriteAuditLogAsync(dbContext, "Lesson", lessonId.ToString(), "Delete", changedByUserId, notes: lesson.Title, cancellationToken: cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            await ReindexLessonsAsync(dbContext, moduleId, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            return OperationResult.Success("Lesson deleted.");
        }

        public async Task<OperationResult> MoveLessonAsync(Guid lessonId, int direction, string? changedByUserId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var lesson = await dbContext.Lessons.FirstOrDefaultAsync(x => x.LessonId == lessonId, cancellationToken);
            if (lesson is null)
            {
                return OperationResult.Failure("Lesson not found.");
            }

            var lessons = await dbContext.Lessons
                .Where(x => x.ModuleId == lesson.ModuleId)
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
            await dbContext.SaveChangesAsync(cancellationToken);
            await ReindexLessonsAsync(dbContext, lesson.ModuleId, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            return OperationResult.Success("Lesson order updated.");
        }

        public async Task<OperationResult<TrainingLessonBlockEditModel>> SaveLessonBlockAsync(TrainingLessonBlockEditModel model, string? changedByUserId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            LessonBlock block;
            if (model.LessonBlockId.HasValue)
            {
                block = await dbContext.LessonBlocks.FirstOrDefaultAsync(x => x.LessonBlockId == model.LessonBlockId.Value, cancellationToken)
                    ?? throw new InvalidOperationException("Lesson block not found.");
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
            block.MarkdownBody = model.MarkdownBody?.Trim();
            block.FileUrl = model.FileUrl?.Trim();
            block.ExternalUrl = model.ExternalUrl?.Trim();
            block.MimeType = model.MimeType?.Trim();
            block.DurationSeconds = model.DurationSeconds;
            block.MetadataJson = model.MetadataJson?.Trim();
            block.MediaAssetId = model.MediaAssetId;
            block.IsRequired = model.IsRequired;

            await dbContext.SaveChangesAsync(cancellationToken);
            await ReindexBlocksAsync(dbContext, block.LessonId, cancellationToken);
            await WriteAuditLogAsync(dbContext, "LessonBlock", block.LessonBlockId.ToString(), "Save", changedByUserId, notes: block.Title, cancellationToken: cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            model.LessonBlockId = block.LessonBlockId;
            model.OrderIndex = block.OrderIndex;
            return OperationResult<TrainingLessonBlockEditModel>.Success(model, "Lesson content saved.");
        }

        public async Task<OperationResult> DeleteLessonBlockAsync(Guid lessonBlockId, string? changedByUserId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var block = await dbContext.LessonBlocks.FirstOrDefaultAsync(x => x.LessonBlockId == lessonBlockId, cancellationToken);
            if (block is null)
            {
                return OperationResult.Failure("Lesson content block not found.");
            }

            var lessonId = block.LessonId;
            dbContext.LessonBlocks.Remove(block);
            await WriteAuditLogAsync(dbContext, "LessonBlock", lessonBlockId.ToString(), "Delete", changedByUserId, notes: block.Title, cancellationToken: cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            await ReindexBlocksAsync(dbContext, lessonId, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
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
            await dbContext.SaveChangesAsync(cancellationToken);
            await ReindexBlocksAsync(dbContext, block.LessonId, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            return OperationResult.Success("Lesson content order updated.");
        }

        private static async Task ReindexModulesAsync(ApplicationDbContext dbContext, Guid courseVersionId, CancellationToken cancellationToken)
        {
            var modules = await dbContext.Modules
                .Where(x => x.CourseVersionId == courseVersionId)
                .OrderBy(x => x.OrderIndex)
                .ThenBy(x => x.Title)
                .ToListAsync(cancellationToken);

            for (var index = 0; index < modules.Count; index++)
            {
                modules[index].OrderIndex = index + 1;
            }
        }

        private static async Task ReindexLessonsAsync(ApplicationDbContext dbContext, Guid moduleId, CancellationToken cancellationToken)
        {
            var lessons = await dbContext.Lessons
                .Where(x => x.ModuleId == moduleId)
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
    }
}
