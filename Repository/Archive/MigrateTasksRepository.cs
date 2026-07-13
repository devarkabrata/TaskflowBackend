using Microsoft.EntityFrameworkCore;
using TaskFlowBackend.Data;
using TaskFlowBackend.Helpers.Pagination;
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

        public async Task<ArchivedTaskItem?> GetArchivedTaskByIdAsync(Guid taskId, CancellationToken ct = default)
        {
            return await _archiveDbContext.ArchivedTaskItems
                .FirstOrDefaultAsync(t => t.Id == taskId, ct);
        }

        public async Task<(List<ArchivedTaskItem>, int)> GetArchivedTasksAsync(Guid teamId, Guid? statusId, string? search, PaginationParams? pagination = null)
        {
            var query = _archiveDbContext.ArchivedTaskItems.Where(t => t.TeamId == teamId);

            if (statusId.HasValue)
            {
                query = query.Where(t => t.StatusId == statusId.Value);
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t => t.Title.Contains(search));
            }

            var total = await query.CountAsync();

            if (pagination != null)
                query = query.Skip(pagination.Skip).Take(pagination.Limit);

            return (await query.ToListAsync(), total);
        }
    }
}