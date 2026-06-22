using System.ComponentModel.DataAnnotations;

namespace TaskFlowBackend.DTOs.Auth
{
    public class CreateUserRequestDto
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 200 characters.")]
        public string Name { get; set; } = string.Empty;

        // Resolved designation — if user selected "Other", send the free-text value here
        [StringLength(200, ErrorMessage = "Title must not exceed 200 characters.")]
        public string? Title { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [StringLength(255, ErrorMessage = "Email must not exceed 255 characters.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
        public string Password { get; set; } = string.Empty;

        public string? AvatarUrl { get; set; } = string.Empty;

        public string? AvatarPublicId { get; set; } = string.Empty;

        public string? AvatarInitials { get; set; } = string.Empty;
    }
}
