using AlgoaBayBMT.Models.Training;
using AlgoaBayBMT.Services.Interfaces;

namespace AlgoaBayBMT.Services
{
    public class ExamService(ICourseService courseService, IRandomisationService randomisationService) : IExamService
    {
        public async Task<ExamAttempt> CreateEnvironmentalLawComplianceExamAsync(CancellationToken cancellationToken = default)
        {
            var examPool = await courseService.GetModuleExamPoolAsync(cancellationToken);
            return new ExamAttempt
            {
                ModuleCode = "M1-ELC",
                Questions = randomisationService.CreateExamQuestionSet(examPool, 25)
            };
        }

        public Task<ExamEvaluationResult> GradeExamAsync(ExamAttempt attempt, Dictionary<string, List<int>> answers, string? userId, CancellationToken cancellationToken = default)
        {
            var results = new List<QuestionResult>();
            var availablePoints = attempt.Questions.Sum(x => x.Points);
            var earnedPoints = 0;

            foreach (var question in attempt.Questions)
            {
                var submittedAnswers = answers.TryGetValue(question.QuestionId, out var selected)
                    ? selected.OrderBy(x => x).ToList()
                    : new List<int>();
                var correctAnswers = question.CorrectOptionIndexes.OrderBy(x => x).ToList();
                var isCorrect = submittedAnswers.SequenceEqual(correctAnswers);
                var awardedPoints = isCorrect ? question.Points : 0;
                earnedPoints += awardedPoints;

                results.Add(new QuestionResult
                {
                    QuestionId = question.QuestionId,
                    IsCorrect = isCorrect,
                    AwardedPoints = awardedPoints,
                    Explanation = question.Explanation
                });
            }

            attempt.Answers = answers;
            attempt.SubmittedOnUtc = DateTime.UtcNow;
            attempt.ScorePercent = availablePoints == 0 ? 0 : Math.Round((decimal)earnedPoints / availablePoints * 100m, 2);
            attempt.Passed = attempt.ScorePercent >= 80m;
            attempt.Summary = attempt.Passed
                ? $"Passed with {attempt.ScorePercent:0.##}%"
                : $"Not yet competent. Score: {attempt.ScorePercent:0.##}%";

            return Task.FromResult(new ExamEvaluationResult
            {
                Attempt = attempt,
                Results = results,
                EarnedPoints = earnedPoints,
                AvailablePoints = availablePoints
            });
        }
    }
}
