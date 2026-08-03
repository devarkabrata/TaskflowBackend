using System.ComponentModel.DataAnnotations;

namespace TaskFlowBackend.DTOs.Users
{
    public class UpdateUserRequestDto
    {
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 200 characters.")]
        public string? Name { get; set; }

        [StringLength(200, ErrorMessage = "Title must not exceed 200 characters.")]
        public string? Title { get; set; }

        [Url(ErrorMessage = "AvatarUrl must be a valid URL.")]
        [StringLength(500, ErrorMessage = "AvatarUrl must not exceed 500 characters.")]
        public string? AvatarUrl { get; set; }

        public string? AvatarPublicId { get; set; }
    }

    public class UpdateUserSettingsRequestDto
    {
        [Required(ErrorMessage = "Need to specify Days to Archieve")]
        public int DaysToArchieve { get; set; }
    }
}
