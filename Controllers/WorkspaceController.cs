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
    [Route("api/workspace")]
    public class WorkspaceController : ControllerBase
    {
        private readonly IWorkspaceService _workspaceService;

        public WorkspaceController(IWorkspaceService workspaceService)
        {
            _workspaceService = workspaceService;
        }

        [HttpGet("{workspaceId:guid}/info")]
        public async Task<ApiResponse<WorkspaceInfoRespDto>> GetWorkspaceInfo(Guid workspaceId)
        {
            var result = await _workspaceService.GetWorkspaceByIdAsync(workspaceId);

            WorkspaceInfoRespDto response = new WorkspaceInfoRespDto
            {
                Id=result!.Id,
                Name=result.Name,
                OwnerId=result.OwnerId,
                CreatedAt=result.CreatedAt,
                UpdatedAt=result.UpdatedAt,
                Owner=new UserInfo
                {
                    Id=result.Owner.Id,
                    Name=result.Owner.Name,
                    Email=result.Owner.Email,
                    AvatarUrl=result.Owner.AvatarUrl,
                    Title=result.Owner.Title
                },
                Teams=result.Teams.Select(t => new TeamsInfo
                {
                    Id=t.Id,
                    Name=t.Name,
                    Color=t.Color,
                    Description=t.Description
                }).ToList(),
                Members=result.Members.Select(m => new UserInfo
                {
                    Id=m.Id,
                    Name=m.User.Name,
                    Email=m.User.Email,
                    AvatarUrl=m.User.AvatarUrl,
                    Title=m.User.Title
                }).ToList()
            };

            return ApiResponse<WorkspaceInfoRespDto>.Success(response, "Workspace Information fetched successfully.");
        }
    }
}