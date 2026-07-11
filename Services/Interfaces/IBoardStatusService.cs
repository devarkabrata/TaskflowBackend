using TaskFlowBackend.DTOs;
using TaskFlowBackend.DTOs.Board;

namespace TaskFlowBackend.Services.Interfaces
{
    public interface IBoardStatusService
    {
        Task<BoardStatusResponseDTO> CreateStatusAsync(BoardStatusRequestDTO request, Guid userId);
        Task<List<BoardStatusCatalogDto>> GetCatalogAsync(Guid teamId, Guid userId);
        Task DeleteStatusAsync(Guid statusId, Guid userId);
    }
}
