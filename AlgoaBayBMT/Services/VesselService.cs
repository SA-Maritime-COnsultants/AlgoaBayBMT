using AlgoaBayBMT.Data;
using AlgoaBayBMT.Services.Interfaces;
using AlgoaBayBMT.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace AlgoaBayBMT.Services
{
    public class VesselService(ApplicationDbContext dbContext) : IVesselService
    {
        public Task<List<Vessel>> GetVesselsAsync(CancellationToken cancellationToken = default) =>
            dbContext.Vessels.AsNoTracking().Include(x => x.Company).OrderBy(x => x.Name).ToListAsync(cancellationToken);

        public Task<Vessel?> GetVesselAsync(int vesselId, CancellationToken cancellationToken = default) =>
            dbContext.Vessels.Include(x => x.Company)
                .Include(x => x.RoleAssignments)
                .FirstOrDefaultAsync(x => x.Id == vesselId, cancellationToken);

        public async Task<OperationResult<Vessel>> CreateVesselAsync(Vessel vessel, CancellationToken cancellationToken = default)
        {
            if (await dbContext.Vessels.AnyAsync(x => x.ImoNumber == vessel.ImoNumber, cancellationToken))
            {
                return OperationResult<Vessel>.Failure("IMO number already exists.");
            }

            dbContext.Vessels.Add(vessel);
            await dbContext.SaveChangesAsync(cancellationToken);
            return OperationResult<Vessel>.Success(vessel, "Vessel created.");
        }

        public async Task<OperationResult<Vessel>> UpdateVesselAsync(Vessel vessel, CancellationToken cancellationToken = default)
        {
            var existing = await dbContext.Vessels.FindAsync([vessel.Id], cancellationToken);
            if (existing is null)
            {
                return OperationResult<Vessel>.Failure("Vessel not found.");
            }

            existing.Name = vessel.Name;
            existing.ImoNumber = vessel.ImoNumber;
            existing.CallSign = vessel.CallSign;
            existing.FlagState = vessel.FlagState;
            existing.CompanyId = vessel.CompanyId;
            existing.IsActive = vessel.IsActive;
            existing.ModifiedOnUtc = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
            return OperationResult<Vessel>.Success(existing, "Vessel updated.");
        }

        public async Task<OperationResult> DeleteVesselAsync(int vesselId, CancellationToken cancellationToken = default)
        {
            var vessel = await dbContext.Vessels.FindAsync([vesselId], cancellationToken);
            if (vessel is null)
            {
                return OperationResult.Failure("Vessel not found.");
            }

            dbContext.Vessels.Remove(vessel);
            await dbContext.SaveChangesAsync(cancellationToken);
            return OperationResult.Success("Vessel deleted.");
        }
    }
}
