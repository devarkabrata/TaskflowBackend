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

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        public async Task<ApiResponse<List<RoleResponseDto>>> GetRoles()
        {
            var result = await _roleService.GetRolesAsync();
            return ApiResponse<List<RoleResponseDto>>.Success(result, "Roles fetched successfully.");
        }
    }
}
