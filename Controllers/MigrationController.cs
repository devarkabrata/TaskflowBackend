using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlowBackend.Helpers.API;
using TaskFlowBackend.Services.Interfaces;

namespace TaskFlowBackend.Controllers
{
    [ApiController]
    [Route("api/migrate")]
    public class MigrationController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly IConfiguration _configuration;

        public MigrationController(ITaskService taskService, IConfiguration configuration)
        {
            _taskService = taskService;
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
    }
}