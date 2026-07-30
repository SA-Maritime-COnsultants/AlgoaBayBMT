using AlgoaBayBMT.Shared.Models;
using AlgoaBayBMT.Shared.Security;
using Microsoft.AspNetCore.Identity;

namespace AlgoaBayBMT.Data
{
    public static class IdentitySeedData
    {
        public static async Task EnsureSeedDataAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            foreach (var roleName in RoleNames.All)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            const string adminEmail = "admin@localhost";
            const string adminPassword = "DavidK8911#:";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser is null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FullName = "System Administrator",
                    RequestedRole = RoleNames.Admin,
                    ApprovalStatus = ApprovalStatus.Approved,
                    IsAccountApproved = true,
                    IsActive = true,
                    ApprovedOnUtc = DateTime.UtcNow,
                    RegisteredOnUtc = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description)));
                }
            }
            else
            {
                adminUser.EmailConfirmed = true;
                adminUser.RequestedRole = RoleNames.Admin;
                adminUser.ApprovalStatus = ApprovalStatus.Approved;
                adminUser.IsAccountApproved = true;
                adminUser.IsActive = true;
                adminUser.ApprovedOnUtc ??= DateTime.UtcNow;
                await userManager.UpdateAsync(adminUser);
            }

            if (!await userManager.IsInRoleAsync(adminUser, RoleNames.Admin))
            {
                await userManager.AddToRoleAsync(adminUser, RoleNames.Admin);
            }
        }
    }
}
