using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlowBackend.DTOs.Tasks.Archive;
using TaskFlowBackend.Helpers.API;
using TaskFlowBackend.Helpers.Pagination;
using TaskFlowBackend.Models;
using TaskFlowBackend.Services.Archive.Interfaces;
using TaskFlowBackend.Services.Interfaces;

namespace TaskFlowBackend.Controllers
{
    [ApiController]
    [Route("api/migrate")]
    public class MigrationController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly ITaskMigrationService _taskMigrationService;
        private readonly IConfiguration _configuration;

        public MigrationController(ITaskService taskService, ITaskMigrationService taskMigrationService, IConfiguration configuration)
        {
            _taskService = taskService;
            _taskMigrationService = taskMigrationService;
            _configuration = configuration;
        }

        [AllowAnonymous]
        [HttpPost("task/mark-and-copy")]
        public async Task<ApiResponse<object>> MarkAndCopy()
        {
            int batchSize = _configuration.GetValue<int>("TaskArchivalSettings:BatchSize", 500); // Default to 500 if not set
            int archivalThresholdDays = _configuration.GetValue<int>("TaskArchivalSettings:ArchivalThresholdDays", 30);
            var result = await _taskService.MarkAndCopyEligibleTasksAsync(batchSize, archivalThresholdDays, default);
            var response = new
            {
                marked = result.Count
            };
            return ApiResponse<object>.Success(response, "Eligible tasks marked and copied successfully.");
        }

        [AllowAnonymous]
        [HttpPost("task/cleanup-delete")]
        public async Task<ApiResponse<object>> CleanupDelete()
        {
            var deleted = await _taskService.DeleteConfirmedArchivedTasksAsync(default);
            var response = new
            {
                deletedTasks = deleted
            };
            return ApiResponse<object>.Success(response, "Confirmed archived tasks deleted successfully.");
        }

        [AllowAnonymous]
        [HttpGet("task/archived")]
        public async Task<ApiResponse<PagedResult<ArchivedTaskResponseDTO>>> GetArchivedTasks(
            [FromQuery] Guid teamId,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10,
            [FromQuery] Guid? statusId = null,
            [FromQuery] string? search = null)
        {
            var result = await _taskMigrationService.GetArchivedTasksAsync(teamId, page, limit, statusId, search);
            return ApiResponse<PagedResult<ArchivedTaskResponseDTO>>.Success(result, "Archived tasks retrieved successfully.");
        }

        [AllowAnonymous]
        [HttpGet("task/archived/{taskId}")]
        public async Task<ApiResponse<ArchivedTaskResponseDTO?>> GetArchivedTaskById([FromRoute] Guid taskId)
        {
            var archivedTask = await _taskMigrationService.GetArchivedTaskByIdAsync(taskId);
            if (archivedTask == null)
            {
                return ApiResponse<ArchivedTaskResponseDTO?>.Failure("Archived task not found.", 404);
            }
            var response = new ArchivedTaskResponseDTO
            {
                Id = archivedTask.Id,
                TaskNumber = archivedTask.TaskNumber,
                Title = archivedTask.Title,
                Description = archivedTask.Description,
                Label = archivedTask.Label,
                Priority = archivedTask.Priority,
                ExpectedCompletion = archivedTask.ExpectedCompletion,
                AssigneeDetails = archivedTask.AssigneeDetails,
                StatusId = archivedTask.StatusId,
                TeamId = archivedTask.TeamId,
                CreatedBy = archivedTask.CreatedBy,
                CreatedAt = archivedTask.CreatedAt,
                UpdatedAt = archivedTask.UpdatedAt
            };
            return ApiResponse<ArchivedTaskResponseDTO?>.Success(response, "Archived task retrieved successfully.");
        }
    }
}