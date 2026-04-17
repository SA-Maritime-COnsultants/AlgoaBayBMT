using System.ComponentModel.DataAnnotations;

namespace AlgoaBayBMT.Models.Training
{
    public enum QuestionKind
    {
        MultipleChoice = 0,
        TrueFalse = 1,
        Scenario = 2
    }

    public sealed class TrainingModuleCatalog
    {
        public string ModuleCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ComplianceSummary { get; set; } = string.Empty;
        public IReadOnlyList<Course> Courses { get; set; } = [];
        public IReadOnlyList<CompetencyTask> ModuleCompetencyTasks { get; set; } = [];
        public IReadOnlyList<Question> ExamPool { get; set; } = [];
    }

    public sealed class Course
    {
        public string CourseCode { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Overview { get; set; } = string.Empty;
        public string RegulatoryReference { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int EstimatedMinutes { get; set; }
        public IReadOnlyList<Lesson> Lessons { get; set; } = [];
        public IReadOnlyList<Flashcard> Flashcards { get; set; } = [];
        public IReadOnlyList<Question> QuestionBank { get; set; } = [];
        public IReadOnlyList<CompetencyTask> ScenarioTasks { get; set; } = [];
    }

    public sealed class Lesson
    {
        public string LessonCode { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string NarrativeHtml { get; set; } = string.Empty;
        public IReadOnlyList<Slide> Slides { get; set; } = [];
        public IReadOnlyList<Question> KnowledgeChecks { get; set; } = [];
    }

    public sealed class Slide
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        public IReadOnlyList<string> Bullets { get; set; } = [];
        public string? Image { get; set; }
        public string? Caption { get; set; }
        public string? BodyHtml { get; set; }
    }

    public sealed class Flashcard
    {
        public string Category { get; set; } = string.Empty;
        public string FrontText { get; set; } = string.Empty;
        public string BackText { get; set; } = string.Empty;
    }

    public sealed class Question
    {
        public string QuestionId { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string? LessonCode { get; set; }
        public QuestionKind Kind { get; set; }
        public string Prompt { get; set; } = string.Empty;
        public string? ScenarioContext { get; set; }
        public IReadOnlyList<string> Options { get; set; } = [];
        public IReadOnlySet<int> CorrectOptionIndexes { get; set; } = new HashSet<int>();
        public bool AllowMultiple { get; set; }
        public string Explanation { get; set; } = string.Empty;
        public int Points { get; set; } = 1;
    }

    public sealed class ScenarioStep
    {
        public string StepId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Prompt { get; set; } = string.Empty;
        public string? VideoUrl { get; set; }
        public string? Guidance { get; set; }
        public IReadOnlyList<ScenarioChoice> Choices { get; set; } = [];
    }

    public sealed class ScenarioChoice
    {
        public string ChoiceId { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string? NextStepId { get; set; }
        public bool IsPreferred { get; set; }
        public decimal Score { get; set; }
        public string Feedback { get; set; } = string.Empty;
    }

    public sealed class CompetencyTask
    {
        public string TaskId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string SuccessCriteria { get; set; } = string.Empty;
        public IReadOnlyList<ScenarioStep> Steps { get; set; } = [];
    }

    public sealed class ExamAttempt
    {
        public Guid AttemptId { get; set; } = Guid.NewGuid();
        public string ModuleCode { get; set; } = string.Empty;
        public DateTime StartedOnUtc { get; set; } = DateTime.UtcNow;
        public DateTime? SubmittedOnUtc { get; set; }
        public IReadOnlyList<Question> Questions { get; set; } = [];
        public Dictionary<string, List<int>> Answers { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public decimal ScorePercent { get; set; }
        public bool Passed { get; set; }
        public string Summary { get; set; } = string.Empty;
    }

    public sealed class QuestionResult
    {
        public string QuestionId { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public int AwardedPoints { get; set; }
        public string Explanation { get; set; } = string.Empty;
    }

    public sealed class ExamEvaluationResult
    {
        public ExamAttempt Attempt { get; set; } = new();
        public IReadOnlyList<QuestionResult> Results { get; set; } = [];
        public int EarnedPoints { get; set; }
        public int AvailablePoints { get; set; }
    }

    public sealed class CompetencyEvaluationResult
    {
        public string TaskId { get; set; } = string.Empty;
        public decimal ScorePercent { get; set; }
        public bool Competent { get; set; }
        public IReadOnlyList<string> Feedback { get; set; } = [];
    }
}
