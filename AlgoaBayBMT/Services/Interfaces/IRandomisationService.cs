using AlgoaBayBMT.Models.Training;

namespace AlgoaBayBMT.Services.Interfaces
{
    public interface IRandomisationService
    {
        IReadOnlyList<Question> CreateExamQuestionSet(IReadOnlyList<Question> examPool, int questionCount);
    }
}
