namespace TaskFlowBackend.DTOs.Events
{
    public class TaskCreatedEvent
    {
        public string Timestamp { get; set; } = DateTime.UtcNow.ToString("o");
        public string To { get; set; } = string.Empty;
        public string? From { get; set; }
        public string TaskTitle { get; set; } = string.Empty;
        public string TaskId { get; set; } = string.Empty;
        public string TeamName { get; set; } = string.Empty;
        public string ExpirationDate { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
    }
}
