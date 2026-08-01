using System;
using System.Collections.Generic;

namespace AlgoaBayBMT.Shared.Models
{
    public class Course
    {
        public Guid CourseId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public string? Description { get; set; }
        public decimal PassMarkPercent { get; set; } = 80m;
        public string? ThumbnailUrl { get; set; }
        public string? TargetAudienceSummary { get; set; }
        public string? RegulatoryReference { get; set; }
        public string? LearningObjectives { get; set; }
        public int ValidityMonths { get; set; } = 12;
        public bool IsMandatory { get; set; } = true;
        public bool IsActive { get; set; } = true;
        public int? EstimatedDurationMinutes { get; set; }
        public decimal Cost { get; set; }
        public Guid? CurrentVersionId { get; set; }
        public string? CreatedByUserId { get; set; }
        public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;
        public string? UpdatedByUserId { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }

        // Soft delete: the course row is retained but hidden from the management/editing lists.
        public bool IsDeleted { get; set; }
        public string? DeletedByUserId { get; set; }
        public DateTime? DeletedOnUtc { get; set; }

        public CourseVersion? CurrentVersion { get; set; }
        public ICollection<CourseVersion> Versions { get; set; } = new List<CourseVersion>();
        public ICollection<CourseAudienceRule> AudienceRules { get; set; } = new List<CourseAudienceRule>();
        public ICollection<TrainingCourseAssessment> Assessments { get; set; } = new List<TrainingCourseAssessment>();
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
        public byte[]? RowVersion { get; set; }

        /// <summary>Published and archived course versions are immutable.</summary>
        public bool IsEditable => Status == CourseVersionStatus.Draft;

        public Course? Course { get; set; }
        /// <summary>Ordered references to shared module versions. Replaces the old owned Modules collection.</summary>
        public ICollection<CourseModule> CourseModules { get; set; } = new List<CourseModule>();
        /// <summary>Rank profiles this course version serves.</summary>
        public ICollection<CourseRankProfile> RankProfiles { get; set; } = new List<CourseRankProfile>();
        public ICollection<CourseCompletionRecord> CompletionRecords { get; set; } = new List<CourseCompletionRecord>();
    }

    /// <summary>
    /// An immutable unit of authored content: the lessons and content blocks themselves.
    /// A module version is NOT owned by a course — courses reference it through
    /// <see cref="CourseModule"/>, so one authored version can serve many courses without
    /// duplicating a single lesson or block. Stable identity lives on <see cref="TrainingModule"/>.
    /// </summary>
    public class TrainingModuleVersion
    {
        public Guid ModuleVersionId { get; set; }
        public Guid ModuleId { get; set; }
        public int VersionNumber { get; set; } = 1;
        public string? VersionLabel { get; set; }
        public ModuleVersionStatus Status { get; set; } = ModuleVersionStatus.Draft;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? EstimatedMinutes { get; set; }
        public bool IsActive { get; set; } = true;
        public bool HasModuleAssessment { get; set; }
        public Guid? AssessmentId { get; set; }
        public decimal? AssessmentPassMarkPercent { get; set; }
        public int AssessmentMaxAttempts { get; set; } = 3;
        public string? ChangeSummary { get; set; }
        public DateTime? PublishedOnUtc { get; set; }
        public string? PublishedByUserId { get; set; }
        public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedOnUtc { get; set; }
        public byte[]? RowVersion { get; set; }

        /// <summary>Published and archived versions may not have their content edited.</summary>
        public bool IsEditable => Status == ModuleVersionStatus.Draft;

        public TrainingModule? Module { get; set; }
        public TrainingCourseAssessment? ModuleAssessment { get; set; }
        public ICollection<TrainingLesson> Lessons { get; set; } = new List<TrainingLesson>();
        public ICollection<TrainingQuestionBankQuestion> QuestionBankQuestions { get; set; } = new List<TrainingQuestionBankQuestion>();
        public ICollection<CourseModule> CourseModules { get; set; } = new List<CourseModule>();
    }

    public class TrainingLesson
    {
        public Guid LessonId { get; set; }
        public Guid ModuleVersionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public int OrderIndex { get; set; }
        public int? EstimatedMinutes { get; set; }
        public bool IsPreview { get; set; }
        public bool IsRequired { get; set; } = true;
        public bool IsActive { get; set; } = true;

        public TrainingModuleVersion? ModuleVersion { get; set; }
        public ICollection<LessonBlock> LessonBlocks { get; set; } = new List<LessonBlock>();
        public ICollection<UserLessonProgress> UserLessonProgressRecords { get; set; } = new List<UserLessonProgress>();
        public ICollection<UserCourseProgress> CurrentCourseProgressRecords { get; set; } = new List<UserCourseProgress>();
    }

    public class LessonBlock
    {
        public Guid LessonBlockId { get; set; }
        public Guid LessonId { get; set; }
        public LessonBlockType BlockType { get; set; }
        public string? Title { get; set; }
        public string? Subtitle { get; set; }
        public int OrderIndex { get; set; }
        public string? MarkdownBody { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? FileUrl { get; set; }
        public string? ExternalUrl { get; set; }
        public string? MimeType { get; set; }
        public int? DurationSeconds { get; set; }
        public string? MetadataJson { get; set; }
        public Guid? MediaAssetId { get; set; }
        public bool IsRequired { get; set; } = true;
        public bool IsActive { get; set; } = true;

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

    public class TrainingCourseAssessment
    {
        public Guid TrainingCourseAssessmentId { get; set; }
        public Guid TrainingCourseId { get; set; }

        /// <summary>
        /// Set when the assessment belongs to a shared module version rather than to the course.
        /// A module-scoped assessment travels with the module into every course that includes it;
        /// a course-scoped assessment (this being null) is only valid within its owning course.
        /// </summary>
        public Guid? ModuleVersionId { get; set; }

        public string Name { get; set; } = string.Empty;
        public string? Instructions { get; set; }
        public decimal PassMarkPercent { get; set; } = 80m;
        public int RandomQuestionCount { get; set; } = 25;
        public int MaxAttempts { get; set; } = 3;
        public int? TimeLimitMinutes { get; set; }
        public bool IsActive { get; set; } = true;

        public Course? Course { get; set; }
        public TrainingModuleVersion? ModuleVersion { get; set; }
        public ICollection<TrainingQuestionBankQuestion> QuestionBankQuestions { get; set; } = new List<TrainingQuestionBankQuestion>();
        public ICollection<UserAssessmentAttempt> Attempts { get; set; } = new List<UserAssessmentAttempt>();
    }

    public class TrainingQuestionBankQuestion
    {
        public Guid TrainingQuestionBankQuestionId { get; set; }
        public Guid TrainingCourseAssessmentId { get; set; }
        public Guid? TrainingModuleVersionId { get; set; }
        public Guid? TrainingLessonId { get; set; }
        public TrainingQuestionType QuestionType { get; set; }
        public string Prompt { get; set; } = string.Empty;
        public string? ScenarioText { get; set; }
        public string? Explanation { get; set; }
        public int? DifficultyLevel { get; set; }
        public decimal Points { get; set; } = 1m;
        public bool IsActive { get; set; } = true;

        public TrainingCourseAssessment? Assessment { get; set; }
        public TrainingModuleVersion? ModuleVersion { get; set; }
        public ICollection<TrainingQuestionBankOption> Options { get; set; } = new List<TrainingQuestionBankOption>();
        public ICollection<UserAssessmentResponse> Responses { get; set; } = new List<UserAssessmentResponse>();
    }

    public class TrainingQuestionBankOption
    {
        public Guid TrainingQuestionBankOptionId { get; set; }
        public Guid TrainingQuestionBankQuestionId { get; set; }
        public string OptionText { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public int OrderIndex { get; set; }

        public TrainingQuestionBankQuestion? Question { get; set; }
        public ICollection<UserAssessmentResponse> Responses { get; set; } = new List<UserAssessmentResponse>();
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
