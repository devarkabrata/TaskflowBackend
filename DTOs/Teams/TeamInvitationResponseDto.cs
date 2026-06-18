namespace TaskFlowBackend.DTOs.Teams
{
    public class TeamInvitationResponseDto
    {
        public Guid Id { get; set; }
        public Guid TeamId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
