using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TaskFlowBackend.DTOs.Workspaces;
using TaskFlowBackend.Helpers.API;
using TaskFlowBackend.Helpers.Pagination;
using TaskFlowBackend.Services.Interfaces;

namespace TaskFlowBackend.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/people")]
    public class PeopleController : ControllerBase
    {
        private readonly IWorkspaceService _workspaceService;

        public PeopleController(IWorkspaceService workspaceService)
        {
            _workspaceService = workspaceService;
        }

        [HttpGet]
        public async Task<ApiResponse<PagedResult<PeopleListItemDto>>> GetPeople(
            [FromQuery] string? search,
            [FromQuery] string? status,
            [FromQuery] Guid? teamId,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 20)
        {
            Guid userId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var result = await _workspaceService.GetPeopleAsync(userId, search, status, teamId, page, limit);
            return ApiResponse<PagedResult<PeopleListItemDto>>.Success(result, "People fetched successfully.");
        }

        [HttpGet("stats")]
        public async Task<ApiResponse<PeopleStatsDto>> GetStats()
        {
            Guid userId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var result = await _workspaceService.GetStatsAsync(userId);
            return ApiResponse<PeopleStatsDto>.Success(result, "Stats fetched successfully.");
        }

        [HttpPost("invite")]
        public async Task<ApiResponse<WorkspaceInvitationResponseDto>> Invite([FromBody] WorkspaceInviteRequestDto dto)
        {
            Guid userId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var (invitation, isNew) = await _workspaceService.InviteAsync(userId, dto.Email);
            int code = isNew ? 201 : 200;
            string message = isNew ? "Invitation sent successfully." : "Invitation resent successfully.";
            return ApiResponse<WorkspaceInvitationResponseDto>.Success(invitation, message, code);
        }

        [HttpPatch("{targetUserId:guid}")]
        public async Task<ApiResponse<PeopleListItemDto>> UpdateMember(Guid targetUserId, [FromBody] UpdateMemberRequestDto dto)
        {
            Guid userId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var result = await _workspaceService.UpdateMemberAsync(userId, targetUserId, dto);
            return ApiResponse<PeopleListItemDto>.Success(result, "Member updated successfully.");
        }

        [HttpDelete("{targetUserId:guid}")]
        public async Task<ApiResponse<object>> RemoveMember(Guid targetUserId)
        {
            Guid userId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            await _workspaceService.RemoveMemberAsync(userId, targetUserId);
            return ApiResponse<object>.Success(null!, "Member removed successfully.");
        }

        [HttpPost("enlist")]
        public async Task<ApiResponse<List<Guid>>> EnlistMembers([FromBody] BulkPeopleEnlistToWorkspaceDTO userIds)
        {
            Guid userId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var workspace = await _workspaceService.GetWorkspaceOrThrowAsync(userId);
            var result = await _workspaceService.AddMembersToWorkspaceAsync(workspace.Id, userIds.UserIds);
            return ApiResponse<List<Guid>>.Success(result, "Members enlisted successfully.");
        }
    }
}
