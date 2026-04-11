using AlgoaBayBMT.Data;
using AlgoaBayBMT.Shared.Models;
using AlgoaBayBMT.Services.Models;

namespace AlgoaBayBMT.Services.Interfaces
{
    public interface IUserManagementService
    {
        Task<List<ApplicationUser>> GetUsersAsync(string? searchTerm, bool crewOnly = false, CancellationToken cancellationToken = default);
        Task<UserAdministrationModel?> GetUserEditorAsync(string userId, CancellationToken cancellationToken = default);
        Task<OperationResult<ApplicationUser>> CreateUserAsync(UserAdministrationModel model, CancellationToken cancellationToken = default);
        Task<OperationResult<ApplicationUser>> UpdateUserAsync(UserAdministrationModel model, CancellationToken cancellationToken = default);
        Task<OperationResult> SetUserActivationAsync(string userId, bool isActive, CancellationToken cancellationToken = default);
        Task<OperationResult> SetUserApprovalAsync(string userId, bool isApproved, CancellationToken cancellationToken = default);
        Task<OperationResult> SetEmailConfirmedAsync(string userId, bool emailConfirmed, CancellationToken cancellationToken = default);
        Task<List<string>> GetAvailableRolesAsync(CancellationToken cancellationToken = default);
        Task<List<string>> GetUserRolesAsync(string userId, CancellationToken cancellationToken = default);
        Task<OperationResult> UpdateUserRolesAsync(string userId, IReadOnlyCollection<string> roleNames, CancellationToken cancellationToken = default);
        Task<OperationResult> DeleteUserAsync(string userId, CancellationToken cancellationToken = default);
        Task<OperationResult> UpdateUserAssignmentsAsync(string userId, int? companyId, int? primaryAreaId, int? vesselId, CancellationToken cancellationToken = default);
        Task<Dictionary<string, IReadOnlyList<string>>> GetUserRolesBulkAsync(IReadOnlyList<string> userIds, CancellationToken cancellationToken = default);
        Task<OperationResult<ApplicationUser>> CreateCrewMemberAsync(AddCrewMemberViewModel model, int? companyId, int? vesselId, string approvedByUserId, CancellationToken cancellationToken = default);
    }
}
