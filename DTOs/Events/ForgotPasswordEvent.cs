namespace TaskFlowBackend.DTOs.Events
{
    public class ForgotPasswordEvent
    {
        public string Timestamp { get; set; } = DateTime.UtcNow.ToString("o");
        public string To { get; set; } = string.Empty;
        public string? From { get; set; }
        public string ResetLink { get; set; } = string.Empty;
        public int ExpiresInMinutes { get; set; } = 30;
    }
}
