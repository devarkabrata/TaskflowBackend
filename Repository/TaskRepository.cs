using Microsoft.EntityFrameworkCore;
using TaskFlowBackend.Data;
using TaskFlowBackend.Helpers.Pagination;
using TaskFlowBackend.Models;
using TaskFlowBackend.Repository.Interfaces;

namespace TaskFlowBackend.Repository
{
    public class TaskRepository : ITaskRepository
    {
        private readonly AppDBContext _context;

        public TaskRepository(AppDBContext context)
        {
            _context = context;
        }

        public async Task<TaskItem?> GetByIdAsync(Guid taskId)
            => await _context.TaskItems
                .Include(t => t.Status)
                .FirstOrDefaultAsync(t => t.Id == taskId && t.DeletedAt == null);

        public async Task<int> GetNextTaskNumberAsync(Guid teamId)
        {
            var max = await _context.TaskItems
                .Where(t => t.TeamId == teamId)
                .Select(t => (int?)t.TaskNumber)
                .MaxAsync();

            return (max ?? 0) + 1;
        }

        public async Task<TaskItem> CreateAsync(TaskItem task)
        {
            await _context.TaskItems.AddAsync(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<TaskItem> UpdateAsync(TaskItem task)
        {
            _context.TaskItems.Update(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<(List<TaskItem> Items, int Total)> SearchAsync(Guid userId, Guid? teamId, string? search, Guid? assigneeId, PaginationParams? pagination = null)
        {
            var query = _context.TaskItems
                .Where(t => t.DeletedAt == null)
                .Where(t => _context.TeamMembers.Any(tm => tm.TeamId == t.TeamId && tm.UserId == userId))
                .Include(t => t.Status)
                .AsQueryable();

            if (teamId.HasValue)
                query = query.Where(t => t.TeamId == teamId.Value);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(t => t.Title.ToLower().Contains(search.ToLower()));

            if (assigneeId.HasValue)
                query = query.Where(t => t.AssigneeIds.Contains(assigneeId.Value));

            var total = await query.CountAsync();

            if (pagination != null)
                query = query.Skip(pagination.Skip).Take(pagination.Limit);

            return (await query.ToListAsync(), total);
        }

        public async Task<List<TaskItem>> GetByTeamIdAsync(Guid teamId)
            => await _context.TaskItems
                .Where(t => t.TeamId == teamId && t.DeletedAt == null)
                .Include(t => t.Status)
                .ToListAsync();

        public async Task<List<User>> GetUsersByIdsAsync(IEnumerable<Guid> userIds)
            => await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();
    }
}
