using System.ComponentModel.DataAnnotations;

namespace TaskFlowBackend.DTOs.Teams
{
    public class UpdateTeamMemberRoleRequestDto
    {
        [Required(ErrorMessage = "RoleId is required.")]
        public Guid RoleId { get; set; }
    }
}
