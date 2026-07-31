namespace TaskFlowBackend.DTOs.Tasks
{
    public class TaskSummaryDto
    {
        public Guid Id { get; set; }
        public int TaskNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string? Label { get; set; }
        public Guid StatusId { get; set; }
        public Guid TeamId { get; set; }
    }

    public class TaskCountDTO
    {
        public int ActiveTasks { get; set; }
        public int ArchieveTask { get; set; }
    }
}