using System.ComponentModel.DataAnnotations;
using TaskFlowBackend.Enums;

namespace TaskFlowBackend.DTOs.Tasks
{
    public class CreateTaskRequestDto
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 500 characters.")]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required(ErrorMessage = "Priority is required.")]
        public Priority Priority { get; set; }

        public LabelType? Label { get; set; }

        [Required(ErrorMessage = "StatusId is required.")]
        public Guid StatusId { get; set; }

        [Required(ErrorMessage = "TeamId is required.")]
        public Guid TeamId { get; set; }

        public List<Guid> AssigneeIds { get; set; } = new List<Guid>();

        public DateTime? ExpectedCompletion { get; set; }

        [Range(0, 100, ErrorMessage = "Progress must be between 0 and 100.")]
        public int Progress { get; set; } = 0;
    }
}
