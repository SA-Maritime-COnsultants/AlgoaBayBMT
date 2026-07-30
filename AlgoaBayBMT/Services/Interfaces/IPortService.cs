using AlgoaBayBMT.Shared.Models;

namespace AlgoaBayBMT.Services.Interfaces
{
    public interface IPortService
    {
        Task<List<Port>> GetByAreaAsync(int areaId);
        Task<Port> CreateAsync(Port port);
        Task UpdateAsync(Port port);
        Task DeactivateAsync(int id);
    }
}
