namespace TaskFlowBackend.DTOs.Board
{
    public class BoardResponseDto
    {
        public Guid TeamId { get; set; }
        public List<BoardStatusResponseDto> Columns { get; set; } = new();
    }
}
