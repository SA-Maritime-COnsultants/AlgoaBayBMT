using AlgoaBayBMT.Shared.Models;

namespace AlgoaBayBMT.Services.Models;

public enum TrainingAccessStatus
{
    NotStarted = 0,
    InProgress = 1,
    Compliant = 2,
    Expired = 3,
    Overdue = 4
}

public sealed class MyTrainingDashboardModel
{
    public string LearnerName { get; set; } = string.Empty;
    public bool HasFullAccess { get; set; }
    public int TotalRequiredCourses { get; set; }
    public int CompletedCourses { get; set; }
    public int InProgressCourses { get; set; }
    public int OverdueOrExpiredCourses { get; set; }
    public decimal CompliancePercent { get; set; }
    public List<MyTrainingCourseListItemModel> RequiredCourses { get; set; } = new();
    public List<MyTrainingCourseListItemModel> AdditionalCourses { get; set; } = new();
    public List<MyTrainingCertificateListItemModel> Certificates { get; set; } = new();
}

public sealed class MyTrainingCourseListItemModel
{
    public Guid CourseId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? AudienceSummary { get; set; }
    public decimal PassMarkPercent { get; set; }
    public int? DurationMinutes { get; set; }
    public int ValidityMonths { get; set; }
    public decimal ProgressPercent { get; set; }
    public DateTime? DueDateUtc { get; set; }
    public DateTime? ExpiryDateUtc { get; set; }
    public TrainingAccessStatus Status { get; set; }
    public string ActionText { get; set; } = "Start";
    public bool IsMandatory { get; set; }
}

public sealed class MyTrainingCertificateListItemModel
{
    public Guid CertificateId { get; set; }
    public Guid CourseId { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string CourseTitle { get; set; } = string.Empty;
    public string CertificateNumber { get; set; } = string.Empty;
    public string VerificationCode { get; set; } = string.Empty;
    public DateTime CompletedOnUtc { get; set; }
    public DateTime ExpiresOnUtc { get; set; }
    public decimal? FinalScorePercent { get; set; }
    public decimal? PassMarkPercent { get; set; }
    public string CompletionStatus { get; set; } = string.Empty;
    public string ViewUrl { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
}

public sealed class TrainingCoursePlayerModel
{
    public Guid CourseId { get; set; }
    public string LearnerName { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? RegulatoryReference { get; set; }
    public decimal PassMarkPercent { get; set; }
    public int ValidityMonths { get; set; }
    public decimal CourseProgressPercent { get; set; }
    public int CompletedLessons { get; set; }
    public int TotalLessons { get; set; }
    public bool HasFullAccess { get; set; }
    public bool IsCompleted { get; set; }
    public Guid? CertificateId { get; set; }
    public string? CertificateUrl { get; set; }
    public Guid CurrentLessonId { get; set; }
    public Guid CurrentBlockId { get; set; }
    public bool CanGoPrevious { get; set; }
    public bool CanGoNext { get; set; }
    public bool CanMarkCurrentBlockComplete { get; set; }
    public string CurrentBlockActionText { get; set; } = "Mark Complete";
    public List<TrainingPlayerModuleModel> Modules { get; set; } = new();
    public TrainingPlayerLessonModel? CurrentLesson { get; set; }
    public TrainingPlayerBlockModel? CurrentBlock { get; set; }
}

public sealed class TrainingPlayerModuleModel
{
    public Guid ModuleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public int CompletedLessons { get; set; }
    public int TotalLessons { get; set; }
    public List<TrainingPlayerLessonSummaryModel> Lessons { get; set; } = new();
}

public sealed class TrainingPlayerLessonSummaryModel
{
    public Guid LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsCurrent { get; set; }
    public bool IsLocked { get; set; }
    public decimal ProgressPercent { get; set; }
}

public sealed class TrainingPlayerLessonModel
{
    public Guid LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public int OrderIndex { get; set; }
    public decimal ProgressPercent { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsLocked { get; set; }
    public List<TrainingPlayerBlockModel> Blocks { get; set; } = new();
}

public sealed class TrainingPlayerBlockModel
{
    public Guid BlockId { get; set; }
    public LessonBlockType BlockType { get; set; }
    public string DisplayType { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    public string? ContentHtml { get; set; }
    public string? SecondaryContentHtml { get; set; }
    public string? IntroTextHtml { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? FileUrl { get; set; }
    public string? ExternalUrl { get; set; }
    public string? AvatarVideoUrl { get; set; }
    public string? MimeType { get; set; }
    public int? DurationSeconds { get; set; }
    public int? SavedVideoSeconds { get; set; }
    public int? SavedFlashCardIndex { get; set; }
    public int? SavedFlashCardMaxViewedIndex { get; set; }
    public bool IsRequired { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsCurrent { get; set; }
    public bool IsLocked { get; set; }
    public string CompletionLabel { get; set; } = string.Empty;
    public Guid? LinkedAssessmentId { get; set; }
    public string? LinkedAssessmentName { get; set; }
    public decimal? LatestScorePercent { get; set; }
    public List<TrainingPlayerQuizQuestionModel> QuizQuestions { get; set; } = new();
    public List<TrainingPlayerAssessmentQuestionModel> AssessmentQuestions { get; set; } = new();
}

public sealed class TrainingPlayerQuizQuestionModel
{
    public int QuestionIndex { get; set; }
    public string Prompt { get; set; } = string.Empty;
    public string FrontText { get; set; } = string.Empty;
    public TrainingQuestionType QuestionType { get; set; }
    public string? ScenarioText { get; set; }
    public string? BackText { get; set; }
    public bool IncludeFrontImage { get; set; }
    public string? FrontImageUrl { get; set; }
    public List<TrainingPlayerQuizOptionModel> Options { get; set; } = new();
}

public sealed class TrainingPlayerQuizOptionModel
{
    public int OptionIndex { get; set; }
    public string OptionText { get; set; } = string.Empty;
}

public sealed class TrainingPlayerAssessmentQuestionModel
{
    public Guid QuestionId { get; set; }
    public string Prompt { get; set; } = string.Empty;
    public string? ScenarioText { get; set; }
    public decimal Points { get; set; }
    public List<TrainingPlayerAssessmentOptionModel> Options { get; set; } = new();
}

public sealed class TrainingPlayerAssessmentOptionModel
{
    public Guid OptionId { get; set; }
    public string OptionText { get; set; } = string.Empty;
}

public sealed class TrainingQuizSubmissionResultModel
{
    public bool Passed { get; set; }
    public decimal ScorePercent { get; set; }
    public string Message { get; set; } = string.Empty;
}

public sealed class TrainingAssessmentSubmissionResultModel
{
    public bool Passed { get; set; }
    public decimal ScorePercent { get; set; }
    public decimal EarnedPoints { get; set; }
    public decimal AvailablePoints { get; set; }
    public string Message { get; set; } = string.Empty;
}

public sealed class TrainingCertificateViewModel
{
    public Guid CertificateId { get; set; }
    public string LearnerFullName { get; set; } = string.Empty;
    public string CourseTitle { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public string CertificateNumber { get; set; } = string.Empty;
    public string VerificationCode { get; set; } = string.Empty;
    public string IssuingOrganisation { get; set; } = "Algoa Bay BMT Training System";
    public string TemplateTitle { get; set; } = "Training Completion Certificate";
    public DateTime CompletedOnUtc { get; set; }
    public DateTime ExpiresOnUtc { get; set; }
    public string VersionLabel { get; set; } = string.Empty;
}
