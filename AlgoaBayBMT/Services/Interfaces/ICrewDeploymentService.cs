using AlgoaBayBMT.Services.Models;
using AlgoaBayBMT.Shared.Models;

namespace AlgoaBayBMT.Services.Interfaces
{
    public interface ICrewDeploymentService
    {
        Task<OperationResult<CrewDeployment>> DeployCrewAsync(CrewDeploymentRequest request, CancellationToken cancellationToken = default);
        Task<OperationResult> EndDeploymentAsync(int deploymentId, DateTime endedOnUtc, CancellationToken cancellationToken = default);
        Task<List<VesselCrewListEntry>> GetCrewListAsync(int vesselId, DateTime? asOfUtc = null, CancellationToken cancellationToken = default);
        Task<List<CrewDeployment>> GetDeploymentHistoryAsync(int vesselId, CancellationToken cancellationToken = default);
    }
}
