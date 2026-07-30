using AlgoaBayBMT.Models.Authoring;
using AlgoaBayBMT.Services.Models;

namespace AlgoaBayBMT.Services.Interfaces;

public interface IContentAuthoringService
{
    Task<LessonAuthoringPageModel> GetLessonAuthoringPageAsync(int lessonId, CancellationToken cancellationToken = default);
    Task<List<LessonContentItem>> GetContentForLessonAsync(int lessonId, CancellationToken cancellationToken = default);
    Task<LessonContentItem> CreateContentItemAsync(LessonContentItem item, CancellationToken cancellationToken = default);
    Task<LessonContentItem> UpdateContentItemAsync(LessonContentItem item, CancellationToken cancellationToken = default);
    Task DeleteContentItemAsync(int id, CancellationToken cancellationToken = default);
}
