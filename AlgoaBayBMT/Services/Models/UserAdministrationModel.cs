using System.ComponentModel.DataAnnotations;
using AlgoaBayBMT.Shared.Security;

namespace AlgoaBayBMT.Services.Models
{
    public sealed class UserAdministrationModel
    {
        public string? UserId { get; set; }

        [Required]
        [Display(Name = "Full name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Assigned role")]
        public string AssignedRole { get; set; } = RoleNames.Crew;

        [Display(Name = "Company")]
        public int? CompanyId { get; set; }

        [Display(Name = "Operational area")]
        public int? PrimaryAreaId { get; set; }

        [Display(Name = "Vessel")]
        public int? VesselId { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Approved")]
        public bool IsAccountApproved { get; set; } = true;

        [Display(Name = "Password")]
        public string? Password { get; set; }

        [Display(Name = "Confirm password")]
        public string? ConfirmPassword { get; set; }
    }
}