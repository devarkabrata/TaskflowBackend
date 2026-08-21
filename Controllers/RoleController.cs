using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlowBackend.DTOs.Roles;
using TaskFlowBackend.Helpers.API;
using TaskFlowBackend.Services.Interfaces;

namespace TaskFlowBackend.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/roles")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private readonly IPermissionService _permissionService;

        public RoleController(IRoleService roleService, IPermissionService permissionService)
        {
            _roleService = roleService;
            _permissionService = permissionService;
        }

        [HttpGet]
        public async Task<ApiResponse<List<RoleResponseDto>>> GetRoles()
        {
            var result = await _roleService.GetRolesAsync();
            return ApiResponse<List<RoleResponseDto>>.Success(result, "Roles fetched successfully.");
        }

        [HttpGet("/permissions")]
        public async Task<ApiResponse<RoleResponseDto>> GetPermissions([FromQuery] Guid teamId, [FromQuery] Guid userId)
        {
            var result = await _permissionService.ListPermissions(userId, teamId);
            return ApiResponse<RoleResponseDto>.Success(result, "Role fetched successfully.");
        }
    }
}
