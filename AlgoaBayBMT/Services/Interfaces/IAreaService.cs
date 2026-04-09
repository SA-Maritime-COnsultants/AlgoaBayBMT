using AlgoaBayBMT.Shared.Models;

namespace AlgoaBayBMT.Services.Interfaces
{
    public interface IAreaService
    {
        Task<List<OperationalArea>> GetAreasAsync(CancellationToken cancellationToken = default);
        Task<List<Port>> GetPortsAsync(CancellationToken cancellationToken = default);
        Task<List<Bay>> GetBaysAsync(CancellationToken cancellationToken = default);
        Task<List<Anchorage>> GetAnchoragesAsync(CancellationToken cancellationToken = default);
        Task<OperationResult<OperationalArea>> CreateAreaAsync(OperationalArea area, CancellationToken cancellationToken = default);
        Task<OperationResult<OperationalArea>> UpdateAreaAsync(OperationalArea area, CancellationToken cancellationToken = default);
        Task<OperationResult<Port>> CreatePortAsync(Port port, CancellationToken cancellationToken = default);
        Task<OperationResult<Port>> UpdatePortAsync(Port port, CancellationToken cancellationToken = default);
        Task<OperationResult<Bay>> CreateBayAsync(Bay bay, CancellationToken cancellationToken = default);
        Task<OperationResult<Bay>> UpdateBayAsync(Bay bay, CancellationToken cancellationToken = default);
        Task<OperationResult<Anchorage>> CreateAnchorageAsync(Anchorage anchorage, CancellationToken cancellationToken = default);
        Task<OperationResult<Anchorage>> UpdateAnchorageAsync(Anchorage anchorage, CancellationToken cancellationToken = default);
        Task<OperationResult> DeleteAreaAsync(int areaId, CancellationToken cancellationToken = default);
        Task<OperationResult> DeletePortAsync(int portId, CancellationToken cancellationToken = default);
        Task<OperationResult> DeleteBayAsync(int bayId, CancellationToken cancellationToken = default);
        Task<OperationResult> DeleteAnchorageAsync(int anchorageId, CancellationToken cancellationToken = default);
        Task<OperationResult> AssignUserToAreaAsync(string userId, int areaId, bool isPrimary, string? assignedByUserId, CancellationToken cancellationToken = default);
        Task<OperationResult> AssignCompanyToAreaAsync(int companyId, int areaId, string? assignedByUserId, CancellationToken cancellationToken = default);
        Task<OperationResult> AssignAuthorityToAreaAsync(int authorityContactId, int areaId, string? assignedByUserId, CancellationToken cancellationToken = default);
    }
}
