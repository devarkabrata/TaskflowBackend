using TaskFlowBackend.DTOs.Roles;
using TaskFlowBackend.Helpers.API;
using TaskFlowBackend.Helpers.CustomException;
using TaskFlowBackend.Repository.Interfaces;
using TaskFlowBackend.Services.Interfaces;

namespace TaskFlowBackend.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepo;

        public RoleService(IRoleRepository roleRepo)
        {
            _roleRepo = roleRepo;
        }

        public async Task<List<RoleResponseDto>> GetRolesAsync()
        {
            var roles = await _roleRepo.GetAllEnabledAsync();
            return roles.Select(r => new RoleResponseDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                Permissions = r.Permissions.Select(p => p.ToString()).ToList()
            }).ToList();
        }

        public async Task EnsureRoleExistsAsync(Guid roleId)
        {
            if (!await _roleRepo.ExistsAndEnabledAsync(roleId))
                throw new ValidationException("Validation failed.", new List<ApiError>
                {
                    new ApiError { Field = "roleId", Code = "ROLE_NOT_FOUND", Message = $"Role {roleId} does not exist or is disabled." }
                });
        }
    }
}
