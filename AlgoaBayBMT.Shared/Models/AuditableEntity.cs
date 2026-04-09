namespace AlgoaBayBMT.Shared.Models
{
    public abstract class AuditableEntity
    {
        public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedOnUtc { get; set; }
    }
}
