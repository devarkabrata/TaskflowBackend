using System.ComponentModel.DataAnnotations;

namespace TaskFlowBackend.DTOs.Teams
{
    public class CreateTeamRequestDto
    {
        [Required(ErrorMessage = "Team name is required.")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Team name must be between 2 and 200 characters.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description must not exceed 500 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Color is required.")]
        [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be a valid hex code e.g. #6155DD.")]
        public string Color { get; set; } = string.Empty;

        // Optional — existing workspace members to add at creation time
        public List<TeamMemberInitDto>? MemberIds { get; set; }
    }

    public class TeamMemberInitDto
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid RoleId { get; set; }
    }
}
