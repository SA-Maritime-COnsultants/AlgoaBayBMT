using AlgoaBayBMT.Data;
using AlgoaBayBMT.Services.Interfaces;
using AlgoaBayBMT.Services.Models;
using AlgoaBayBMT.Shared.Models;
using AlgoaBayBMT.Shared.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AlgoaBayBMT.Services
{
    public class UserManagementService(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager) : IUserManagementService
    {
        public Task<List<ApplicationUser>> GetUsersAsync(string? searchTerm, bool crewOnly = false, CancellationToken cancellationToken = default)
        {
            var query = dbContext.Users
                .AsNoTracking()
                .Include(x => x.Company)
                .Include(x => x.PrimaryArea)
                .Include(x => x.Vessel)
                .AsQueryable();

            if (crewOnly)
            {
                query = query.Where(x => x.IsCrew);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(x =>
                    (x.Email != null && x.Email.Contains(searchTerm)) ||
                    (x.FullName != null && x.FullName.Contains(searchTerm)) ||
                    (x.RequestedRole != null && x.RequestedRole.Contains(searchTerm)));
            }

            return query.OrderBy(x => x.FullName).ThenBy(x => x.Email).ToListAsync(cancellationToken);
        }

        public async Task<UserAdministrationModel?> GetUserEditorAsync(string userId, CancellationToken cancellationToken = default)
        {
            var user = await dbContext.Users
                .AsNoTracking()
                .Include(x => x.Company)
                .Include(x => x.PrimaryArea)
                .Include(x => x.Vessel)
                .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

            if (user is null)
            {
                return null;
            }

            var roles = await userManager.GetRolesAsync(user);
            var assignedRole = roles.FirstOrDefault() ?? user.RequestedRole ?? RoleNames.Crew;

            return new UserAdministrationModel
            {
                UserId = user.Id,
                FullName = user.FullName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                AssignedRole = assignedRole,
                CompanyId = user.CompanyId,
                PrimaryAreaId = user.PrimaryAreaId,
                VesselId = user.VesselId,
                IsActive = user.IsActive,
                IsAccountApproved = user.IsAccountApproved
            };
        }

        public async Task<OperationResult<ApplicationUser>> CreateUserAsync(UserAdministrationModel model, CancellationToken cancellationToken = default)
        {
            var validation = await ValidateModelAsync(model, isEdit: false, cancellationToken);
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
                RequestedRole = model.AssignedRole,
                ApprovalStatus = model.IsAccountApproved ? ApprovalStatus.Approved : ApprovalStatus.PendingAccountApproval,
                IsAccountApproved = model.IsAccountApproved,
                CompanyId = model.CompanyId,
                PrimaryAreaId = model.PrimaryAreaId,
                VesselId = model.VesselId,
                IsActive = model.IsActive,
                RegisteredOnUtc = DateTime.UtcNow,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(user, model.Password!);
            if (!createResult.Succeeded)
            {
                return OperationResult<ApplicationUser>.Failure(createResult.Errors.Select(x => x.Description).ToArray());
            }

            var roleResult = await AssignSingleRoleAsync(user, model.AssignedRole, cancellationToken);
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

            var validation = await ValidateModelAsync(model, isEdit: true, cancellationToken);
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
            user.RequestedRole = model.AssignedRole;
            user.CompanyId = model.CompanyId;
            user.PrimaryAreaId = model.PrimaryAreaId;
            user.VesselId = model.VesselId;
            user.IsActive = model.IsActive;
            user.IsAccountApproved = model.IsAccountApproved;
            user.ApprovalStatus = model.IsAccountApproved ? ApprovalStatus.Approved : ApprovalStatus.PendingAccountApproval;
            user.ApprovedOnUtc = model.IsAccountApproved ? (user.ApprovedOnUtc ?? DateTime.UtcNow) : user.ApprovedOnUtc;

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

            var roleResult = await AssignSingleRoleAsync(user, model.AssignedRole, cancellationToken);
            if (!roleResult.Succeeded)
            {
                return roleResult;
            }

            return OperationResult<ApplicationUser>.Success(user, "User updated.");
        }

        private async Task<OperationResult<ApplicationUser>?> ValidateModelAsync(UserAdministrationModel model, bool isEdit, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(model.FullName))
            {
                return OperationResult<ApplicationUser>.Failure("Full name is required.");
            }

            if (string.IsNullOrWhiteSpace(model.Email))
            {
                return OperationResult<ApplicationUser>.Failure("Email is required.");
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

            if (model.CompanyId.HasValue && !await dbContext.BunkeringCompanies.AnyAsync(x => x.Id == model.CompanyId.Value, cancellationToken))
            {
                return OperationResult<ApplicationUser>.Failure("Selected company was not found.");
            }

            if (model.PrimaryAreaId.HasValue && !await dbContext.OperationalAreas.AnyAsync(x => x.Id == model.PrimaryAreaId.Value, cancellationToken))
            {
                return OperationResult<ApplicationUser>.Failure("Selected operational area was not found.");
            }

            if (model.VesselId.HasValue && !await dbContext.Vessels.AnyAsync(x => x.Id == model.VesselId.Value, cancellationToken))
            {
                return OperationResult<ApplicationUser>.Failure("Selected vessel was not found.");
            }

            return null;
        }

        private async Task<OperationResult<ApplicationUser>> AssignSingleRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
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

        public async Task<OperationResult> UpdateUserAssignmentsAsync(string userId, int? companyId, int? primaryAreaId, int? vesselId, CancellationToken cancellationToken = default)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
            if (user is null)
            {
                return OperationResult.Failure("User not found.");
            }

            user.CompanyId = companyId;
            user.PrimaryAreaId = primaryAreaId;
            user.VesselId = vesselId;
            await dbContext.SaveChangesAsync(cancellationToken);
            return OperationResult.Success("User assignments updated.");
        }
    }
}
