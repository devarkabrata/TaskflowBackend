using Microsoft.AspNetCore.Mvc;
using TaskFlowBackend.DTOs.Auth;
using TaskFlowBackend.Helpers.API;
using TaskFlowBackend.Services.Interfaces;

namespace TaskFlowBackend.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("signup")]
        public async Task<ApiResponse<object>> Signup([FromBody] SignupRequestDto dto)
        {
            var result = await _authService.SignupAsync(dto);
            return ApiResponse<object>.Success(result, "Signed up Successfully", 200);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            return Ok(ApiResponse<AuthResponseDto>.Success(result, "Login successful."));
        }
    }
}
