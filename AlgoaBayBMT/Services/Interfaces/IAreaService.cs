using AlgoaBayBMT.Shared.Models;

namespace AlgoaBayBMT.Services.Interfaces
{
    public interface IAreaService
    {
        Task<List<AreaOfOperation>> GetAllAsync();
        Task<AreaOfOperation?> GetByIdAsync(int id);
        Task<AreaOfOperation> CreateAsync(AreaOfOperation area);
        Task UpdateAsync(AreaOfOperation area);
        Task DeactivateAsync(int id);
    }
}
