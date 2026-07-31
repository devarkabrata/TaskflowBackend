using TaskFlowBackend.Models;

namespace TaskFlowBackend.Repository.Interfaces
{
    public interface IWorkspaceRepository
    {
        Task<Workspace?> GetByIdAsync(Guid id);
        Task<Workspace?> GetByOwnerIdAsync(Guid ownerId);
        Task<Workspace> CreateAsync(Workspace workspace);
        Task<int> GetCountByOwnerIdAsync(Guid ownerId);
    }
}
