namespace AlgoaBayBMT.Models;

public class OperationStatusModel
{
    public int Id { get; set; }
    public string VesselName { get; set; } = string.Empty;
    public string OperationType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public string BerthLocation { get; set; } = string.Empty;
}
