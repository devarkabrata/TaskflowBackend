namespace TaskFlowBackend.DTOs
{
    public class HealthResponseDTO
    {
        public DateTime Timestamp {get; set;} = DateTime.UtcNow;
        public string Source {get; set;} = "";
    }
}