using TaskFlowBackend.DTOs.Tasks.Archive;
using TaskFlowBackend.Helpers.Pagination;
using TaskFlowBackend.Models;

namespace TaskFlowBackend.Services.Archive.Interfaces
{
    public interface ITaskMigrationService
    {
       Task<PagedResult<ArchivedTaskResponseDTO>> GetArchivedTasksAsync(Guid teamId, int page, int limit, Guid? statusId, string? search);
       Task<ArchivedTaskItem?> GetArchivedTaskByIdAsync(Guid taskId);
    }
}