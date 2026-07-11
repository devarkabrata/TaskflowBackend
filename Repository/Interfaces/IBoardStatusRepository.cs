using TaskFlowBackend.Models;

namespace TaskFlowBackend.Repository.Interfaces
{
    public interface IBoardStatusRepository
    {
        Task<BoardStatus> AddAsync(BoardStatus status);
        Task<List<BoardStatus>> GetByTeamIdAsync(Guid teamId);
        Task<BoardStatus?> GetByIdAsync(Guid statusId);
        Task<bool> DeleteAsync(Guid statusId);
        Task SeedDefaultsAsync(Guid teamId);
    }
}
