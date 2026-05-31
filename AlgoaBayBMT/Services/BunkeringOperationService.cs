using AlgoaBayBMT.Data;
using AlgoaBayBMT.Services.Interfaces;
using AlgoaBayBMT.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace AlgoaBayBMT.Services
{
    public class BunkeringOperationService(IDbContextFactory<ApplicationDbContext> dbContextFactory) : IBunkeringOperationService
    {
        public async Task<List<BunkeringOperation>> GetOperationsForUserAsync(string userId, bool isAdmin, bool isCompanyManager, int? companyId, CancellationToken cancellationToken = default)
        {
            await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var query = db.BunkeringOperations
                .AsNoTracking()
                .Include(x => x.BunkerVessel)
                .Include(x => x.CustomerVessel)
                .Include(x => x.BunkerFuel)
                .Include(x => x.PumpingIntervals)
                .AsQueryable();

            if (isAdmin)
            {
                return await query.OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);
            }

            if (isCompanyManager && companyId.HasValue)
            {
                return await query
                    .Where(x => x.BunkerVessel.OwningOperatorId == companyId.Value)
                    .OrderByDescending(x => x.CreatedAt)
                    .ToListAsync(cancellationToken);
            }

            return await query
                .Where(x => x.CreatedByUserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<Vessel?> GetAssignedBunkerVesselAsync(string userId, bool isAdmin, bool isCompanyManager, int? companyId, CancellationToken cancellationToken = default)
        {
            await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            if ((isAdmin || isCompanyManager) && companyId.HasValue)
            {
                return await db.Vessels.AsNoTracking()
                    .Where(x => x.IsActive && !x.IsDeleted && x.OwningOperatorId == companyId.Value)
                    .OrderBy(x => x.Name)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            var activeAssignment = await db.CrewAssignments.AsNoTracking()
                .Include(x => x.Vessel)
                .Include(x => x.CrewMember)
                .Where(x => x.CrewMember.ApplicationUserId == userId && x.Status == CrewAssignmentStatus.SignedOn && x.Vessel.IsActive)
                .OrderByDescending(x => x.SignOnDateUtc)
                .FirstOrDefaultAsync(cancellationToken);

            return activeAssignment?.Vessel;
        }

        public async Task<List<BunkerFuel>> GetFuelsAsync(CancellationToken cancellationToken = default)
        {
            await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            return await db.BunkerFuels.AsNoTracking().OrderBy(x => x.Name).ToListAsync(cancellationToken);
        }

        public async Task<OperationResult<BunkeringOperation>> SaveOperationAsync(BunkeringOperation operation, IReadOnlyCollection<BunkeringOperationPumpingInterval> intervals, IReadOnlyCollection<ISGOTTChecklistStageResponse> stageResponses, CancellationToken cancellationToken = default)
        {
            if (operation.CustomerVesselId <= 0) return OperationResult<BunkeringOperation>.Failure("Customer vessel is required.");
            if (operation.BunkerVesselId <= 0) return OperationResult<BunkeringOperation>.Failure("Bunker vessel is required.");
            if (operation.BunkerFuelId <= 0) return OperationResult<BunkeringOperation>.Failure("Bunker fuel is required.");
            if (operation.TotalQuantity <= 0) return OperationResult<BunkeringOperation>.Failure("Total quantity must be greater than zero.");
            if (intervals.Count == 0) return OperationResult<BunkeringOperation>.Failure("At least one pumping interval is required.");

            await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);

            operation.CreatedAt = DateTime.UtcNow;
            operation.PumpingIntervals = intervals.Select(x => new BunkeringOperationPumpingInterval
            {
                PumpStartTime = x.PumpStartTime,
                PumpEndTime = x.PumpEndTime,
                BunkerFuelId = x.BunkerFuelId,
                Quantity = x.Quantity,
                WindSpeed = x.WindSpeed,
                WindDirection = x.WindDirection,
                SeaState = x.SeaState,
                Swell = x.Swell,
                Visibility = x.Visibility,
                Notes = x.Notes
            }).ToList();
            operation.ISGOTTStageResponses = stageResponses.ToList();

            db.BunkeringOperations.Add(operation);
            await db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            return OperationResult<BunkeringOperation>.Success(operation, "Bunkering operation saved.");
        }
    }
}
