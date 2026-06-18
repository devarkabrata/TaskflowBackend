using System.ComponentModel.DataAnnotations;
using TaskFlowBackend.Enums;

namespace TaskFlowBackend.DTOs.Teams
{
    public class UpdateTeamMemberRoleRequestDto
    {
        [Required(ErrorMessage = "Role is required.")]
        public TeamRole Role { get; set; }
    }
}
