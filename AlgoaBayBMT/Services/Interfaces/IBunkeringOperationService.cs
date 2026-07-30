using AlgoaBayBMT.Shared.Models;

namespace AlgoaBayBMT.Services.Interfaces
{
    public interface IBunkeringOperationService
    {
        Task<List<BunkeringOperation>> GetOperationsForUserAsync(string userId, bool isAdmin, bool isCompanyManager, int? companyId, CancellationToken cancellationToken = default);
        Task<Vessel?> GetAssignedBunkerVesselAsync(string userId, bool isAdmin, bool isCompanyManager, int? companyId, CancellationToken cancellationToken = default);
        Task<List<BunkerFuel>> GetFuelsAsync(CancellationToken cancellationToken = default);
        Task<OperationResult<BunkeringOperation>> SaveOperationAsync(BunkeringOperation operation, IReadOnlyCollection<BunkeringOperationPumpingInterval> intervals, IReadOnlyCollection<ISGOTTChecklistStageResponse> stageResponses, CancellationToken cancellationToken = default);
    }
}
