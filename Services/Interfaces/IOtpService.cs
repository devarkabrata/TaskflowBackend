using TaskFlowBackend.DTOs.Otp;

namespace TaskFlowBackend.Services.Interfaces
{
    public interface IOtpService
    {
        Task<OtpGeneratedResponseDto> GenerateOtpAsync(GenerateOtpRequestDto dto);
        Task<OtpVerifiedResponseDto> VerifyOtpAsync(VerifyOtpRequestDto dto);
    }
}
