namespace TaskFlowBackend.DTOs.Users
{
    public class UserResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string AvatarInitials { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
    }
}
