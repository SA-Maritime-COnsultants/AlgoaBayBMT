using AlgoaBayBMT.Shared.Models;

namespace AlgoaBayBMT.Services.Models;

public enum CrewComplianceOverallStatus
{
    FullyCompliant = 0,
    PendingTraining = 1,
    PendingApproval = 2,
    PendingPayment = 3,
    Expired = 4,
    Unknown = 99
}

public sealed class CrewComplianceDashboardModel
{
    public int CrewMemberId { get; set; }
    public string CrewMemberName { get; set; } = string.Empty;
    public string? EmployerName { get; set; }
    public CrewComplianceOverallStatus OverallStatus { get; set; } = CrewComplianceOverallStatus.Unknown;
    public string OverallStatusText { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public int RequiredCourseCount { get; set; }
    public int CompletedCourseCount { get; set; }
    public int PendingApprovalCount { get; set; }
    public int PendingPaymentCount { get; set; }
    public int ExpiredCourseCount { get; set; }
    public decimal CompliancePercent { get; set; }
    public bool VesselIsNonCompliant { get; set; }
    public List<CrewComplianceCourseItemModel> Courses { get; set; } = [];
}

public sealed class CrewComplianceCourseItemModel
{
    public Guid CourseId { get; set; }
    public Guid? AssignmentId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? AudienceSummary { get; set; }
    public decimal Cost { get; set; }
    public decimal PassMarkPercent { get; set; }
    public decimal ProgressPercent { get; set; }
    public AssignmentStatus? AssignmentStatus { get; set; }
    public ProgressStatus? ProgressStatus { get; set; }
    public DateTime? DueDateUtc { get; set; }
    public DateTime? CompletedOnUtc { get; set; }
    public DateTime? ExpiresOnUtc { get; set; }
    public int? InvoiceId { get; set; }
    public InvoiceStatus? InvoiceStatus { get; set; }
    public decimal? CompletionScorePercent { get; set; }
    public bool IsMandatory { get; set; }
    public bool IsExpired { get; set; }
    public bool CanRegister { get; set; }
    public string StatusText { get; set; } = string.Empty;
}

public sealed class TrainingApprovalQueueItemModel
{
    public Guid AssignmentId { get; set; }
    public int CrewMemberId { get; set; }
    public string CrewMemberName { get; set; } = string.Empty;
    public int? OperatorId { get; set; }
    public string? OperatorName { get; set; }
    public string? CrewEmail { get; set; }
    public Guid CourseId { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string CourseTitle { get; set; } = string.Empty;
    public decimal CourseCost { get; set; }
    public DateTime RegisteredOnUtc { get; set; }
    public string? RegisteredByUserId { get; set; }
    public string? Reason { get; set; }
}

public sealed class TrainingRegistrationSelectionModel
{
    public int CrewMemberId { get; set; }
    public List<Guid> CourseIds { get; set; } = [];
    public DateTime? DueDateUtc { get; set; }
    public string? Reason { get; set; }
}

public sealed class TrainingRegistrationSubmissionResultModel
{
    public int CreatedCount { get; set; }
    public List<Guid> AssignmentIds { get; set; } = [];
}

public sealed class TrainingApprovalResultModel
{
    public int ApprovedCount { get; set; }
    public List<int> InvoiceIds { get; set; } = [];
    public List<Guid> AssignmentIds { get; set; } = [];
    public bool CredentialsEnsured { get; set; }
    public bool EmailSent { get; set; }
}
