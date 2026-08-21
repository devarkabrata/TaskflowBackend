using System.ComponentModel.DataAnnotations;

namespace TaskFlowBackend.DTOs.Teams
{
    public class UpdateTeamRequestDto
    {
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Team name must be between 2 and 200 characters.")]
        public string? Name { get; set; }

        [StringLength(500, ErrorMessage = "Description must not exceed 500 characters.")]
        public string? Description { get; set; }

        [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be a valid hex code e.g. #6155DD.")]
        public string? Color { get; set; }

        public List<TeamMemberUpdateDto>? Members { get; set; }
    }

    public class TeamMemberUpdateDto
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid RoleId { get; set; }
    }
}
