using TaskFlowBackend.DTOs.Roles;
using TaskFlowBackend.Enums;
using TaskFlowBackend.Helpers.CustomException;
using TaskFlowBackend.Repository.Interfaces;
using TaskFlowBackend.Services.Interfaces;

namespace TaskFlowBackend.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly ITeamMemberRepository _memberRepo;

        public PermissionService(ITeamMemberRepository memberRepo)
        {
            _memberRepo = memberRepo;
        }

        public async Task<RoleResponseDto> ListPermissions(Guid userId, Guid teamId)
        {
            var member = await _memberRepo.GetAsync(teamId, userId);
            if(member == null)
            {
                throw new NotFoundException("Member not found");
            }
            if(!member.Role.IsEnable)
            {
                throw new ForbiddenException($"The role of this member id not enabled.");
            }
            return new RoleResponseDto
            {
                Id = member.Role.Id,
                Name = member.Role.Name,
                Description = member.Role.Description,
                Permissions = member.Role.Permissions.Select(p => p.ToString()).ToList()
            };
        }

        public async Task<bool> HasPermissionAsync(Guid userId, Guid teamId, PermissionType permission)
        {
            var member = await _memberRepo.GetAsync(teamId, userId);
            return member != null && member.Role.IsEnable && member.Role.Permissions.Contains(permission);
        }

        public async Task EnsureHasPermissionAsync(Guid userId, Guid teamId, PermissionType permission)
        {
            if (!await HasPermissionAsync(userId, teamId, permission))
                throw new ForbiddenException($"You don't have '{permission}' permission for this team.");
        }
    }
}
