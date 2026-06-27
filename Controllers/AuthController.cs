using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TaskFlowBackend.DTOs.Auth;
using TaskFlowBackend.DTOs.Users;
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
        private readonly IUserService _userService;

        public AuthController(IAuthService authService, IUserService userService)
        {
            _authService = authService;
            _userService = userService;
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

        [Authorize]
        [HttpGet("me")]
        public async Task<ApiResponse<UserResponseDto>> GetUserById()
        {
            var id = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            
            var result = await _userService.GetUser(id);

            var response = new UserResponseDto
            {
                Id = result!.Id,
                Name = result.Name,
                Email = result.Email,
                Title = result.Title,
                AvatarInitials = result.AvatarInitials,
                AvatarUrl = result.AvatarUrl,
                Workspaces = result.OwnedWorkspaces,
                Teams = result.AdminTeams
            };

            return ApiResponse<UserResponseDto>.Success(response, "User fetched successfully.");
        }
    }
}
