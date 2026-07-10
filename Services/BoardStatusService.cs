using TaskFlowBackend.DTOs.Board;
using TaskFlowBackend.Helpers.CustomException;
using TaskFlowBackend.Repository.Interfaces;
using TaskFlowBackend.Services.Interfaces;

namespace TaskFlowBackend.Services
{
    public class BoardStatusService : IBoardStatusService
    {
        private readonly IBoardStatusRepository _boardStatusRepo;
        private readonly ITeamRepository _teamRepo;

        public BoardStatusService(IBoardStatusRepository boardStatusRepo, ITeamRepository teamRepo)
        {
            _boardStatusRepo = boardStatusRepo;
            _teamRepo = teamRepo;
        }

        public async Task<List<BoardStatusCatalogDto>> GetCatalogAsync(Guid teamId, Guid userId)
        {
            var team = await _teamRepo.GetByIdAsync(teamId) ?? throw new NotFoundException("Team not found.");
            if (!team.Members.Any(m => m.UserId == userId))
                throw new ForbiddenException("You are not a member of this team.");

            var statuses = await _boardStatusRepo.GetByTeamIdAsync(teamId);
            return statuses.Select(s => new BoardStatusCatalogDto
            {
                StatusId = s.Id,
                StatusName = s.Name
            }).ToList();
        }
    }
}
