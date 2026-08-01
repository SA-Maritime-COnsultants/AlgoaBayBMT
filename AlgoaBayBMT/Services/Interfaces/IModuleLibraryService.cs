using AlgoaBayBMT.Services.Models;
using AlgoaBayBMT.Shared.Models;

namespace AlgoaBayBMT.Services.Interfaces
{
    /// <summary>
    /// Owns shared modules: their stable identity, their authored versions, and the version
    /// lifecycle. A module is authored once here and referenced by many courses.
    /// </summary>
    public interface IModuleLibraryService
    {
        /// <summary>Backs the Module Library panel and page. Search and category are optional filters.</summary>
        Task<List<ModuleLibraryItemModel>> SearchModulesAsync(
            string? query = null,
            string? category = null,
            bool includeArchived = false,
            CancellationToken cancellationToken = default);

        Task<List<string>> GetCategoriesAsync(CancellationToken cancellationToken = default);

        Task<ModuleLibraryItemModel?> GetModuleAsync(Guid moduleId, CancellationToken cancellationToken = default);

        Task<TrainingModuleEditModel?> GetModuleVersionAsync(Guid moduleVersionId, CancellationToken cancellationToken = default);

        Task<OperationResult<ModuleLibraryItemModel>> SaveModuleAsync(ModuleLibraryItemModel model, string? changedByUserId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates the next draft version by deep-copying the source version's lessons and content
        /// blocks with new identifiers. New ids are correct here: the copy is new content, while
        /// the source version keeps the ids that existing learner progress references.
        /// </summary>
        Task<OperationResult<TrainingModuleEditModel>> CreateModuleVersionAsync(Guid moduleId, string? changedByUserId, CancellationToken cancellationToken = default);

        /// <summary>Freezes the version. Its lessons and blocks become uneditable from this point.</summary>
        Task<OperationResult> PublishModuleVersionAsync(Guid moduleVersionId, string? changedByUserId, CancellationToken cancellationToken = default);

        /// <summary>Archive rather than delete whenever a course or learner evidence refers to it.</summary>
        Task<OperationResult> ArchiveModuleAsync(Guid moduleId, bool archived, string? changedByUserId, CancellationToken cancellationToken = default);

        /// <summary>Hard delete. Refused when any course references it or learner evidence exists.</summary>
        Task<OperationResult> DeleteModuleAsync(Guid moduleId, string? changedByUserId, CancellationToken cancellationToken = default);

        Task<ModuleUsageModel> GetModuleUsageAsync(Guid moduleId, CancellationToken cancellationToken = default);
    }
}
