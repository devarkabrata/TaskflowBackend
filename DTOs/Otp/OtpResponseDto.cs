using TaskFlowBackend.Enums;

namespace TaskFlowBackend.DTOs.Otp
{
    public class OtpGeneratedResponseDto
    {
        public string Email { get; set; } = string.Empty;
        public OtpEventType Event { get; set; }
        public int ExpiresInMinutes { get; set; }
    }

    public class OtpVerifiedResponseDto
    {
        public bool Verified { get; set; }
        public OtpEventType Event { get; set; }
    }
}
