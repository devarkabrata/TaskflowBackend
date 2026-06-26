using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TaskFlowBackend.DTOs.Auth;
using TaskFlowBackend.Helpers.API;
using TaskFlowBackend.Helpers.CustomException;
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

        [AllowAnonymous]
        [HttpPost("signup")]
        public async Task<ApiResponse<SignupResponseDto>> Signup([FromBody] SignupRequestDto dto)
        {
            var result = await _authService.SignupAsync(dto);

            SignupResponseDto response = new SignupResponseDto
            {
                Id = result!.Id,
                Name = result.Name,
                Title = result.Title ?? "",
                Email = result.Email,
                AvatarInitials = result.AvatarInitials ?? ""
            };

            return ApiResponse<SignupResponseDto>.Success(response, "User Signed up Successfully", 201);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ApiResponse<AuthResponseDto>> Login([FromBody] LoginRequestDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            return ApiResponse<AuthResponseDto>.Success(result, "Login successful.");
        }

        [AllowAnonymous]
        [HttpPatch("refresh")]
        public async Task<ApiResponse<AuthResponseDto>> Refresh([FromBody] RefreshTokenRequestDTO dto)
        {
            var result = await _authService.RefreshAsync(dto.RefreshToken);

            if(result.IsNullOrEmpty())
            {
                throw new NotFoundException("Refresh token not found");
            }

            AuthResponseDto response = new AuthResponseDto
            {
                Token = result ?? "",
                RefreshToken = dto.RefreshToken
            };
            return ApiResponse<AuthResponseDto>.Success(response, "Token refreshed successfully.");
        }
    }
}
