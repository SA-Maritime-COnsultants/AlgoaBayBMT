using AlgoaBayBMT.Data;
using AlgoaBayBMT.Services.Interfaces;
using AlgoaBayBMT.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace AlgoaBayBMT.Services
{
    public class PortService(IDbContextFactory<ApplicationDbContext> dbContextFactory) : IPortService
    {
        public async Task<List<Port>> GetByAreaAsync(int areaId)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync();
            return await dbContext.Ports
                .AsNoTracking()
                .Where(x => x.AreaOfOperationId == areaId)
                .OrderByDescending(x => x.IsActive)
                .ThenBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<Port> CreateAsync(Port port)
        {
            ArgumentNullException.ThrowIfNull(port);
            Normalize(port);
            Validate(port);

            await using var dbContext = await dbContextFactory.CreateDbContextAsync();
            _ = await dbContext.AreasOfOperation.AnyAsync(x => x.Id == port.AreaOfOperationId)
                ? true
                : throw new InvalidOperationException($"Area of Operation with id {port.AreaOfOperationId} was not found.");

            dbContext.Ports.Add(port);
            await dbContext.SaveChangesAsync();
            return port;
        }

        public async Task UpdateAsync(Port port)
        {
            ArgumentNullException.ThrowIfNull(port);
            Normalize(port);
            Validate(port);

            await using var dbContext = await dbContextFactory.CreateDbContextAsync();
            var existing = await dbContext.Ports.FirstOrDefaultAsync(x => x.Id == port.Id)
                ?? throw new InvalidOperationException($"Port/Anchorage with id {port.Id} was not found.");

            existing.Name = port.Name;
            existing.Type = port.Type;
            existing.Latitude = port.Latitude;
            existing.Longitude = port.Longitude;
            existing.Depth = port.Depth;
            existing.MaxVesselSize = port.MaxVesselSize;
            existing.IsAnchorage = port.IsAnchorage;
            existing.IsBunkeringAllowed = port.IsBunkeringAllowed;
            existing.IsActive = port.IsActive;
            await dbContext.SaveChangesAsync();
        }

        public async Task DeactivateAsync(int id)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync();
            var existing = await dbContext.Ports.FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new InvalidOperationException($"Port/Anchorage with id {id} was not found.");
            existing.IsActive = false;
            await dbContext.SaveChangesAsync();
        }

        private static void Normalize(Port port)
        {
            port.Name = port.Name.Trim();
            port.Type = port.IsAnchorage ? "Anchorage" : "Port";
        }

        private static void Validate(Port port)
        {
            if (string.IsNullOrWhiteSpace(port.Name))
            {
                throw new InvalidOperationException("Port/Anchorage name is required.");
            }

            if (port.Type is not "Port" and not "Anchorage")
            {
                throw new InvalidOperationException("Port type must be either 'Port' or 'Anchorage'.");
            }
        }
    }
}
