using AlgoaBayBMT.Data;
using AlgoaBayBMT.Services.Interfaces;
using AlgoaBayBMT.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace AlgoaBayBMT.Services.Crew
{
    public class CrewChangeHistoryService(ApplicationDbContext dbContext) : ICrewChangeHistoryService
    {
        public async Task RecordAsync(CrewChangeHistory entry, CancellationToken cancellationToken = default)
        {
            entry.ChangedOnUtc = DateTime.UtcNow;
            dbContext.CrewChangeHistory.Add(entry);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public Task<List<CrewChangeHistory>> GetHistoryAsync(int vesselId, CancellationToken cancellationToken = default) =>
            dbContext.CrewChangeHistory
                .AsNoTracking()
                .Where(x => x.VesselId == vesselId)
                .OrderByDescending(x => x.ChangedOnUtc)
                .ToListAsync(cancellationToken);
    }
}
