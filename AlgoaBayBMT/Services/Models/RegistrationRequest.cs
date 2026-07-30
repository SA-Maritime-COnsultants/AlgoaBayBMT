using System.ComponentModel.DataAnnotations;
using AlgoaBayBMT.Shared.Models;

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
        public string? CellNo { get; set; }
        public string? Address { get; set; }
        public string? Country { get; set; }
        public bool IsCrew { get; set; }
        public CrewRank? CrewRank { get; set; }
        public string? SidNumber { get; set; }
        public string? SidIssuingCountry { get; set; }
        public string? SidIssuingAuthority { get; set; }
        public DateTime? SidIssueDate { get; set; }
        public DateTime? SidExpiryDate { get; set; }
    }
}
