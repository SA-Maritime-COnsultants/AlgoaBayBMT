using AlgoaBayBMT.Data;
using AlgoaBayBMT.Services.Interfaces;
using AlgoaBayBMT.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace AlgoaBayBMT.Services
{
    public class BargeDeploymentService(IDbContextFactory<ApplicationDbContext> dbContextFactory) : IBargeDeploymentService
    {
        public async Task<List<BargeDeployment>> GetActiveByAreaAsync(int areaId)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync();
            return await dbContext.BargeDeployments
                .AsNoTracking()
                .Include(x => x.BunkerBarge)
                    .ThenInclude(x => x.BunkerOperator)
                .Include(x => x.AreaOfOperation)
                .Where(x => x.AreaOfOperationId == areaId && x.DeployedTo == null)
                .OrderBy(x => x.DeployedFrom)
                .ToListAsync();
        }

        public async Task<List<BargeDeployment>> GetByBargeAsync(int bargeId)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync();
            return await dbContext.BargeDeployments
                .AsNoTracking()
                .Include(x => x.AreaOfOperation)
                .Include(x => x.BunkerBarge)
                .Where(x => x.BunkerBargeId == bargeId)
                .OrderByDescending(x => x.DeployedFrom)
                .ToListAsync();
        }

        public async Task<BargeDeployment> DeployAsync(int bargeId, int areaId, DateTime from, string? notes, string performedBy)
        {
            if (string.IsNullOrWhiteSpace(performedBy))
            {
                performedBy = "System";
            }

            await using var dbContext = await dbContextFactory.CreateDbContextAsync();

            var barge = await dbContext.BunkerBarges
                .Include(x => x.BunkerOperator)
                .FirstOrDefaultAsync(x => x.Id == bargeId && x.IsActive)
                ?? throw new InvalidOperationException("Active bunker barge was not found.");

            var area = await dbContext.AreasOfOperation.FirstOrDefaultAsync(x => x.Id == areaId && x.IsActive)
                ?? throw new InvalidOperationException("Active area of operation was not found.");

            var operatorAssigned = await dbContext.OperatorAreaAssignments.AnyAsync(x =>
                x.BunkerOperatorId == barge.BunkerOperatorId &&
                x.AreaOfOperationId == areaId &&
                x.AssignedFrom <= from &&
                (x.AssignedTo == null || x.AssignedTo >= from));

            if (!operatorAssigned)
            {
                throw new InvalidOperationException($"Operator '{barge.BunkerOperator.Name}' is not assigned to area '{area.Name}' at the deployment start time.");
            }

            var hasOverlap = await dbContext.BargeDeployments.AnyAsync(x =>
                x.BunkerBargeId == bargeId &&
                x.DeployedFrom <= from &&
                (x.DeployedTo == null || x.DeployedTo >= from));

            if (hasOverlap)
            {
                throw new InvalidOperationException("The bunker barge already has a deployment overlapping the requested start time.");
            }

            var deployment = new BargeDeployment
            {
                BunkerBargeId = bargeId,
                AreaOfOperationId = areaId,
                DeployedFrom = from,
                Notes = notes
            };

            deployment.AuditEntries.Add(new BargeDeploymentAudit
            {
                Timestamp = DateTime.UtcNow,
                Action = "Deployed",
                PerformedBy = performedBy,
                Details = $"Barge '{barge.Name}' deployed to '{area.Name}' from {from:yyyy-MM-dd HH:mm}."
            });

            dbContext.BargeDeployments.Add(deployment);
            await dbContext.SaveChangesAsync();
            return deployment;
        }

        public async Task EndDeploymentAsync(int deploymentId, DateTime to, string performedBy)
        {
            if (string.IsNullOrWhiteSpace(performedBy))
            {
                performedBy = "System";
            }

            await using var dbContext = await dbContextFactory.CreateDbContextAsync();
            var deployment = await dbContext.BargeDeployments
                .Include(x => x.BunkerBarge)
                .Include(x => x.AreaOfOperation)
                .FirstOrDefaultAsync(x => x.Id == deploymentId)
                ?? throw new InvalidOperationException($"Deployment with id {deploymentId} was not found.");

            if (deployment.DeployedTo.HasValue)
            {
                throw new InvalidOperationException("This deployment has already ended.");
            }

            if (to < deployment.DeployedFrom)
            {
                throw new InvalidOperationException("Deployment end time cannot be earlier than the deployment start time.");
            }

            deployment.DeployedTo = to;
            dbContext.BargeDeploymentAudits.Add(new BargeDeploymentAudit
            {
                BargeDeploymentId = deployment.Id,
                Timestamp = DateTime.UtcNow,
                Action = "Ended",
                PerformedBy = performedBy,
                Details = $"Barge '{deployment.BunkerBarge.Name}' deployment to '{deployment.AreaOfOperation.Name}' ended at {to:yyyy-MM-dd HH:mm}."
            });

            await dbContext.SaveChangesAsync();
        }

        public async Task<List<BargeDeploymentAudit>> GetAuditTrailAsync(int deploymentId)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync();
            return await dbContext.BargeDeploymentAudits
                .AsNoTracking()
                .Where(x => x.BargeDeploymentId == deploymentId)
                .OrderByDescending(x => x.Timestamp)
                .ToListAsync();
        }
    }
}
