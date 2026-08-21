using TaskFlowBackend.Models;

namespace TaskFlowBackend.Repository.Interfaces
{
    public interface IRoleRepository
    {
        Task<List<Roles>> GetAllEnabledAsync();
        Task<Roles?> GetByIdAsync(Guid id);
        Task<bool> ExistsAndEnabledAsync(Guid id);
    }
}
