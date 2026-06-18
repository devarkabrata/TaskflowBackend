namespace TaskFlowBackend.DTOs.Workspaces
{
    public class WorkspaceMemberResponseDto
    {
        public Guid Id { get; set; }
        public Guid WorkspaceId { get; set; }
        public Guid UserId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? JoinedAt { get; set; }
    }
}
