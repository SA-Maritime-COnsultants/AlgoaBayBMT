using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using AlgoaBayBMT.Shared.Models;
using AlgoaBayBMT.Shared.Security;

namespace AlgoaBayBMT.Services.Models
{
    public sealed class UserAdministrationModel : IValidatableObject
    {
        private string? sidNumber;
        private bool isCrew;

        public string? UserId { get; set; }

        [Required]
        [Display(Name = "Full name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Cell No")]
        [StringLength(30)]
        public string? CellNo { get; set; }

        [Display(Name = "Address")]
        [StringLength(300)]
        public string? Address { get; set; }

        [Display(Name = "Country")]
        [StringLength(100)]
        public string? Country { get; set; }

        [Display(Name = "Assigned role")]
        public string AssignedRole { get; set; } = RoleNames.Crew;

        [Display(Name = "Crew")]
        public bool IsCrew
        {
            get => isCrew;
            set
            {
                isCrew = value;
                if (!isCrew)
                {
                    CrewRank = null;
                    SidNumber = null;
                    SidIssuingCountry = null;
                    SidIssuingAuthority = null;
                    SidIssueDate = null;
                    SidExpiryDate = null;
                }
            }
        }

        [Display(Name = "Crew rank")]
        public CrewRank? CrewRank { get; set; }

        [Display(Name = "SID Number")]
        [StringLength(20)]
        [RegularExpression("^[A-Z0-9-]{6,20}$", ErrorMessage = "SID must be 6–20 characters and may contain only letters, numbers, and hyphens.")]
        public string? SidNumber
        {
            get => sidNumber;
            set => sidNumber = value?.Trim().ToUpperInvariant();
        }

        [Display(Name = "SID Issuing Country")]
        [StringLength(100)]
        public string? SidIssuingCountry { get; set; }

        [Display(Name = "SID Issuing Authority")]
        [StringLength(150)]
        public string? SidIssuingAuthority { get; set; }

        [Display(Name = "SID Issue Date")]
        public DateTime? SidIssueDate { get; set; }

        [Display(Name = "SID Expiry Date")]
        public DateTime? SidExpiryDate { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Approved")]
        public bool IsAccountApproved { get; set; } = true;

        [Display(Name = "Email Confirmed")]
        public bool EmailConfirmed { get; set; }

        [Display(Name = "Password")]
        public string? Password { get; set; }

        [Display(Name = "Confirm password")]
        public string? ConfirmPassword { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(CellNo))
            {
                yield return new ValidationResult("Cell No is required.", [nameof(CellNo)]);
            }

            if (string.IsNullOrWhiteSpace(Address))
            {
                yield return new ValidationResult("Address is required.", [nameof(Address)]);
            }

            if (string.IsNullOrWhiteSpace(Country))
            {
                yield return new ValidationResult("Country is required.", [nameof(Country)]);
            }

            if (!IsCrew)
            {
                yield break;
            }

            if (CrewRank is null)
            {
                yield return new ValidationResult("Rank is required when Crew is selected.", [nameof(CrewRank)]);
            }

            if (string.IsNullOrWhiteSpace(SidNumber) || !Regex.IsMatch(SidNumber, "^[A-Z0-9-]{6,20}$"))
            {
                yield return new ValidationResult("SID must be 6–20 characters and may contain only letters, numbers, and hyphens.", [nameof(SidNumber)]);
            }

            if (string.IsNullOrWhiteSpace(SidIssuingCountry))
            {
                yield return new ValidationResult("SID issuing country is required when Crew is selected.", [nameof(SidIssuingCountry)]);
            }

            if (string.IsNullOrWhiteSpace(SidIssuingAuthority))
            {
                yield return new ValidationResult("SID issuing authority is required when Crew is selected.", [nameof(SidIssuingAuthority)]);
            }
        }
    }
}