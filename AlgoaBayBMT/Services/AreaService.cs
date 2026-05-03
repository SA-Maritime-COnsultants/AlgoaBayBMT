using AlgoaBayBMT.Data;
using AlgoaBayBMT.Services.Interfaces;
using AlgoaBayBMT.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace AlgoaBayBMT.Services
{
    public class AreaService(IDbContextFactory<ApplicationDbContext> dbContextFactory) : IAreaService
    {
        public async Task<List<AreaOfOperation>> GetAllAsync()
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync();
            return await dbContext.AreasOfOperation
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<AreaOfOperation?> GetByIdAsync(int id)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync();
            return await dbContext.AreasOfOperation
                .AsNoTracking()
                .Include(x => x.Ports)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<AreaOfOperation> CreateAsync(AreaOfOperation area)
        {
            ArgumentNullException.ThrowIfNull(area);
            area.Name = area.Name.Trim();
            ValidateArea(area);

            await using var dbContext = await dbContextFactory.CreateDbContextAsync();
            dbContext.AreasOfOperation.Add(area);
            await dbContext.SaveChangesAsync();
            return area;
        }

        public async Task UpdateAsync(AreaOfOperation area)
        {
            ArgumentNullException.ThrowIfNull(area);
            area.Name = area.Name.Trim();
            ValidateArea(area);

            await using var dbContext = await dbContextFactory.CreateDbContextAsync();
            var existing = await dbContext.AreasOfOperation.FirstOrDefaultAsync(x => x.Id == area.Id)
                ?? throw new InvalidOperationException($"Area of Operation with id {area.Id} was not found.");

            existing.Name = area.Name;
            existing.Description = area.Description;
            existing.RegionCode = area.RegionCode;
            existing.EnvironmentalSensitivityRating = area.EnvironmentalSensitivityRating;
            existing.IsActive = area.IsActive;
            await dbContext.SaveChangesAsync();
        }

        public async Task DeactivateAsync(int id)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync();
            var existing = await dbContext.AreasOfOperation.FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new InvalidOperationException($"Area of Operation with id {id} was not found.");
            existing.IsActive = false;
            await dbContext.SaveChangesAsync();
        }

        private static void ValidateArea(AreaOfOperation area)
        {
            if (string.IsNullOrWhiteSpace(area.Name))
            {
                throw new InvalidOperationException("Area name is required.");
            }
        }
    }
}
