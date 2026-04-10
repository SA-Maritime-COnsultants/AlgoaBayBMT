using AlgoaBayBMT.Data;
using AlgoaBayBMT.Services.Interfaces;
using AlgoaBayBMT.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace AlgoaBayBMT.Services
{
    public class VesselRoleAssignmentService(ApplicationDbContext dbContext) : IVesselRoleAssignmentService
    {
        public Task<List<VesselRoleAssignment>> GetAssignmentsAsync(CancellationToken cancellationToken = default) =>
            dbContext.VesselRoleAssignments.Include(x => x.Vessel).Include(x => x.Company).OrderByDescending(x => x.AssignedOnUtc).ToListAsync(cancellationToken);

        public async Task<OperationResult> AssignRoleAsync(int vesselId, string userId, VesselRoleType vesselRoleType, string? assignedByUserId, int? companyId, CancellationToken cancellationToken = default)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId && x.IsCrew, cancellationToken);
            if (user is null)
            {
                return OperationResult.Failure("Crew member not found.");
            }

            var exists = await dbContext.VesselRoleAssignments.AnyAsync(x =>
                x.VesselId == vesselId && x.UserId == userId && x.VesselRoleType == vesselRoleType && x.IsActive,
                cancellationToken);

            if (exists)
            {
                return OperationResult.Failure("The vessel role assignment already exists.");
            }

            dbContext.VesselRoleAssignments.Add(new VesselRoleAssignment
            {
                VesselId = vesselId,
                UserId = userId,
                VesselRoleType = vesselRoleType,
                AssignedByUserId = assignedByUserId,
                CompanyId = companyId,
                AssignedOnUtc = DateTime.UtcNow,
                IsActive = true
            });

            await dbContext.SaveChangesAsync(cancellationToken);
            return OperationResult.Success("Vessel role assigned.");
        }

        public async Task<OperationResult> RemoveAssignmentAsync(int assignmentId, CancellationToken cancellationToken = default)
        {
            var assignment = await dbContext.VesselRoleAssignments.FirstOrDefaultAsync(x => x.Id == assignmentId, cancellationToken);
            if (assignment is null)
            {
                return OperationResult.Failure("Assignment not found.");
            }

            assignment.IsActive = false;
            assignment.EndedOnUtc = DateTime.UtcNow;
            assignment.ModifiedOnUtc = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
            return OperationResult.Success("Assignment closed.");
        }
    }
}
