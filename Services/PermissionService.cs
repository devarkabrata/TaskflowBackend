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
