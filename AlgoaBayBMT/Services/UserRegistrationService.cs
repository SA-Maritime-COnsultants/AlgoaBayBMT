using AlgoaBayBMT.Data;
using AlgoaBayBMT.Services.Interfaces;
using AlgoaBayBMT.Services.Models;
using AlgoaBayBMT.Shared.Models;
using AlgoaBayBMT.Shared.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AlgoaBayBMT.Services
{
    public class UserRegistrationService(
        UserManager<ApplicationUser> userManager,
        IUserStore<ApplicationUser> userStore,
        ApplicationDbContext dbContext) : IUserRegistrationService
    {
        public async Task<OperationResult<ApplicationUser>> RegisterAsync(RegistrationRequest request, CancellationToken cancellationToken = default)
        {
            if (!string.Equals(request.Password, request.ConfirmPassword, StringComparison.Ordinal))
            {
                return OperationResult<ApplicationUser>.Failure("Password confirmation does not match.");
            }

            if (string.Equals(request.RequestedRole, RoleNames.Crew, StringComparison.OrdinalIgnoreCase) && request.VesselId is null)
            {
                return OperationResult<ApplicationUser>.Failure("Crew registration requires a vessel assignment.");
            }

            if (await userManager.FindByEmailAsync(request.Email) is not null)
            {
                return OperationResult<ApplicationUser>.Failure("A user with this email already exists.");
            }

            if (request.CompanyId.HasValue && !await dbContext.BunkeringCompanies.AnyAsync(x => x.Id == request.CompanyId.Value, cancellationToken))
            {
                return OperationResult<ApplicationUser>.Failure("Selected company was not found.");
            }

            if (request.VesselId.HasValue && !await dbContext.Vessels.AnyAsync(x => x.Id == request.VesselId.Value, cancellationToken))
            {
                return OperationResult<ApplicationUser>.Failure("Selected vessel was not found.");
            }

            if (request.PrimaryAreaId.HasValue && !await dbContext.OperationalAreas.AnyAsync(x => x.Id == request.PrimaryAreaId.Value, cancellationToken))
            {
                return OperationResult<ApplicationUser>.Failure("Selected operational area was not found.");
            }

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FullName = request.FullName,
                RequestedRole = request.RequestedRole,
                ApprovalStatus = ApprovalStatus.PendingEmailConfirmation,
                IsAccountApproved = false,
                CompanyId = request.CompanyId,
                VesselId = request.VesselId,
                PrimaryAreaId = request.PrimaryAreaId,
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

            return OperationResult<ApplicationUser>.Success(user, "Registration submitted. Email confirmation and approval are still required.");
        }
    }
}
