using AlgoaBayBMT.Data;
using AlgoaBayBMT.Shared.Models;
using AlgoaBayBMT.Services.Models;

namespace AlgoaBayBMT.Services.Interfaces
{
    public interface IUserManagementService
    {
        Task<List<ApplicationUser>> GetUsersAsync(string? searchTerm, CancellationToken cancellationToken = default);
        Task<UserAdministrationModel?> GetUserEditorAsync(string userId, CancellationToken cancellationToken = default);
        Task<OperationResult<ApplicationUser>> CreateUserAsync(UserAdministrationModel model, CancellationToken cancellationToken = default);
        Task<OperationResult<ApplicationUser>> UpdateUserAsync(UserAdministrationModel model, CancellationToken cancellationToken = default);
        Task<OperationResult> SetUserActivationAsync(string userId, bool isActive, CancellationToken cancellationToken = default);
        Task<OperationResult> UpdateUserAssignmentsAsync(string userId, int? companyId, int? primaryAreaId, int? vesselId, CancellationToken cancellationToken = default);
    }
}
