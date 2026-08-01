using AlgoaBayBMT.Services.Models;
using AlgoaBayBMT.Shared.Models;

namespace AlgoaBayBMT.Services.Interfaces
{
    /// <summary>
    /// Composes a course version out of shared modules and rank profiles, validates it, and
    /// publishes it. Rank configuration is written only through this service — never near
    /// SaveCourseAsync, which rebuilds CourseAudienceRules and would wipe it.
    /// </summary>
    public interface ICourseCompositionService
    {
        /// <summary>Loads everything the four Course Builder steps need in one call.</summary>
        Task<CourseCompositionModel?> GetCompositionAsync(Guid courseId, CancellationToken cancellationToken = default);

        /// <summary>Adds a reference to a shared module version. Copies no content.</summary>
        Task<OperationResult> AddModuleToCourseAsync(Guid courseVersionId, Guid moduleVersionId, string? changedByUserId, CancellationToken cancellationToken = default);

        Task<OperationResult> RemoveModuleFromCourseAsync(Guid courseModuleId, string? changedByUserId, CancellationToken cancellationToken = default);

        /// <summary>Positional reorder, taking the full ordered list as shown after a drag.</summary>
        Task<OperationResult> ReorderCourseModulesAsync(Guid courseVersionId, IReadOnlyList<Guid> orderedCourseModuleIds, string? changedByUserId, CancellationToken cancellationToken = default);

        /// <summary>Sets which rank profiles the course serves. Deselecting drops its inclusions.</summary>
        Task<OperationResult> SetCourseRankProfilesAsync(Guid courseVersionId, IReadOnlyList<Guid> rankProfileIds, string? changedByUserId, CancellationToken cancellationToken = default);

        /// <summary>Ticks or unticks one cell of the rank x module matrix in step 3.</summary>
        Task<OperationResult> SetRankModuleInclusionAsync(Guid courseRankProfileId, Guid courseModuleId, bool isIncluded, string? changedByUserId, CancellationToken cancellationToken = default);

        /// <summary>Runs the seven publish rules and computes per-rank duration.</summary>
        Task<PublishValidationResultModel> ValidateForPublishAsync(Guid courseVersionId, CancellationToken cancellationToken = default);

        /// <summary>Validates, then publishes inside one transaction with concurrency protection.</summary>
        Task<OperationResult> PublishAsync(Guid courseId, Guid courseVersionId, string? changedByUserId, CancellationToken cancellationToken = default);

        /// <summary>The per-rank sequences shown in Review &amp; Publish, with exact version references.</summary>
        Task<List<ResolvedTrainingSequenceModel>> GetRankSequencesAsync(Guid courseVersionId, CancellationToken cancellationToken = default);
    }
}
