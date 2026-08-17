using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TaskFlowBackend.DTOs.Board;
using TaskFlowBackend.DTOs.Tasks;
using TaskFlowBackend.Helpers.API;
using TaskFlowBackend.Helpers.Pagination;
using TaskFlowBackend.Services.Interfaces;

namespace TaskFlowBackend.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/tasks")]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TaskController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpGet]
        public async Task<ApiResponse<PagedResult<TaskResponseDto>>> GetTasks(
            [FromQuery] string? search,
            [FromQuery] Guid? teamId,
            [FromQuery] Guid? statusId,
            [FromQuery] Guid? assigneeId,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 20)
        {
            var userId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var result = await _taskService.ListTasksAsync(userId, search, teamId, statusId, assigneeId, page, limit);
            return ApiResponse<PagedResult<TaskResponseDto>>.Success(result, "Tasks fetched successfully.");
        }

        // Must be declared before {id:guid} to avoid route conflict
        [HttpGet("my")]
        public async Task<ApiResponse<PagedResult<TaskResponseDto>>> GetMyTasks(
            [FromQuery] string? search,
            [FromQuery] Guid? teamId,
            [FromQuery] Guid? statusId,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 20)
        {
            var userId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var result = await _taskService.ListTasksAsync(userId, search, teamId, statusId, userId, page, limit);
            return ApiResponse<PagedResult<TaskResponseDto>>.Success(result, "Your tasks fetched successfully.");
        }

        // Must be declared before {id:guid} to avoid route conflict
        [HttpGet("team/{teamId:guid}/board")]
        public async Task<ApiResponse<BoardResponseDto>> GetBoard(Guid teamId, [FromQuery] Guid assigneeId = default)
        {
            var userId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var result = await _taskService.GetBoardAsync(teamId, userId, assigneeId);
            return ApiResponse<BoardResponseDto>.Success(result, "Board fetched successfully.");
        }

        [HttpPost]
        public async Task<ApiResponse<TaskResponseDto>> CreateTask([FromBody] CreateTaskRequestDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var result = await _taskService.CreateTaskAsync(dto, userId);
            return ApiResponse<TaskResponseDto>.Success(result, "Task created successfully.", 201);
        }

        [HttpGet("{id:guid}")]
        public async Task<ApiResponse<TaskResponseDto>> GetTask(Guid id)
        {
            var userId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var result = await _taskService.GetTaskAsync(id, userId);
            return ApiResponse<TaskResponseDto>.Success(result, "Task fetched successfully.");
        }

        [HttpPut("{id:guid}")]
        public async Task<ApiResponse<TaskResponseDto>> UpdateTask(Guid id, [FromBody] UpdateTaskRequestDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var result = await _taskService.UpdateTaskAsync(id, dto, userId);
            return ApiResponse<TaskResponseDto>.Success(result, "Task updated successfully.");
        }

        [HttpDelete("{id:guid}")]
        public async Task<ApiResponse<object>> DeleteTask(Guid id)
        {
            var userId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            await _taskService.DeleteTaskAsync(id, userId);
            return ApiResponse<object>.Success(null!, "Task deleted successfully.", 204);
        }

        [HttpPatch("{id:guid}/status")]
        public async Task<ApiResponse<TaskResponseDto>> ChangeStatus(Guid id, [FromBody] StatusChangeRequestDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var result = await _taskService.ChangeStatusAsync(id, dto.StatusId, userId, dto.Progress);
            return ApiResponse<TaskResponseDto>.Success(result, "Task status updated successfully.");
        }
    }
}
