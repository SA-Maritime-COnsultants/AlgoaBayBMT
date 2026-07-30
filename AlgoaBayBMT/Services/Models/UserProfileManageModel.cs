using AlgoaBayBMT.Shared.Models;
using System.ComponentModel.DataAnnotations;

namespace AlgoaBayBMT.Services.Models
{
    public class UserProfileManageModel
    {
        public string UserId { get; set; } = string.Empty;

        public string? UserName { get; set; }

        public string? Email { get; set; }

        public bool EmailConfirmed { get; set; }

        public bool IsCrew { get; set; }

        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(256)]
        public string FullName { get; set; } = string.Empty;

        [Phone]
        [StringLength(30)]
        public string? CellNo { get; set; }

        [StringLength(300)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? Country { get; set; }

        public SeagoingCommercialQualification? Qualification { get; set; }

        [StringLength(20)]
        public string? SidNumber { get; set; }

        [StringLength(100)]
        public string? SidIssuingCountry { get; set; }

        [StringLength(150)]
        public string? SidIssuingAuthority { get; set; }

        public DateTime? SidIssueDate { get; set; }

        public DateTime? SidExpiryDate { get; set; }

        [StringLength(150)]
        public string? GivenNames { get; set; }

        [StringLength(20)]
        public string? Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [StringLength(150)]
        public string? PlaceOfBirth { get; set; }

        [StringLength(100)]
        public string? Nationality { get; set; }

        [StringLength(50)]
        public string? PassportNumber { get; set; }

        public DateTime? PassportExpiry { get; set; }

        public byte[]? ProfilePicture { get; set; }

        [StringLength(100)]
        public string? ProfilePictureContentType { get; set; }
    }
}
