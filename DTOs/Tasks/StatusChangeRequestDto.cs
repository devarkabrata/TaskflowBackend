using System.ComponentModel.DataAnnotations;

namespace TaskFlowBackend.DTOs.Tasks
{
    public class StatusChangeRequestDto
    {
        [Required(ErrorMessage = "StatusId is required.")]
        public Guid StatusId { get; set; }
    }
}
