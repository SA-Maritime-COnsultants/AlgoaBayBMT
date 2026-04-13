using AlgoaBayBMT.Shared.Models;
using System.ComponentModel.DataAnnotations;

namespace AlgoaBayBMT.Services.Models
{
    public sealed class TrainingDashboardModel
    {
        public int TotalCourses { get; set; }
        public int PublishedCourses { get; set; }
        public int DraftCourses { get; set; }
        public int TotalModules { get; set; }
        public int TotalLessons { get; set; }
        public List<TrainingCourseListItemModel> RecentCourses { get; set; } = new();
    }

    public sealed class TrainingCourseListItemModel
    {
        public Guid CourseId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? TargetAudienceSummary { get; set; }
        public int ValidityMonths { get; set; }
        public bool IsMandatory { get; set; }
        public bool IsActive { get; set; }
        public Guid? CurrentVersionId { get; set; }
        public int? CurrentVersionNumber { get; set; }
        public string? CurrentVersionLabel { get; set; }
        public CourseVersionStatus? CurrentVersionStatus { get; set; }
        public int ModuleCount { get; set; }
        public int LessonCount { get; set; }
        public DateTime CreatedOnUtc { get; set; }
    }

    public sealed class TrainingCourseEditModel
    {
        public Guid? CourseId { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        [StringLength(500)]
        public string? ThumbnailUrl { get; set; }

        [StringLength(500)]
        public string? TargetAudienceSummary { get; set; }

        [StringLength(250)]
        public string? RegulatoryReference { get; set; }

        public string? LearningObjectives { get; set; }

        [Range(1, 120)]
        public int ValidityMonths { get; set; } = 12;

        [Range(1, 1440)]
        public int? EstimatedDurationMinutes { get; set; }

        public bool IsMandatory { get; set; } = true;

        public bool IsActive { get; set; } = true;
    }

    public sealed class TrainingCourseBuilderModel
    {
        public Guid CourseId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid CurrentVersionId { get; set; }
        public int CurrentVersionNumber { get; set; }
        public string? CurrentVersionLabel { get; set; }
        public CourseVersionStatus CurrentVersionStatus { get; set; }
        public DateTime? PublishedOnUtc { get; set; }
        public List<TrainingModuleEditModel> Modules { get; set; } = new();
    }

    public sealed class TrainingModuleEditModel
    {
        public Guid? ModuleId { get; set; }
        public Guid CourseVersionId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public int OrderIndex { get; set; }
        public int? EstimatedMinutes { get; set; }
        public bool IsActive { get; set; } = true;
        public List<TrainingLessonEditModel> Lessons { get; set; } = new();
    }

    public sealed class TrainingLessonEditModel
    {
        public Guid? LessonId { get; set; }
        public Guid ModuleId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Summary { get; set; }

        public LessonType LessonType { get; set; } = LessonType.ContentOnly;
        public LessonCompletionRule CompletionRule { get; set; } = LessonCompletionRule.ManualButton;
        public int OrderIndex { get; set; }
        public int? EstimatedMinutes { get; set; }
        public bool IsRequired { get; set; } = true;
        public bool IsPreview { get; set; }
        public bool IsActive { get; set; } = true;
        public List<TrainingLessonBlockEditModel> Blocks { get; set; } = new();
    }

    public sealed class TrainingLessonBlockEditModel
    {
        public Guid? LessonBlockId { get; set; }
        public Guid LessonId { get; set; }
        public LessonBlockType BlockType { get; set; } = LessonBlockType.Markdown;

        [StringLength(200)]
        public string? Title { get; set; }

        public int OrderIndex { get; set; }
        public string? MarkdownBody { get; set; }

        [StringLength(1000)]
        public string? FileUrl { get; set; }

        [StringLength(1000)]
        public string? ExternalUrl { get; set; }

        [StringLength(100)]
        public string? MimeType { get; set; }

        public int? DurationSeconds { get; set; }
        public string? MetadataJson { get; set; }
        public Guid? MediaAssetId { get; set; }
        public bool IsRequired { get; set; } = true;
    }
}
