namespace TaskFlowBackend.DTOs.Users
{
    public class MeResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string AvatarInitials { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public List<WorkspaceMembershipDto> Workspaces { get; set; } = new();
        public List<TeamMembershipDto> Teams { get; set; } = new();
    }

    public class WorkspaceMembershipDto
    {
        public string WorkspaceId { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; }
    }

    public class TeamMembershipDto
    {
        public string TeamId { get; set; } = string.Empty;
        public string WorkspaceId { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; }
    }
}
