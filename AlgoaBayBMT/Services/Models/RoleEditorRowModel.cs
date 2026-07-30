namespace AlgoaBayBMT.Services.Models
{
    public class RoleEditorRowModel
    {
        public string RoleId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public string NormalizedName { get; set; } = string.Empty;
        public int UserCount { get; set; }
    }
}
