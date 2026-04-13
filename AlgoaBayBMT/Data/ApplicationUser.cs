using AlgoaBayBMT.Shared.Models;
using Microsoft.AspNetCore.Identity;

namespace AlgoaBayBMT.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? CellNo { get; set; }
        public string? Address { get; set; }
        public string? Country { get; set; }
        public string? RequestedRole { get; set; }
        public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.PendingEmailConfirmation;
        public bool IsCrew { get; set; }
        public SeagoingCommercialQualification? Qualification { get; set; }
        public CrewRank? CrewRank { get; set; }
        public string? SidNumber { get; set; }
        public string? SidIssuingCountry { get; set; }
        public string? SidIssuingAuthority { get; set; }
        public DateTime? SidIssueDate { get; set; }
        public DateTime? SidExpiryDate { get; set; }
        public bool IsAccountApproved { get; set; }
        public string? ApprovedByUserId { get; set; }
        public DateTime? ApprovedOnUtc { get; set; }
        public string? ApprovalNotes { get; set; }
        public int? CompanyId { get; set; }
        public int? PrimaryAreaId { get; set; }
        public int? VesselId { get; set; }
        public byte[]? ProfilePicture { get; set; }
        public string? ProfilePictureContentType { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime RegisteredOnUtc { get; set; } = DateTime.UtcNow;

        public BunkeringCompany? Company { get; set; }
        public OperationalArea? PrimaryArea { get; set; }
        public Vessel? Vessel { get; set; }
        public CrewMemberDetails? CrewMemberDetails { get; set; }
        public ICollection<UserAreaAssignment> UserAreaAssignments { get; set; } = new List<UserAreaAssignment>();
        public ICollection<VesselRoleAssignment> VesselRoleAssignments { get; set; } = new List<VesselRoleAssignment>();
        public ICollection<CrewDeployment> CrewDeployments { get; set; } = new List<CrewDeployment>();
        public ICollection<VesselCrewListEntry> VesselCrewListEntries { get; set; } = new List<VesselCrewListEntry>();
    }
}
