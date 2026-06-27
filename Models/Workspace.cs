namespace TaskFlowBackend.Models
{
    public class Workspace
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid OwnerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public User Owner { get; set; } = null!;
        public ICollection<WorkspaceMember> Members { get; set; } = new List<WorkspaceMember>();
        public ICollection<WorkspaceInvitation> Invitations { get; set; } = new List<WorkspaceInvitation>();
        public ICollection<Team> Teams { get; set; } = new List<Team>();

    }
}
