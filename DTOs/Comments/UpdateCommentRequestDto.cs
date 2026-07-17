using System.ComponentModel.DataAnnotations;

namespace TaskFlowBackend.DTOs.Comments
{
    public class UpdateCommentRequestDto
    {
        [Required(ErrorMessage = "Body is required.")]
        public string Body { get; set; } = string.Empty;
    }
}
