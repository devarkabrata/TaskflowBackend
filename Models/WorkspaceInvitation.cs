using TaskFlowBackend.Enums;

namespace TaskFlowBackend.Models
{
    public class WorkspaceInvitation
    {
        public Guid Id { get; set; }
        public Guid WorkspaceId { get; set; }
        public Guid InvitedBy { get; set; }
        public string Email { get; set; } = string.Empty;
        public InvitationStatus Status { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public Workspace Workspace { get; set; } = null!;
        public User Sender { get; set; } = null!;
    }
}
