using AlgoaBayBMT.Services.Models;
using AlgoaBayBMT.Shared.Models;

namespace AlgoaBayBMT.Services.Interfaces
{
    /// <summary>
    /// The single source of truth for "which modules, in which order, does this learner do?".
    ///
    /// Resolution is deterministic: the same course version and rank always produce the same
    /// ordered sequence, because published course versions, their module references and the
    /// authored module versions they point at are all immutable.
    /// </summary>
    public interface ITrainingResolutionService
    {
        /// <summary>
        /// CourseVersion + rank -> ordered module versions -> lesson ids.
        /// When <paramref name="crewRank"/> is null, matches no profile, or
        /// <paramref name="forceUnfiltered"/> is set (full-access users), the complete course
        /// sequence is returned with <see cref="ResolvedTrainingSequenceModel.IsUnfiltered"/> true.
        /// </summary>
        Task<ResolvedTrainingSequenceModel?> ResolveAsync(
            Guid courseVersionId,
            CrewRank? crewRank,
            bool forceUnfiltered = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Resolves for a learner and pins the result onto their assignment, so later authoring
        /// and republishing cannot change the training they were given. Idempotent: an assignment
        /// that is already pinned is replayed, not re-resolved.
        /// </summary>
        Task<ResolvedTrainingSequenceModel?> ResolveForLearnerAsync(
            string userId,
            Guid courseId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Same resolution as <see cref="ResolveForLearnerAsync"/> (replays a pinned assignment
        /// exactly, or resolves live and reports what pinning would select) but never writes.
        /// Callers that already hold unsaved changes on their own DbContext — e.g. mid-way through
        /// a progress update — must use this instead of <see cref="ResolveForLearnerAsync"/>, which
        /// pins on a separate connection and would race the caller's own SaveChangesAsync.
        /// </summary>
        Task<ResolvedTrainingSequenceModel?> ResolveForLearnerReadOnlyAsync(
            string userId,
            Guid courseId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Writes ResolvedCourseVersionId / ResolvedRankProfileId / ResolvedOnUtc onto the
        /// assignment. Call at registration time.
        /// </summary>
        Task<OperationResult<ResolvedTrainingSequenceModel>> SnapshotAssignmentAsync(
            Guid userTrainingAssignmentId,
            string? changedByUserId,
            CancellationToken cancellationToken = default);
    }
}
