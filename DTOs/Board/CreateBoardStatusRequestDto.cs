using System.ComponentModel.DataAnnotations;

namespace TaskFlowBackend.DTOs.Board
{
    public class CreateBoardStatusRequestDto
    {
        [Required(ErrorMessage = "Status name is required.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Status name must be between 1 and 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(300, ErrorMessage = "Description must not exceed 300 characters.")]
        public string? Description { get; set; }
    }
}
