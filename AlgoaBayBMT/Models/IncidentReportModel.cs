namespace AlgoaBayBMT.Models;

public class IncidentReportModel
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Severity { get; set; } = "Normal";
    public DateTime ReportedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
}
