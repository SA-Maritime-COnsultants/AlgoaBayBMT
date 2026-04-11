using AlgoaBayBMT.Data;
using AlgoaBayBMT.Services.Models;
using AlgoaBayBMT.Shared.Models;

namespace AlgoaBayBMT.Services.Interfaces
{
    public interface ICrewAssignmentService
    {
        Task<List<CrewDeployment>> GetActiveCrewAsync(int vesselId, CancellationToken cancellationToken = default);
        Task<CrewDeployment?> GetCurrentAssignmentAsync(string userId, CancellationToken cancellationToken = default);
        Task<int?> GetCurrentVesselIdAsync(string userId, CancellationToken cancellationToken = default);
        Task<List<ApplicationUser>> SearchUnassignedCrewAsync(string? searchTerm, CancellationToken cancellationToken = default);
        Task<OperationResult<CrewDeployment>> SignOnAsync(SignOnRequest request, CancellationToken cancellationToken = default);
        Task<OperationResult> SignOffAsync(SignOffRequest request, CancellationToken cancellationToken = default);
    }
}
