using AlgoaBayBMT.Services.Models;
using AlgoaBayBMT.Shared.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;

namespace AlgoaBayBMT.Services.Interfaces
{
    public interface ITrainingAssetStorageService
    {
        Task<OperationResult<TrainingMediaUploadModel>> SaveCourseThumbnailAsync(IBrowserFile file, string? uploadedByUserId, CancellationToken cancellationToken = default);
        Task<OperationResult<TrainingMediaUploadModel>> SaveLessonAssetAsync(IBrowserFile file, string assetCategory, string? uploadedByUserId, CancellationToken cancellationToken = default);
        Task<OperationResult<TrainingMediaUploadModel>> SaveLessonAvatarVideoAsync(IBrowserFile file, string? uploadedByUserId, CancellationToken cancellationToken = default);
        Task<OperationResult<TrainingMediaUploadModel>> SaveRichTextImageAsync(IBrowserFile file, string? uploadedByUserId, CancellationToken cancellationToken = default);
        Task<OperationResult<TrainingMediaUploadModel>> SaveRichTextImageAsync(IFormFile file, string? uploadedByUserId, CancellationToken cancellationToken = default);
        Task DeleteMediaAssetAsync(Guid? mediaAssetId, CancellationToken cancellationToken = default);
        Task DeleteFilesAsync(IEnumerable<string> urls, CancellationToken cancellationToken = default);
    }
}
