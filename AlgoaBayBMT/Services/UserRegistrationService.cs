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
    public class UserRegistrationService(
        UserManager<ApplicationUser> userManager,
        IUserStore<ApplicationUser> userStore,
        ApplicationDbContext dbContext,
        RoleManager<IdentityRole> roleManager) : IUserRegistrationService
    {
        private static readonly Regex SidNumberRegex = new("^[A-Z0-9-]{6,20}$", RegexOptions.Compiled, TimeSpan.FromSeconds(1));

        public async Task<OperationResult<ApplicationUser>> RegisterAsync(RegistrationRequest request, CancellationToken cancellationToken = default)
        {
            if (!string.Equals(request.Password, request.ConfirmPassword, StringComparison.Ordinal))
            {
                return OperationResult<ApplicationUser>.Failure("Password confirmation does not match.");
            }

            if (string.IsNullOrWhiteSpace(request.CellNo))
            {
                return OperationResult<ApplicationUser>.Failure("Cell number is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Address))
            {
                return OperationResult<ApplicationUser>.Failure("Address is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Country))
            {
                return OperationResult<ApplicationUser>.Failure("Country is required.");
            }

            if (request.IsCrew && request.CrewRank is null)
            {
                return OperationResult<ApplicationUser>.Failure("Crew rank is required for crew registrations.");
            }

            var normalizedSidNumber = request.IsCrew ? NormalizeSidNumber(request.SidNumber) : null;

            if (request.IsCrew && string.IsNullOrWhiteSpace(normalizedSidNumber))
            {
                return OperationResult<ApplicationUser>.Failure("SID number is required for crew registrations.");
            }

            if (request.IsCrew && !string.IsNullOrWhiteSpace(normalizedSidNumber) && !SidNumberRegex.IsMatch(normalizedSidNumber))
            {
                return OperationResult<ApplicationUser>.Failure("SID must be 6–20 characters and may contain only letters, numbers, and hyphens.");
            }

            if (request.IsCrew && string.IsNullOrWhiteSpace(request.SidIssuingCountry))
            {
                return OperationResult<ApplicationUser>.Failure("SID issuing country is required for crew registrations.");
            }

            if (request.IsCrew && string.IsNullOrWhiteSpace(request.SidIssuingAuthority))
            {
                return OperationResult<ApplicationUser>.Failure("SID issuing authority is required for crew registrations.");
            }

            if (await userManager.FindByEmailAsync(request.Email) is not null)
            {
                return OperationResult<ApplicationUser>.Failure("A user with this email already exists.");
            }

            if (request.CompanyId.HasValue && !await dbContext.BunkeringCompanies.AnyAsync(x => x.Id == request.CompanyId.Value, cancellationToken))
            {
                return OperationResult<ApplicationUser>.Failure("Selected company was not found.");
            }

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FullName = request.FullName,
                CellNo = request.CellNo?.Trim(),
                Address = request.Address?.Trim(),
                Country = request.Country?.Trim(),
                RequestedRole = RoleNames.Customer,
                IsCrew = request.IsCrew,
                CrewRank = request.IsCrew ? request.CrewRank : null,
                SidNumber = request.IsCrew ? normalizedSidNumber : null,
                SidIssuingCountry = request.IsCrew ? request.SidIssuingCountry?.Trim() : null,
                SidIssuingAuthority = request.IsCrew ? request.SidIssuingAuthority?.Trim() : null,
                SidIssueDate = request.IsCrew ? request.SidIssueDate : null,
                SidExpiryDate = request.IsCrew ? request.SidExpiryDate : null,
                ApprovalStatus = ApprovalStatus.PendingEmailConfirmation,
                IsAccountApproved = false,
                CompanyId = request.CompanyId,
                IsActive = true,
                RegisteredOnUtc = DateTime.UtcNow,
                EmailConfirmed = false
            };

            var emailStore = userStore as IUserEmailStore<ApplicationUser>
                ?? throw new NotSupportedException("The configured user store does not support email.");

            await emailStore.SetEmailAsync(user, request.Email, cancellationToken);
            var result = await userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return OperationResult<ApplicationUser>.Failure(result.Errors.Select(x => x.Description).ToArray());
            }

            if (!await roleManager.RoleExistsAsync(RoleNames.Customer))
            {
                var createRoleResult = await roleManager.CreateAsync(new IdentityRole(RoleNames.Customer));
                if (!createRoleResult.Succeeded)
                {
                    return OperationResult<ApplicationUser>.Failure(createRoleResult.Errors.Select(x => x.Description).ToArray());
                }
            }

            var roleResult = await userManager.AddToRoleAsync(user, RoleNames.Customer);
            if (!roleResult.Succeeded)
            {
                return OperationResult<ApplicationUser>.Failure(roleResult.Errors.Select(x => x.Description).ToArray());
            }

            return OperationResult<ApplicationUser>.Success(user, "Registration submitted. Email confirmation and approval are still required.");
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
    }
}
