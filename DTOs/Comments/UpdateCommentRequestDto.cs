using System.ComponentModel.DataAnnotations;

namespace TaskFlowBackend.DTOs.Comments
{
    public class UpdateCommentRequestDto
    {
        [Required(ErrorMessage = "Body is required.")]
        public string Body { get; set; } = string.Empty;

        public List<string>? ImageUrls { get; set; }

        public List<string>? ImagePublicIds { get; set; }
    }
}
