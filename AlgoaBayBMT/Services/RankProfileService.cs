using AlgoaBayBMT.Data;
using AlgoaBayBMT.Services.Interfaces;
using AlgoaBayBMT.Services.Models;
using AlgoaBayBMT.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace AlgoaBayBMT.Services
{
    /// <inheritdoc />
    public sealed class RankProfileService(
        IDbContextFactory<ApplicationDbContext> dbContextFactory,
        ILogger<RankProfileService> logger) : IRankProfileService
    {
        public async Task<List<RankProfileEditModel>> GetRankProfilesAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var query = dbContext.RankProfiles.AsNoTracking().Include(x => x.Ranks).AsQueryable();
            if (!includeInactive)
            {
                query = query.Where(x => x.IsActive);
            }

            var profiles = await query.OrderBy(x => x.OrderIndex).ThenBy(x => x.Name).ToListAsync(cancellationToken);

            var usageCounts = await dbContext.CourseRankProfiles.AsNoTracking()
                .GroupBy(x => x.RankProfileId)
                .Select(x => new { RankProfileId = x.Key, Count = x.Count() })
                .ToDictionaryAsync(x => x.RankProfileId, x => x.Count, cancellationToken);

            return profiles.Select(profile => MapProfile(profile, usageCounts.GetValueOrDefault(profile.RankProfileId))).ToList();
        }

        public async Task<RankProfileEditModel?> GetRankProfileAsync(Guid rankProfileId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var profile = await dbContext.RankProfiles.AsNoTracking()
                .Include(x => x.Ranks)
                .FirstOrDefaultAsync(x => x.RankProfileId == rankProfileId, cancellationToken);
            if (profile is null)
            {
                return null;
            }

            var usage = await dbContext.CourseRankProfiles.AsNoTracking()
                .CountAsync(x => x.RankProfileId == rankProfileId, cancellationToken);

            return MapProfile(profile, usage);
        }

        public async Task<OperationResult<RankProfileEditModel>> SaveRankProfileAsync(RankProfileEditModel model, string? changedByUserId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                return OperationResult<RankProfileEditModel>.Failure("A level name is required.");
            }

            var code = string.IsNullOrWhiteSpace(model.Code)
                ? BuildCodeFromName(model.Name)
                : model.Code.Trim().ToUpperInvariant();

            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            return await TransactionalExecution.ExecuteAsync(dbContext, cancellationToken, async transaction =>
            {
                var codeTaken = await dbContext.RankProfiles
                    .AnyAsync(x => x.Code == code && (model.RankProfileId == null || x.RankProfileId != model.RankProfileId), cancellationToken);
                if (codeTaken)
                {
                    return OperationResult<RankProfileEditModel>.Failure($"The code '{code}' is already used by another level.");
                }

                RankProfile profile;
                if (model.RankProfileId.HasValue)
                {
                    var existing = await dbContext.RankProfiles
                        .Include(x => x.Ranks)
                        .FirstOrDefaultAsync(x => x.RankProfileId == model.RankProfileId.Value, cancellationToken);
                    if (existing is null)
                    {
                        return OperationResult<RankProfileEditModel>.Failure("Level not found. It may have been deleted.");
                    }
                    profile = existing;
                    profile.UpdatedByUserId = changedByUserId;
                    profile.UpdatedOnUtc = DateTime.UtcNow;
                }
                else
                {
                    var nextOrder = await dbContext.RankProfiles.MaxAsync(x => (int?)x.OrderIndex, cancellationToken) ?? 0;
                    profile = new RankProfile
                    {
                        RankProfileId = Guid.NewGuid(),
                        OrderIndex = nextOrder + 1,
                        CreatedByUserId = changedByUserId,
                        CreatedOnUtc = DateTime.UtcNow
                    };
                    dbContext.RankProfiles.Add(profile);
                }

                profile.Code = code;
                profile.Name = model.Name.Trim();
                profile.Description = model.Description?.Trim();
                profile.IsActive = model.IsActive;

                // Replace the rank membership wholesale: the multi-select is the complete intent.
                var desiredRanks = model.Ranks.Distinct().ToList();
                var currentRanks = profile.Ranks.ToList();

                foreach (var removed in currentRanks.Where(x => !desiredRanks.Contains(x.CrewRank)))
                {
                    dbContext.RankProfileRanks.Remove(removed);
                }

                foreach (var added in desiredRanks.Where(rank => currentRanks.All(x => x.CrewRank != rank)))
                {
                    dbContext.RankProfileRanks.Add(new RankProfileRank
                    {
                        RankProfileRankId = Guid.NewGuid(),
                        RankProfileId = profile.RankProfileId,
                        CrewRank = added
                    });
                }

                try
                {
                    await dbContext.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                }
                catch (DbUpdateException ex)
                {
                    logger.LogError(ex, "Failed to save rank profile {RankProfileId}", profile.RankProfileId);
                    return OperationResult<RankProfileEditModel>.Failure("The level could not be saved. Please try again.");
                }

                model.RankProfileId = profile.RankProfileId;
                model.Code = profile.Code;
                model.OrderIndex = profile.OrderIndex;
                return OperationResult<RankProfileEditModel>.Success(model, "Level saved.");
            });
        }

        public async Task<OperationResult> DeleteRankProfileAsync(Guid rankProfileId, string? changedByUserId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var profile = await dbContext.RankProfiles.FirstOrDefaultAsync(x => x.RankProfileId == rankProfileId, cancellationToken);
            if (profile is null)
            {
                return OperationResult.Failure("Level not found.");
            }

            // A profile in use by a course, or pinned to a learner's history, must not vanish.
            var courseUsage = await dbContext.CourseRankProfiles.CountAsync(x => x.RankProfileId == rankProfileId, cancellationToken);
            if (courseUsage > 0)
            {
                return OperationResult.Failure($"This level is used by {courseUsage} course version(s). Remove it from those courses first, or deactivate it instead.");
            }

            var evidenceUsage = await dbContext.UserTrainingAssignments.AnyAsync(x => x.ResolvedRankProfileId == rankProfileId, cancellationToken)
                || await dbContext.CourseCompletionRecords.AnyAsync(x => x.RankProfileId == rankProfileId, cancellationToken);
            if (evidenceUsage)
            {
                return OperationResult.Failure("This level is referenced by learner training records. Deactivate it instead of deleting it.");
            }

            dbContext.RankProfiles.Remove(profile);
            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex, "Failed to delete rank profile {RankProfileId}", rankProfileId);
                return OperationResult.Failure("The level could not be deleted. Please try again.");
            }

            return OperationResult.Success("Level deleted.");
        }

        public async Task<OperationResult> MoveRankProfileAsync(Guid rankProfileId, int direction, string? changedByUserId, CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var profiles = await dbContext.RankProfiles.OrderBy(x => x.OrderIndex).ToListAsync(cancellationToken);
            var index = profiles.FindIndex(x => x.RankProfileId == rankProfileId);
            var targetIndex = index + direction;
            if (index < 0 || targetIndex < 0 || targetIndex >= profiles.Count)
            {
                return OperationResult.Failure("Level cannot be moved further.");
            }

            (profiles[index].OrderIndex, profiles[targetIndex].OrderIndex) = (profiles[targetIndex].OrderIndex, profiles[index].OrderIndex);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Materialise the order ONCE. Sorting inside the loop would re-evaluate against the
            // key being mutated, shifting positions underneath the iteration.
            var sorted = profiles.OrderBy(x => x.OrderIndex).ToList();
            for (var i = 0; i < sorted.Count; i++)
            {
                sorted[i].OrderIndex = i + 1;
            }
            await dbContext.SaveChangesAsync(cancellationToken);

            return OperationResult.Success("Level order updated.");
        }

        public async Task<RankCoverageModel> GetRankCoverageAsync(CancellationToken cancellationToken = default)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var memberships = await dbContext.RankProfileRanks.AsNoTracking()
                .Join(dbContext.RankProfiles.AsNoTracking().Where(x => x.IsActive),
                    rank => rank.RankProfileId,
                    profile => profile.RankProfileId,
                    (rank, profile) => new { rank.CrewRank, profile.Name })
                .ToListAsync(cancellationToken);

            var byRank = memberships
                .GroupBy(x => x.CrewRank)
                .ToDictionary(x => x.Key, x => x.Select(m => m.Name).Distinct().ToList());

            var coverage = new RankCoverageModel();
            foreach (var rank in Enum.GetValues<CrewRank>())
            {
                if (!byRank.TryGetValue(rank, out var profileNames))
                {
                    coverage.UnassignedRanks.Add(rank);
                }
                else if (profileNames.Count > 1)
                {
                    coverage.OverlappingRanks.Add(new RankOverlapModel { CrewRank = rank, ProfileNames = profileNames });
                }
            }

            return coverage;
        }

        public async Task<RankProfileEditModel?> ResolveProfileForRankAsync(Guid courseVersionId, CrewRank? crewRank, CancellationToken cancellationToken = default)
        {
            if (crewRank is null)
            {
                return null;
            }

            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            // First match by CourseRankProfile.OrderIndex keeps resolution deterministic even when
            // an admin has put the same rank in more than one level.
            var match = await dbContext.CourseRankProfiles.AsNoTracking()
                .Where(x => x.CourseVersionId == courseVersionId && x.IsActive)
                .OrderBy(x => x.OrderIndex)
                .Join(dbContext.RankProfiles.AsNoTracking().Where(x => x.IsActive),
                    courseRankProfile => courseRankProfile.RankProfileId,
                    profile => profile.RankProfileId,
                    (courseRankProfile, profile) => new { courseRankProfile.OrderIndex, profile })
                .Where(x => x.profile.Ranks.Any(rank => rank.CrewRank == crewRank.Value))
                .OrderBy(x => x.OrderIndex)
                .Select(x => x.profile)
                .FirstOrDefaultAsync(cancellationToken);

            if (match is null)
            {
                return null;
            }

            var ranks = await dbContext.RankProfileRanks.AsNoTracking()
                .Where(x => x.RankProfileId == match.RankProfileId)
                .Select(x => x.CrewRank)
                .ToListAsync(cancellationToken);

            return new RankProfileEditModel
            {
                RankProfileId = match.RankProfileId,
                Code = match.Code,
                Name = match.Name,
                Description = match.Description,
                OrderIndex = match.OrderIndex,
                IsActive = match.IsActive,
                IsSystem = match.IsSystem,
                Ranks = ranks
            };
        }

        private static RankProfileEditModel MapProfile(RankProfile profile, int courseUsageCount) => new()
        {
            RankProfileId = profile.RankProfileId,
            Code = profile.Code,
            Name = profile.Name,
            Description = profile.Description,
            OrderIndex = profile.OrderIndex,
            IsActive = profile.IsActive,
            IsSystem = profile.IsSystem,
            Ranks = profile.Ranks.Select(x => x.CrewRank).OrderBy(x => x.ToString()).ToList(),
            CourseUsageCount = courseUsageCount
        };

        private static string BuildCodeFromName(string name)
        {
            var cleaned = new string(name.Trim().ToUpperInvariant()
                .Select(c => char.IsLetterOrDigit(c) ? c : '_')
                .ToArray());

            while (cleaned.Contains("__", StringComparison.Ordinal))
            {
                cleaned = cleaned.Replace("__", "_", StringComparison.Ordinal);
            }

            cleaned = cleaned.Trim('_');
            return cleaned.Length > 50 ? cleaned[..50] : cleaned;
        }
    }
}
