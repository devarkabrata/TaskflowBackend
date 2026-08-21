using System.ComponentModel.DataAnnotations;

namespace TaskFlowBackend.DTOs.Teams
{
    public class AddTeamMemberRequestDto
    {
        [Required(ErrorMessage = "UserId is required.")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "RoleId is required.")]
        public Guid RoleId { get; set; }
    }
}
