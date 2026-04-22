using AlgoaBayBMT.Shared.Models;
using System.ComponentModel.DataAnnotations;

namespace AlgoaBayBMT.Services.Models
{
    public sealed class TrainingDashboardModel
    {
        public int TotalCourses { get; set; }
        public int ActiveCourses { get; set; }
        public int PublishedCourses { get; set; }
        public int DraftCourses { get; set; }
        public int TotalModules { get; set; }
        public int TotalLessons { get; set; }
        public int TotalContentBlocks { get; set; }
        public int TotalQuestionBankQuestions { get; set; }
        public int ActiveLearners { get; set; }
        public int EnrolledLearners { get; set; }
        public int StartedLearners { get; set; }
        public int CompletedLearners { get; set; }
        public List<TrainingCourseListItemModel> RecentCourses { get; set; } = new();
    }

    public sealed class TrainingCourseListItemModel
    {
        public Guid CourseId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public string? Description { get; set; }
        public string? TargetAudienceSummary { get; set; }
        public TrainingAudienceType AudienceType { get; set; } = TrainingAudienceType.All;
        public decimal PassMarkPercent { get; set; }
        public int? DurationMinutes { get; set; }
        public int ValidityMonths { get; set; }
        public bool IsMandatory { get; set; }
        public bool IsActive { get; set; }
        public string? ThumbnailUrl { get; set; }
        public Guid? CurrentVersionId { get; set; }
        public int? CurrentVersionNumber { get; set; }
        public string? CurrentVersionLabel { get; set; }
        public CourseVersionStatus? CurrentVersionStatus { get; set; }
        public int ModuleCount { get; set; }
        public int LessonCount { get; set; }
        public int EnrolledLearnerCount { get; set; }
        public int StartedLearnerCount { get; set; }
        public int CompletedLearnerCount { get; set; }
        public DateTime CreatedOnUtc { get; set; }
    }

    public enum TrainingCourseStudentStatus
    {
        NotStarted = 0,
        InProgress = 1,
        Completed = 2,
        Expired = 3,
        Failed = 4
    }

    public sealed class TrainingCourseStudentStatusPageModel
    {
        public Guid CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public decimal PassMarkPercent { get; set; }
        public int ValidityMonths { get; set; }
        public int EnrolledLearnerCount { get; set; }
        public int StartedLearnerCount { get; set; }
        public int CompletedLearnerCount { get; set; }
        public List<TrainingCourseStudentStatusModel> Students { get; set; } = new();
    }

    public sealed class TrainingCourseStudentStatusModel
    {
        public string UserId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public TrainingCourseStudentStatus Status { get; set; }
        public DateTime? StartedOnUtc { get; set; }
        public DateTime? CompletedOnUtc { get; set; }
        public DateTime? ExpiryDateUtc { get; set; }
        public DateTime? DueDateUtc { get; set; }
        public decimal? ResultPercent { get; set; }
        public string? CertificateNumber { get; set; }
        public string? CertificateViewUrl { get; set; }
        public string? CertificateDownloadUrl { get; set; }
    }

    public sealed class TrainingCourseEditModel
    {
        public Guid? CourseId { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Summary { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        [StringLength(500)]
        public string? ThumbnailUrl { get; set; }

        [StringLength(500)]
        public string? TargetAudienceSummary { get; set; }

        public TrainingAudienceType AudienceType { get; set; } = TrainingAudienceType.All;

        [StringLength(250)]
        public string? RegulatoryReference { get; set; }

        public string? LearningObjectives { get; set; }

        [Range(1, 100)]
        public decimal PassMarkPercent { get; set; } = 80m;

        [Range(1, 120)]
        public int ValidityMonths { get; set; } = 12;

        [Range(1, 1440)]
        public int? EstimatedDurationMinutes { get; set; }

        [Range(1, 1440)]
        public int? DurationMinutes { get; set; }

        public bool IsMandatory { get; set; } = true;

        public bool IsActive { get; set; } = true;
    }

    public sealed class TrainingCourseBuilderModel
    {
        public Guid CourseId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public string? Description { get; set; }
        public decimal PassMarkPercent { get; set; }
        public Guid CurrentVersionId { get; set; }
        public int CurrentVersionNumber { get; set; }
        public string? CurrentVersionLabel { get; set; }
        public CourseVersionStatus CurrentVersionStatus { get; set; }
        public DateTime? PublishedOnUtc { get; set; }
        public List<TrainingModuleEditModel> Modules { get; set; } = new();
        public List<TrainingCourseAssessmentEditModel> Assessments { get; set; } = new();
    }

    public sealed class TrainingModuleEditModel
    {
        public Guid? ModuleId { get; set; }
        public Guid CourseVersionId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(200)]
        public string Name
        {
            get => Title;
            set => Title = value;
        }

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(1000)]
        public string? Summary
        {
            get => Description;
            set => Description = value;
        }

        public int OrderIndex { get; set; }
        public int? EstimatedMinutes { get; set; }
        public bool IsActive { get; set; } = true;
        public bool HasModuleAssessment { get; set; }
        public Guid? AssessmentId { get; set; }
        public string? AssessmentName { get; set; }
        [Range(1, 100)]
        public decimal? AssessmentPassMarkPercent { get; set; }
        [Range(1, 10)]
        public int AssessmentMaxAttempts { get; set; } = 3;
        public List<TrainingLessonEditModel> Lessons { get; set; } = new();
    }

    public sealed class TrainingLessonEditModel
    {
        public Guid? LessonId { get; set; }
        public Guid ModuleId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(200)]
        public string Name
        {
            get => Title;
            set => Title = value;
        }

        [StringLength(1000)]
        public string? Summary { get; set; }

        public int OrderIndex { get; set; }
        public int? EstimatedMinutes { get; set; }
        public bool IsPreview { get; set; }
        public bool IsActive { get; set; } = true;
        public List<TrainingLessonBlockEditModel> Blocks { get; set; } = new();
    }

    public sealed class TrainingLessonBlockEditModel
    {
        public Guid UiKey { get; set; } = Guid.NewGuid();
        public Guid? LessonBlockId { get; set; }
        public Guid LessonId { get; set; }
        public LessonBlockType BlockType { get; set; } = LessonBlockType.TextNarrative;

        [StringLength(200)]
        public string? Title { get; set; }

        [StringLength(300)]
        public string? Subtitle { get; set; }

        public int OrderIndex { get; set; }
        public string? MarkdownBody { get; set; }

        public string? ContentHtml
        {
            get => MarkdownBody;
            set => MarkdownBody = value;
        }

        public string? SecondaryContentHtml { get; set; }

        public string? IntroTextHtml { get; set; }

        [StringLength(500)]
        public string? ThumbnailUrl { get; set; }

        [StringLength(1000)]
        public string? FileUrl { get; set; }

        [StringLength(1000)]
        public string? ExternalUrl { get; set; }

        [StringLength(100)]
        public string? MimeType { get; set; }

        public int? DurationSeconds { get; set; }
        public string? MetadataJson { get; set; }
        public Guid? MediaAssetId { get; set; }
        public Guid? LinkedAssessmentId { get; set; }
        public string? LinkedAssessmentName { get; set; }
        public bool IsRequired { get; set; } = true;
        public bool IsActive { get; set; } = true;
        public bool IsExpanded { get; set; }
        public List<TrainingLessonQuizQuestionEditModel> QuizQuestions { get; set; } = new();
    }

    public sealed class TrainingLessonQuizQuestionEditModel
    {
        public Guid UiKey { get; set; } = Guid.NewGuid();
        public int OrderIndex { get; set; }

        [Required]
        [StringLength(500)]
        public string Prompt { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? FrontText { get; set; }

        public TrainingQuestionType QuestionType { get; set; } = TrainingQuestionType.MultipleChoice;

        [StringLength(2000)]
        public string? ScenarioText { get; set; }

        [StringLength(2000)]
        public string? BackText { get; set; }

        public bool IncludeFrontImage { get; set; }

        [StringLength(1000)]
        public string? FrontImageUrl { get; set; }

        [StringLength(260)]
        public string? FrontImageFileName { get; set; }

        public List<TrainingLessonQuizOptionEditModel> Options { get; set; } =
        [
            new() { OrderIndex = 1 },
            new() { OrderIndex = 2 }
        ];
    }

    public sealed class TrainingLessonQuizOptionEditModel
    {
        [Required]
        [StringLength(500)]
        public string OptionText { get; set; } = string.Empty;

        public bool IsCorrect { get; set; }
        public int OrderIndex { get; set; }
    }

    public sealed class TrainingLessonBlockMetadataModel
    {
        public string? SecondaryContentHtml { get; set; }
        public string? IntroTextHtml { get; set; }
        public Guid? LinkedAssessmentId { get; set; }
        public string? LinkedAssessmentName { get; set; }
        public List<TrainingLessonQuizQuestionEditModel> QuizQuestions { get; set; } = new();
    }

    public sealed class TrainingMediaUploadModel
    {
        public Guid MediaAssetId { get; set; }
        public string Url { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
    }

    public sealed class TrainingCourseAssessmentEditModel
    {
        public Guid? TrainingCourseAssessmentId { get; set; }
        public Guid TrainingCourseId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        public string? Instructions { get; set; }

        [Range(1, 100)]
        public decimal PassMarkPercent { get; set; } = 80m;

        [Range(1, 200)]
        public int RandomQuestionCount { get; set; } = 25;

        [Range(1, 10)]
        public int MaxAttempts { get; set; } = 3;

        [Range(1, 240)]
        public int? TimeLimitMinutes { get; set; }

        public bool IsActive { get; set; } = true;
        public int QuestionBankCount { get; set; }
    }

    public sealed class TrainingQuestionBankQuestionEditModel
    {
        public Guid? TrainingQuestionBankQuestionId { get; set; }
        public Guid TrainingCourseAssessmentId { get; set; }
        public Guid? TrainingModuleId { get; set; }
        public TrainingQuestionType QuestionType { get; set; } = TrainingQuestionType.MultipleChoice;

        [Required]
        public string Prompt { get; set; } = string.Empty;

        public string? ScenarioText { get; set; }
        public string? Explanation { get; set; }
        public int? DifficultyLevel { get; set; }

        [Range(0.25, 20)]
        public decimal Points { get; set; } = 1m;

        public bool IsActive { get; set; } = true;
        public List<TrainingQuestionBankOptionEditModel> Options { get; set; } = new();
    }

    public sealed class TrainingQuestionBankOptionEditModel
    {
        public Guid? TrainingQuestionBankOptionId { get; set; }

        [Required]
        [StringLength(1000)]
        public string OptionText { get; set; } = string.Empty;

        public bool IsCorrect { get; set; }
        public int OrderIndex { get; set; }
    }

    public sealed class TrainingQuestionBankPageModel
    {
        public Guid CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public decimal PassMarkPercent { get; set; }
        public int ValidityMonths { get; set; }
        public List<TrainingModuleLookupModel> Modules { get; set; } = new();
        public List<TrainingCourseAssessmentEditModel> Assessments { get; set; } = new();
        public List<TrainingQuestionBankQuestionEditModel> Questions { get; set; } = new();
    }

    public sealed class TrainingModuleLookupModel
    {
        public Guid ModuleId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
