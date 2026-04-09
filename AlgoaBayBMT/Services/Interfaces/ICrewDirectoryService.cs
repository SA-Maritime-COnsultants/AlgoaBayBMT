using AlgoaBayBMT.Data;
using AlgoaBayBMT.Services.Models;

namespace AlgoaBayBMT.Services.Interfaces
{
    public interface ICrewDirectoryService
    {
        Task<List<ApplicationUser>> SearchSeafarersAsync(string? searchTerm, CancellationToken cancellationToken = default);
        Task<List<CrewComplianceSummaryItem>> GetComplianceSummaryAsync(int vesselId, DateTime? asOfUtc = null, CancellationToken cancellationToken = default);
    }
}
