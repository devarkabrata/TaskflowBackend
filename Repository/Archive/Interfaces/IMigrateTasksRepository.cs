using TaskFlowBackend.Models;

namespace TaskFlowBackend.Repository.Archive.Interfaces
{
    public interface IMigrateTasksRepository
    {
        Task<Task> MigrateTasksToArchiveAsync(List<ArchivedTaskItem> taskItems, CancellationToken cancellationToken = default);
        Task<List<Guid>> GetConfirmedTaskIds(List<Guid> tasks, CancellationToken ct = default);
    }
}