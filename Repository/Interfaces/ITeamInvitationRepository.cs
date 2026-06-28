using TaskFlowBackend.Models;

namespace TaskFlowBackend.Repository.Interfaces
{
    public interface ITeamInvitationRepository
    {
        Task<List<TeamInvitation>> GetByTeamIdAsync(Guid teamId);
        Task<int> GetPendingCountAsync(Guid teamId);
        Task<TeamInvitation?> GetByEmailAndTeamAsync(Guid teamId, string email);
        Task<TeamInvitation> AddAsync(TeamInvitation invitation);
    }
}
