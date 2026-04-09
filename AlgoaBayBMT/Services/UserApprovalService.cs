using AlgoaBayBMT.Data;
using AlgoaBayBMT.Services.Interfaces;
using AlgoaBayBMT.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace AlgoaBayBMT.Services
{
    public class UserApprovalService(ApplicationDbContext dbContext) : IUserApprovalService
    {
        public Task<List<ApplicationUser>> GetPendingUsersAsync(CancellationToken cancellationToken = default) =>
            dbContext.Users
                .OrderBy(x => x.EmailConfirmed)
                .ThenBy(x => x.RegisteredOnUtc)
                .Where(x => !x.IsAccountApproved || !x.EmailConfirmed)
                .ToListAsync(cancellationToken);

        public async Task<OperationResult> ApproveAsync(string userId, string approvedByUserId, string? notes, CancellationToken cancellationToken = default)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
            if (user is null)
            {
                return OperationResult.Failure("User not found.");
            }

            if (!user.EmailConfirmed)
            {
                user.ApprovalStatus = ApprovalStatus.PendingEmailConfirmation;
                user.ApprovalNotes = "Email confirmation required before approval.";
                await dbContext.SaveChangesAsync(cancellationToken);
                return OperationResult.Failure("Email confirmation is still pending.");
            }

            user.IsAccountApproved = true;
            user.ApprovedByUserId = approvedByUserId;
            user.ApprovedOnUtc = DateTime.UtcNow;
            user.ApprovalNotes = notes;
            user.ApprovalStatus = ApprovalStatus.Approved;
            user.IsActive = true;
            await dbContext.SaveChangesAsync(cancellationToken);
            return OperationResult.Success("User approved.");
        }

        public async Task<OperationResult> RejectAsync(string userId, string approvedByUserId, string? notes, CancellationToken cancellationToken = default)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
            if (user is null)
            {
                return OperationResult.Failure("User not found.");
            }

            user.IsAccountApproved = false;
            user.ApprovedByUserId = approvedByUserId;
            user.ApprovedOnUtc = DateTime.UtcNow;
            user.ApprovalNotes = notes;
            user.ApprovalStatus = ApprovalStatus.Rejected;
            user.IsActive = false;
            await dbContext.SaveChangesAsync(cancellationToken);
            return OperationResult.Success("User rejected.");
        }
    }
}
