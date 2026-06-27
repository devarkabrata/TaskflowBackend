using TaskFlowBackend.DTOs.Workspaces;
using TaskFlowBackend.Helpers.Pagination;
using TaskFlowBackend.Models;

namespace TaskFlowBackend.Services.Interfaces
{
    public interface IWorkspaceService
    {
        Task<Workspace> CreateDefaultWorkspaceAsync(Guid userId, string userName);
        Task<List<Guid>> AddMembersToWorkspaceAsync(Guid workspaceId, List<Guid> userIds);
        Task<PagedResult<PeopleListItemDto>> GetPeopleAsync(Guid requestingUserId, string? search, string? status, Guid? teamId, int page, int limit);
        Task<PeopleStatsDto> GetStatsAsync(Guid requestingUserId);
        Task<(WorkspaceInvitationResponseDto dto, bool isNew)> InviteAsync(Guid requestingUserId, string email);
        Task<PeopleListItemDto> UpdateMemberAsync(Guid requestingUserId, Guid targetUserId, UpdateMemberRequestDto dto);
        Task RemoveMemberAsync(Guid requestingUserId, Guid targetUserId);
    }
}
