using AlgoaBayBMT.Data;
using AlgoaBayBMT.Services.Interfaces;
using AlgoaBayBMT.Services.Models;
using AlgoaBayBMT.Shared.Models;
using AlgoaBayBMT.Shared.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
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
            var query = dbContext.Users
                .AsNoTracking()
                .Include(x => x.Company)
                .Include(x => x.CrewMemberDetails)
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
                    (x.RequestedRole != null && x.RequestedRole.Contains(searchTerm)) ||
                    (x.Country != null && x.Country.Contains(searchTerm)) ||
                    (x.CellNo != null && x.CellNo.Contains(searchTerm)) ||
                    (x.SidNumber != null && x.SidNumber.Contains(searchTerm)) ||
                    (x.CrewMemberDetails != null && x.CrewMemberDetails.PassportNumber != null && x.CrewMemberDetails.PassportNumber.Contains(searchTerm)));
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
                CompanyId = user.CompanyId,
                PrimaryAreaId = user.PrimaryAreaId,
                VesselId = user.VesselId,
                IsActive = user.IsActive,
                IsAccountApproved = user.IsAccountApproved,
                EmailConfirmed = user.EmailConfirmed
            };
        }

        public async Task<UserProfileManageModel?> GetSelfProfileAsync(string userId, CancellationToken cancellationToken = default)
        {
            var user = await dbContext.Users
                .AsNoTracking()
                .Include(x => x.CrewMemberDetails)
                .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

            return user is null ? null : MapSelfProfile(user);
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
                CompanyId = model.CompanyId,
                PrimaryAreaId = model.PrimaryAreaId,
                VesselId = model.VesselId,
                IsActive = model.IsActive,
                RegisteredOnUtc = DateTime.UtcNow,
                EmailConfirmed = model.EmailConfirmed
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
            user.CompanyId = model.CompanyId;
            user.PrimaryAreaId = model.PrimaryAreaId;
            user.VesselId = model.VesselId;
            user.IsActive = model.IsActive;
            user.IsAccountApproved = model.IsAccountApproved;
            user.EmailConfirmed = model.EmailConfirmed;
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

            var user = await dbContext.Users
                .Include(x => x.CrewMemberDetails)
                .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

            if (user is null)
            {
                return OperationResult<UserProfileManageModel>.Failure("User not found.");
            }

            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

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

            if (user.IsCrew)
            {
                user.CrewMemberDetails ??= new CrewMemberDetails
                {
                    UserId = user.Id,
                    CreatedOnUtc = DateTime.UtcNow
                };

                user.CrewMemberDetails.GivenNames = model.GivenNames?.Trim();
                user.CrewMemberDetails.Gender = model.Gender?.Trim();
                user.CrewMemberDetails.DateOfBirth = model.DateOfBirth;
                user.CrewMemberDetails.PlaceOfBirth = model.PlaceOfBirth?.Trim();
                user.CrewMemberDetails.Nationality = model.Nationality?.Trim();
                user.CrewMemberDetails.PassportNumber = model.PassportNumber?.Trim();
                user.CrewMemberDetails.PassportExpiry = model.PassportExpiry;
                user.CrewMemberDetails.ModifiedOnUtc = DateTime.UtcNow;
            }

            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                await transaction.RollbackAsync(cancellationToken);
                return OperationResult<UserProfileManageModel>.Failure(updateResult.Errors.Select(x => x.Description).ToArray());
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return OperationResult<UserProfileManageModel>.Success(MapSelfProfile(user), "Profile updated.");
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

            var distinctRoles = roleNames.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal).ToArray();
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

        public async Task<OperationResult<ApplicationUser>> CreateCrewMemberAsync(
            AddCrewMemberViewModel model,
            int? companyId,
            int? vesselId,
            string approvedByUserId,
            CancellationToken cancellationToken = default)
        {
            var validationContext = new ValidationContext(model);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(model, validationContext, validationResults, true))
            {
                return OperationResult<ApplicationUser>.Failure(validationResults.Select(x => x.ErrorMessage ?? "Validation error.").ToArray());
            }

            var email = model.Email.Trim();
            var sidNumber = NormalizeSidNumber(model.SidNumber);

            if (await userManager.FindByEmailAsync(email) is not null)
            {
                return OperationResult<ApplicationUser>.Failure("A user with this email already exists.");
            }

            if (!string.IsNullOrWhiteSpace(sidNumber) && await dbContext.Users.AnyAsync(x => x.SidNumber == sidNumber, cancellationToken))
            {
                return OperationResult<ApplicationUser>.Failure("A crew member with this SID number already exists.");
            }

            if (!await roleManager.RoleExistsAsync(RoleNames.Crew))
            {
                return OperationResult<ApplicationUser>.Failure("The CREW role is not configured in the system.");
            }

            if (companyId.HasValue && !await dbContext.BunkeringCompanies.AnyAsync(x => x.Id == companyId.Value, cancellationToken))
            {
                return OperationResult<ApplicationUser>.Failure("The selected vessel company was not found.");
            }

            if (vesselId.HasValue && !await dbContext.Vessels.AnyAsync(x => x.Id == vesselId.Value, cancellationToken))
            {
                return OperationResult<ApplicationUser>.Failure("The selected vessel was not found.");
            }

            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

            var createdOnUtc = DateTime.UtcNow;
            var password = GenerateCrewPassword();
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = model.FullName.Trim(),
                CellNo = model.CellNo.Trim(),
                Country = model.Nationality.Trim(),
                RequestedRole = RoleNames.Crew,
                ApprovalStatus = ApprovalStatus.Approved,
                IsCrew = true,
                Qualification = model.Qualification,
                CrewRank = model.Qualification?.ToCrewRank(),
                SidNumber = sidNumber,
                SidIssuingAuthority = model.SidIssuingAuthority.Trim(),
                SidIssueDate = model.SidIssueDate,
                SidExpiryDate = model.SidExpiryDate,
                IsAccountApproved = true,
                ApprovedByUserId = approvedByUserId,
                ApprovedOnUtc = createdOnUtc,
                CompanyId = companyId,
                VesselId = vesselId,
                IsActive = true,
                RegisteredOnUtc = createdOnUtc
            };

            var createResult = await userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                await transaction.RollbackAsync(cancellationToken);
                return OperationResult<ApplicationUser>.Failure(createResult.Errors.Select(x => x.Description).ToArray());
            }

            var roleResult = await AssignSingleRoleAsync(user, RoleNames.Crew, cancellationToken);
            if (!roleResult.Succeeded)
            {
                await transaction.RollbackAsync(cancellationToken);
                return OperationResult<ApplicationUser>.Failure(roleResult.Errors.ToArray());
            }

            dbContext.CrewMemberDetails.Add(new CrewMemberDetails
            {
                UserId = user.Id,
                Gender = model.Gender.Trim(),
                DateOfBirth = model.DateOfBirth,
                Nationality = model.Nationality.Trim(),
                PassportNumber = model.PassportNumber.Trim(),
                PassportExpiry = model.PassportExpiry
            });

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            user.CrewMemberDetails = await dbContext.CrewMemberDetails.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == user.Id, cancellationToken);
            return OperationResult<ApplicationUser>.Success(user, $"Crew member created successfully. Temporary password: {password}");
        }

        private static string GenerateCrewPassword()
        {
            const string uppercase = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            const string lowercase = "abcdefghijkmnopqrstuvwxyz";
            const string digits = "23456789";
            const string specials = "!@$?_-#";

            Span<char> password = stackalloc char[8];
            password[0] = uppercase[RandomNumberGenerator.GetInt32(uppercase.Length)];
            password[1] = lowercase[RandomNumberGenerator.GetInt32(lowercase.Length)];
            password[2] = digits[RandomNumberGenerator.GetInt32(digits.Length)];
            password[3] = specials[RandomNumberGenerator.GetInt32(specials.Length)];

            var all = string.Concat(uppercase, lowercase, digits, specials);
            for (var i = 4; i < password.Length; i++)
            {
                password[i] = all[RandomNumberGenerator.GetInt32(all.Length)];
            }

            for (var i = password.Length - 1; i > 0; i--)
            {
                var swapIndex = RandomNumberGenerator.GetInt32(i + 1);
                (password[i], password[swapIndex]) = (password[swapIndex], password[i]);
            }

            return new string(password);
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
            GivenNames = user.CrewMemberDetails?.GivenNames,
            Gender = user.CrewMemberDetails?.Gender,
            DateOfBirth = user.CrewMemberDetails?.DateOfBirth,
            PlaceOfBirth = user.CrewMemberDetails?.PlaceOfBirth,
            Nationality = user.CrewMemberDetails?.Nationality,
            PassportNumber = user.CrewMemberDetails?.PassportNumber,
            PassportExpiry = user.CrewMemberDetails?.PassportExpiry,
            ProfilePicture = user.ProfilePicture,
            ProfilePictureContentType = user.ProfilePictureContentType
        };
    }
}
