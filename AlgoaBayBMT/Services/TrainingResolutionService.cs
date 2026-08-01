using AlgoaBayBMT.Data;
using AlgoaBayBMT.Services.Interfaces;
using AlgoaBayBMT.Services.Models;
using AlgoaBayBMT.Shared.Models;
using AlgoaBayBMT.Shared.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AlgoaBayBMT.Services
{
    /// <inheritdoc />
    public sealed class TrainingResolutionService(
        IDbContextFactory<ApplicationDbContext> dbContextFactory,
        UserManager<ApplicationUser> userManager,
        IRankProfileService rankProfileService,
        ILogger<TrainingResolutionService> logger) : ITrainingResolutionService
    {
        /// <summary>
        /// These roles bypass audience filtering elsewhere in the learner service, so they also
        /// bypass rank filtering here — otherwise an administrator could not preview a course.
        /// </summary>
        private static readonly string[] FullAccessRoles = [RoleNames.Admin, RoleNames.Dffe, RoleNames.Samsa];

        public async Task<ResolvedTrainingSequenceModel?> ResolveAsync(
            Guid courseVersionId,
            CrewRank? crewRank,
            bool forceUnfiltered = false,
            CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            return await ResolveInternalAsync(dbContext, courseVersionId, crewRank, forceUnfiltered, explicitRankProfileId: null, cancellationToken);
        }

        public async Task<ResolvedTrainingSequenceModel?> ResolveForLearnerAsync(
            string userId,
            Guid courseId,
            CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var (resolved, assignment) = await ResolveForLearnerInternalAsync(dbContext, userId, courseId, cancellationToken);
            if (resolved is null)
            {
                return null;
            }

            // Pin on first access so an unpinned assignment stops floating with the current version.
            if (assignment is not null && assignment.ResolvedCourseVersionId is null)
            {
                assignment.ResolvedCourseVersionId = resolved.CourseVersionId;
                assignment.ResolvedRankProfileId = resolved.RankProfileId;
                assignment.ResolvedOnUtc = DateTime.UtcNow;
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            return resolved;
        }

        public async Task<ResolvedTrainingSequenceModel?> ResolveForLearnerReadOnlyAsync(
            string userId,
            Guid courseId,
            CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var (resolved, _) = await ResolveForLearnerInternalAsync(dbContext, userId, courseId, cancellationToken);
            return resolved;
        }

        private async Task<(ResolvedTrainingSequenceModel? Resolved, UserTrainingAssignment? Assignment)> ResolveForLearnerInternalAsync(
            ApplicationDbContext dbContext,
            string userId,
            Guid courseId,
            CancellationToken cancellationToken)
        {
            var assignment = await dbContext.UserTrainingAssignments
                .FirstOrDefaultAsync(x => x.UserId == userId && x.CourseId == courseId, cancellationToken);

            var user = await userManager.FindByIdAsync(userId);
            var hasFullAccess = user is not null && await HasFullAccessAsync(user);

            // A pinned assignment is replayed, never re-resolved. The pinned rank profile is used
            // DIRECTLY rather than being re-derived from the learner's rank: re-deriving would go
            // back through first-match-by-OrderIndex and could land on a different profile if the
            // rank overlaps two levels, or if the pinned level was later deselected — silently
            // rewriting training the learner has already done.
            if (assignment?.ResolvedCourseVersionId is Guid pinnedVersionId)
            {
                var pinned = await ResolveInternalAsync(
                    dbContext,
                    pinnedVersionId,
                    crewRank: null,
                    forceUnfiltered: assignment.ResolvedRankProfileId is null,
                    explicitRankProfileId: assignment.ResolvedRankProfileId,
                    cancellationToken);
                if (pinned is not null)
                {
                    return (pinned, assignment);
                }

                // Do not substitute a different sequence for pinned history. Returning null makes
                // the caller surface "training unavailable" rather than quietly serving the wrong
                // modules against an existing completion record.
                logger.LogError(
                    "Pinned training for user {UserId} on course {CourseId} could not be resolved (version {CourseVersionId}, rank profile {RankProfileId}). Refusing to substitute a different sequence.",
                    userId, courseId, pinnedVersionId, assignment.ResolvedRankProfileId);
                return (null, assignment);
            }

            var course = await dbContext.Courses.AsNoTracking()
                .FirstOrDefaultAsync(x => x.CourseId == courseId && !x.IsDeleted, cancellationToken);
            if (course?.CurrentVersionId is not Guid currentVersionId)
            {
                return (null, assignment);
            }

            var resolved = await ResolveInternalAsync(dbContext, currentVersionId, user?.CrewRank, hasFullAccess, explicitRankProfileId: null, cancellationToken);
            return (resolved, assignment);
        }

        public async Task<OperationResult<ResolvedTrainingSequenceModel>> SnapshotAssignmentAsync(
            Guid userTrainingAssignmentId,
            string? changedByUserId,
            CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var assignment = await dbContext.UserTrainingAssignments
                .FirstOrDefaultAsync(x => x.UserTrainingAssignmentId == userTrainingAssignmentId, cancellationToken);
            if (assignment is null)
            {
                return OperationResult<ResolvedTrainingSequenceModel>.Failure("Training assignment not found.");
            }

            var course = await dbContext.Courses.AsNoTracking()
                .FirstOrDefaultAsync(x => x.CourseId == assignment.CourseId, cancellationToken);
            if (course?.CurrentVersionId is not Guid courseVersionId)
            {
                return OperationResult<ResolvedTrainingSequenceModel>.Failure("This course has no published version to assign.");
            }

            var user = await userManager.FindByIdAsync(assignment.UserId);
            var crewRank = user?.CrewRank;

            // Prefer the crew record's rank: registration is crew-driven and CrewMembers.Rank is
            // the value the compliance workflow keys off.
            var crewRankFromCrewMember = await dbContext.CrewMembers.AsNoTracking()
                .Where(x => x.ApplicationUserId == assignment.UserId && x.Rank != null)
                .Select(x => x.Rank)
                .FirstOrDefaultAsync(cancellationToken);
            if (crewRankFromCrewMember is not null)
            {
                crewRank = crewRankFromCrewMember;
            }

            var hasFullAccess = user is not null && await HasFullAccessAsync(user);
            var resolved = await ResolveInternalAsync(dbContext, courseVersionId, crewRank, hasFullAccess, explicitRankProfileId: null, cancellationToken);
            if (resolved is null)
            {
                return OperationResult<ResolvedTrainingSequenceModel>.Failure("This course version has no modules to assign.");
            }

            assignment.ResolvedCourseVersionId = resolved.CourseVersionId;
            assignment.ResolvedRankProfileId = resolved.RankProfileId;
            assignment.ResolvedOnUtc = DateTime.UtcNow;

            dbContext.TrainingAuditLogs.Add(new TrainingAuditLog
            {
                TrainingAuditLogId = Guid.NewGuid(),
                EntityName = "UserTrainingAssignment",
                EntityId = assignment.UserTrainingAssignmentId.ToString(),
                ActionType = "ResolveSequence",
                ChangedByUserId = changedByUserId,
                ChangedOnUtc = DateTime.UtcNow,
                AfterJson = System.Text.Json.JsonSerializer.Serialize(new
                {
                    resolved.CourseVersionId,
                    resolved.RankProfileId,
                    resolved.RankProfileName,
                    resolved.IsUnfiltered,
                    ModuleVersionIds = resolved.Modules.Select(x => x.ModuleVersionId)
                }),
                Notes = resolved.RankProfileName ?? "Unfiltered (no matching rank profile)"
            });

            await dbContext.SaveChangesAsync(cancellationToken);
            return OperationResult<ResolvedTrainingSequenceModel>.Success(resolved, "Training sequence resolved.");
        }

        /// <summary>
        /// The resolution itself:
        /// CourseVersion + rank -> CourseRankProfile -> included CourseModules (course order)
        /// -> ModuleVersions -> lessons.
        /// </summary>
        private async Task<ResolvedTrainingSequenceModel?> ResolveInternalAsync(
            ApplicationDbContext dbContext,
            Guid courseVersionId,
            CrewRank? crewRank,
            bool forceUnfiltered,
            Guid? explicitRankProfileId,
            CancellationToken cancellationToken)
        {
            var version = await dbContext.CourseVersions.AsNoTracking()
                .FirstOrDefaultAsync(x => x.CourseVersionId == courseVersionId, cancellationToken);
            if (version is null)
            {
                return null;
            }

            RankProfileEditModel? profile;
            if (explicitRankProfileId is Guid pinnedProfileId)
            {
                // Replaying pinned history: use the exact profile that was recorded. If it is no
                // longer selected on this course version there is no honest sequence to serve, so
                // return null rather than substituting a different one.
                var stillSelected = await dbContext.CourseRankProfiles.AsNoTracking()
                    .AnyAsync(x => x.CourseVersionId == courseVersionId && x.RankProfileId == pinnedProfileId, cancellationToken);
                if (!stillSelected)
                {
                    return null;
                }

                profile = await rankProfileService.GetRankProfileAsync(pinnedProfileId, cancellationToken);
                if (profile is null)
                {
                    return null;
                }
            }
            else
            {
                // Full-access and rankless users take the unfiltered path: every module the course
                // defines, in course order. Without this an administrator could never preview a course.
                profile = forceUnfiltered
                    ? null
                    : await rankProfileService.ResolveProfileForRankAsync(courseVersionId, crewRank, cancellationToken);
            }

            var isUnfiltered = profile is null;

            var courseModules = await dbContext.CourseModules.AsNoTracking()
                .Where(x => x.CourseVersionId == courseVersionId)
                .OrderBy(x => x.OrderIndex)
                .Join(dbContext.ModuleVersions.AsNoTracking().Where(x => x.IsActive),
                    courseModule => courseModule.ModuleVersionId,
                    moduleVersion => moduleVersion.ModuleVersionId,
                    (courseModule, moduleVersion) => new { courseModule, moduleVersion })
                .Join(dbContext.TrainingModules.AsNoTracking(),
                    entry => entry.moduleVersion.ModuleId,
                    module => module.ModuleId,
                    (entry, module) => new { entry.courseModule, entry.moduleVersion, module })
                .ToListAsync(cancellationToken);

            if (courseModules.Count == 0)
            {
                return new ResolvedTrainingSequenceModel
                {
                    CourseId = version.CourseId,
                    CourseVersionId = courseVersionId,
                    RankProfileId = profile?.RankProfileId,
                    RankProfileName = profile?.Name,
                    IsUnfiltered = isUnfiltered
                };
            }

            // A missing CourseRankModule row means included, so only explicit exclusions are stored.
            var exclusions = new HashSet<Guid>();
            if (profile?.RankProfileId is Guid rankProfileId)
            {
                var courseRankProfileId = await dbContext.CourseRankProfiles.AsNoTracking()
                    .Where(x => x.CourseVersionId == courseVersionId && x.RankProfileId == rankProfileId)
                    .Select(x => (Guid?)x.CourseRankProfileId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (courseRankProfileId is Guid crpId)
                {
                    var excluded = await dbContext.CourseRankModules.AsNoTracking()
                        .Where(x => x.CourseRankProfileId == crpId && !x.IsIncluded)
                        .Select(x => x.CourseModuleId)
                        .ToListAsync(cancellationToken);
                    exclusions = excluded.ToHashSet();
                }
            }

            var includedModules = courseModules.Where(x => !exclusions.Contains(x.courseModule.CourseModuleId)).ToList();
            var moduleVersionIds = includedModules.Select(x => x.moduleVersion.ModuleVersionId).ToList();

            var lessons = await dbContext.Lessons.AsNoTracking()
                .Where(x => moduleVersionIds.Contains(x.ModuleVersionId) && x.IsActive)
                .OrderBy(x => x.OrderIndex)
                .Select(x => new { x.LessonId, x.ModuleVersionId, x.OrderIndex, x.EstimatedMinutes })
                .ToListAsync(cancellationToken);

            var lessonsByModule = lessons
                .GroupBy(x => x.ModuleVersionId)
                .ToDictionary(x => x.Key, x => x.OrderBy(l => l.OrderIndex).ToList());

            var sequence = new ResolvedTrainingSequenceModel
            {
                CourseId = version.CourseId,
                CourseVersionId = courseVersionId,
                RankProfileId = profile?.RankProfileId,
                RankProfileName = profile?.Name,
                IsUnfiltered = isUnfiltered
            };

            foreach (var entry in includedModules)
            {
                var moduleLessons = lessonsByModule.GetValueOrDefault(entry.moduleVersion.ModuleVersionId) ?? [];

                sequence.Modules.Add(new ResolvedModuleModel
                {
                    CourseModuleId = entry.courseModule.CourseModuleId,
                    ModuleVersionId = entry.moduleVersion.ModuleVersionId,
                    ModuleId = entry.moduleVersion.ModuleId,
                    Code = entry.module.Code,
                    Title = entry.moduleVersion.Title,
                    VersionNumber = entry.moduleVersion.VersionNumber,
                    OrderIndex = entry.courseModule.OrderIndex,
                    IsRequired = entry.courseModule.IsRequired,
                    EstimatedMinutes = entry.moduleVersion.EstimatedMinutes
                        ?? (moduleLessons.Sum(x => x.EstimatedMinutes) is int summed and > 0 ? summed : null),
                    LessonIds = moduleLessons.Select(x => x.LessonId).ToList()
                });
            }

            sequence.TotalDurationMinutes = sequence.Modules.Sum(x => x.EstimatedMinutes ?? 0);
            return sequence;
        }

        private async Task<bool> HasFullAccessAsync(ApplicationUser user)
        {
            foreach (var role in FullAccessRoles)
            {
                if (await userManager.IsInRoleAsync(user, role))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
