namespace TaskFlowBackend.DTOs.Board
{
    public class BoardStatusResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Position { get; set; }
        public int TotalTasks { get; set; }

        // Populated with up to 5 tasks — will be typed once TaskItem model is built
        public List<object> Tasks { get; set; } = new();
    }
}
