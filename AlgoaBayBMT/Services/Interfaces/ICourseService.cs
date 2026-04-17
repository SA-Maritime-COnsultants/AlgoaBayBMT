using AlgoaBayBMT.Models.Training;

namespace AlgoaBayBMT.Services.Interfaces
{
    public interface ICourseService
    {
        Task<TrainingModuleCatalog> GetEnvironmentalLawComplianceModuleAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Course>> GetCoursesAsync(CancellationToken cancellationToken = default);
        Task<Course?> GetCourseAsync(string courseSlug, CancellationToken cancellationToken = default);
        Task<Lesson?> GetLessonAsync(string courseSlug, string lessonSlug, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Question>> GetModuleExamPoolAsync(CancellationToken cancellationToken = default);
    }
}
