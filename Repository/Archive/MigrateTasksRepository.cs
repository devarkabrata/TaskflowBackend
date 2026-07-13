using Microsoft.EntityFrameworkCore;
using TaskFlowBackend.Data;
using TaskFlowBackend.Models;
using TaskFlowBackend.Repository.Archive.Interfaces;

namespace TaskFlowBackend.Repository.Archive
{
    public class MigrateTasksRepository : IMigrateTasksRepository
    {
        private readonly ArchiveDBContext _archiveDbContext;

        public MigrateTasksRepository(ArchiveDBContext archiveDbContext)
        {
            _archiveDbContext = archiveDbContext;
        }

        public async Task<Task> MigrateTasksToArchiveAsync(List<ArchivedTaskItem> taskItems, CancellationToken cancellationToken = default)
        {
            if (taskItems == null || !taskItems.Any())
            {
                throw new ArgumentException("Task items list cannot be null or empty.", nameof(taskItems));
            }

            await _archiveDbContext.ArchivedTaskItems.AddRangeAsync(taskItems, cancellationToken);
            await _archiveDbContext.SaveChangesAsync(cancellationToken);
            return Task.CompletedTask;
        }

        public async Task<List<Guid>> GetConfirmedTaskIds(List<Guid> tasks, CancellationToken ct = default)
        {
            return await _archiveDbContext.ArchivedTaskItems
                .Where(t => tasks.Contains(t.Id))
                .Select(t => t.Id)
                .ToListAsync(ct);
        }
    }
}