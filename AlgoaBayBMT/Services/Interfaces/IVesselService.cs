using AlgoaBayBMT.Shared.Models;

namespace AlgoaBayBMT.Services.Interfaces
{
    public interface IVesselService
    {
        Task<List<Vessel>> GetVesselsAsync(CancellationToken cancellationToken = default);
        Task<OperationResult<Vessel>> CreateVesselAsync(Vessel vessel, CancellationToken cancellationToken = default);
        Task<OperationResult<Vessel>> UpdateVesselAsync(Vessel vessel, CancellationToken cancellationToken = default);
        Task<OperationResult> DeleteVesselAsync(int vesselId, CancellationToken cancellationToken = default);
        Task<Vessel?> GetVesselAsync(int vesselId, CancellationToken cancellationToken = default);
    }
}
