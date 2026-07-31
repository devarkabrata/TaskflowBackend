using TaskFlowBackend.DTOs.Tasks;

namespace TaskFlowBackend.DTOs.Users
{
    public class StatResponseDto
    {
        public int WorkspaceCount { get; set; }
        public int TeamCount { get; set; }
        public TaskCountDTO TaskCount { get; set; } = new();
    }
}
