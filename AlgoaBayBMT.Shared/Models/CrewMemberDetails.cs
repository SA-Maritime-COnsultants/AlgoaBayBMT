namespace AlgoaBayBMT.Shared.Models
{
    public class CrewMemberDetails : AuditableEntity
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string? GivenNames { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? PlaceOfBirth { get; set; }
        public string? Nationality { get; set; }
        public string? PassportNumber { get; set; }
        public DateTime? PassportExpiry { get; set; }
    }
}
