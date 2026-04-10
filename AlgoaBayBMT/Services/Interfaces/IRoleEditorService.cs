using AlgoaBayBMT.Services.Models;
using AlgoaBayBMT.Shared.Models;

namespace AlgoaBayBMT.Services.Interfaces
{
    public interface IRoleEditorService
    {
        Task<List<RoleEditorRowModel>> GetRolesAsync();
        Task<OperationResult> UpdateRoleAsync(string roleId, string newName);
        Task<OperationResult> DeleteRoleAsync(string roleId);
    }
}
