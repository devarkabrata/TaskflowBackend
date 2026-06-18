using System.ComponentModel.DataAnnotations;
using TaskFlowBackend.Enums;

namespace TaskFlowBackend.DTOs.Tasks
{
    public class UpdateTaskRequestDto
    {
        [StringLength(500, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 500 characters.")]
        public string? Title { get; set; }

        public string? Description { get; set; }

        public Priority? Priority { get; set; }

        public LabelType? Label { get; set; }

        public Guid? StatusId { get; set; }

        public List<Guid>? AssigneeIds { get; set; }

        public DateTime? ExpectedCompletion { get; set; }

        [Range(0, 100, ErrorMessage = "Progress must be between 0 and 100.")]
        public int? Progress { get; set; }
    }
}
