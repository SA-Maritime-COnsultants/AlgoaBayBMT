using AlgoaBayBMT.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace AlgoaBayBMT.Data
{
    /// <summary>
    /// Seeds the training levels that courses target. A rank profile groups crew ranks into a
    /// training level; the Course Builder then decides which shared modules each level receives.
    /// </summary>
    public static class TrainingSeedData
    {
        /// <summary>
        /// The four starting levels. Every <see cref="CrewRank"/> member appears exactly once, so a
        /// learner always resolves to a single profile without needing a tie-break.
        /// These are a starting point only: admins add, rename, reorder and re-map them at
        /// /training-management/rank-profiles, and this seeder never overwrites their edits.
        /// </summary>
        private static readonly (string Code, string Name, string Description, CrewRank[] Ranks)[] SeedProfiles =
        [
            ("GENERAL_CREW", "General Crew",
                "Deckhands, ratings and catering crew.",
                [
                    CrewRank.OrdinarySeaman,
                    CrewRank.DeckCadet,
                    CrewRank.EngineCadet,
                    CrewRank.Cook,
                    CrewRank.Steward,
                    CrewRank.AbleSeaman,
                    CrewRank.Bosun
                ]),
            ("OFFICER", "Officer",
                "Watchkeeping deck and engineering officers.",
                [
                    CrewRank.ChiefOfficer,
                    CrewRank.SecondEngineer,
                    CrewRank.ThirdEngineer
                ]),
            ("SENIOR_OFFICER", "Senior Officer",
                "Command and senior engineering ranks.",
                [
                    CrewRank.Captain,
                    CrewRank.Master,
                    CrewRank.ChiefEngineer
                ]),
            ("MANAGEMENT", "Management",
                "Shore-based and operational management roles.",
                [
                    CrewRank.POAC,
                    CrewRank.DesignatedPersonAshore,
                    CrewRank.BunkerOperatorManager
                ])
        ];

        /// <summary>
        /// Idempotent. Creates any missing seed profile and adds missing rank memberships to it,
        /// but never removes a rank an admin has deliberately moved elsewhere, and never renames
        /// or reorders a profile that already exists.
        /// </summary>
        public static async Task EnsureSeedDataAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
        {
            using var scope = serviceProvider.CreateScope();
            var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var existingProfiles = await dbContext.RankProfiles
                .Include(x => x.Ranks)
                .ToListAsync(cancellationToken);

            // A rank already claimed by any profile is left alone: an admin may have moved it.
            var claimedRanks = existingProfiles
                .SelectMany(profile => profile.Ranks)
                .Select(rank => rank.CrewRank)
                .ToHashSet();

            var orderIndex = 0;
            foreach (var (code, name, description, ranks) in SeedProfiles)
            {
                orderIndex++;

                var profile = existingProfiles.FirstOrDefault(x => string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase));
                if (profile is null)
                {
                    profile = new RankProfile
                    {
                        RankProfileId = Guid.NewGuid(),
                        Code = code,
                        Name = name,
                        Description = description,
                        OrderIndex = orderIndex,
                        IsActive = true,
                        IsSystem = true,
                        CreatedOnUtc = DateTime.UtcNow
                    };
                    dbContext.RankProfiles.Add(profile);
                }

                foreach (var rank in ranks)
                {
                    var alreadyInThisProfile = profile.Ranks.Any(x => x.CrewRank == rank);
                    if (alreadyInThisProfile || claimedRanks.Contains(rank))
                    {
                        continue;
                    }

                    dbContext.RankProfileRanks.Add(new RankProfileRank
                    {
                        RankProfileRankId = Guid.NewGuid(),
                        RankProfileId = profile.RankProfileId,
                        CrewRank = rank
                    });
                    claimedRanks.Add(rank);
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
