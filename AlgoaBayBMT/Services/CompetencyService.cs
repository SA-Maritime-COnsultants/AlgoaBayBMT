using AlgoaBayBMT.Models.Training;
using AlgoaBayBMT.Services.Interfaces;

namespace AlgoaBayBMT.Services
{
    public class CompetencyService : ICompetencyService
    {
        public CompetencyEvaluationResult EvaluateTask(CompetencyTask task, IReadOnlyList<string> selectedChoiceIds)
        {
            var selected = new HashSet<string>(selectedChoiceIds, StringComparer.OrdinalIgnoreCase);
            var feedback = new List<string>();
            var available = 0m;
            var earned = 0m;

            foreach (var step in task.Steps)
            {
                var preferredChoice = step.Choices.OrderByDescending(x => x.Score).FirstOrDefault();
                if (preferredChoice is not null)
                {
                    available += preferredChoice.Score;
                }

                var chosen = step.Choices.FirstOrDefault(x => selected.Contains(x.ChoiceId));
                if (chosen is null)
                {
                    feedback.Add($"{step.Title}: No response captured.");
                    continue;
                }

                earned += chosen.Score;
                feedback.Add($"{step.Title}: {chosen.Feedback}");
            }

            var scorePercent = available <= 0m ? 0m : Math.Round(earned / available * 100m, 2);
            return new CompetencyEvaluationResult
            {
                TaskId = task.TaskId,
                ScorePercent = scorePercent,
                Competent = scorePercent >= 75m,
                Feedback = feedback
            };
        }
    }
}
