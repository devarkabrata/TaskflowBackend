using TaskFlowBackend.Enums;

namespace TaskFlowBackend.Models
{
    public class Roles
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public bool IsEnable { get; set; } = true;
        public List<PermissionType> Permissions { get; set; } = [];
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<TeamMember> TeamMembers { get; set; } = new List<TeamMember>();
        public ICollection<TeamInvitation> TeamInvitations { get; set; } = new List<TeamInvitation>();
    }
}
