using AlgoaBayBMT.Data;
using AlgoaBayBMT.Services.Interfaces;
using AlgoaBayBMT.Services.Models;
using AlgoaBayBMT.Shared.Models;
using Microsoft.AspNetCore.Identity;

namespace AlgoaBayBMT.Services
{
    public class RoleEditorService(
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager) : IRoleEditorService
    {
        public async Task<List<RoleEditorRowModel>> GetRolesAsync()
        {
            var roles = roleManager.Roles.OrderBy(r => r.Name).ToList();
            var result = new List<RoleEditorRowModel>(roles.Count);

            foreach (var role in roles)
            {
                var users = await userManager.GetUsersInRoleAsync(role.Name!);
                result.Add(new RoleEditorRowModel
                {
                    RoleId = role.Id,
                    RoleName = role.Name ?? string.Empty,
                    NormalizedName = role.NormalizedName ?? string.Empty,
                    UserCount = users.Count
                });
            }

            return result;
        }

        public async Task<OperationResult> UpdateRoleAsync(string roleId, string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
                return OperationResult.Failure("Role name cannot be empty.");

            var role = await roleManager.FindByIdAsync(roleId);
            if (role is null)
                return OperationResult.Failure("Role not found.");

            role.Name = newName.Trim();
            var result = await roleManager.UpdateAsync(role);

            return result.Succeeded
                ? OperationResult.Success("Role updated successfully.")
                : OperationResult.Failure(result.Errors.Select(e => e.Description).ToArray());
        }

        public async Task<OperationResult> DeleteRoleAsync(string roleId)
        {
            var role = await roleManager.FindByIdAsync(roleId);
            if (role is null)
                return OperationResult.Failure("Role not found.");

            var usersInRole = await userManager.GetUsersInRoleAsync(role.Name!);
            if (usersInRole.Count > 0)
                return OperationResult.Failure($"Cannot delete '{role.Name}': {usersInRole.Count} user(s) are still assigned to this role.");

            var result = await roleManager.DeleteAsync(role);

            return result.Succeeded
                ? OperationResult.Success("Role deleted successfully.")
                : OperationResult.Failure(result.Errors.Select(e => e.Description).ToArray());
        }
    }
}
