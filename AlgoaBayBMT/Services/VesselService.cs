using AlgoaBayBMT.Data;
using AlgoaBayBMT.Services.Interfaces;
using AlgoaBayBMT.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace AlgoaBayBMT.Services
{
    public class VesselService(IDbContextFactory<ApplicationDbContext> dbContextFactory, ILogger<VesselService> logger) : IVesselService
    {
        public async Task<List<Vessel>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
        {
            await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var query = db.Vessels.AsNoTracking().Include(x => x.OwningOperator).AsQueryable();
            if (!includeInactive) query = query.Where(x => x.IsActive);
            return await query.OrderBy(x => x.Name).ToListAsync(cancellationToken);
        }

        public async Task<List<Vessel>> GetByCompanyAsync(int bunkerOperatorId, bool includeInactive = false, CancellationToken cancellationToken = default)
        {
            await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var query = db.Vessels.AsNoTracking().Include(x => x.OwningOperator)
                .Where(x => x.OwningOperatorId == bunkerOperatorId);
            if (!includeInactive) query = query.Where(x => x.IsActive);
            return await query.OrderBy(x => x.Name).ToListAsync(cancellationToken);
        }

        public async Task<Vessel?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            return await db.Vessels.AsNoTracking()
                .Include(x => x.OwningOperator)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<Vessel> CreateAsync(Vessel vessel, string? performedByUserId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(vessel);
            try
            {
                await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
                vessel.CreatedOnUtc = DateTime.UtcNow;
                vessel.CreatedByUserId = performedByUserId;
                vessel.IsDeleted = false;
                db.Vessels.Add(vessel);
                await db.SaveChangesAsync(cancellationToken);
                logger.LogInformation("Vessel {VesselId} '{Name}' created by {User}", vessel.Id, vessel.Name, performedByUserId);
                return vessel;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to create vessel '{Name}'", vessel?.Name);
                throw;
            }
        }

        public async Task<Vessel> UpdateAsync(Vessel vessel, string? performedByUserId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(vessel);
            try
            {
                await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
                var existing = await db.Vessels.FirstOrDefaultAsync(x => x.Id == vessel.Id, cancellationToken)
                               ?? throw new InvalidOperationException($"Vessel {vessel.Id} not found.");
                existing.Name = vessel.Name;
                existing.IMO = vessel.IMO;
                existing.MMSI = vessel.MMSI;
                existing.CallSign = vessel.CallSign;
                existing.Flag = vessel.Flag;
                existing.VesselType = vessel.VesselType;
                existing.GrossTonnage = vessel.GrossTonnage;
                existing.LengthOverall = vessel.LengthOverall;
                existing.Beam = vessel.Beam;
                existing.OwningOperatorId = vessel.OwningOperatorId;
                existing.IsActive = vessel.IsActive;
                existing.ModifiedOnUtc = DateTime.UtcNow;
                existing.ModifiedByUserId = performedByUserId;
                await db.SaveChangesAsync(cancellationToken);
                return existing;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to update vessel {VesselId}", vessel?.Id);
                throw;
            }
        }

        public async Task SoftDeleteAsync(int id, string? performedByUserId, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
                var vessel = await db.Vessels.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                if (vessel is null) return;
                vessel.IsDeleted = true;
                vessel.IsActive = false;
                vessel.DeletedOnUtc = DateTime.UtcNow;
                vessel.DeletedByUserId = performedByUserId;
                await db.SaveChangesAsync(cancellationToken);
                logger.LogInformation("Vessel {VesselId} soft-deleted by {User}", id, performedByUserId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to soft-delete vessel {VesselId}", id);
                throw;
            }
        }
    }
}
