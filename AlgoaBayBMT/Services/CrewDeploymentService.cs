using AlgoaBayBMT.Data;
using AlgoaBayBMT.Services.Interfaces;
using AlgoaBayBMT.Services.Models;
using AlgoaBayBMT.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace AlgoaBayBMT.Services
{
    public class CrewDeploymentService(ApplicationDbContext dbContext) : ICrewDeploymentService
    {
        public async Task<OperationResult<CrewDeployment>> DeployCrewAsync(CrewDeploymentRequest request, CancellationToken cancellationToken = default)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);
            if (user is null)
            {
                return OperationResult<CrewDeployment>.Failure("Crew member not found.");
            }

            var vessel = await dbContext.Vessels.FirstOrDefaultAsync(x => x.Id == request.VesselId, cancellationToken);
            if (vessel is null)
            {
                return OperationResult<CrewDeployment>.Failure("Vessel not found.");
            }

            var existing = await dbContext.CrewDeployments.AnyAsync(x => x.UserId == request.UserId && x.Status == DeploymentStatus.Active, cancellationToken);
            if (existing)
            {
                return OperationResult<CrewDeployment>.Failure("Crew member already has an active deployment.");
            }

            var deployment = new CrewDeployment
            {
                UserId = request.UserId,
                VesselId = request.VesselId,
                OnboardRoleName = request.OnboardRoleName,
                OnboardRoleType = request.OnboardRoleType,
                Status = DeploymentStatus.Active,
                StartedOnUtc = request.StartedOnUtc,
                DeployedByUserId = request.DeployedByUserId
            };

            foreach (var snapshot in request.ComplianceSnapshots)
            {
                deployment.ComplianceSnapshots.Add(new CrewDeploymentComplianceSnapshot
                {
                    ComplianceItemName = snapshot.ComplianceItemName,
                    ComplianceState = snapshot.ComplianceState,
                    ExpiresOnUtc = snapshot.ExpiresOnUtc,
                    RecordedOnUtc = request.StartedOnUtc
                });
            }

            var latestExpiry = request.ComplianceSnapshots
                .Where(x => x.ExpiresOnUtc.HasValue)
                .OrderBy(x => x.ExpiresOnUtc)
                .Select(x => x.ExpiresOnUtc)
                .FirstOrDefault();

            var complianceState = request.ComplianceSnapshots.Count == 0
                ? ComplianceState.Unknown
                : request.ComplianceSnapshots.MaxBy(x => x.ComplianceState)?.ComplianceState ?? ComplianceState.Unknown;

            dbContext.CrewDeployments.Add(deployment);
            await dbContext.SaveChangesAsync(cancellationToken);

            dbContext.VesselCrewListEntries.Add(new VesselCrewListEntry
            {
                VesselId = request.VesselId,
                UserId = request.UserId,
                CrewDeploymentId = deployment.Id,
                OnboardRoleName = request.OnboardRoleName,
                OnboardRoleType = request.OnboardRoleType,
                JoinedOnUtc = request.StartedOnUtc,
                ComplianceState = complianceState,
                ComplianceExpiresOnUtc = latestExpiry,
                IsCurrent = true
            });

            user.VesselId = request.VesselId;
            await dbContext.SaveChangesAsync(cancellationToken);
            return OperationResult<CrewDeployment>.Success(deployment, "Crew member deployed.");
        }

        public async Task<OperationResult> EndDeploymentAsync(int deploymentId, DateTime endedOnUtc, CancellationToken cancellationToken = default)
        {
            var deployment = await dbContext.CrewDeployments.FirstOrDefaultAsync(x => x.Id == deploymentId, cancellationToken);
            if (deployment is null)
            {
                return OperationResult.Failure("Deployment not found.");
            }

            deployment.Status = DeploymentStatus.Completed;
            deployment.EndedOnUtc = endedOnUtc;
            deployment.ModifiedOnUtc = DateTime.UtcNow;

            var entries = await dbContext.VesselCrewListEntries
                .Where(x => x.CrewDeploymentId == deploymentId && x.IsCurrent)
                .ToListAsync(cancellationToken);

            foreach (var entry in entries)
            {
                entry.IsCurrent = false;
                entry.LeftOnUtc = endedOnUtc;
                entry.ModifiedOnUtc = DateTime.UtcNow;
            }

            var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == deployment.UserId, cancellationToken);
            if (user is not null && user.VesselId == deployment.VesselId)
            {
                user.VesselId = null;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            return OperationResult.Success("Deployment ended.");
        }

        public Task<List<VesselCrewListEntry>> GetCrewListAsync(int vesselId, DateTime? asOfUtc = null, CancellationToken cancellationToken = default)
        {
            var pointInTime = asOfUtc ?? DateTime.UtcNow;
            return dbContext.VesselCrewListEntries
                .Where(x => x.VesselId == vesselId && x.JoinedOnUtc <= pointInTime && (x.LeftOnUtc == null || x.LeftOnUtc >= pointInTime))
                .Include(x => x.Vessel)
                .OrderBy(x => x.OnboardRoleType)
                .ThenBy(x => x.OnboardRoleName)
                .ToListAsync(cancellationToken);
        }

        public Task<List<CrewDeployment>> GetDeploymentHistoryAsync(int vesselId, CancellationToken cancellationToken = default) =>
            dbContext.CrewDeployments
                .Where(x => x.VesselId == vesselId)
                .Include(x => x.ComplianceSnapshots)
                .OrderByDescending(x => x.StartedOnUtc)
                .ToListAsync(cancellationToken);
    }
}
