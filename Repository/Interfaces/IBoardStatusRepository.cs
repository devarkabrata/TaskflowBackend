using TaskFlowBackend.Models;

namespace TaskFlowBackend.Repository.Interfaces
{
    public interface IBoardStatusRepository
    {
        Task<List<BoardStatus>> GetByTeamIdAsync(Guid teamId);
        Task<BoardStatus?> GetByIdAsync(Guid statusId);
        Task SeedDefaultsAsync(Guid teamId);
    }
}
