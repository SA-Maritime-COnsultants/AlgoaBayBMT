using System.ComponentModel.DataAnnotations;

namespace AlgoaBayBMT.Services.Models
{
    public class RegistrationRequest
    {
        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string? FullName { get; set; }
        public string RequestedRole { get; set; } = string.Empty;
        public int? CompanyId { get; set; }
        public int? VesselId { get; set; }
        public int? PrimaryAreaId { get; set; }
    }
}
