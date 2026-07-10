using TaskFlowBackend.DTOs.Tasks;

namespace TaskFlowBackend.DTOs.Board
{
    public class BoardStatusResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Position { get; set; }
        public int TotalTasks { get; set; }

        // All non-deleted tasks in this team currently on this status
        public List<TaskResponseDto> Tasks { get; set; } = new();
    }
}
