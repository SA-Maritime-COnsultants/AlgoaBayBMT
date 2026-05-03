using AlgoaBayBMT.Shared.Models;

namespace AlgoaBayBMT.Services.Interfaces
{
    public interface IBargeService
    {
        Task<List<BunkerBarge>> GetAllAsync();
        Task<BunkerBarge?> GetByIdAsync(int id);
        Task<BunkerBarge> CreateAsync(BunkerBarge barge);
        Task UpdateAsync(BunkerBarge barge);
    }
}
