namespace TaskFlowBackend.DTOs.Events
{
    public class WorkspaceInviteEvent
    {
        public string Timestamp { get; set; } = DateTime.UtcNow.ToString("o");
        public string To { get; set; } = string.Empty;
        public string? From { get; set; }
        public string WorkspaceName { get; set; } = string.Empty;
        public string InvitedBy { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string InviteLink { get; set; } = string.Empty;
    }
}
