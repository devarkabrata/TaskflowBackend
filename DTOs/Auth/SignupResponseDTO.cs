using System.ComponentModel.DataAnnotations;

namespace TaskFlowBackend.DTOs.Auth
{
    public class SignupResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string Email { get; set; } = string.Empty;
        public string AvatarInitials { get; set; } = string.Empty;
    }
}
