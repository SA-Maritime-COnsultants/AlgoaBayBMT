using AlgoaBayBMT.Data;
using AlgoaBayBMT.Shared.Models;
using AlgoaBayBMT.Services.Models;

namespace AlgoaBayBMT.Services.Interfaces
{
    public interface IUserManagementService
    {
        Task<List<ApplicationUser>> GetUsersAsync(string? searchTerm, bool crewOnly = false, CancellationToken cancellationToken = default);
        Task<UserAdministrationModel?> GetUserEditorAsync(string userId, CancellationToken cancellationToken = default);
        Task<UserProfileManageModel?> GetSelfProfileAsync(string userId, CancellationToken cancellationToken = default);
        Task<OperationResult<ApplicationUser>> CreateUserAsync(UserAdministrationModel model, CancellationToken cancellationToken = default);
        Task<OperationResult<ApplicationUser>> UpdateUserAsync(UserAdministrationModel model, CancellationToken cancellationToken = default);
        Task<OperationResult<UserProfileManageModel>> UpdateSelfProfileAsync(string userId, UserProfileManageModel model, CancellationToken cancellationToken = default);
        Task<OperationResult> SetUserActivationAsync(string userId, bool isActive, CancellationToken cancellationToken = default);
        Task<OperationResult> SetUserApprovalAsync(string userId, bool isApproved, CancellationToken cancellationToken = default);
        Task<OperationResult> SetEmailConfirmedAsync(string userId, bool emailConfirmed, CancellationToken cancellationToken = default);
        Task<List<string>> GetAvailableRolesAsync(CancellationToken cancellationToken = default);
        Task<List<string>> GetUserRolesAsync(string userId, CancellationToken cancellationToken = default);
        Task<OperationResult> UpdateUserRolesAsync(string userId, IReadOnlyCollection<string> roleNames, CancellationToken cancellationToken = default);
        Task<OperationResult> DeleteUserAsync(string userId, CancellationToken cancellationToken = default);
        Task<Dictionary<string, IReadOnlyList<string>>> GetUserRolesBulkAsync(IReadOnlyList<string> userIds, CancellationToken cancellationToken = default);

        /// <summary>Assigns a user to a BunkerOperator company and (optionally) the COMPANY_MANAGER role.</summary>
        Task<OperationResult> AssignCompanyAsync(string userId, int? companyId, bool grantCompanyManagerRole, CancellationToken cancellationToken = default);

        /// <summary>Toggles the IsCrewManager flag on a user.</summary>
        Task<OperationResult> SetCrewManagerAsync(string userId, bool isCrewManager, CancellationToken cancellationToken = default);

        /// <summary>Returns users belonging to a given company (by CompanyId).</summary>
        Task<List<ApplicationUser>> GetUsersByCompanyAsync(int companyId, CancellationToken cancellationToken = default);
    }
}
