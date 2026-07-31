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
        Task DeleteAsync(TaskItem task);
        Task<int> DeleteRangeAsync(IEnumerable<TaskItem> tasks);
        Task<(List<TaskItem> Items, int Total)> SearchAsync(Guid userId, Guid? teamId, string? search, Guid? assigneeId, PaginationParams? pagination = null);
        Task<List<TaskItem>> GetByTeamIdAsync(Guid teamId);
        Task<List<User>> GetUsersByIdsAsync(IEnumerable<Guid> userIds);
        Task<List<TaskItem>> GetUnarchivedTasksOlderthanThresold(Guid statusId, DateTime cutoff, int batchSize, CancellationToken ct = default);
        Task UpdateTasksAsArchivedAsync(List<TaskItem> tasks, CancellationToken ct = default);
        Task<List<Guid>> GetArchievedTasks();
        Task<List<TaskItem>> GetTasksByIdsAsync(IEnumerable<Guid> taskIds, CancellationToken ct = default);
        Task<(int tasks, int archieve_tasks)> GetTaskCountByUserAsync(Guid userId);
    }
}
