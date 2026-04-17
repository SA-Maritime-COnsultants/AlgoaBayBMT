using AlgoaBayBMT.Services.Models;
using AlgoaBayBMT.Shared.Models;

namespace AlgoaBayBMT.Services.Interfaces;

public interface ILearnerTrainingService
{
    Task<MyTrainingDashboardModel> GetDashboardAsync(string userId, CancellationToken cancellationToken = default);
    Task<TrainingCoursePlayerModel?> GetCoursePlayerAsync(string userId, Guid courseId, Guid? lessonId = null, Guid? blockId = null, CancellationToken cancellationToken = default);
    Task<OperationResult> CompleteBlockAsync(string userId, Guid courseId, Guid lessonId, Guid blockId, CancellationToken cancellationToken = default);
    Task<OperationResult<TrainingQuizSubmissionResultModel>> SubmitQuizBlockAsync(string userId, Guid courseId, Guid lessonId, Guid blockId, Dictionary<int, List<int>> answers, CancellationToken cancellationToken = default);
    Task<OperationResult<TrainingAssessmentSubmissionResultModel>> SubmitAssessmentBlockAsync(string userId, Guid courseId, Guid lessonId, Guid blockId, Dictionary<Guid, Guid?> answers, CancellationToken cancellationToken = default);
    Task<TrainingCertificateViewModel?> GetCertificateAsync(string userId, Guid certificateId, CancellationToken cancellationToken = default);
}
