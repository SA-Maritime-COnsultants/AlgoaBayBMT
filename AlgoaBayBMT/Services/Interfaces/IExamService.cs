using AlgoaBayBMT.Models.Training;

namespace AlgoaBayBMT.Services.Interfaces
{
    public interface IExamService
    {
        Task<ExamAttempt> CreateEnvironmentalLawComplianceExamAsync(CancellationToken cancellationToken = default);
        Task<ExamEvaluationResult> GradeExamAsync(ExamAttempt attempt, Dictionary<string, List<int>> answers, string? userId, CancellationToken cancellationToken = default);
    }
}
