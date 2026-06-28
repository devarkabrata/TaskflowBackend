using TaskFlowBackend.DTOs.Teams;

namespace TaskFlowBackend.Services.Interfaces
{
    public interface ITeamService
    {
        Task<List<TeamResponseDto>> GetMyTeamsAsync(Guid userId);
        Task<TeamResponseDto> CreateTeamAsync(CreateTeamRequestDto dto, Guid userId);
        Task<TeamResponseDto> GetTeamAsync(Guid teamId, Guid userId);
        Task<TeamStatsDto> GetStatsAsync(Guid userId);
        Task<TeamResponseDto> UpdateDetailsAsync(Guid teamId, UpdateTeamRequestDto dto, Guid userId);
        Task<TeamResponseDto> SyncMembersAsync(Guid teamId, List<TeamMemberUpdateDto> members, Guid userId);
        Task DeleteTeamAsync(Guid teamId, Guid userId);
        Task<TeamInvitationResponseDto> InviteToTeamAsync(Guid teamId, TeamInviteRequestDto dto, Guid userId);
        Task RemoveMemberAsync(Guid teamId, Guid targetUserId, Guid requesterId);
    }
}
