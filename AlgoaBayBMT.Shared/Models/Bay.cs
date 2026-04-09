namespace AlgoaBayBMT.Shared.Models
{
    public class Bay : AuditableEntity
    {
        public int Id { get; set; }
        public int OperationalAreaId { get; set; }
        public int? PortId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        public OperationalArea? OperationalArea { get; set; }
        public Port? Port { get; set; }
        public ICollection<Anchorage> Anchorages { get; set; } = new List<Anchorage>();
    }
}
