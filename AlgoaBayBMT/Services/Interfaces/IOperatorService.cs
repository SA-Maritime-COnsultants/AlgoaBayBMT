using AlgoaBayBMT.Shared.Models;

namespace AlgoaBayBMT.Services.Interfaces
{
    public interface IOperatorService
    {
        Task<List<BunkerOperator>> GetAllAsync();
        Task<BunkerOperator?> GetByIdAsync(int id);
        Task<BunkerOperator> CreateAsync(BunkerOperator op);
        Task UpdateAsync(BunkerOperator op);
        Task AssignToAreaAsync(int operatorId, int areaId, DateTime from);
        Task EndAreaAssignmentAsync(int assignmentId, DateTime to);
    }
}
