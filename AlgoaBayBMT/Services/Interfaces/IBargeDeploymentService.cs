using AlgoaBayBMT.Shared.Models;

namespace AlgoaBayBMT.Services.Interfaces
{
    public interface IBargeDeploymentService
    {
        Task<List<BargeDeployment>> GetActiveByAreaAsync(int areaId);
        Task<List<BargeDeployment>> GetByBargeAsync(int bargeId);
        Task<BargeDeployment> DeployAsync(int bargeId, int areaId, DateTime from, string? notes, string performedBy);
        Task EndDeploymentAsync(int deploymentId, DateTime to, string performedBy);
        Task<List<BargeDeploymentAudit>> GetAuditTrailAsync(int deploymentId);
    }
}
