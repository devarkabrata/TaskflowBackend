using TaskFlowBackend.DTOs.Board;

namespace TaskFlowBackend.Services.Interfaces
{
    public interface IBoardStatusService
    {
        Task<List<BoardStatusCatalogDto>> GetCatalogAsync(Guid teamId, Guid userId);
    }
}
