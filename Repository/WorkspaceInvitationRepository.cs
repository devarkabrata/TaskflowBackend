using Microsoft.EntityFrameworkCore;
using TaskFlowBackend.Data;
using TaskFlowBackend.Enums;
using TaskFlowBackend.Helpers.Pagination;
using TaskFlowBackend.Models;
using TaskFlowBackend.Repository.Interfaces;

namespace TaskFlowBackend.Repository
{
    public class WorkspaceInvitationRepository : IWorkspaceInvitationRepository
    {
        private readonly AppDBContext _context;

        public WorkspaceInvitationRepository(AppDBContext context)
        {
            _context = context;
        }

        public async Task<(List<WorkspaceInvitation> Items, int Total)> GetAllPendingAsync(Guid workspaceId, string? search, PaginationParams? pagination = null)
        {
            var query = _context.WorkspaceInvitations
                .Where(i => i.WorkspaceId == workspaceId && i.Status == InvitationStatus.Pending)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(i => i.Email.Contains(search));

            var total = await query.CountAsync();

            if (pagination != null)
                query = query.Skip(pagination.Skip).Take(pagination.Limit);

            return (await query.ToListAsync(), total);
        }

        public async Task<WorkspaceInvitation?> GetPendingByEmailAsync(Guid workspaceId, string email)
            => await _context.WorkspaceInvitations.FirstOrDefaultAsync(i =>
                i.WorkspaceId == workspaceId &&
                i.Email == email &&
                i.Status == InvitationStatus.Pending);

        public async Task<WorkspaceInvitation> CreateAsync(WorkspaceInvitation invitation)
        {
            await _context.WorkspaceInvitations.AddAsync(invitation);
            await _context.SaveChangesAsync();
            return invitation;
        }

        public async Task<WorkspaceInvitation> UpdateAsync(WorkspaceInvitation invitation)
        {
            _context.WorkspaceInvitations.Update(invitation);
            await _context.SaveChangesAsync();
            return invitation;
        }
    }
}
