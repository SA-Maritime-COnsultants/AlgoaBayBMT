using AlgoaBayBMT.Services.Models;
using AlgoaBayBMT.Shared.Models;

namespace AlgoaBayBMT.Services.Interfaces
{
    public interface ITrainingManagementService
    {
        Task<TrainingDashboardModel> GetDashboardAsync(CancellationToken cancellationToken = default);
        Task<OperationResult> ResetTrainingDataAsync(string? changedByUserId, CancellationToken cancellationToken = default);
        Task<List<TrainingCourseListItemModel>> GetCoursesAsync(CancellationToken cancellationToken = default);
        Task<TrainingCourseEditModel?> GetCourseAsync(Guid courseId, CancellationToken cancellationToken = default);
        Task<OperationResult<TrainingCourseEditModel>> SaveCourseAsync(TrainingCourseEditModel model, string? changedByUserId, CancellationToken cancellationToken = default);
        Task<OperationResult> ToggleCourseActiveAsync(Guid courseId, bool isActive, string? changedByUserId, CancellationToken cancellationToken = default);
        Task<OperationResult<CourseVersion>> CreateNextCourseVersionAsync(Guid courseId, string? changedByUserId, CancellationToken cancellationToken = default);
        Task<OperationResult> PublishCourseVersionAsync(Guid courseId, Guid courseVersionId, string? changedByUserId, CancellationToken cancellationToken = default);
        Task<TrainingCourseBuilderModel?> GetCourseBuilderAsync(Guid courseId, CancellationToken cancellationToken = default);
        Task<OperationResult<TrainingModuleEditModel>> SaveModuleAsync(TrainingModuleEditModel model, string? changedByUserId, CancellationToken cancellationToken = default);
        Task<OperationResult> DeleteModuleAsync(Guid moduleId, string? changedByUserId, CancellationToken cancellationToken = default);
        Task<OperationResult> MoveModuleAsync(Guid moduleId, int direction, string? changedByUserId, CancellationToken cancellationToken = default);
        Task<OperationResult<TrainingLessonEditModel>> SaveLessonAsync(TrainingLessonEditModel model, string? changedByUserId, CancellationToken cancellationToken = default);
        Task<OperationResult> DeleteLessonAsync(Guid lessonId, string? changedByUserId, CancellationToken cancellationToken = default);
        Task<OperationResult> MoveLessonAsync(Guid lessonId, int direction, string? changedByUserId, CancellationToken cancellationToken = default);
        Task<OperationResult<TrainingLessonBlockEditModel>> SaveLessonBlockAsync(TrainingLessonBlockEditModel model, string? changedByUserId, CancellationToken cancellationToken = default);
        Task<OperationResult> DeleteLessonBlockAsync(Guid lessonBlockId, string? changedByUserId, CancellationToken cancellationToken = default);
        Task<OperationResult> MoveLessonBlockAsync(Guid lessonBlockId, int direction, string? changedByUserId, CancellationToken cancellationToken = default);
        Task<TrainingQuestionBankPageModel?> GetQuestionBankPageAsync(Guid courseId, CancellationToken cancellationToken = default);
        Task<OperationResult<TrainingCourseAssessmentEditModel>> SaveCourseAssessmentAsync(TrainingCourseAssessmentEditModel model, string? changedByUserId, CancellationToken cancellationToken = default);
        Task<OperationResult<TrainingQuestionBankQuestionEditModel>> SaveQuestionBankQuestionAsync(TrainingQuestionBankQuestionEditModel model, string? changedByUserId, CancellationToken cancellationToken = default);
        Task<OperationResult> DeleteQuestionBankQuestionAsync(Guid trainingQuestionBankQuestionId, string? changedByUserId, CancellationToken cancellationToken = default);
    }
}
