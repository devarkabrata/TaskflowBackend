using System.ComponentModel.DataAnnotations;

namespace TaskFlowBackend.DTOs.Board
{
    public class UpdateBoardStatusRequestDto
    {
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Status name must be between 1 and 100 characters.")]
        public string? Name { get; set; }

        [StringLength(300, ErrorMessage = "Description must not exceed 300 characters.")]
        public string? Description { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Position must be a non-negative number.")]
        public int? Position { get; set; }
    }
}
