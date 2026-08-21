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

            var response = new SignupResponseDto
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

            if (result.IsNullOrEmpty())
                throw new NotFoundException("Refresh token not found");

            var response = new AuthResponseDto
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
            var subClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (string.IsNullOrEmpty(subClaim))
                throw new UnauthorizedException("Invalid or missing token claims.");

            var id = Guid.Parse(subClaim);
            var result = await _userService.GetUser(id);

            if (result == null)
                throw new NotFoundException("User not found.");

            var response = new UserResponseDto
            {
                Id = result.Id,
                Name = result.Name,
                Email = result.Email,
                Title = result.Title ?? "",
                AvatarInitials = result.AvatarInitials ?? "",
                AvatarUrl = result.AvatarUrl,
                Workspaces = result.WorkspaceMemberships?.Select(wm => new UserWorkspaceMembershipDto
                {
                    WorkspaceId = wm.WorkspaceId,
                    Name = wm.Workspace.Name,
                    Role = wm.Workspace.OwnerId == result.Id ? "owner" : "member",
                    Status = wm.Status.ToString().ToLower(),
                    JoinedAt = wm.JoinedAt
                }).ToList() ?? new(),
                Teams = result.TeamMemberships?.Select(tm => new UserTeamMembershipDto
                {
                    TeamId = tm.TeamId,
                    TeamName = tm.Team.Name,
                    WorkspaceId = tm.Team.WorkspaceId,
                    Role = tm.Role.Name ?? "",
                    JoinedAt = tm.JoinedAt
                }).ToList() ?? new()
            };

            return ApiResponse<UserResponseDto>.Success(response, "User fetched successfully.");
        }

        [Authorize]
        [HttpGet("me/stats")]
        public async Task<ApiResponse<StatResponseDto>> GetUserStatsById()
        {
            var subClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (string.IsNullOrEmpty(subClaim))
                throw new UnauthorizedException("Invalid or missing token claims.");

            var id = Guid.Parse(subClaim);
            var result = await _userService.GetUser(id);

            if (result == null)
                throw new NotFoundException("User not found.");

            var response = await _userService.GetUserStatsAsync(result.Id);

            return ApiResponse<StatResponseDto>.Success(response, "User stats fetched successfully.");
        }

        [Authorize]
        [HttpGet("me/settings")]
        public async Task<ApiResponse<SettingsResponseDto>> GetUserSettingsById()
        {
            var subClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (string.IsNullOrEmpty(subClaim))
                throw new UnauthorizedException("Invalid or missing token claims.");

            var id = Guid.Parse(subClaim);
            var result = await _userService.GetUserSettings(id);

            if (result == null)
                throw new NotFoundException("User settings not found.");

            var response = new SettingsResponseDto
            {
                Id = result.Id,
                UserId = result.UserId,
                DaysToArchieve = result.DaysToArchieve,
                CreatedAt = result.CreatedAt,
                UpdatedAt = result.UpdatedAt
            };

            return ApiResponse<SettingsResponseDto>.Success(response, "User settings fetched successfully.");
        }
    }
}
