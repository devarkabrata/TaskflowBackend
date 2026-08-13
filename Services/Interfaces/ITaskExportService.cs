using TaskFlowBackend.DTOs.Export;

namespace TaskFlowBackend.Services.Interfaces
{
    public interface ITaskExportService
    {
        Task<byte[]> ExportTeamTasksAsync(TaskExportRequestDto request, Guid userId);
    }
}
