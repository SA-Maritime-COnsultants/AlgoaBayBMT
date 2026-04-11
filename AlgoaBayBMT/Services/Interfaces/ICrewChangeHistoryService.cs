using AlgoaBayBMT.Shared.Models;

namespace AlgoaBayBMT.Services.Interfaces
{
    public interface ICrewChangeHistoryService
    {
        Task RecordAsync(CrewChangeHistory entry, CancellationToken cancellationToken = default);
        Task<List<CrewChangeHistory>> GetHistoryAsync(int vesselId, CancellationToken cancellationToken = default);
    }
}
