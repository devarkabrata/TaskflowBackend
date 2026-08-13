using TaskFlowBackend.DTOs.Export;

namespace TaskFlowBackend.Services.Interfaces
{
    public interface ITaskExportService
    {
        Task<byte[]> ExportTeamTasksToCsvAsync(TaskCsvExportRequestDto request, Guid userId);
    }
}
