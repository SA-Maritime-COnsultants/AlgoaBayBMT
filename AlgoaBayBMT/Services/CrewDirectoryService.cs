using AlgoaBayBMT.Data;
using AlgoaBayBMT.Services.Interfaces;
using AlgoaBayBMT.Services.Models;
using AlgoaBayBMT.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace AlgoaBayBMT.Services
{
    public class CrewDirectoryService(ApplicationDbContext dbContext) : ICrewDirectoryService
    {
        public Task<List<ApplicationUser>> SearchSeafarersAsync(string? searchTerm, CancellationToken cancellationToken = default)
        {
            var query = dbContext.Users.AsNoTracking().Include(x => x.Vessel).AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(x =>
                    (x.FullName != null && x.FullName.Contains(searchTerm)) ||
                    (x.Email != null && x.Email.Contains(searchTerm)));
            }

            return query.OrderBy(x => x.FullName).ThenBy(x => x.Email).ToListAsync(cancellationToken);
        }

        public async Task<List<CrewComplianceSummaryItem>> GetComplianceSummaryAsync(int vesselId, DateTime? asOfUtc = null, CancellationToken cancellationToken = default)
        {
            var pointInTime = asOfUtc ?? DateTime.UtcNow;
            var crewEntries = await dbContext.VesselCrewListEntries
                .AsNoTracking()
                .Where(x => x.VesselId == vesselId && x.JoinedOnUtc <= pointInTime && (x.LeftOnUtc == null || x.LeftOnUtc >= pointInTime))
                .Include(x => x.Vessel)
                .ToListAsync(cancellationToken);

            var users = await dbContext.Users
                .Where(x => crewEntries.Select(c => c.UserId).Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, cancellationToken);

            return crewEntries.Select(entry => new CrewComplianceSummaryItem
            {
                UserId = entry.UserId,
                CrewMemberName = users.TryGetValue(entry.UserId, out var user) ? (user.FullName ?? user.Email ?? entry.UserId) : entry.UserId,
                VesselId = entry.VesselId,
                VesselName = entry.Vessel?.Name ?? string.Empty,
                OnboardRoleName = entry.OnboardRoleName,
                ComplianceState = entry.ComplianceState,
                ComplianceExpiresOnUtc = entry.ComplianceExpiresOnUtc
            }).OrderBy(x => x.CrewMemberName).ToList();
        }
    }
}
