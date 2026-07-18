namespace TaskFlowBackend.DTOs.Teams
{
    public class TeamResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Color { get; set; } = string.Empty;
        public Guid WorkspaceId { get; set; }
        public Guid AdminId { get; set; }
        public int PendingInvites { get; set; }
        public List<TeamMemberSummaryDto>? Members { get; set; } = new();
        public Dictionary<string, int> StatusTaskCounts { get; set; } = new(); 
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class TeamMemberSummaryDto
    {
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string AvatarInitials { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
