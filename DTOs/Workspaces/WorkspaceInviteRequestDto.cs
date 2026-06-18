using System.ComponentModel.DataAnnotations;

namespace TaskFlowBackend.DTOs.Workspaces
{
    public class WorkspaceInviteRequestDto
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [StringLength(255, ErrorMessage = "Email must not exceed 255 characters.")]
        public string Email { get; set; } = string.Empty;
    }
}
