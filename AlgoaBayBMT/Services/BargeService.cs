using AlgoaBayBMT.Data;
using AlgoaBayBMT.Services.Interfaces;
using AlgoaBayBMT.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace AlgoaBayBMT.Services
{
    public class BargeService(IDbContextFactory<ApplicationDbContext> dbContextFactory) : IBargeService
    {
        public async Task<List<BunkerBarge>> GetAllAsync()
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync();
            return await dbContext.BunkerBarges
                .AsNoTracking()
                .Include(x => x.BunkerOperator)
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<BunkerBarge?> GetByIdAsync(int id)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync();
            return await dbContext.BunkerBarges
                .AsNoTracking()
                .Include(x => x.BunkerOperator)
                .Include(x => x.Deployments)
                    .ThenInclude(x => x.AreaOfOperation)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<BunkerBarge> CreateAsync(BunkerBarge barge)
        {
            ArgumentNullException.ThrowIfNull(barge);
            Normalize(barge);
            Validate(barge);

            await using var dbContext = await dbContextFactory.CreateDbContextAsync();
            var operatorExists = await dbContext.BunkerOperators.AnyAsync(x => x.Id == barge.BunkerOperatorId && x.IsActive);
            if (!operatorExists)
            {
                throw new InvalidOperationException("Active bunker operator was not found.");
            }

            dbContext.BunkerBarges.Add(barge);
            await dbContext.SaveChangesAsync();
            return barge;
        }

        public async Task UpdateAsync(BunkerBarge barge)
        {
            ArgumentNullException.ThrowIfNull(barge);
            Normalize(barge);
            Validate(barge);

            await using var dbContext = await dbContextFactory.CreateDbContextAsync();
            var existing = await dbContext.BunkerBarges.FirstOrDefaultAsync(x => x.Id == barge.Id)
                ?? throw new InvalidOperationException($"Bunker barge with id {barge.Id} was not found.");

            var operatorExists = await dbContext.BunkerOperators.AnyAsync(x => x.Id == barge.BunkerOperatorId);
            if (!operatorExists)
            {
                throw new InvalidOperationException("Bunker operator was not found.");
            }

            existing.Name = barge.Name;
            existing.IMO = barge.IMO;
            existing.MMSI = barge.MMSI;
            existing.CallSign = barge.CallSign;
            existing.CapacityMT = barge.CapacityMT;
            existing.FuelTypesSupported = barge.FuelTypesSupported;
            existing.PumpingRate = barge.PumpingRate;
            existing.LastInspectionDate = barge.LastInspectionDate;
            existing.CertificationExpiry = barge.CertificationExpiry;
            existing.CrewCapacity = barge.CrewCapacity;
            existing.IsActive = barge.IsActive;
            existing.BunkerOperatorId = barge.BunkerOperatorId;
            await dbContext.SaveChangesAsync();
        }

        private static void Normalize(BunkerBarge barge)
        {
            barge.Name = barge.Name.Trim();
        }

        private static void Validate(BunkerBarge barge)
        {
            if (string.IsNullOrWhiteSpace(barge.Name))
            {
                throw new InvalidOperationException("Bunker barge name is required.");
            }
        }
    }
}
