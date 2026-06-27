using TaskFlowBackend.Helpers.Pagination;
using TaskFlowBackend.Models;

namespace TaskFlowBackend.Repository.Interfaces
{
    public interface IWorkspaceMemberRepository
    {
        Task<(List<WorkspaceMember> Items, int Total)> GetMembersAsync(Guid workspaceId, string? search, Guid? teamId, PaginationParams? pagination = null);
        Task<WorkspaceMember?> GetByUserIdAsync(Guid workspaceId, Guid userId);
        Task<Dictionary<Guid, List<Guid>>> GetUserTeamIdsAsync(Guid workspaceId, List<Guid> userIds);
        Task<(int total, int active, int pendingInvites, int totalTeams)> GetStatsAsync(Guid workspaceId);
        Task<WorkspaceMember> AddAsync(WorkspaceMember member);
        Task<List<WorkspaceMember>> BulkAddAsync(Guid workspaceId, List<Guid> userIds);
        Task<WorkspaceMember> UpdateAsync(WorkspaceMember member);
        Task<bool> RemoveAsync(Guid workspaceId, Guid userId);
    }
}
