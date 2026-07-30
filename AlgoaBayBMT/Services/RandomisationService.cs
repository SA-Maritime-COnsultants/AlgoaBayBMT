using AlgoaBayBMT.Models.Training;
using AlgoaBayBMT.Services.Interfaces;

namespace AlgoaBayBMT.Services
{
    public class RandomisationService : IRandomisationService
    {
        public IReadOnlyList<Question> CreateExamQuestionSet(IReadOnlyList<Question> examPool, int questionCount)
        {
            if (examPool.Count < questionCount)
            {
                throw new InvalidOperationException("The exam pool does not contain enough questions.");
            }

            var mcq = examPool.Where(x => x.Kind == QuestionKind.MultipleChoice).OrderBy(_ => Random.Shared.Next()).Take(12);
            var tf = examPool.Where(x => x.Kind == QuestionKind.TrueFalse).OrderBy(_ => Random.Shared.Next()).Take(8);
            var scenario = examPool.Where(x => x.Kind == QuestionKind.Scenario).OrderBy(_ => Random.Shared.Next()).Take(5);

            var combined = mcq
                .Concat(tf)
                .Concat(scenario)
                .DistinctBy(x => x.QuestionId)
                .Take(questionCount)
                .ToList();

            if (combined.Count < questionCount)
            {
                var remaining = examPool
                    .Where(x => combined.All(y => y.QuestionId != x.QuestionId))
                    .OrderBy(_ => Random.Shared.Next())
                    .Take(questionCount - combined.Count);

                combined.AddRange(remaining);
            }

            return combined
                .OrderBy(_ => Random.Shared.Next())
                .ToList();
        }
    }
}
