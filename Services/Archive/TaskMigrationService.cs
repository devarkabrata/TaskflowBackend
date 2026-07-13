using TaskFlowBackend.DTOs.Tasks.Archive;
using TaskFlowBackend.Helpers.Pagination;
using TaskFlowBackend.Models;
using TaskFlowBackend.Repository.Archive.Interfaces;
using TaskFlowBackend.Services.Archive.Interfaces;

namespace TaskFlowBackend.Services.Archive
{
    public class TaskMigrationService : ITaskMigrationService
    {
        private readonly IMigrateTasksRepository _migrateTasksRepository;

        public TaskMigrationService(IMigrateTasksRepository migrateTasksRepository)
        {
            _migrateTasksRepository = migrateTasksRepository;
        }

        public async Task<PagedResult<ArchivedTaskResponseDTO>> GetArchivedTasksAsync(Guid teamId, int page, int limit, Guid? statusId, string? search)
        {
            var paginationParams = new PaginationParams
            {
                Page = page,
                Limit = limit
            };

            var (archivedTasks, total) = await _migrateTasksRepository.GetArchivedTasksAsync(teamId, statusId, search, paginationParams);

            var data = archivedTasks.Select(task => new ArchivedTaskResponseDTO
            {
                Id = task.Id,
                TaskNumber = task.TaskNumber,
                Title = task.Title,
                AssigneeDetails = task.AssigneeDetails,
                StatusId = task.StatusId,
                TeamId = task.TeamId,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt
            }).ToList();

            return new PagedResult<ArchivedTaskResponseDTO> { Data = data, Total = total, Page = page, Limit = limit };
        }

        public async Task<ArchivedTaskItem?> GetArchivedTaskByIdAsync(Guid taskId)
        {
            var archivedTask = await _migrateTasksRepository.GetArchivedTaskByIdAsync(taskId);
            if (archivedTask == null)
            {
                return null;
            }
            return archivedTask;
        }
    }
}