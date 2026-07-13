namespace TaskFlowBackend.DTOs
{
    public class BoardStatusResponseDTO
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Position { get; set; }
        public Guid TeamId { get; set; }
        public bool IsArchievable { get; set; }
    }
}