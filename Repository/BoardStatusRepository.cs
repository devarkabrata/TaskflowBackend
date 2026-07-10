using Microsoft.EntityFrameworkCore;
using TaskFlowBackend.Data;
using TaskFlowBackend.Models;
using TaskFlowBackend.Repository.Interfaces;

namespace TaskFlowBackend.Repository
{
    public class BoardStatusRepository : IBoardStatusRepository
    {
        private readonly AppDBContext _context;

        public BoardStatusRepository(AppDBContext context)
        {
            _context = context;
        }

        public async Task<List<BoardStatus>> GetByTeamIdAsync(Guid teamId)
            => await _context.BoardStatuses
                .Where(b => b.TeamId == teamId)
                .OrderBy(b => b.Position)
                .ToListAsync();

        public async Task<BoardStatus?> GetByIdAsync(Guid statusId)
            => await _context.BoardStatuses
                .FirstOrDefaultAsync(b => b.Id == statusId);

        public async Task SeedDefaultsAsync(Guid teamId)
        {
            var now = DateTime.UtcNow;
            var defaults = new List<BoardStatus>
            {
                new BoardStatus { Id = Guid.NewGuid(), TeamId = teamId, Name = "To Do", Position = 0, CreatedAt = now, UpdatedAt = now },
                new BoardStatus { Id = Guid.NewGuid(), TeamId = teamId, Name = "In Progress", Position = 1, CreatedAt = now, UpdatedAt = now },
                new BoardStatus { Id = Guid.NewGuid(), TeamId = teamId, Name = "In Review", Position = 2, CreatedAt = now, UpdatedAt = now },
                new BoardStatus { Id = Guid.NewGuid(), TeamId = teamId, Name = "Done", Position = 3, CreatedAt = now, UpdatedAt = now }
            };

            await _context.BoardStatuses.AddRangeAsync(defaults);
            await _context.SaveChangesAsync();
        }
    }
}
