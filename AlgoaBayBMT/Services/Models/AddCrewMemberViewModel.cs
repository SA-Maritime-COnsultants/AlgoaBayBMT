using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using AlgoaBayBMT.Shared.Models;

namespace AlgoaBayBMT.Services.Models
{
    public sealed class AddCrewMemberViewModel : IValidatableObject
    {
        private static readonly Regex InternationalCellRegex = new(@"^\+[1-9]\d{7,14}$", RegexOptions.Compiled, TimeSpan.FromSeconds(1));
        private static readonly Regex SidNumberRegex = new(@"^[A-Z0-9-]{6,20}$", RegexOptions.Compiled, TimeSpan.FromSeconds(1));

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        [Display(Name = "Full name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Qualification is required.")]
        [Display(Name = "Qualification")]
        public SeagoingCommercialQualification? Qualification { get; set; }

        [Required]
        [StringLength(30)]
        [Display(Name = "Cell number")]
        public string CellNo { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Nationality")]
        public string Nationality { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Display(Name = "SID number")]
        public string SidNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "SID expiry date")]
        public DateTime? SidExpiryDate { get; set; }

        [Required]
        [Display(Name = "SID issue date")]
        public DateTime? SidIssueDate { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "SID issuing authority")]
        public string SidIssuingAuthority { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Display(Name = "Gender")]
        public string Gender { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Date of birth")]
        public DateTime? DateOfBirth { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Passport number")]
        public string PassportNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Passport expiry")]
        public DateTime? PassportExpiry { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrWhiteSpace(CellNo) && !InternationalCellRegex.IsMatch(CellNo.Trim()))
            {
                yield return new ValidationResult("Cell number must be in international format, e.g. +27821234567.", [nameof(CellNo)]);
            }

            var normalizedSid = SidNumber?.Trim().ToUpperInvariant();
            if (!string.IsNullOrWhiteSpace(normalizedSid) && !SidNumberRegex.IsMatch(normalizedSid))
            {
                yield return new ValidationResult("SID must be 6–20 characters and may contain only letters, numbers, and hyphens.", [nameof(SidNumber)]);
            }

            if (SidIssueDate.HasValue && SidExpiryDate.HasValue && SidExpiryDate.Value.Date <= SidIssueDate.Value.Date)
            {
                yield return new ValidationResult("SID expiry date must be later than the SID issue date.", [nameof(SidExpiryDate)]);
            }

            if (PassportExpiry.HasValue && DateOfBirth.HasValue && PassportExpiry.Value.Date <= DateOfBirth.Value.Date)
            {
                yield return new ValidationResult("Passport expiry date is not valid.", [nameof(PassportExpiry)]);
            }
        }
    }
}
