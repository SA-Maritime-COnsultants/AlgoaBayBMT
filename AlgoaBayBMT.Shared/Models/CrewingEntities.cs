using System.ComponentModel.DataAnnotations;

namespace AlgoaBayBMT.Shared.Models
{
    // ─────────────────────────────────────────────────────────────────
    // Vessels
    // ─────────────────────────────────────────────────────────────────
    public class Vessel
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(20)]
        public string? IMO { get; set; }

        [StringLength(20)]
        public string? MMSI { get; set; }

        [StringLength(20)]
        public string? CallSign { get; set; }

        [StringLength(50)]
        public string? Flag { get; set; }

        [StringLength(80)]
        public string? VesselType { get; set; }

        public double? GrossTonnage { get; set; }
        public double? LengthOverall { get; set; }

        public double? LOA
        {
            get => LengthOverall;
            set => LengthOverall = value;
        }

        public double? Beam { get; set; }

        public int? OwningOperatorId { get; set; }
        public BunkerOperator? OwningOperator { get; set; }

        public bool IsActive { get; set; } = true;

        // Audit / soft-delete (scoped to new crewing module entities only)
        public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;
        public string? CreatedByUserId { get; set; }
        public DateTime? ModifiedOnUtc { get; set; }
        public string? ModifiedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedOnUtc { get; set; }
        public string? DeletedByUserId { get; set; }

        public ICollection<CrewAssignment> CrewAssignments { get; set; } = new List<CrewAssignment>();
        public ICollection<VesselComplianceSnapshot> ComplianceSnapshots { get; set; } = new List<VesselComplianceSnapshot>();
    }

    // ─────────────────────────────────────────────────────────────────
    // Crew member master record
    // ─────────────────────────────────────────────────────────────────
    public class CrewMember
    {
        public int Id { get; set; }

        // Optional link to ApplicationUser (login + LMS access)
        [StringLength(450)]
        public string? ApplicationUserId { get; set; }

        // Personal details
        [Required, StringLength(120)]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(120)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(120)]
        public string? MiddleNames { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [StringLength(100)]
        public string? Nationality { get; set; }

        [StringLength(20)]
        public string? Gender { get; set; }

        // Identity documents
        [StringLength(50)]
        public string? PassportNumber { get; set; }

        [StringLength(100)]
        public string? PassportIssuingCountry { get; set; }

        public DateTime? PassportExpiryDate { get; set; }

        [StringLength(50)]
        public string? NationalIdNumber { get; set; }

        [StringLength(20)]
        public string? SidNumber { get; set; }

        [StringLength(100)]
        public string? SidIssuingCountry { get; set; }

        [StringLength(150)]
        public string? SidIssuingAuthority { get; set; }

        public DateTime? SidExpiryDate { get; set; }

        // Rank / qualification
        public CrewRank? Rank { get; set; }

        public SeagoingCommercialQualification? PrimaryQualification { get; set; }

        // Employer
        public int? EmployerOperatorId { get; set; }
        public BunkerOperator? EmployerOperator { get; set; }

        [StringLength(200)]
        public string? EmployerName { get; set; }

        // Contact
        [EmailAddress, StringLength(254)]
        public string? Email { get; set; }

        [Phone, StringLength(50)]
        public string? Phone { get; set; }

        [Phone, StringLength(50)]
        public string? EmergencyContactNumber { get; set; }

        [StringLength(150)]
        public string? EmergencyContactName { get; set; }

        [StringLength(500)]
        public string? PhysicalAddress { get; set; }

        public bool IsActive { get; set; } = true;

        // Audit / soft-delete
        public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;
        public string? CreatedByUserId { get; set; }
        public DateTime? ModifiedOnUtc { get; set; }
        public string? ModifiedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedOnUtc { get; set; }
        public string? DeletedByUserId { get; set; }

        public ICollection<CrewAssignment> Assignments { get; set; } = new List<CrewAssignment>();
        public ICollection<CrewDocument> Documents { get; set; } = new List<CrewDocument>();
        public ICollection<ComplianceResult> ComplianceResults { get; set; } = new List<ComplianceResult>();
    }

    // ─────────────────────────────────────────────────────────────────
    // Crew document store (passports, IDs, certificates of competency, etc.)
    // ─────────────────────────────────────────────────────────────────
    public enum CrewDocumentType
    {
        Passport = 0,
        NationalId = 1,
        SeafarerIdentityDocument = 2,
        CertificateOfCompetency = 3,
        MedicalCertificate = 4,
        Visa = 5,
        Other = 99
    }

    public class CrewDocument
    {
        public int Id { get; set; }

        public int CrewMemberId { get; set; }
        public CrewMember CrewMember { get; set; } = null!;

        public CrewDocumentType DocumentType { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(100)]
        public string? IssuingAuthority { get; set; }

        [StringLength(100)]
        public string? IssuingCountry { get; set; }

        [StringLength(100)]
        public string? DocumentNumber { get; set; }

        public DateTime? IssuedDate { get; set; }
        public DateTime? ExpiryDate { get; set; }

        [StringLength(500)]
        public string? StorageUrl { get; set; }

        [StringLength(100)]
        public string? ContentType { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;
        public string? CreatedByUserId { get; set; }
        public DateTime? ModifiedOnUtc { get; set; }
        public string? ModifiedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedOnUtc { get; set; }
        public string? DeletedByUserId { get; set; }
    }

    // ─────────────────────────────────────────────────────────────────
    // Crew assignment — sign-on / sign-off events
    // ─────────────────────────────────────────────────────────────────
    public enum CrewAssignmentStatus
    {
        SignedOn = 0,
        SignedOff = 1,
        Cancelled = 2
    }

    public class CrewAssignment
    {
        public int Id { get; set; }

        public int CrewMemberId { get; set; }
        public CrewMember CrewMember { get; set; } = null!;

        public int VesselId { get; set; }
        public Vessel Vessel { get; set; } = null!;

        public CrewRank RankOnAssignment { get; set; }

        public DateTime SignOnDateUtc { get; set; }
        public DateTime? SignOffDateUtc { get; set; }

        [StringLength(150)]
        public string? PortOfSignOn { get; set; }

        [StringLength(150)]
        public string? PortOfSignOff { get; set; }

        public CrewAssignmentStatus Status { get; set; } = CrewAssignmentStatus.SignedOn;

        [StringLength(1000)]
        public string? Notes { get; set; }

        // Compliance approval at sign-on (linked to ComplianceResult.Id)
        public int? ApprovingComplianceResultId { get; set; }
        public ComplianceResult? ApprovingComplianceResult { get; set; }

        public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;
        public string? CreatedByUserId { get; set; }
        public DateTime? ModifiedOnUtc { get; set; }
        public string? ModifiedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedOnUtc { get; set; }
        public string? DeletedByUserId { get; set; }
    }

    // ─────────────────────────────────────────────────────────────────
    // Vessel point-in-time compliance snapshot
    // ─────────────────────────────────────────────────────────────────
    public enum VesselComplianceState
    {
        FullyCompliant = 0,
        PartiallyCompliant = 1,
        NonCompliant = 2,
        Unknown = 99
    }

    public class VesselComplianceSnapshot
    {
        public int Id { get; set; }

        public int VesselId { get; set; }
        public Vessel Vessel { get; set; } = null!;

        public DateTime EvaluatedOnUtc { get; set; } = DateTime.UtcNow;

        public VesselComplianceState State { get; set; }

        public int CompliantCrewCount { get; set; }
        public int NonCompliantCrewCount { get; set; }
        public int ExpiringSoonCount { get; set; }
        public int TotalCrewCount { get; set; }

        [StringLength(4000)]
        public string? Summary { get; set; }

        [StringLength(450)]
        public string? EvaluatedByUserId { get; set; }
    }

    // ─────────────────────────────────────────────────────────────────
    // Compliance Engine
    // ─────────────────────────────────────────────────────────────────
    public enum ComplianceRequirementScope
    {
        Rank = 0,
        VesselType = 1,
        Operator = 2,
        Universal = 3
    }

    public class ComplianceRule
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public ComplianceRequirementScope Scope { get; set; }

        // Filters (nullable depending on scope)
        public CrewRank? RequiredForRank { get; set; }

        [StringLength(80)]
        public string? RequiredForVesselType { get; set; }

        public int? RequiredForOperatorId { get; set; }
        public BunkerOperator? RequiredForOperator { get; set; }

        // Required course (links to existing Course entity)
        public Guid? RequiredCourseId { get; set; }
        public Course? RequiredCourse { get; set; }

        // Required document type (passport/SID/medical)
        public CrewDocumentType? RequiredDocumentType { get; set; }

        public int? CertificateValidityMonths { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;
        public string? CreatedByUserId { get; set; }
        public DateTime? ModifiedOnUtc { get; set; }
        public string? ModifiedByUserId { get; set; }
    }

    public class ComplianceResult
    {
        public int Id { get; set; }

        public int CrewMemberId { get; set; }
        public CrewMember CrewMember { get; set; } = null!;

        public int? VesselId { get; set; }
        public Vessel? Vessel { get; set; }

        public DateTime EvaluatedOnUtc { get; set; } = DateTime.UtcNow;

        public ComplianceState State { get; set; }

        // Comma-separated rule IDs that failed (kept simple for reporting)
        [StringLength(2000)]
        public string? MissingRuleIds { get; set; }

        // Comma-separated course IDs assigned as remediation
        [StringLength(2000)]
        public string? AssignedCourseIds { get; set; }

        [StringLength(4000)]
        public string? Summary { get; set; }

        public bool TriggeredTrainingAssignment { get; set; }
        public bool TriggeredInvoiceGeneration { get; set; }

        public int? GeneratedInvoiceId { get; set; }
        public Invoice? GeneratedInvoice { get; set; }

        [StringLength(450)]
        public string? EvaluatedByUserId { get; set; }
    }

    // ─────────────────────────────────────────────────────────────────
    // Certificate expiry tracker (drives notifications)
    // ─────────────────────────────────────────────────────────────────
    public enum ExpirySource
    {
        TrainingCertificate = 0,
        CrewDocument = 1
    }

    public enum ExpiryEventStatus
    {
        Pending = 0,
        Notified = 1,
        Acknowledged = 2,
        Expired = 3
    }

    public class CertificateExpiryEvent
    {
        public int Id { get; set; }

        public int CrewMemberId { get; set; }
        public CrewMember CrewMember { get; set; } = null!;

        public ExpirySource Source { get; set; }

        // FK depends on Source — we keep both nullable, only one is set
        public Guid? TrainingCertificateId { get; set; }
        public TrainingCertificate? TrainingCertificate { get; set; }

        public int? CrewDocumentId { get; set; }
        public CrewDocument? CrewDocument { get; set; }

        public DateTime ExpiryDateUtc { get; set; }
        public int DaysUntilExpiry { get; set; }

        public ExpiryEventStatus Status { get; set; } = ExpiryEventStatus.Pending;

        public DateTime DetectedOnUtc { get; set; } = DateTime.UtcNow;
        public DateTime? NotifiedOnUtc { get; set; }
        public DateTime? AcknowledgedOnUtc { get; set; }

        [StringLength(450)]
        public string? AcknowledgedByUserId { get; set; }
    }

    // ─────────────────────────────────────────────────────────────────
    // Compliance audit trail (regulatory-facing)
    // ─────────────────────────────────────────────────────────────────
    public enum ComplianceAuditAction
    {
        CrewSignOnEvaluated = 0,
        CrewSignOffRecorded = 1,
        VesselEvaluated = 2,
        TrainingAutoAssigned = 3,
        InvoiceAutoGenerated = 4,
        DocumentExpired = 5,
        CertificateExpired = 6,
        ManualOverride = 99
    }

    public class ComplianceAuditEntry
    {
        public int Id { get; set; }

        public ComplianceAuditAction Action { get; set; }

        public int? CrewMemberId { get; set; }
        public CrewMember? CrewMember { get; set; }

        public int? VesselId { get; set; }
        public Vessel? Vessel { get; set; }

        public int? ComplianceResultId { get; set; }
        public ComplianceResult? ComplianceResult { get; set; }

        [StringLength(4000)]
        public string? Details { get; set; }

        [StringLength(2000)]
        public string? PayloadJson { get; set; }

        public DateTime OccurredOnUtc { get; set; } = DateTime.UtcNow;

        [StringLength(450)]
        public string? PerformedByUserId { get; set; }

        [StringLength(150)]
        public string? PerformedByDisplayName { get; set; }
    }
}
