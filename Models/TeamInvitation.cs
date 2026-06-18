using TaskFlowBackend.Enums;

namespace TaskFlowBackend.Models
{
    public class TeamInvitation
    {
        public Guid Id { get; set; }
        public Guid TeamId { get; set; }
        public Guid InvitedBy { get; set; }
        public string Email { get; set; } = string.Empty;
        public TeamRole Role { get; set; }
        public InvitationStatus Status { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public Team Team { get; set; } = null!;
        public User Sender { get; set; } = null!;
    }
}
