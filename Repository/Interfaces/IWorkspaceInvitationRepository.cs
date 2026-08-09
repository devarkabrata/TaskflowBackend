using TaskFlowBackend.Helpers.Pagination;
using TaskFlowBackend.Models;

namespace TaskFlowBackend.Repository.Interfaces
{
    public interface IWorkspaceInvitationRepository
    {
        Task<(List<WorkspaceInvitation> Items, int Total)> GetAllPendingAsync(Guid workspaceId, string? search, PaginationParams? pagination = null);
        Task<WorkspaceInvitation?> GetPendingByEmailAsync(Guid workspaceId, string email);
        Task<List<WorkspaceInvitation>> GetPendingByEmailAcrossWorkspacesAsync(string email);
        Task<WorkspaceInvitation?> GetByIdAsync(Guid workspaceId, Guid id);
        Task<WorkspaceInvitation> CreateAsync(WorkspaceInvitation invitation);
        Task<WorkspaceInvitation> UpdateAsync(WorkspaceInvitation invitation);
        Task<bool> DeleteAsync(Guid workspaceId, Guid id);
    }
}
