using Microsoft.EntityFrameworkCore;
using TaskFlowBackend.Data;
using TaskFlowBackend.Enums;
using TaskFlowBackend.Models;
using TaskFlowBackend.Repository.Interfaces;

namespace TaskFlowBackend.Repository
{
    public class TeamInvitationRepository : ITeamInvitationRepository
    {
        private readonly AppDBContext _context;

        public TeamInvitationRepository(AppDBContext context)
        {
            _context = context;
        }

        public async Task<List<TeamInvitation>> GetByTeamIdAsync(Guid teamId)
            => await _context.TeamInvitations
                .Where(i => i.TeamId == teamId)
                .ToListAsync();

        public async Task<int> GetPendingCountAsync(Guid teamId)
            => await _context.TeamInvitations
                .CountAsync(i => i.TeamId == teamId && i.Status == InvitationStatus.Pending);

        public async Task<TeamInvitation?> GetByEmailAndTeamAsync(Guid teamId, string email)
            => await _context.TeamInvitations
                .FirstOrDefaultAsync(i => i.TeamId == teamId
                    && i.Email == email
                    && i.Status == InvitationStatus.Pending);

        public async Task<TeamInvitation> AddAsync(TeamInvitation invitation)
        {
            await _context.TeamInvitations.AddAsync(invitation);
            await _context.SaveChangesAsync();
            return invitation;
        }
    }
}
