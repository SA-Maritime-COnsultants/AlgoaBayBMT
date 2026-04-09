using AlgoaBayBMT.Data;
using AlgoaBayBMT.Shared.Models;
using AlgoaBayBMT.Shared.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace AlgoaBayBMT.Security
{
    public class VesselCommandHandler(ApplicationDbContext dbContext) : AuthorizationHandler<VesselCommandRequirement>
    {
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, VesselCommandRequirement requirement)
        {
            if (context.User.IsInRole(RoleNames.Admin) || context.User.IsInRole(RoleNames.SeniorManager))
            {
                context.Succeed(requirement);
                return;
            }

            var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
            {
                return;
            }

            var hasCommandAssignment = await dbContext.VesselRoleAssignments.AnyAsync(x =>
                x.UserId == userId &&
                x.IsActive &&
                (x.VesselRoleType == VesselRoleType.Master || x.VesselRoleType == VesselRoleType.ChiefOfficer),
                CancellationToken.None);

            if (hasCommandAssignment)
            {
                context.Succeed(requirement);
            }
        }
    }
}
