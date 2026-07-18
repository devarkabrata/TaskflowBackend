using TaskFlowBackend.Models;

namespace TaskFlowBackend.Repository.Interfaces
{
    public interface ITeamRepository
    {
        Task<Team?> GetByIdAsync(Guid teamId);
        Task<List<Team>> GetByWorkspaceIdForUserAsync(Guid workspaceId, Guid userId);
        Task<List<Team>> GetByUserMembershipAsync(Guid userId);
        Task<List<Team>> GetByWorkspaceIdForAdminAsync(Guid workspaceId, Guid adminUserId);
        Task<Team> CreateAsync(Team team);
        Task<Team> UpdateAsync(Team team);
        Task DeleteAsync(Team team);
    }
}
