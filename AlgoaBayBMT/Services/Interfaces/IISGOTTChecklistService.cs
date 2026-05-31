using AlgoaBayBMT.Shared.Models;

namespace AlgoaBayBMT.Services.Interfaces
{
    public interface IISGOTTChecklistService
    {
        Task<OperationResult> ValidateMandatoryResponsesAsync(IReadOnlyCollection<ISGOTTStageTemplate> templates, IReadOnlyCollection<ISGOTTChecklistStageResponse> responses, CancellationToken cancellationToken = default);
    }
}
