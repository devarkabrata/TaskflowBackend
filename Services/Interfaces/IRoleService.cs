using TaskFlowBackend.DTOs.Roles;

namespace TaskFlowBackend.Services.Interfaces
{
    public interface IRoleService
    {
        Task<List<RoleResponseDto>> GetRolesAsync();
        Task EnsureRoleExistsAsync(Guid roleId);
    }
}
