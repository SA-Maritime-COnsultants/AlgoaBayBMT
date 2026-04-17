using AlgoaBayBMT.Shared.Models;
using System.ComponentModel.DataAnnotations;

namespace AlgoaBayBMT.Services.Models;

public sealed class TrainingDashboardSummaryModel
{
    public int TotalCourses { get; set; }
    public int ActiveCourses { get; set; }
    public int PublishedCourses { get; set; }
    public int TotalModules { get; set; }
    public int TotalLessons { get; set; }
    public int TotalContentBlocks { get; set; }
    public int TotalQuestionBankQuestions { get; set; }
    public int ActiveLearners { get; set; }
    public List<TrainingCourseListItemModel> RecentCourses { get; set; } = new();
}

public sealed class TrainingCourseEditModelV2
{
    public Guid? CourseId { get; set; }

    [Required]
    [StringLength(50)]
    public string CourseCode { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Summary { get; set; }

    public string? Description { get; set; }

    [Range(1, 100)]
    public decimal PassMarkPercent { get; set; } = 80m;

    [Range(1, 1440)]
    public int? DurationMinutes { get; set; }

    [Range(1, 120)]
    public int ValidityMonths { get; set; } = 12;

    public bool IsMandatory { get; set; } = true;
    public bool IsActive { get; set; } = true;

    [StringLength(500)]
    public string? ThumbnailUrl { get; set; }

    [StringLength(500)]
    public string? TargetAudienceSummary { get; set; }

    [StringLength(250)]
    public string? RegulatoryReference { get; set; }
}

public sealed class TrainingCourseBuilderWorkspaceModel
{
    public Guid CourseId { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public decimal PassMarkPercent { get; set; }
    public int ValidityMonths { get; set; }
    public Guid CurrentVersionId { get; set; }
    public string VersionLabel { get; set; } = string.Empty;
    public CourseVersionStatus CurrentVersionStatus { get; set; }
    public List<TrainingBuilderModuleNodeModel> Modules { get; set; } = new();
    public List<TrainingCourseAssessmentEditModel> Assessments { get; set; } = new();
}

public sealed class TrainingBuilderModuleNodeModel
{
    public Guid? ModuleId { get; set; }
    public Guid CourseVersionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public int OrderIndex { get; set; }
    public int? EstimatedMinutes { get; set; }
    public bool IsActive { get; set; } = true;
    public List<TrainingBuilderLessonNodeModel> Lessons { get; set; } = new();
}

public sealed class TrainingBuilderLessonNodeModel
{
    public Guid? LessonId { get; set; }
    public Guid ModuleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public int OrderIndex { get; set; }
    public int? EstimatedMinutes { get; set; }
    public bool IsRequired { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public LessonCompletionRule CompletionRule { get; set; } = LessonCompletionRule.ManualButton;
    public List<TrainingContentBlockEditModel> ContentBlocks { get; set; } = new();
    public List<TrainingKnowledgeCheckQuestionEditModel> KnowledgeChecks { get; set; } = new();
}

public sealed class TrainingContentBlockEditModel
{
    public Guid? LessonBlockId { get; set; }
    public Guid LessonId { get; set; }
    public LessonBlockType BlockType { get; set; } = LessonBlockType.CourseOverview;
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    public string? BodyMarkdown { get; set; }
    public string? ExternalUrl { get; set; }
    public string? UploadedFileUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? JsonConfig { get; set; }
    public int OrderIndex { get; set; }
    public bool IsRequired { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public Guid? MediaAssetId { get; set; }
}

public sealed class TrainingKnowledgeCheckQuestionEditModel
{
    public Guid? TrainingKnowledgeCheckQuestionId { get; set; }
    public Guid TrainingLessonId { get; set; }
    public TrainingQuestionType QuestionType { get; set; } = TrainingQuestionType.MultipleChoice;
    public string Prompt { get; set; } = string.Empty;
    public string? Explanation { get; set; }
    public int OrderIndex { get; set; }
    public decimal Points { get; set; } = 1m;
    public bool IsActive { get; set; } = true;
    public List<TrainingKnowledgeCheckOptionEditModel> Options { get; set; } = new();
}

public sealed class TrainingKnowledgeCheckOptionEditModel
{
    public Guid? TrainingKnowledgeCheckOptionId { get; set; }
    public string OptionText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int OrderIndex { get; set; }
}

public sealed class TrainingPreviewBlockModel
{
    public LessonBlockType BlockType { get; set; }
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    public string? BodyHtml { get; set; }
    public string? ExternalUrl { get; set; }
    public string? UploadedFileUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
}
