namespace TaskFlowBackend.DTOs.Tasks
{
    public class TaskResponseDto
    {
        public Guid Id { get; set; }
        public int TaskNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string? Label { get; set; }
        public Guid StatusId { get; set; }
        public string? Status { get; set; }
        public Guid TeamId { get; set; }
        public List<AssigneeSummaryDto> Assignees { get; set; } = new List<AssigneeSummaryDto>();
        public DateTime? ExpectedCompletion { get; set; }
        public int Progress { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class AssigneeSummaryDto
    {
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string AvatarInitials { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
    }
}
