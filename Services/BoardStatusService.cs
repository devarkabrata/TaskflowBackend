using TaskFlowBackend.DTOs;
using TaskFlowBackend.DTOs.Board;
using TaskFlowBackend.Helpers.API;
using TaskFlowBackend.Helpers.CustomException;
using TaskFlowBackend.Models;
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

        public async Task<BoardStatusResponseDTO> CreateStatusAsync(BoardStatusRequestDTO request, Guid userId)
        {
            var team = await _teamRepo.GetByIdAsync(request.TeamId);

            if(team == null)
                throw new NotFoundException("Team not found.");

            if (!team.Members.Any(m => m.UserId == userId))
                throw new ForbiddenException("You are not a member of this team.");

            var positions = (await _boardStatusRepo.GetByTeamIdAsync(request.TeamId)).Select(s => s.Position).ToList();

            if(positions.Contains(request.Position))
                throw new ForbiddenException("Position already exists for this team.");

            if(request.Position < 0 || request.Position > positions.Count)
                throw new ForbiddenException("Position is out of range for this team.");

            var newStatus = new BoardStatus
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Position = request.Position,
                TeamId = request.TeamId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdStatus = await _boardStatusRepo.AddAsync(newStatus);

            return new BoardStatusResponseDTO
            {
                Name = createdStatus.Name,
                Description = createdStatus.Description,
                Position = createdStatus.Position,
                TeamId = createdStatus.TeamId
            };
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

        public async Task DeleteStatusAsync(Guid statusId, Guid userId)
        {
            var status = await _boardStatusRepo.GetByIdAsync(statusId) ?? throw new NotFoundException("Status not found.");
            var team = await _teamRepo.GetByIdAsync(status.TeamId) ?? throw new NotFoundException("Team not found.");

            if (!team.Members.Any(m => m.UserId == userId))
                throw new ForbiddenException("You are not a member of this team.");

            if (!status.IsDeletable)
                throw new ValidationException("Validation failed.", new List<ApiError>
                {
                    new ApiError { Field = "statusId", Code = "INVALID_STATUS_NOT_DELETABLE", Message = "This status is a default column and cannot be deleted." }
                });

            await _boardStatusRepo.DeleteAsync(statusId);
        }
    }
}
