namespace TaskFlowBackend.DTOs.Export
{
    public class TaskExportRowDto
    {
        public int TaskNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public List<string> AssigneeNames { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
