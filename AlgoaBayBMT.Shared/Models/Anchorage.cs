namespace AlgoaBayBMT.Shared.Models
{
    public class Anchorage : AuditableEntity
    {
        public int Id { get; set; }
        public int OperationalAreaId { get; set; }
        public int? PortId { get; set; }
        public int? BayId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        public OperationalArea? OperationalArea { get; set; }
        public Port? Port { get; set; }
        public Bay? Bay { get; set; }
    }
}
