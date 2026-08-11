using TaskFlowBackend.DTOs.Tasks.Archive;
using TaskFlowBackend.Enums;

namespace TaskFlowBackend.Models
{
    public class ArchivedTaskItem
    {
        public Guid Id { get; set; }
        public int TaskNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Priority Priority { get; set; }
        public LabelType? Label { get; set; }
        public Guid StatusId { get; set; }
        public string? Status { get; set; }
        public Guid TeamId { get; set; }
        public List<TaskAssigneeDto> AssigneeDetails { get; set; } = new List<TaskAssigneeDto>();
        public DateTime? ExpectedCompletion { get; set; }
        public int Progress { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
