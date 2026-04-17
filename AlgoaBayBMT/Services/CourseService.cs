using AlgoaBayBMT.Models.Training;
using AlgoaBayBMT.Services.Interfaces;
using AlgoaBayBMT.Services.Training;

namespace AlgoaBayBMT.Services
{
    public class CourseService : ICourseService
    {
        private static readonly TrainingModuleCatalog ModuleCatalog = EnvironmentalLawComplianceContent.BuildModule();

        public Task<TrainingModuleCatalog> GetEnvironmentalLawComplianceModuleAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(ModuleCatalog);

        public Task<IReadOnlyList<Course>> GetCoursesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(ModuleCatalog.Courses);

        public Task<Course?> GetCourseAsync(string courseSlug, CancellationToken cancellationToken = default)
            => Task.FromResult(ModuleCatalog.Courses.FirstOrDefault(x => string.Equals(x.Slug, courseSlug, StringComparison.OrdinalIgnoreCase)));

        public async Task<Lesson?> GetLessonAsync(string courseSlug, string lessonSlug, CancellationToken cancellationToken = default)
        {
            var course = await GetCourseAsync(courseSlug, cancellationToken);
            return course?.Lessons.FirstOrDefault(x => string.Equals(x.Slug, lessonSlug, StringComparison.OrdinalIgnoreCase));
        }

        public Task<IReadOnlyList<Question>> GetModuleExamPoolAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(ModuleCatalog.ExamPool);
    }
}
