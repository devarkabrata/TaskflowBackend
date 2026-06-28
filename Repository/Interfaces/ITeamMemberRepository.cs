using TaskFlowBackend.Models;

namespace TaskFlowBackend.Repository.Interfaces
{
    public interface ITeamMemberRepository
    {
        Task<TeamMember?> GetAsync(Guid teamId, Guid userId);
        Task<List<TeamMember>> GetByTeamIdAsync(Guid teamId);
        Task AddAsync(TeamMember member);
        Task AddRangeAsync(List<TeamMember> members);
        Task RemoveAsync(TeamMember member);
        Task<bool> IsMemberAsync(Guid teamId, Guid userId);
        Task SyncAsync(List<TeamMember> toAdd, List<TeamMember> toRemove, List<TeamMember> toUpdate);
    }
}
