namespace TaskFlowBackend.Models
{
    public class Team
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Color { get; set; } = "#6155DD";
        public Guid WorkspaceId { get; set; }
        public Guid AdminId { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public Workspace Workspace { get; set; } = null!;
        public User Admin { get; set; } = null!;
        public User Creator { get; set; } = null!;
        public ICollection<TeamMember> Members { get; set; } = new List<TeamMember>();
        public ICollection<TeamInvitation> Invitations { get; set; } = new List<TeamInvitation>();
        public ICollection<BoardStatus> BoardStatuses { get; set; } = new List<BoardStatus>();
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}
