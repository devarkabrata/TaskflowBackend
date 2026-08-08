using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlowBackend.DTOs.Users;
using TaskFlowBackend.DTOs.Workspaces;
using TaskFlowBackend.Helpers.API;
using TaskFlowBackend.Helpers.Pagination;
using TaskFlowBackend.Models;
using TaskFlowBackend.Services.Interfaces;

namespace TaskFlowBackend.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ApiResponse<List<UserResponseDto>>> GetAllUsers([FromQuery] string? search = null, [FromQuery] int? limit = null, [FromQuery] int? page = null, [FromQuery] Guid? workspaceId = null)
        {
            var paginationParams = new PaginationParams
            {
                Limit = limit ?? 20,
                Page = page ?? 1,
            };

            var result = await _userService.GetAllUsers(search, paginationParams, workspaceId);

            var response = result.Select(user => new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Title = user.Title,
                AvatarInitials = user.AvatarInitials,
                AvatarUrl = user.AvatarUrl,
            }).ToList();

            return ApiResponse<List<UserResponseDto>>.Success(response, "Users fetched successfully.");
        }

        [HttpGet("{id:guid}")]
        public async Task<ApiResponse<UserResponseDto>> GetUserById(Guid id)
        {
            var result = await _userService.GetUser(id);

            var response = new UserResponseDto
            {
                Id = result!.Id,
                Name = result.Name,
                Email = result.Email,
                Title = result.Title,
                AvatarInitials = result.AvatarInitials,
                AvatarUrl = result.AvatarUrl,
                Workspaces = result.OwnedWorkspaces.Select(w => new UserWorkspaceMembershipDto
                {
                    WorkspaceId = w.Id,
                    Name = w.Name,
                    Role = w.OwnerId == result.Id ? "owner" : "member",
                    Status = "active",
                }).ToList(),
                Teams = result.AdminTeams.Select(t => new UserTeamMembershipDto
                {
                    TeamId = t.Id,
                    TeamName = t.Name,
                    WorkspaceId = t.WorkspaceId,
                    Role = "admin",
                }).ToList()
            };

            return ApiResponse<UserResponseDto>.Success(response, "User fetched successfully.");
        }

        [HttpPut("{id:guid}")]
        public async Task<ApiResponse<UserResponseDto>> UpdateUser(Guid id, [FromBody] UpdateUserRequestDto userUpdateDto)
        {
            var result = await _userService.UpdateUser(id, userUpdateDto);

            var response = new UserResponseDto
            {
                Id = result!.Id,
                Name = result.Name,
                Email = result.Email,
                Title = result.Title,
                AvatarInitials = result.AvatarInitials,
                AvatarUrl = result.AvatarUrl,
            };

            return ApiResponse<UserResponseDto>.Success(response, "User updated successfully.");
        }

        [HttpPut("{id:guid}/settings")]
        public async Task<ApiResponse<SettingsResponseDto>> UpdateUserSettings(Guid id, [FromBody] UpdateUserSettingsRequestDto userSettingsDto)
        {
            var result = await _userService.UpdateUserSettings(id, userSettingsDto);

            var response = new SettingsResponseDto
            {
                Id = result!.Id,
                UserId = result.UserId,
                DaysToArchieve = result.DaysToArchieve,
                CreatedAt = result.CreatedAt,
                UpdatedAt = result.UpdatedAt
            };

            return ApiResponse<SettingsResponseDto>.Success(response, "User settings updated successfully.");
        }

        [HttpGet("{id:guid}/settings")]
        public async Task<ApiResponse<SettingsResponseDto>> GetUserSettings(Guid id)
        {
            var result = await _userService.GetUserSettings(id);

            var response = new SettingsResponseDto
            {
                Id = result!.Id,
                UserId = result.UserId,
                DaysToArchieve = result.DaysToArchieve,
                CreatedAt = result.CreatedAt,
                UpdatedAt = result.UpdatedAt
            };

            return ApiResponse<SettingsResponseDto>.Success(response, "User settings fetched successfully.");
        }

        [HttpPut("{id:guid}/password")]
        public async Task<ApiResponse<UserResponseDto>> UpdateUserPassword(Guid id, [FromBody] UpdateUserPasswordRequestDto userPasswordDto)
        {
            var result = await _userService.UpdateUserPassword(id, userPasswordDto);

            var response = new UserResponseDto
            {
                Id = result!.Id,
                Name = result.Name,
                Email = result.Email,
                Title = result.Title,
                AvatarInitials = result.AvatarInitials,
                AvatarUrl = result.AvatarUrl,
            };

            return ApiResponse<UserResponseDto>.Success(response, "User password updated successfully.");
        }
    }
}