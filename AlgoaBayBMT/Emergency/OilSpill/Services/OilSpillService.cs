using AlgoaBayBMT.Data;
using AlgoaBayBMT.Emergency.OilSpill.DTOs;
using AlgoaBayBMT.Emergency.OilSpill.Models;
using Microsoft.EntityFrameworkCore;

namespace AlgoaBayBMT.Emergency.OilSpill.Services
{
    public class OilSpillService : IOilSpillService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IIncidentFormService _formService;

        public OilSpillService(
            IDbContextFactory<ApplicationDbContext> contextFactory,
            IIncidentFormService formService)
        {
            _contextFactory = contextFactory;
            _formService = formService;
        }

        public async Task<OilSpillIncident> CreateSpillAsync(CreateOilSpillRequest request)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var incident = new OilSpillIncident
            {
                BunkeringOperationId = request.BunkeringOperationId,
                SpillName = request.SpillName,
                SpillStartTime = request.SpillStartTime,
                ProductType = request.ProductType,
                EstimatedVolume = request.EstimatedVolume,
                ReleaseRate = request.ReleaseRate,
                SourceType = request.SourceType,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                Status = OilSpillStatus.Active,
                CreatedBy = request.CreatedBy,
                Commander = request.Commander,
                CreatedAt = DateTime.UtcNow
            };

            context.OilSpillIncidents.Add(incident);
            await context.SaveChangesAsync();

            // Automation: auto-create ICS-209 (always linked to SITREP) and ICS-201 briefing.
            await _formService.EnsureInitialFormsAsync(incident.Id, request.CreatedBy);

            return incident;
        }

        public async Task<OilSpillIncident?> GetSpillByIdAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.OilSpillIncidents
                .Include(s => s.ModelRuns)
                .Include(s => s.ResponseActions)
                .Include(s => s.Operation)
                    .ThenInclude(o => o.CustomerVessel)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IReadOnlyList<OilSpillIncident>> GetActiveSpillsAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.OilSpillIncidents
                .Where(s => s.Status == OilSpillStatus.Active)
                .OrderByDescending(s => s.SpillStartTime)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IReadOnlyList<OilSpillIncident>> GetSpillsByOperationAsync(int bunkeringOperationId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.OilSpillIncidents
                .Where(s => s.BunkeringOperationId == bunkeringOperationId)
                .OrderByDescending(s => s.SpillStartTime)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> UpdateSpillAsync(UpdateOilSpillRequest request)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var incident = await context.OilSpillIncidents.FirstOrDefaultAsync(s => s.Id == request.Id);
            if (incident is null)
            {
                return false;
            }

            if (request.SpillName is not null)
            {
                incident.SpillName = request.SpillName;
            }

            if (request.SpillEndTime.HasValue)
            {
                incident.SpillEndTime = request.SpillEndTime;
            }

            if (request.Status.HasValue)
            {
                incident.Status = request.Status.Value;
            }

            if (request.EstimatedVolume.HasValue)
            {
                incident.EstimatedVolume = request.EstimatedVolume.Value;
            }

            if (request.ReleaseRate.HasValue)
            {
                incident.ReleaseRate = request.ReleaseRate;
            }

            incident.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CloseSpillAsync(int id, DateTime closedAt)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var incident = await context.OilSpillIncidents.FirstOrDefaultAsync(s => s.Id == id);
            if (incident is null)
            {
                return false;
            }

            incident.Status = OilSpillStatus.Closed;
            incident.SpillEndTime = closedAt;
            incident.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();
            return true;
        }
    }
}
