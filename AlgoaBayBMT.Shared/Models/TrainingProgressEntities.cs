using System;
using System.Collections.Generic;

namespace AlgoaBayBMT.Shared.Models
{
    public class UserTrainingAssignment
    {
        public Guid UserTrainingAssignmentId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public Guid CourseId { get; set; }
        public string? AssignedByUserId { get; set; }
        public DateTime AssignedOnUtc { get; set; } = DateTime.UtcNow;
        public DateTime? DueDateUtc { get; set; }
        public string? Reason { get; set; }
        public AssignmentStatus Status { get; set; } = AssignmentStatus.Assigned;
        public string? RegisteredByUserId { get; set; }
        public DateTime? RegisteredOnUtc { get; set; }
        public string? ApprovedByUserId { get; set; }
        public DateTime? ApprovedOnUtc { get; set; }
        public string? ApprovalNotes { get; set; }
        public int? InvoiceId { get; set; }
        public DateTime? CompletedOnUtc { get; set; }
        public decimal? CompletionScorePercent { get; set; }
        public DateTime? PaidOnUtc { get; set; }
        public string? PaidByUserId { get; set; }

        public Course? Course { get; set; }
        public Invoice? Invoice { get; set; }
    }

    public class UserLessonProgress
    {
        public Guid UserLessonProgressId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public Guid LessonId { get; set; }
        public ProgressStatus Status { get; set; } = ProgressStatus.NotStarted;
        public DateTime? StartedOnUtc { get; set; }
        public DateTime? CompletedOnUtc { get; set; }
        public DateTime? LastAccessedOnUtc { get; set; }
        public decimal PercentComplete { get; set; }
        public int? VideoSecondsWatched { get; set; }
        public decimal? ScrollPercent { get; set; }
        public string? CompletionEvidenceJson { get; set; }
        public bool KnowledgeCheckPassed { get; set; }

        public TrainingLesson? Lesson { get; set; }
    }

    public class UserCourseProgress
    {
        public Guid UserCourseProgressId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public Guid CourseId { get; set; }
        public ProgressStatus Status { get; set; } = ProgressStatus.NotStarted;
        public DateTime? AssignedOnUtc { get; set; }
        public DateTime? StartedOnUtc { get; set; }
        public DateTime? CompletedOnUtc { get; set; }
        public DateTime? LastAccessedOnUtc { get; set; }
        public decimal PercentComplete { get; set; }
        public Guid? CurrentLessonId { get; set; }
        public DateTime? ExpiryDateUtc { get; set; }

        public Course? Course { get; set; }
        public TrainingLesson? CurrentLesson { get; set; }
    }

    public class AssessmentAttempt
    {
        public Guid AssessmentAttemptId { get; set; }
        public Guid AssessmentId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int AttemptNumber { get; set; }
        public DateTime StartedOnUtc { get; set; } = DateTime.UtcNow;
        public DateTime? SubmittedOnUtc { get; set; }
        public decimal? ScorePercent { get; set; }
        public bool Passed { get; set; }

        public Assessment? Assessment { get; set; }
        public ICollection<AssessmentResponse> Responses { get; set; } = new List<AssessmentResponse>();
    }

    public class AssessmentResponse
    {
        public Guid AssessmentResponseId { get; set; }
        public Guid AssessmentAttemptId { get; set; }
        public Guid AssessmentQuestionId { get; set; }
        public Guid? SelectedOptionId { get; set; }
        public string? FreeTextAnswer { get; set; }
        public bool? IsCorrect { get; set; }
        public decimal? AwardedPoints { get; set; }

        public AssessmentAttempt? AssessmentAttempt { get; set; }
        public AssessmentQuestion? AssessmentQuestion { get; set; }
        public AssessmentOption? SelectedOption { get; set; }
    }

    public class CourseCompletionRecord
    {
        public Guid CourseCompletionRecordId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public Guid CourseId { get; set; }
        public Guid CourseVersionId { get; set; }
        public DateTime CompletedOnUtc { get; set; }
        public DateTime ExpiryDateUtc { get; set; }
        public decimal? FinalScorePercent { get; set; }
        public string CertificateNumber { get; set; } = string.Empty;

        public Course? Course { get; set; }
        public CourseVersion? CourseVersion { get; set; }
        public TrainingCertificate? TrainingCertificate { get; set; }
    }

    public class UserAssessmentAttempt
    {
        public Guid UserAssessmentAttemptId { get; set; }
        public Guid TrainingCourseAssessmentId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int AttemptNumber { get; set; }
        public DateTime StartedOnUtc { get; set; } = DateTime.UtcNow;
        public DateTime? SubmittedOnUtc { get; set; }
        public decimal? ScorePercent { get; set; }
        public bool Passed { get; set; }

        public TrainingCourseAssessment? Assessment { get; set; }
        public ICollection<UserAssessmentResponse> Responses { get; set; } = new List<UserAssessmentResponse>();
    }

    public class UserAssessmentResponse
    {
        public Guid UserAssessmentResponseId { get; set; }
        public Guid UserAssessmentAttemptId { get; set; }
        public Guid TrainingQuestionBankQuestionId { get; set; }
        public Guid? SelectedOptionId { get; set; }
        public string? FreeTextAnswer { get; set; }
        public bool? IsCorrect { get; set; }
        public decimal? AwardedPoints { get; set; }

        public UserAssessmentAttempt? Attempt { get; set; }
        public TrainingQuestionBankQuestion? Question { get; set; }
        public TrainingQuestionBankOption? SelectedOption { get; set; }
    }

    public class TrainingCertificate
    {
        public Guid TrainingCertificateId { get; set; }
        public Guid CourseCompletionRecordId { get; set; }
        public string CertificateNumber { get; set; } = string.Empty;
        public string? FilePath { get; set; }
        public string VerificationCode { get; set; } = string.Empty;
        public DateTime IssuedOnUtc { get; set; }
        public DateTime ExpiresOnUtc { get; set; }
        public DateTime? RevokedOnUtc { get; set; }

        public CourseCompletionRecord? CourseCompletionRecord { get; set; }
    }

    public class TrainingAuditLog
    {
        public Guid TrainingAuditLogId { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string ActionType { get; set; } = string.Empty;
        public string? ChangedByUserId { get; set; }
        public DateTime ChangedOnUtc { get; set; } = DateTime.UtcNow;
        public string? BeforeJson { get; set; }
        public string? AfterJson { get; set; }
        public string? Notes { get; set; }
    }
}
