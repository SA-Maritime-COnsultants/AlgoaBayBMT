using AlgoaBayBMT.Shared.Models;

namespace AlgoaBayBMT.Services.Interfaces
{
    public interface IVesselRoleAssignmentService
    {
        Task<List<VesselRoleAssignment>> GetAssignmentsAsync(CancellationToken cancellationToken = default);
        Task<OperationResult> AssignRoleAsync(int vesselId, string userId, VesselRoleType vesselRoleType, string? assignedByUserId, int? companyId, CancellationToken cancellationToken = default);
        Task<OperationResult> RemoveAssignmentAsync(int assignmentId, CancellationToken cancellationToken = default);
    }
}
