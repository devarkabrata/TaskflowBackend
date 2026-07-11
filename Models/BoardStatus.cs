namespace TaskFlowBackend.Models
{
    public class BoardStatus
    {
        public Guid Id { get; set; }
        public Guid TeamId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Position { get; set; }
        public bool IsDeletable { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public Team Team { get; set; } = null!;
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}
