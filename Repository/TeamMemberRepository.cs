using Microsoft.EntityFrameworkCore;
using TaskFlowBackend.Data;
using TaskFlowBackend.Models;
using TaskFlowBackend.Repository.Interfaces;

namespace TaskFlowBackend.Repository
{
    public class TeamMemberRepository : ITeamMemberRepository
    {
        private readonly AppDBContext _context;

        public TeamMemberRepository(AppDBContext context)
        {
            _context = context;
        }

        public async Task<TeamMember?> GetAsync(Guid teamId, Guid userId)
            => await _context.TeamMembers
                .Include(m => m.User)
                .Include(m => m.Role)
                .FirstOrDefaultAsync(m => m.TeamId == teamId && m.UserId == userId);

        public async Task<List<TeamMember>> GetByTeamIdAsync(Guid teamId)
            => await _context.TeamMembers
                .Where(m => m.TeamId == teamId)
                .Include(m => m.User)
                .Include(m => m.Role)
                .ToListAsync();

        public async Task AddAsync(TeamMember member)
        {
            await _context.TeamMembers.AddAsync(member);
            await _context.SaveChangesAsync();
        }

        public async Task AddRangeAsync(List<TeamMember> members)
        {
            if (!members.Any()) return;
            await _context.TeamMembers.AddRangeAsync(members);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveAsync(TeamMember member)
        {
            _context.TeamMembers.Remove(member);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsMemberAsync(Guid teamId, Guid userId)
            => await _context.TeamMembers.AnyAsync(m => m.TeamId == teamId && m.UserId == userId);

        public async Task SyncAsync(List<TeamMember> toAdd, List<TeamMember> toRemove, List<TeamMember> toUpdate)
        {
            if (toRemove.Any())
                _context.TeamMembers.RemoveRange(toRemove);

            if (toAdd.Any())
                await _context.TeamMembers.AddRangeAsync(toAdd);

            // toUpdate entities are already change-tracked with updated Role values
            await _context.SaveChangesAsync();
        }
    }
}
