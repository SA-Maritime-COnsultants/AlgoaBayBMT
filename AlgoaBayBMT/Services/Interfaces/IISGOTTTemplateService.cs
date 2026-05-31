using AlgoaBayBMT.Shared.Models;

namespace AlgoaBayBMT.Services.Interfaces
{
    public interface IISGOTTTemplateService
    {
        Task<List<ISGOTTStageTemplate>> GetActiveTemplatesAsync(CancellationToken cancellationToken = default);
    }
}
