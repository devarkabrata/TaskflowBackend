namespace TaskFlowBackend.DTOs.Comments
{
    public class CommentResponseDto
    {
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }
        public CommentAuthorDto Author { get; set; } = null!;
        public string Body { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CommentAuthorDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
    }
}
