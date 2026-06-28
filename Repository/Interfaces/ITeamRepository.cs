using TaskFlowBackend.Models;

namespace TaskFlowBackend.Repository.Interfaces
{
    public interface ITeamRepository
    {
        Task<Team?> GetByIdAsync(Guid teamId);
        Task<List<Team>> GetByWorkspaceIdForUserAsync(Guid workspaceId, Guid userId);
        Task<Team> CreateAsync(Team team);
        Task<Team> UpdateAsync(Team team);
        Task DeleteAsync(Team team);
    }
}
