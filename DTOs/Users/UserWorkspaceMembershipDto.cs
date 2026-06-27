namespace TaskFlowBackend.DTOs.Users
{
    public class UserWorkspaceMembershipDto
    {
        public Guid WorkspaceId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? JoinedAt { get; set; }
    }
}
