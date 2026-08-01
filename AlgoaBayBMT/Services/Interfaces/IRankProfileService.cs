using AlgoaBayBMT.Services.Models;
using AlgoaBayBMT.Shared.Models;

namespace AlgoaBayBMT.Services.Interfaces
{
    /// <summary>
    /// Manages the training levels (rank profiles) that courses target, and resolves a learner's
    /// crew rank to the profile a given course version serves.
    /// </summary>
    public interface IRankProfileService
    {
        Task<List<RankProfileEditModel>> GetRankProfilesAsync(bool includeInactive = false, CancellationToken cancellationToken = default);

        Task<RankProfileEditModel?> GetRankProfileAsync(Guid rankProfileId, CancellationToken cancellationToken = default);

        Task<OperationResult<RankProfileEditModel>> SaveRankProfileAsync(RankProfileEditModel model, string? changedByUserId, CancellationToken cancellationToken = default);

        /// <summary>Blocked while any course version still targets the profile.</summary>
        Task<OperationResult> DeleteRankProfileAsync(Guid rankProfileId, string? changedByUserId, CancellationToken cancellationToken = default);

        Task<OperationResult> MoveRankProfileAsync(Guid rankProfileId, int direction, string? changedByUserId, CancellationToken cancellationToken = default);

        /// <summary>Reports ranks no profile covers, and ranks claimed by more than one profile.</summary>
        Task<RankCoverageModel> GetRankCoverageAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// The profile a course version serves for this rank: the lowest
        /// <see cref="CourseRankProfile.OrderIndex"/> whose profile contains the rank.
        /// Returns null when the rank is null or matches nothing, which the resolver treats as
        /// "give the learner the full unfiltered sequence".
        /// </summary>
        Task<RankProfileEditModel?> ResolveProfileForRankAsync(Guid courseVersionId, CrewRank? crewRank, CancellationToken cancellationToken = default);
    }
}
