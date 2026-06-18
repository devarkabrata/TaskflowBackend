using TaskFlowBackend.Enums;

namespace TaskFlowBackend.Models
{
    public class TaskItem
    {
        public Guid Id { get; set; }
        public int TaskNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Priority Priority { get; set; }
        public LabelType? Label { get; set; }
        public Guid StatusId { get; set; }
        public Guid TeamId { get; set; }
        public Guid[] AssigneeIds { get; set; } = Array.Empty<Guid>();
        public DateTime? ExpectedCompletion { get; set; }
        public int Progress { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public BoardStatus Status { get; set; } = null!;
        public Team Team { get; set; } = null!;
        public User Creator { get; set; } = null!;
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
