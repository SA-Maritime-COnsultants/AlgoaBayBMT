using AlgoaBayBMT.Shared.Models;

namespace AlgoaBayBMT.Services.Interfaces
{
    public interface IVesselService
    {
        Task<List<Vessel>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
        Task<List<Vessel>> GetByCompanyAsync(int bunkerOperatorId, bool includeInactive = false, CancellationToken cancellationToken = default);
        Task<Vessel?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Vessel> CreateAsync(Vessel vessel, string? performedByUserId, CancellationToken cancellationToken = default);
        Task<Vessel> UpdateAsync(Vessel vessel, string? performedByUserId, CancellationToken cancellationToken = default);
        Task SoftDeleteAsync(int id, string? performedByUserId, CancellationToken cancellationToken = default);
    }
}
