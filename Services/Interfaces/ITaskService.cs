using TaskFlowBackend.DTOs.Board;
using TaskFlowBackend.DTOs.Tasks;
using TaskFlowBackend.Helpers.Pagination;
using TaskFlowBackend.Models;

namespace TaskFlowBackend.Services.Interfaces
{
    public interface ITaskService
    {
        Task<TaskResponseDto> CreateTaskAsync(CreateTaskRequestDto dto, Guid userId);
        Task<TaskResponseDto> GetTaskAsync(Guid taskId, Guid userId);
        Task<TaskResponseDto> UpdateTaskAsync(Guid taskId, UpdateTaskRequestDto dto, Guid userId);
        Task DeleteTaskAsync(Guid taskId, Guid userId);
        Task<PagedResult<TaskResponseDto>> ListTasksAsync(Guid userId, string? search, Guid? teamId, Guid? assigneeId, int page, int limit);
        Task<BoardResponseDto> GetBoardAsync(Guid teamId, Guid userId, Guid assigneeId = default);
        Task<TaskResponseDto> ChangeStatusAsync(Guid taskId, Guid statusId, Guid userId, int? progress = null);
        Task<List<TaskItem>> MarkAndCopyEligibleTasksAsync(int batchSize, int olderThanDays, CancellationToken ct = default);
        Task<int> DeleteConfirmedArchivedTasksAsync(CancellationToken ct);
        Task<TaskCountDTO> GetTaskCountByUser(Guid userId);
    }
}
