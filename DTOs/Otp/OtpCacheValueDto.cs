namespace TaskFlowBackend.DTOs.Otp
{
    public class OtpCacheValueDto
    {
        public string Otp { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
