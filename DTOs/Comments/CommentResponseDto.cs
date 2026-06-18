namespace TaskFlowBackend.DTOs.Comments
{
    public class CommentResponseDto
    {
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }
        public Guid AuthorId { get; set; }
        public string Body { get; set; } = string.Empty;
        public List<string> ImageUrls { get; set; } = new List<string>();
        public List<string> ImagePublicIds { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
