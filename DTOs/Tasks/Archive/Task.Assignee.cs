namespace TaskFlowBackend.DTOs.Tasks.Archive
{
    public class TaskAssigneeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? AvatarInitials { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; } = string.Empty;
    }
}