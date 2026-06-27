using Microsoft.EntityFrameworkCore;
using TaskFlowBackend.Data;
using TaskFlowBackend.Enums;
using TaskFlowBackend.Helpers.Pagination;
using TaskFlowBackend.Models;
using TaskFlowBackend.Repository.Interfaces;

namespace TaskFlowBackend.Repository
{
    public class WorkspaceMemberRepository : IWorkspaceMemberRepository
    {
        private readonly AppDBContext _context;

        public WorkspaceMemberRepository(AppDBContext context)
        {
            _context = context;
        }

        public async Task<(List<WorkspaceMember> Items, int Total)> GetMembersAsync(Guid workspaceId, string? search, Guid? teamId, PaginationParams? pagination = null)
        {
            var query = _context.WorkspaceMembers
                .Where(m => m.WorkspaceId == workspaceId && m.Status == WorkspaceMemberStatus.Active)
                .Include(m => m.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(m => m.User.Name.Contains(search) || m.User.Email.Contains(search));

            if (teamId.HasValue)
                query = query.Where(m => _context.TeamMembers.Any(tm => tm.TeamId == teamId.Value && tm.UserId == m.UserId));

            var total = await query.CountAsync();

            if (pagination != null)
                query = query.Skip(pagination.Skip).Take(pagination.Limit);

            return (await query.ToListAsync(), total);
        }

        public async Task<WorkspaceMember?> GetByUserIdAsync(Guid workspaceId, Guid userId)
            => await _context.WorkspaceMembers
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.WorkspaceId == workspaceId && m.UserId == userId);

        public async Task<Dictionary<Guid, List<Guid>>> GetUserTeamIdsAsync(Guid workspaceId, List<Guid> userIds)
        {
            var workspaceTeamIds = await _context.Teams
                .Where(t => t.WorkspaceId == workspaceId)
                .Select(t => t.Id)
                .ToListAsync();

            var memberships = await _context.TeamMembers
                .Where(tm => workspaceTeamIds.Contains(tm.TeamId) && userIds.Contains(tm.UserId))
                .ToListAsync();

            return memberships
                .GroupBy(tm => tm.UserId)
                .ToDictionary(g => g.Key, g => g.Select(tm => tm.TeamId).ToList());
        }

        public async Task<(int total, int active, int pendingInvites, int totalTeams)> GetStatsAsync(Guid workspaceId)
        {
            var total = await _context.WorkspaceMembers.CountAsync(m => m.WorkspaceId == workspaceId);
            var active = await _context.WorkspaceMembers.CountAsync(m => m.WorkspaceId == workspaceId && m.Status == WorkspaceMemberStatus.Active);
            var pendingInvites = await _context.WorkspaceInvitations.CountAsync(i => i.WorkspaceId == workspaceId && i.Status == InvitationStatus.Pending);
            var totalTeams = await _context.Teams.CountAsync(t => t.WorkspaceId == workspaceId);
            return (total, active, pendingInvites, totalTeams);
        }

        public async Task<WorkspaceMember> AddAsync(WorkspaceMember member)
        {
            await _context.WorkspaceMembers.AddAsync(member);
            await _context.SaveChangesAsync();
            return member;
        }

        public async Task<List<WorkspaceMember>> BulkAddAsync(Guid workspaceId, List<Guid> userIds)
        {
            var existingUserIds = await _context.WorkspaceMembers
                .Where(m => m.WorkspaceId == workspaceId && userIds.Contains(m.UserId))
                .Select(m => m.UserId)
                .ToListAsync();

            var newMembers = userIds
                .Except(existingUserIds)
                .Select(uid => new WorkspaceMember
                {
                    Id = Guid.NewGuid(),
                    WorkspaceId = workspaceId,
                    UserId = uid,
                    Status = WorkspaceMemberStatus.Active,
                    JoinedAt = DateTime.UtcNow
                })
                .ToList();

            if (newMembers.Any())
            {
                await _context.WorkspaceMembers.AddRangeAsync(newMembers);
                await _context.SaveChangesAsync();
            }

            return newMembers;
        }

        public async Task<WorkspaceMember> UpdateAsync(WorkspaceMember member)
        {
            _context.WorkspaceMembers.Update(member);
            await _context.SaveChangesAsync();
            return member;
        }

        public async Task<bool> RemoveAsync(Guid workspaceId, Guid userId)
        {
            var member = await _context.WorkspaceMembers
                .FirstOrDefaultAsync(m => m.WorkspaceId == workspaceId && m.UserId == userId);

            if (member == null) return false;

            var workspaceTeamIds = await _context.Teams
                .Where(t => t.WorkspaceId == workspaceId)
                .Select(t => t.Id)
                .ToListAsync();

            var teamMemberships = await _context.TeamMembers
                .Where(tm => tm.UserId == userId && workspaceTeamIds.Contains(tm.TeamId))
                .ToListAsync();

            _context.TeamMembers.RemoveRange(teamMemberships);
            _context.WorkspaceMembers.Remove(member);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
