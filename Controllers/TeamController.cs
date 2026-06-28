using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TaskFlowBackend.DTOs.Teams;
using TaskFlowBackend.Helpers.API;
using TaskFlowBackend.Services.Interfaces;

namespace TaskFlowBackend.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/teams")]
    public class TeamController : ControllerBase
    {
        private readonly ITeamService _teamService;

        public TeamController(ITeamService teamService)
        {
            _teamService = teamService;
        }

        [HttpGet]
        public async Task<ApiResponse<List<TeamResponseDto>>> GetMyTeams()
        {
            var userId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var result = await _teamService.GetMyTeamsAsync(userId);
            return ApiResponse<List<TeamResponseDto>>.Success(result, "Teams fetched successfully.");
        }

        [HttpPost]
        public async Task<ApiResponse<TeamResponseDto>> CreateTeam([FromBody] CreateTeamRequestDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var result = await _teamService.CreateTeamAsync(dto, userId);
            return ApiResponse<TeamResponseDto>.Success(result, "Team created successfully.", 201);
        }

        // Must be declared before {id} to avoid route conflict
        [HttpGet("stats")]
        public async Task<ApiResponse<TeamStatsDto>> GetStats()
        {
            var userId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var result = await _teamService.GetStatsAsync(userId);
            return ApiResponse<TeamStatsDto>.Success(result, "Stats fetched successfully.");
        }

        [HttpGet("{id:guid}")]
        public async Task<ApiResponse<TeamResponseDto>> GetTeam(Guid id)
        {
            var userId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var result = await _teamService.GetTeamAsync(id, userId);
            return ApiResponse<TeamResponseDto>.Success(result, "Team fetched successfully.");
        }

        [HttpPut("{id:guid}")]
        public async Task<ApiResponse<TeamResponseDto>> UpdateTeam(Guid id, [FromBody] UpdateTeamRequestDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            TeamResponseDto? result = null;

            bool hasDetails = dto.Name != null || dto.Description != null || dto.Color != null;
            if (hasDetails)
                result = await _teamService.UpdateDetailsAsync(id, dto, userId);

            if (dto.Members != null)
                result = await _teamService.SyncMembersAsync(id, dto.Members, userId);

            result ??= await _teamService.GetTeamAsync(id, userId);

            return ApiResponse<TeamResponseDto>.Success(result, "Team updated successfully.");
        }

        [HttpDelete("{id:guid}")]
        public async Task<ApiResponse<object>> DeleteTeam(Guid id)
        {
            var userId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            await _teamService.DeleteTeamAsync(id, userId);
            return ApiResponse<object>.Success(null!, "Team deleted successfully.", 204);
        }

        [HttpPost("{id:guid}/invite")]
        public async Task<ApiResponse<TeamInvitationResponseDto>> InviteToTeam(Guid id, [FromBody] TeamInviteRequestDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var result = await _teamService.InviteToTeamAsync(id, dto, userId);
            return ApiResponse<TeamInvitationResponseDto>.Success(result, "Invitation sent successfully.", 201);
        }

        [HttpDelete("{id:guid}/members/{targetUserId:guid}")]
        public async Task<ApiResponse<object>> RemoveMember(Guid id, Guid targetUserId)
        {
            var userId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            await _teamService.RemoveMemberAsync(id, targetUserId, userId);
            return ApiResponse<object>.Success(null!, "Member removed successfully.", 204);
        }
    }
}
