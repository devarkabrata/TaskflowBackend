namespace TaskFlowBackend.DTOs.Events
{
    public class TeamCreatedEvent
    {
        public string Timestamp { get; set; } = DateTime.UtcNow.ToString("o");
        public string To { get; set; } = string.Empty;
        public string? From { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
    }
}
