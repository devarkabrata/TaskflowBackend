using TaskFlowBackend.Helpers.Pagination;
using TaskFlowBackend.Models;

namespace TaskFlowBackend.Repository.Interfaces
{
    public interface ITaskRepository
    {
        Task<TaskItem?> GetByIdAsync(Guid taskId);
        Task<int> GetNextTaskNumberAsync(Guid teamId);
        Task<TaskItem> CreateAsync(TaskItem task);
        Task<TaskItem> UpdateAsync(TaskItem task);
        Task<(List<TaskItem> Items, int Total)> SearchAsync(Guid userId, Guid? teamId, string? search, Guid? assigneeId, PaginationParams? pagination = null);
        Task<List<TaskItem>> GetByTeamIdAsync(Guid teamId);
        Task<List<User>> GetUsersByIdsAsync(IEnumerable<Guid> userIds);
    }
}
