namespace TaskFlowBackend.DTOs.Events
{
    public class OTPEvent
    {
        public string Timestamp { get; set; } = DateTime.UtcNow.ToString("o");
        public string To { get; set; } = string.Empty;
        public string? From { get; set; }
        public string OTP { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? For { get; set; }
        public int? Ttl { get; set; }
    }
}
