using AlgoaBayBMT.Services.Interfaces;
using AlgoaBayBMT.Shared.Models;

namespace AlgoaBayBMT.Services
{
    public class ISGOTTChecklistService : IISGOTTChecklistService
    {
        public Task<OperationResult> ValidateMandatoryResponsesAsync(IReadOnlyCollection<ISGOTTStageTemplate> templates, IReadOnlyCollection<ISGOTTChecklistStageResponse> responses, CancellationToken cancellationToken = default)
        {
            foreach (var item in templates.SelectMany(x => x.Items).Where(x => x.IsMandatory && x.IsActive))
            {
                var response = responses.SelectMany(x => x.ItemResponses).FirstOrDefault(x => x.ItemTemplateId == item.Id);
                if (response is null)
                {
                    return Task.FromResult(OperationResult.Failure($"Mandatory ISGOTT item missing: {item.Text}"));
                }

                var hasValue = item.ItemType switch
                {
                    ISGOTTItemType.YesNoNa => response.YesNoNaValue.HasValue,
                    ISGOTTItemType.Text => !string.IsNullOrWhiteSpace(response.TextValue),
                    ISGOTTItemType.Numeric => response.NumericValue.HasValue,
                    _ => false
                };

                if (!hasValue)
                {
                    return Task.FromResult(OperationResult.Failure($"Mandatory ISGOTT item incomplete: {item.Text}"));
                }
            }

            return Task.FromResult(OperationResult.Success());
        }
    }
}
