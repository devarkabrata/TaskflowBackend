using Microsoft.EntityFrameworkCore;
using TaskFlowBackend.Data;
using TaskFlowBackend.Models;
using TaskFlowBackend.Repository.Interfaces;

namespace TaskFlowBackend.Repository
{
    public class TeamRepository : ITeamRepository
    {
        private readonly AppDBContext _context;

        public TeamRepository(AppDBContext context)
        {
            _context = context;
        }

        public async Task<Team?> GetByIdAsync(Guid teamId)
            => await _context.Teams
                .Include(t => t.Members)
                    .ThenInclude(m => m.User)
                .Include(t => t.Invitations)
                .FirstOrDefaultAsync(t => t.Id == teamId);

        public async Task<List<Team>> GetByWorkspaceIdForUserAsync(Guid workspaceId, Guid userId)
            => await _context.Teams
                .Where(t => t.WorkspaceId == workspaceId && t.Members.Any(m => m.UserId == userId))
                .Include(t => t.Members)
                    .ThenInclude(m => m.User)
                .Include(t => t.Invitations)
                .Include(t => t.Tasks)
                    .ThenInclude(task => task.Status)
                .ToListAsync();

        public async Task<List<Team>> GetByWorkspaceIdForAdminAsync(Guid workspaceId, Guid adminUserId)
            => await _context.Teams
                .Where(t => t.WorkspaceId == workspaceId && t.AdminId == adminUserId)
                .Include(t => t.Tasks)
                    .ThenInclude(task => task.Status)
                .ToListAsync();

        public async Task<Team> CreateAsync(Team team)
        {
            await _context.Teams.AddAsync(team);
            await _context.SaveChangesAsync();
            return team;
        }

        public async Task<Team> UpdateAsync(Team team)
        {
            _context.Teams.Update(team);
            await _context.SaveChangesAsync();
            return team;
        }

        public async Task DeleteAsync(Team team)
        {
            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();
        }
    }
}
