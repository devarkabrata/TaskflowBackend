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

        public async Task<BoardStatus> AddAsync(BoardStatus status)
        {
            await _context.BoardStatuses.AddAsync(status);
            await _context.SaveChangesAsync();
            return status;
        }

        public async Task<bool> DeleteAsync(Guid statusId)
        {
            var status = await _context.BoardStatuses.FindAsync(statusId);
            if (status == null)
                return false;

            _context.BoardStatuses.Remove(status);
            await _context.SaveChangesAsync();
            return true;
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
                new BoardStatus { Id = Guid.NewGuid(), TeamId = teamId, Name = "To Do", Position = 0, Description = "Tasks that need to be done", IsDeletable=false, IsArchievable=false, CreatedAt = now, UpdatedAt = now },
                new BoardStatus { Id = Guid.NewGuid(), TeamId = teamId, Name = "In Progress", Position = 1, Description = "Tasks that are currently being worked on", IsDeletable=false, IsArchievable=false, CreatedAt = now, UpdatedAt = now },
                new BoardStatus { Id = Guid.NewGuid(), TeamId = teamId, Name = "Done", Position = 3, Description = "Tasks that have been completed", IsDeletable=false, IsArchievable=true, CreatedAt = now, UpdatedAt = now }
            };

            await _context.BoardStatuses.AddRangeAsync(defaults);
            await _context.SaveChangesAsync();
        }
    }
}
