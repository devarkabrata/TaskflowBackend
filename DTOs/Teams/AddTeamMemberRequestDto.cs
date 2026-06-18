using System.ComponentModel.DataAnnotations;
using TaskFlowBackend.Enums;

namespace TaskFlowBackend.DTOs.Teams
{
    public class AddTeamMemberRequestDto
    {
        [Required(ErrorMessage = "UserId is required.")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Role is required.")]
        public TeamRole Role { get; set; }
    }
}
