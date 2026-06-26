using TaskFlowBackend.Helpers.Pagination;
using TaskFlowBackend.Models;

namespace TaskFlowBackend.Repository.Interfaces
{
    public interface IWorkspaceInvitationRepository
    {
        Task<(List<WorkspaceInvitation> Items, int Total)> GetAllPendingAsync(Guid workspaceId, string? search, PaginationParams? pagination = null);
        Task<WorkspaceInvitation?> GetPendingByEmailAsync(Guid workspaceId, string email);
        Task<WorkspaceInvitation> CreateAsync(WorkspaceInvitation invitation);
        Task<WorkspaceInvitation> UpdateAsync(WorkspaceInvitation invitation);
    }
}
