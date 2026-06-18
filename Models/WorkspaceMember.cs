using TaskFlowBackend.Enums;

namespace TaskFlowBackend.Models
{
    public class WorkspaceMember
    {
        public Guid Id { get; set; }
        public Guid WorkspaceId { get; set; }
        public Guid UserId { get; set; }
        public WorkspaceMemberStatus Status { get; set; }
        public DateTime? JoinedAt { get; set; }

        // Navigation properties
        public Workspace Workspace { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
