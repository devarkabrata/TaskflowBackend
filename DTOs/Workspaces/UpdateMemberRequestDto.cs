using System.ComponentModel.DataAnnotations;

namespace TaskFlowBackend.DTOs.Workspaces
{
    public class UpdateMemberRequestDto
    {
        [StringLength(200, ErrorMessage = "Title must not exceed 200 characters.")]
        public string? Title { get; set; }
    }
}
