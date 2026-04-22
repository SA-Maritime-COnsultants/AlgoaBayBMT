using AlgoaBayBMT.Data;
using AlgoaBayBMT.Services.Interfaces;
using AlgoaBayBMT.Services.Models;
using AlgoaBayBMT.Shared.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Security.Cryptography;

namespace AlgoaBayBMT.Services
{
    public sealed class TrainingAssetStorageService(
        IDbContextFactory<ApplicationDbContext> dbContextFactory,
        IWebHostEnvironment environment) : ITrainingAssetStorageService
    {
        private const string TrainingUploadsRoot = "uploads/training";
        private static readonly HashSet<string> ImageExtensions = [".jpg", ".jpeg", ".png", ".webp", ".gif"];
        private static readonly HashSet<string> VideoExtensions = [".mp4", ".webm", ".mov", ".m4v"];
        private static readonly HashSet<string> AttachmentExtensions = [".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".zip"];

        public Task<OperationResult<TrainingMediaUploadModel>> SaveCourseThumbnailAsync(IBrowserFile file, string? uploadedByUserId, CancellationToken cancellationToken = default)
            => SaveAssetAsync(file, "course-thumbnails", ImageExtensions, 5 * 1024 * 1024, uploadedByUserId, cancellationToken);

        public Task<OperationResult<TrainingMediaUploadModel>> SaveLessonAssetAsync(IBrowserFile file, string assetCategory, string? uploadedByUserId, CancellationToken cancellationToken = default)
        {
            var normalizedCategory = string.IsNullOrWhiteSpace(assetCategory)
                ? "lesson-assets"
                : assetCategory.Trim().ToLowerInvariant();

            return normalizedCategory switch
            {
                "video" => SaveAssetAsync(file, "videos", VideoExtensions, 250L * 1024 * 1024, uploadedByUserId, cancellationToken),
                "image" => SaveAssetAsync(file, "images", ImageExtensions, 10 * 1024 * 1024, uploadedByUserId, cancellationToken),
                _ => SaveAssetAsync(file, "attachments", AttachmentExtensions, 25L * 1024 * 1024, uploadedByUserId, cancellationToken)
            };
        }

        public Task<OperationResult<TrainingMediaUploadModel>> SaveLessonAvatarVideoAsync(IBrowserFile file, string? uploadedByUserId, CancellationToken cancellationToken = default)
            => SaveAssetAsync(file, "avatar-videos", VideoExtensions, 250L * 1024 * 1024, uploadedByUserId, cancellationToken);

        public Task<OperationResult<TrainingMediaUploadModel>> SaveRichTextImageAsync(IBrowserFile file, string? uploadedByUserId, CancellationToken cancellationToken = default)
            => SaveAssetAsync(file, "rte-images", ImageExtensions, 10 * 1024 * 1024, uploadedByUserId, cancellationToken);

        public Task<OperationResult<TrainingMediaUploadModel>> SaveRichTextImageAsync(IFormFile file, string? uploadedByUserId, CancellationToken cancellationToken = default)
            => SaveAssetAsync(file, "rte-images", ImageExtensions, 10 * 1024 * 1024, uploadedByUserId, cancellationToken);

        public async Task DeleteMediaAssetAsync(Guid? mediaAssetId, CancellationToken cancellationToken = default)
        {
            if (!mediaAssetId.HasValue)
            {
                return;
            }

            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var asset = await dbContext.MediaAssets.FirstOrDefaultAsync(x => x.MediaAssetId == mediaAssetId.Value, cancellationToken);
            if (asset is null)
            {
                return;
            }

            DeletePhysicalFile(asset.RelativePath);
            dbContext.MediaAssets.Remove(asset);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public Task DeleteFilesAsync(IEnumerable<string> urls, CancellationToken cancellationToken = default)
        {
            foreach (var url in urls.Where(static x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                DeletePhysicalFile(url);
            }

            return Task.CompletedTask;
        }

        public static bool TryMapToStreamEndpoint(string? url, IWebHostEnvironment environment, out string streamUrl)
        {
            streamUrl = string.Empty;
            var relativePath = NormalizeRelativePath(url);
            if (string.IsNullOrWhiteSpace(relativePath) || !relativePath.StartsWith(TrainingUploadsRoot, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var extension = Path.GetExtension(relativePath);
            if (!VideoExtensions.Contains(extension.ToLowerInvariant()))
            {
                return false;
            }

            var absolutePath = Path.Combine(environment.WebRootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(absolutePath))
            {
                return false;
            }

            streamUrl = $"/training-media/stream?path={Uri.EscapeDataString(relativePath)}";
            return true;
        }

        public static string GetContentTypeForPath(string path)
            => GetContentType(Path.GetExtension(path));

        private async Task<OperationResult<TrainingMediaUploadModel>> SaveAssetAsync(
            IBrowserFile file,
            string category,
            HashSet<string> allowedExtensions,
            long maxSizeBytes,
            string? uploadedByUserId,
            CancellationToken cancellationToken)
        {
            if (file is null)
            {
                return OperationResult<TrainingMediaUploadModel>.Failure("No file was selected.");
            }

            var extension = Path.GetExtension(file.Name);
            if (string.IsNullOrWhiteSpace(extension) || !allowedExtensions.Contains(extension.ToLowerInvariant()))
            {
                return OperationResult<TrainingMediaUploadModel>.Failure("The selected file type is not supported.");
            }

            if (file.Size <= 0 || file.Size > maxSizeBytes)
            {
                return OperationResult<TrainingMediaUploadModel>.Failure($"The selected file exceeds the allowed size of {maxSizeBytes / 1024 / 1024} MB.");
            }

            var safeName = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
            var relativePath = Path.Combine(TrainingUploadsRoot, category, safeName).Replace("\\", "/");
            var absolutePath = Path.Combine(environment.WebRootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
            var directory = Path.GetDirectoryName(absolutePath)!;
            Directory.CreateDirectory(directory);

            await using var inputStream = file.OpenReadStream(maxSizeBytes, cancellationToken);
            await using var outputStream = new FileStream(absolutePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            using var sha256 = SHA256.Create();
            await inputStream.CopyToAsync(outputStream, cancellationToken);
            await outputStream.FlushAsync(cancellationToken);
            outputStream.Position = 0;
            var hashBytes = await sha256.ComputeHashAsync(outputStream, cancellationToken);
            var hash = Convert.ToHexString(hashBytes);

            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var asset = new MediaAsset
            {
                MediaAssetId = Guid.NewGuid(),
                FileName = file.Name,
                StoredFileName = safeName,
                RelativePath = relativePath,
                ContentType = string.IsNullOrWhiteSpace(file.ContentType) ? GetContentType(extension) : file.ContentType,
                FileSizeBytes = file.Size,
                UploadedByUserId = uploadedByUserId,
                UploadedOnUtc = DateTime.UtcNow,
                HashSha256 = hash
            };

            dbContext.MediaAssets.Add(asset);
            await dbContext.SaveChangesAsync(cancellationToken);

            var result = new TrainingMediaUploadModel
            {
                MediaAssetId = asset.MediaAssetId,
                FileName = asset.FileName,
                ContentType = asset.ContentType,
                FileSizeBytes = asset.FileSizeBytes,
                Url = $"/{asset.RelativePath.TrimStart('/')}"
            };

            return OperationResult<TrainingMediaUploadModel>.Success(result, "File uploaded successfully.");
        }

        private async Task<OperationResult<TrainingMediaUploadModel>> SaveAssetAsync(
            IFormFile file,
            string category,
            HashSet<string> allowedExtensions,
            long maxSizeBytes,
            string? uploadedByUserId,
            CancellationToken cancellationToken)
        {
            if (file is null)
            {
                return OperationResult<TrainingMediaUploadModel>.Failure("No file was selected.");
            }

            var extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(extension) || !allowedExtensions.Contains(extension.ToLowerInvariant()))
            {
                return OperationResult<TrainingMediaUploadModel>.Failure("The selected file type is not supported.");
            }

            if (file.Length <= 0 || file.Length > maxSizeBytes)
            {
                return OperationResult<TrainingMediaUploadModel>.Failure($"The selected file exceeds the allowed size of {maxSizeBytes / 1024 / 1024} MB.");
            }

            var safeName = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
            var relativePath = Path.Combine(TrainingUploadsRoot, category, safeName).Replace("\\", "/");
            var absolutePath = Path.Combine(environment.WebRootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);

            await using (var outputStream = new FileStream(absolutePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            {
                await using var inputStream = file.OpenReadStream();
                await inputStream.CopyToAsync(outputStream, cancellationToken);
                await outputStream.FlushAsync(cancellationToken);
            }

            string hash;
            await using (var fileStream = new FileStream(absolutePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using var sha256 = SHA256.Create();
                hash = Convert.ToHexString(await sha256.ComputeHashAsync(fileStream, cancellationToken));
            }

            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var asset = new MediaAsset
            {
                MediaAssetId = Guid.NewGuid(),
                FileName = file.FileName,
                StoredFileName = safeName,
                RelativePath = relativePath,
                ContentType = string.IsNullOrWhiteSpace(file.ContentType) ? GetContentType(extension) : file.ContentType,
                FileSizeBytes = file.Length,
                UploadedByUserId = uploadedByUserId,
                UploadedOnUtc = DateTime.UtcNow,
                HashSha256 = hash
            };

            dbContext.MediaAssets.Add(asset);
            await dbContext.SaveChangesAsync(cancellationToken);

            return OperationResult<TrainingMediaUploadModel>.Success(new TrainingMediaUploadModel
            {
                MediaAssetId = asset.MediaAssetId,
                FileName = asset.FileName,
                ContentType = asset.ContentType,
                FileSizeBytes = asset.FileSizeBytes,
                Url = $"/{asset.RelativePath.TrimStart('/')}"
            }, "File uploaded successfully.");
        }

        private void DeletePhysicalFile(string? relativeOrAbsoluteUrl)
        {
            var relativePath = NormalizeRelativePath(relativeOrAbsoluteUrl);
            if (string.IsNullOrWhiteSpace(relativePath) || !relativePath.StartsWith(TrainingUploadsRoot, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var absolutePath = Path.Combine(environment.WebRootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(absolutePath))
            {
                File.Delete(absolutePath);
            }
        }

        private static string NormalizeRelativePath(string? relativeOrAbsoluteUrl)
        {
            if (string.IsNullOrWhiteSpace(relativeOrAbsoluteUrl))
            {
                return string.Empty;
            }

            var normalized = relativeOrAbsoluteUrl.Trim();
            if (Uri.TryCreate(normalized, UriKind.Absolute, out var uri))
            {
                normalized = uri.AbsolutePath;
            }

            return normalized.TrimStart('~').TrimStart('/').Replace("\\", "/");
        }

        private static string GetContentType(string extension) => extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            ".mp4" => "video/mp4",
            ".webm" => "video/webm",
            ".mov" => "video/quicktime",
            ".m4v" => "video/x-m4v",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".ppt" => "application/vnd.ms-powerpoint",
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            ".zip" => "application/zip",
            _ => "application/octet-stream"
        };
    }
}
