using System;
using System.Collections.Generic;

namespace AlgoaBayBMT.Shared.Models
{
    public class Course
    {
        public Guid CourseId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? TargetAudienceSummary { get; set; }
        public string? RegulatoryReference { get; set; }
        public string? LearningObjectives { get; set; }
        public int ValidityMonths { get; set; } = 12;
        public bool IsMandatory { get; set; } = true;
        public bool IsActive { get; set; } = true;
        public int? EstimatedDurationMinutes { get; set; }
        public Guid? CurrentVersionId { get; set; }
        public string? CreatedByUserId { get; set; }
        public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;
        public string? UpdatedByUserId { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }

        public CourseVersion? CurrentVersion { get; set; }
        public ICollection<CourseVersion> Versions { get; set; } = new List<CourseVersion>();
        public ICollection<CourseAudienceRule> AudienceRules { get; set; } = new List<CourseAudienceRule>();
        public ICollection<UserTrainingAssignment> UserTrainingAssignments { get; set; } = new List<UserTrainingAssignment>();
        public ICollection<UserCourseProgress> UserCourseProgressRecords { get; set; } = new List<UserCourseProgress>();
        public ICollection<CourseCompletionRecord> CompletionRecords { get; set; } = new List<CourseCompletionRecord>();
    }

    public class CourseVersion
    {
        public Guid CourseVersionId { get; set; }
        public Guid CourseId { get; set; }
        public int VersionNumber { get; set; }
        public string? VersionLabel { get; set; }
        public CourseVersionStatus Status { get; set; } = CourseVersionStatus.Draft;
        public DateTime? EffectiveFromUtc { get; set; }
        public DateTime? EffectiveToUtc { get; set; }
        public string? ChangeSummary { get; set; }
        public string? ApprovedByUserId { get; set; }
        public DateTime? ApprovedOnUtc { get; set; }
        public DateTime? ReviewDateUtc { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedOnUtc { get; set; }

        public Course? Course { get; set; }
        public ICollection<TrainingModule> Modules { get; set; } = new List<TrainingModule>();
        public ICollection<CourseCompletionRecord> CompletionRecords { get; set; } = new List<CourseCompletionRecord>();
    }

    public class TrainingModule
    {
        public Guid ModuleId { get; set; }
        public Guid CourseVersionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int OrderIndex { get; set; }
        public int? EstimatedMinutes { get; set; }
        public bool IsActive { get; set; } = true;

        public CourseVersion? CourseVersion { get; set; }
        public ICollection<TrainingLesson> Lessons { get; set; } = new List<TrainingLesson>();
    }

    public class TrainingLesson
    {
        public Guid LessonId { get; set; }
        public Guid ModuleId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public LessonType LessonType { get; set; } = LessonType.ContentOnly;
        public LessonCompletionRule CompletionRule { get; set; } = LessonCompletionRule.ManualButton;
        public int OrderIndex { get; set; }
        public int? EstimatedMinutes { get; set; }
        public bool IsRequired { get; set; } = true;
        public bool IsPreview { get; set; }
        public bool IsActive { get; set; } = true;

        public TrainingModule? Module { get; set; }
        public ICollection<LessonBlock> LessonBlocks { get; set; } = new List<LessonBlock>();
        public Assessment? Assessment { get; set; }
        public ICollection<UserLessonProgress> UserLessonProgressRecords { get; set; } = new List<UserLessonProgress>();
        public ICollection<UserCourseProgress> CurrentCourseProgressRecords { get; set; } = new List<UserCourseProgress>();
    }

    public class LessonBlock
    {
        public Guid LessonBlockId { get; set; }
        public Guid LessonId { get; set; }
        public LessonBlockType BlockType { get; set; }
        public string? Title { get; set; }
        public int OrderIndex { get; set; }
        public string? MarkdownBody { get; set; }
        public string? FileUrl { get; set; }
        public string? ExternalUrl { get; set; }
        public string? MimeType { get; set; }
        public int? DurationSeconds { get; set; }
        public string? MetadataJson { get; set; }
        public Guid? MediaAssetId { get; set; }
        public bool IsRequired { get; set; } = true;

        public TrainingLesson? Lesson { get; set; }
        public MediaAsset? MediaAsset { get; set; }
    }

    public class MediaAsset
    {
        public Guid MediaAssetId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string StoredFileName { get; set; } = string.Empty;
        public string RelativePath { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public string? UploadedByUserId { get; set; }
        public DateTime UploadedOnUtc { get; set; } = DateTime.UtcNow;
        public string? HashSha256 { get; set; }

        public ICollection<LessonBlock> LessonBlocks { get; set; } = new List<LessonBlock>();
    }

    public class Assessment
    {
        public Guid AssessmentId { get; set; }
        public Guid LessonId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Instructions { get; set; }
        public decimal PassMarkPercent { get; set; } = 80m;
        public int MaxAttempts { get; set; } = 3;
        public bool RandomizeQuestions { get; set; }
        public int? TimeLimitMinutes { get; set; }
        public bool ShowFeedbackAfterSubmit { get; set; } = true;
        public bool IsActive { get; set; } = true;

        public TrainingLesson? Lesson { get; set; }
        public ICollection<AssessmentQuestion> Questions { get; set; } = new List<AssessmentQuestion>();
        public ICollection<AssessmentAttempt> Attempts { get; set; } = new List<AssessmentAttempt>();
    }

    public class AssessmentQuestion
    {
        public Guid AssessmentQuestionId { get; set; }
        public Guid AssessmentId { get; set; }
        public QuestionType QuestionType { get; set; }
        public string PromptMarkdown { get; set; } = string.Empty;
        public string? ExplanationMarkdown { get; set; }
        public int OrderIndex { get; set; }
        public decimal Points { get; set; } = 1m;

        public Assessment? Assessment { get; set; }
        public ICollection<AssessmentOption> Options { get; set; } = new List<AssessmentOption>();
        public ICollection<AssessmentResponse> Responses { get; set; } = new List<AssessmentResponse>();
    }

    public class AssessmentOption
    {
        public Guid AssessmentOptionId { get; set; }
        public Guid AssessmentQuestionId { get; set; }
        public string OptionText { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public int OrderIndex { get; set; }

        public AssessmentQuestion? AssessmentQuestion { get; set; }
        public ICollection<AssessmentResponse> Responses { get; set; } = new List<AssessmentResponse>();
    }

    public class CourseAudienceRule
    {
        public Guid CourseAudienceRuleId { get; set; }
        public Guid CourseId { get; set; }
        public CourseAudienceRuleType RuleType { get; set; }
        public string? ApplicationRoleName { get; set; }
        public string? OnBoardRole { get; set; }
        public string? Qualification { get; set; }
        public bool IsMandatory { get; set; } = true;
        public string? Notes { get; set; }

        public Course? Course { get; set; }
    }
}
