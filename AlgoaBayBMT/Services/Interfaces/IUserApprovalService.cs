using AlgoaBayBMT.Data;
using AlgoaBayBMT.Shared.Models;

namespace AlgoaBayBMT.Services.Interfaces
{
    public interface IUserApprovalService
    {
        Task<List<ApplicationUser>> GetPendingUsersAsync(CancellationToken cancellationToken = default);
        Task<OperationResult> ApproveAsync(string userId, string approvedByUserId, string? notes, CancellationToken cancellationToken = default);
        Task<OperationResult> RejectAsync(string userId, string approvedByUserId, string? notes, CancellationToken cancellationToken = default);
    }
}
