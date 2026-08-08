using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlowBackend.DTOs.Otp;
using TaskFlowBackend.Helpers.API;
using TaskFlowBackend.Services.Interfaces;

namespace TaskFlowBackend.Controllers
{
    [ApiController]
    [Route("api/otp")]
    public class OtpController : ControllerBase
    {
        private readonly IOtpService _otpService;

        public OtpController(IOtpService otpService)
        {
            _otpService = otpService;
        }

        [AllowAnonymous]
        [HttpPost("generate")]
        public async Task<ApiResponse<OtpGeneratedResponseDto>> Generate([FromBody] GenerateOtpRequestDto dto, [FromQuery] bool platform = false)
        {
            var result = await _otpService.GenerateOtpAsync(dto, platform);
            return ApiResponse<OtpGeneratedResponseDto>.Success(result, "OTP sent successfully.");
        }

        [AllowAnonymous]
        [HttpPost("verify")]
        public async Task<ApiResponse<OtpVerifiedResponseDto>> Verify([FromBody] VerifyOtpRequestDto dto)
        {
            var result = await _otpService.VerifyOtpAsync(dto);
            return ApiResponse<OtpVerifiedResponseDto>.Success(result, "OTP verified successfully.");
        }
    }
}
