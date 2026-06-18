namespace TaskFlowBackend.DTOs.Teams
{
    public class TeamMemberResponseDto
    {
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string AvatarInitials { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; }
    }
}
