using AlgoaBayBMT.Data;
using AlgoaBayBMT.Services.Interfaces;
using AlgoaBayBMT.Services.Models;
using AlgoaBayBMT.Shared.Models;
using AlgoaBayBMT.Shared.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace AlgoaBayBMT.Services
{
    public class UserManagementService(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager) : IUserManagementService
    {
        private static readonly Regex SidNumberRegex = new("^[A-Z0-9-]{6,20}$", RegexOptions.Compiled, TimeSpan.FromSeconds(1));

        public Task<List<ApplicationUser>> GetUsersAsync(string? searchTerm, bool crewOnly = false, CancellationToken cancellationToken = default)
        {
            var query = dbContext.Users.AsNoTracking().AsQueryable();

            if (crewOnly)
            {
                query = query.Where(x => x.IsCrew);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(x =>
                    (x.Email != null && x.Email.Contains(searchTerm)) ||
                    (x.FullName != null && x.FullName.Contains(searchTerm)) ||
                    (x.RequestedRole != null && x.RequestedRole.Contains(searchTerm)) ||
                    (x.Country != null && x.Country.Contains(searchTerm)) ||
                    (x.CellNo != null && x.CellNo.Contains(searchTerm)) ||
                    (x.SidNumber != null && x.SidNumber.Contains(searchTerm)));
            }

            return query.OrderBy(x => x.FullName).ThenBy(x => x.Email).ToListAsync(cancellationToken);
        }

        public async Task<UserAdministrationModel?> GetUserEditorAsync(string userId, CancellationToken cancellationToken = default)
        {
            var user = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
            if (user is null)
            {
                return null;
            }

            var roles = await userManager.GetRolesAsync(user);
            var assignedRole = roles.FirstOrDefault() ?? user.RequestedRole ?? RoleNames.Customer;

            return new UserAdministrationModel
            {
                UserId = user.Id,
                FullName = user.FullName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                CellNo = user.CellNo,
                Address = user.Address,
                Country = user.Country,
                AssignedRole = assignedRole,
                IsCrew = user.IsCrew,
                CrewRank = user.CrewRank,
                SidNumber = user.SidNumber,
                SidIssuingCountry = user.SidIssuingCountry,
                SidIssuingAuthority = user.SidIssuingAuthority,
                SidIssueDate = user.SidIssueDate,
                SidExpiryDate = user.SidExpiryDate,
                IsActive = user.IsActive,
                IsAccountApproved = user.IsAccountApproved,
                EmailConfirmed = user.EmailConfirmed,
                CompanyId = user.CompanyId,
                IsCrewManager = user.IsCrewManager,
                IsBunkerManager = user.IsBunkerManager
            };
        }

        public async Task<UserProfileManageModel?> GetSelfProfileAsync(string userId, CancellationToken cancellationToken = default)
        {
            var user = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
            return user is null ? null : MapSelfProfile(user);
        }

        public async Task<OperationResult<ApplicationUser>> CreateUserAsync(UserAdministrationModel model, CancellationToken cancellationToken = default)
        {
            var validation = await ValidateModelAsync(model, isEdit: false);
            if (validation is not null)
            {
                return validation;
            }

            if (await userManager.FindByEmailAsync(model.Email) is not null)
            {
                return OperationResult<ApplicationUser>.Failure("A user with this email already exists.");
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                CellNo = model.CellNo?.Trim(),
                Address = model.Address?.Trim(),
                Country = model.Country?.Trim(),
                RequestedRole = model.AssignedRole,
                ApprovalStatus = model.IsAccountApproved ? ApprovalStatus.Approved : ApprovalStatus.PendingAccountApproval,
                IsAccountApproved = model.IsAccountApproved,
                IsCrew = model.IsCrew,
                CrewRank = model.IsCrew ? model.CrewRank : null,
                SidNumber = model.IsCrew ? NormalizeSidNumber(model.SidNumber) : null,
                SidIssuingCountry = model.IsCrew ? model.SidIssuingCountry?.Trim() : null,
                SidIssuingAuthority = model.IsCrew ? model.SidIssuingAuthority?.Trim() : null,
                SidIssueDate = model.IsCrew ? model.SidIssueDate : null,
                SidExpiryDate = model.IsCrew ? model.SidExpiryDate : null,
                IsActive = model.IsActive,
                RegisteredOnUtc = DateTime.UtcNow,
                EmailConfirmed = model.EmailConfirmed
            };

            var createResult = await userManager.CreateAsync(user, model.Password!);
            if (!createResult.Succeeded)
            {
                return OperationResult<ApplicationUser>.Failure(createResult.Errors.Select(x => x.Description).ToArray());
            }

            var roleResult = await AssignSingleRoleAsync(user, model.AssignedRole);
            if (!roleResult.Succeeded)
            {
                return roleResult;
            }

            return OperationResult<ApplicationUser>.Success(user, "User created and role assigned.");
        }

        public async Task<OperationResult<ApplicationUser>> UpdateUserAsync(UserAdministrationModel model, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(model.UserId))
            {
                return OperationResult<ApplicationUser>.Failure("User id is required.");
            }

            var validation = await ValidateModelAsync(model, isEdit: true);
            if (validation is not null)
            {
                return validation;
            }

            var user = await userManager.FindByIdAsync(model.UserId);
            if (user is null)
            {
                return OperationResult<ApplicationUser>.Failure("User not found.");
            }

            var existingUser = await userManager.FindByEmailAsync(model.Email);
            if (existingUser is not null && existingUser.Id != user.Id)
            {
                return OperationResult<ApplicationUser>.Failure("A user with this email already exists.");
            }

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.UserName = model.Email;
            user.CellNo = model.CellNo?.Trim();
            user.Address = model.Address?.Trim();
            user.Country = model.Country?.Trim();
            user.RequestedRole = model.AssignedRole;
            user.IsCrew = model.IsCrew;
            user.CrewRank = model.IsCrew ? model.CrewRank : null;
            user.SidNumber = model.IsCrew ? NormalizeSidNumber(model.SidNumber) : null;
            user.SidIssuingCountry = model.IsCrew ? model.SidIssuingCountry?.Trim() : null;
            user.SidIssuingAuthority = model.IsCrew ? model.SidIssuingAuthority?.Trim() : null;
            user.SidIssueDate = model.IsCrew ? model.SidIssueDate : null;
            user.SidExpiryDate = model.IsCrew ? model.SidExpiryDate : null;
            user.IsActive = model.IsActive;
            user.IsAccountApproved = model.IsAccountApproved;
            user.EmailConfirmed = model.EmailConfirmed;
            user.ApprovalStatus = model.IsAccountApproved ? ApprovalStatus.Approved : ApprovalStatus.PendingAccountApproval;
            user.ApprovedOnUtc = model.IsAccountApproved ? (user.ApprovedOnUtc ?? DateTime.UtcNow) : user.ApprovedOnUtc;
            user.IsCrewManager = model.IsCrewManager;
            user.IsBunkerManager = model.IsBunkerManager;

            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return OperationResult<ApplicationUser>.Failure(updateResult.Errors.Select(x => x.Description).ToArray());
            }

            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                if (await userManager.HasPasswordAsync(user))
                {
                    var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
                    var passwordResetResult = await userManager.ResetPasswordAsync(user, resetToken, model.Password);
                    if (!passwordResetResult.Succeeded)
                    {
                        return OperationResult<ApplicationUser>.Failure(passwordResetResult.Errors.Select(x => x.Description).ToArray());
                    }
                }
                else
                {
                    var passwordAddResult = await userManager.AddPasswordAsync(user, model.Password);
                    if (!passwordAddResult.Succeeded)
                    {
                        return OperationResult<ApplicationUser>.Failure(passwordAddResult.Errors.Select(x => x.Description).ToArray());
                    }
                }
            }

            var roleResult = await AssignSingleRoleAsync(user, model.AssignedRole);
            if (!roleResult.Succeeded)
            {
                return roleResult;
            }

            return OperationResult<ApplicationUser>.Success(user, "User updated.");
        }

        public async Task<OperationResult<UserProfileManageModel>> UpdateSelfProfileAsync(
            string userId,
            UserProfileManageModel model,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return OperationResult<UserProfileManageModel>.Failure("User id is required.");
            }

            if (string.IsNullOrWhiteSpace(model.FullName))
            {
                return OperationResult<UserProfileManageModel>.Failure("Full name is required.");
            }

            if (model.ProfilePicture is not null)
            {
                if (string.IsNullOrWhiteSpace(model.ProfilePictureContentType) ||
                    !model.ProfilePictureContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                {
                    return OperationResult<UserProfileManageModel>.Failure("Profile picture must be a supported image.");
                }

                if (model.ProfilePicture.Length > 2 * 1024 * 1024)
                {
                    return OperationResult<UserProfileManageModel>.Failure("Profile picture must be 2 MB or smaller.");
                }
            }

            var normalizedSidNumber = NormalizeSidNumber(model.SidNumber);
            if (!string.IsNullOrWhiteSpace(normalizedSidNumber) && !SidNumberRegex.IsMatch(normalizedSidNumber))
            {
                return OperationResult<UserProfileManageModel>.Failure("SID must be 6–20 characters and may contain only letters, numbers, and hyphens.");
            }

            var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
            if (user is null)
            {
                return OperationResult<UserProfileManageModel>.Failure("User not found.");
            }

            user.FullName = model.FullName.Trim();
            user.CellNo = model.CellNo?.Trim();
            user.Address = model.Address?.Trim();
            user.Country = model.Country?.Trim();
            user.Qualification = model.Qualification;
            user.CrewRank = model.Qualification?.ToCrewRank();
            user.SidNumber = normalizedSidNumber;
            user.SidIssuingCountry = model.SidIssuingCountry?.Trim();
            user.SidIssuingAuthority = model.SidIssuingAuthority?.Trim();
            user.SidIssueDate = model.SidIssueDate;
            user.SidExpiryDate = model.SidExpiryDate;

            if (model.ProfilePicture is not null)
            {
                user.ProfilePicture = model.ProfilePicture;
                user.ProfilePictureContentType = model.ProfilePictureContentType?.Trim();
            }

            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return OperationResult<UserProfileManageModel>.Failure(updateResult.Errors.Select(x => x.Description).ToArray());
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            return OperationResult<UserProfileManageModel>.Success(MapSelfProfile(user), "Profile updated.");
        }

        private async Task<OperationResult<ApplicationUser>?> ValidateModelAsync(UserAdministrationModel model, bool isEdit)
        {
            if (string.IsNullOrWhiteSpace(model.FullName))
            {
                return OperationResult<ApplicationUser>.Failure("Full name is required.");
            }

            if (string.IsNullOrWhiteSpace(model.Email))
            {
                return OperationResult<ApplicationUser>.Failure("Email is required.");
            }

            if (string.IsNullOrWhiteSpace(model.CellNo))
            {
                return OperationResult<ApplicationUser>.Failure("Cell No is required.");
            }

            if (string.IsNullOrWhiteSpace(model.Address))
            {
                return OperationResult<ApplicationUser>.Failure("Address is required.");
            }

            if (string.IsNullOrWhiteSpace(model.Country))
            {
                return OperationResult<ApplicationUser>.Failure("Country is required.");
            }

            if (string.IsNullOrWhiteSpace(model.AssignedRole))
            {
                return OperationResult<ApplicationUser>.Failure("A role must be selected.");
            }

            if (!await roleManager.RoleExistsAsync(model.AssignedRole))
            {
                return OperationResult<ApplicationUser>.Failure("The selected role does not exist.");
            }

            if (!isEdit)
            {
                if (string.IsNullOrWhiteSpace(model.Password))
                {
                    return OperationResult<ApplicationUser>.Failure("Password is required for new users.");
                }

                if (!string.Equals(model.Password, model.ConfirmPassword, StringComparison.Ordinal))
                {
                    return OperationResult<ApplicationUser>.Failure("Password confirmation does not match.");
                }
            }

            if (model.IsCrew)
            {
                if (model.CrewRank is null)
                {
                    return OperationResult<ApplicationUser>.Failure("Crew rank is required for crew users.");
                }

                var normalizedSidNumber = NormalizeSidNumber(model.SidNumber);
                if (string.IsNullOrWhiteSpace(normalizedSidNumber) || !SidNumberRegex.IsMatch(normalizedSidNumber))
                {
                    return OperationResult<ApplicationUser>.Failure("SID must be 6–20 characters and may contain only letters, numbers, and hyphens.");
                }

                if (string.IsNullOrWhiteSpace(model.SidIssuingCountry))
                {
                    return OperationResult<ApplicationUser>.Failure("SID issuing country is required for crew users.");
                }

                if (string.IsNullOrWhiteSpace(model.SidIssuingAuthority))
                {
                    return OperationResult<ApplicationUser>.Failure("SID issuing authority is required for crew users.");
                }
            }

            return null;
        }

        private async Task<OperationResult<ApplicationUser>> AssignSingleRoleAsync(ApplicationUser user, string roleName)
        {
            var currentRoles = await userManager.GetRolesAsync(user);
            if (currentRoles.Count > 0)
            {
                var removeResult = await userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                {
                    return OperationResult<ApplicationUser>.Failure(removeResult.Errors.Select(x => x.Description).ToArray());
                }
            }

            var addResult = await userManager.AddToRoleAsync(user, roleName);
            if (!addResult.Succeeded)
            {
                return OperationResult<ApplicationUser>.Failure(addResult.Errors.Select(x => x.Description).ToArray());
            }

            return OperationResult<ApplicationUser>.Success(user, "Role assigned.");
        }

        public async Task<OperationResult> SetUserActivationAsync(string userId, bool isActive, CancellationToken cancellationToken = default)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
            if (user is null)
            {
                return OperationResult.Failure("User not found.");
            }

            user.IsActive = isActive;
            await dbContext.SaveChangesAsync(cancellationToken);
            return OperationResult.Success("User activation updated.");
        }

        public async Task<OperationResult> SetUserApprovalAsync(string userId, bool isApproved, CancellationToken cancellationToken = default)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
            if (user is null)
            {
                return OperationResult.Failure("User not found.");
            }

            user.IsAccountApproved = isApproved;
            user.ApprovalStatus = isApproved ? ApprovalStatus.Approved : ApprovalStatus.PendingAccountApproval;
            user.ApprovedOnUtc = isApproved ? (user.ApprovedOnUtc ?? DateTime.UtcNow) : user.ApprovedOnUtc;

            await dbContext.SaveChangesAsync(cancellationToken);
            return OperationResult.Success("User approval updated.");
        }

        public async Task<OperationResult> SetEmailConfirmedAsync(string userId, bool emailConfirmed, CancellationToken cancellationToken = default)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return OperationResult.Failure("User not found.");
            }

            user.EmailConfirmed = emailConfirmed;
            var result = await userManager.UpdateAsync(user);
            return result.Succeeded
                ? OperationResult.Success("Email confirmation updated.")
                : OperationResult.Failure(result.Errors.Select(x => x.Description).ToArray());
        }

        public Task<List<string>> GetAvailableRolesAsync(CancellationToken cancellationToken = default) =>
            roleManager.Roles.OrderBy(x => x.Name).Select(x => x.Name!).ToListAsync(cancellationToken);

        public async Task<List<string>> GetUserRolesAsync(string userId, CancellationToken cancellationToken = default)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return [];
            }

            return (await userManager.GetRolesAsync(user)).OrderBy(x => x).ToList();
        }

        public async Task<OperationResult> UpdateUserRolesAsync(string userId, IReadOnlyCollection<string> roleNames, CancellationToken cancellationToken = default)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return OperationResult.Failure("User not found.");
            }

            var distinctRoles = (roleNames ?? Array.Empty<string>())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            if (distinctRoles.Length == 0)
            {
                return OperationResult.Failure("Select at least one role.");
            }

            foreach (var roleName in distinctRoles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    return OperationResult.Failure($"Role '{roleName}' does not exist.");
                }
            }

            var currentRoles = await userManager.GetRolesAsync(user);
            var rolesToRemove = currentRoles.Except(distinctRoles, StringComparer.Ordinal).ToArray();
            var rolesToAdd = distinctRoles.Except(currentRoles, StringComparer.Ordinal).ToArray();

            if (rolesToRemove.Length > 0)
            {
                var removeResult = await userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removeResult.Succeeded)
                {
                    return OperationResult.Failure(removeResult.Errors.Select(x => x.Description).ToArray());
                }
            }

            if (rolesToAdd.Length > 0)
            {
                var addResult = await userManager.AddToRolesAsync(user, rolesToAdd);
                if (!addResult.Succeeded)
                {
                    return OperationResult.Failure(addResult.Errors.Select(x => x.Description).ToArray());
                }
            }

            user.RequestedRole = distinctRoles[0];
            var updateResult = await userManager.UpdateAsync(user);
            return updateResult.Succeeded
                ? OperationResult.Success("User roles updated.")
                : OperationResult.Failure(updateResult.Errors.Select(x => x.Description).ToArray());
        }

        public async Task<OperationResult> DeleteUserAsync(string userId, CancellationToken cancellationToken = default)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return OperationResult.Failure("User not found.");
            }

            if (user.IsActive)
            {
                return OperationResult.Failure("Only deactivated users can be permanently deleted.");
            }

            try
            {
                var result = await userManager.DeleteAsync(user);
                return result.Succeeded
                    ? OperationResult.Success("User permanently deleted.")
                    : OperationResult.Failure(result.Errors.Select(x => x.Description).ToArray());
            }
            catch (DbUpdateException)
            {
                return OperationResult.Failure("This user could not be deleted because related records still exist. Remove or reassign dependent data first.");
            }
        }

        private static string? NormalizeSidNumber(string? sidNumber)
        {
            if (string.IsNullOrWhiteSpace(sidNumber))
            {
                return null;
            }

            var normalized = sidNumber.Trim().ToUpperInvariant();
            return normalized.Any(char.IsWhiteSpace) ? null : normalized;
        }

        public async Task<Dictionary<string, IReadOnlyList<string>>> GetUserRolesBulkAsync(
            IReadOnlyList<string> userIds,
            CancellationToken cancellationToken = default)
        {
            if (userIds.Count == 0)
            {
                return new Dictionary<string, IReadOnlyList<string>>();
            }

            var rows = await dbContext.UserRoles
                .Where(ur => userIds.Contains(ur.UserId))
                .Join(dbContext.Roles,
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => new { ur.UserId, r.Name })
                .ToListAsync(cancellationToken);

            return rows
                .GroupBy(x => x.UserId)
                .ToDictionary(
                    g => g.Key,
                    g => (IReadOnlyList<string>)g.Select(x => x.Name!).OrderBy(n => n).ToList());
        }

        private static UserProfileManageModel MapSelfProfile(ApplicationUser user) => new()
        {
            UserId = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            EmailConfirmed = user.EmailConfirmed,
            IsCrew = user.IsCrew,
            FullName = user.FullName ?? string.Empty,
            CellNo = user.CellNo,
            Address = user.Address,
            Country = user.Country,
            Qualification = user.Qualification,
            SidNumber = user.SidNumber,
            SidIssuingCountry = user.SidIssuingCountry,
            SidIssuingAuthority = user.SidIssuingAuthority,
            SidIssueDate = user.SidIssueDate,
            SidExpiryDate = user.SidExpiryDate,
            ProfilePicture = user.ProfilePicture,
            ProfilePictureContentType = user.ProfilePictureContentType
        };

    // ── Company / CrewManager helpers ─────────────────────────────────────────

    public async Task<OperationResult> AssignCompanyAsync(
        string userId, int? companyId, bool grantCompanyManagerRole,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return OperationResult.Failure("User not found.");

        user.CompanyId = companyId;
        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            return OperationResult.Failure(updateResult.Errors.Select(e => e.Description).ToArray());

        if (grantCompanyManagerRole && companyId.HasValue)
        {
            if (!await userManager.IsInRoleAsync(user, RoleNames.CompanyManager))
                await userManager.AddToRoleAsync(user, RoleNames.CompanyManager);
        }
        else if (!companyId.HasValue)
        {
            // Removing company assignment also strips the role
            if (await userManager.IsInRoleAsync(user, RoleNames.CompanyManager))
                await userManager.RemoveFromRoleAsync(user, RoleNames.CompanyManager);
        }

        return OperationResult.Success();
    }

    public async Task<OperationResult> SetCrewManagerAsync(
        string userId, bool isCrewManager,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return OperationResult.Failure("User not found.");

        user.IsCrewManager = isCrewManager;
        var result = await userManager.UpdateAsync(user);
        return result.Succeeded
            ? OperationResult.Success()
            : OperationResult.Failure(result.Errors.Select(e => e.Description).ToArray());
    }

    public async Task<OperationResult> SetBunkerManagerAsync(
        string userId, bool isBunkerManager,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return OperationResult.Failure("User not found.");

        user.IsBunkerManager = isBunkerManager;
        var result = await userManager.UpdateAsync(user);
        return result.Succeeded
            ? OperationResult.Success()
            : OperationResult.Failure(result.Errors.Select(e => e.Description).ToArray());
    }

    public Task<List<ApplicationUser>> GetUsersByCompanyAsync(int companyId, CancellationToken cancellationToken = default)
        => dbContext.Users.AsNoTracking()
            .Where(x => x.CompanyId == companyId && x.IsActive)
            .OrderBy(x => x.FullName)
            .ToListAsync(cancellationToken);
    }
}
