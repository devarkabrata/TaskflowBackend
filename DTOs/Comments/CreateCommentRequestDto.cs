using System.ComponentModel.DataAnnotations;

namespace TaskFlowBackend.DTOs.Comments
{
    public class CreateCommentRequestDto
    {
        [Required(ErrorMessage = "Body is required.")]
        public string Body { get; set; } = string.Empty;

        public List<string> ImageUrls { get; set; } = new List<string>();

        public List<string> ImagePublicIds { get; set; } = new List<string>();
    }
}
