namespace TaskFlowBackend.DTOs.Workspaces
{
    public class WorkspaceResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid OwnerId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
