namespace TaskFlowBackend.Models
{
    public class TeamMember
    {
        public Guid TeamId { get; set; }
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
        public DateTime JoinedAt { get; set; }

        // Navigation properties
        public Team Team { get; set; } = null!;
        public User User { get; set; } = null!;
        public Roles Role { get; set; } = null!;
    }
}
