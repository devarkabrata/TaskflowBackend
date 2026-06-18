namespace TaskFlowBackend.DTOs.Workspaces
{
    public class WorkspaceInvitationResponseDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
