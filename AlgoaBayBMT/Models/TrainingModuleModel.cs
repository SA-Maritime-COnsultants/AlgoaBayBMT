namespace AlgoaBayBMT.Models;

public class TrainingModuleModel
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ProgressPercent { get; set; }
    public bool IsCertified { get; set; }
    public string Category { get; set; } = string.Empty;
}
