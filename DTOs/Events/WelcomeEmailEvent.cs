namespace TaskFlowBackend.DTOs.Events
{
    public class WelcomeEmailEvent
    {
        public string Timestamp { get; set; } = DateTime.UtcNow.ToString("o");
        public string To { get; set; } = string.Empty;
        public string? From { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string WelcomeMessage { get; set; } = string.Empty;
    }
}
