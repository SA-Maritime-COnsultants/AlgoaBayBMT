using AlgoaBayBMT.Models.Training;

namespace AlgoaBayBMT.Services.Interfaces
{
    public interface ICompetencyService
    {
        CompetencyEvaluationResult EvaluateTask(CompetencyTask task, IReadOnlyList<string> selectedChoiceIds);
    }
}
