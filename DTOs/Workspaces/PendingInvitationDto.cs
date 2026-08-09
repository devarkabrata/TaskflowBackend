namespace TaskFlowBackend.DTOs.Workspaces
{
    public class PendingInvitationDto
    {
        public Guid Id { get; set; }
        public Guid WorkspaceId { get; set; }
        public string WorkspaceName { get; set; } = string.Empty;
        public string InvitedBy { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
