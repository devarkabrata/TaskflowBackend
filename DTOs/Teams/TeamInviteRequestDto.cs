using System.ComponentModel.DataAnnotations;
using TaskFlowBackend.Enums;

namespace TaskFlowBackend.DTOs.Teams
{
    public class TeamInviteRequestDto
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [StringLength(255, ErrorMessage = "Email must not exceed 255 characters.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required.")]
        public TeamRole Role { get; set; }

        public bool AddToWorkspace { get; set; } = false;
    }
}
