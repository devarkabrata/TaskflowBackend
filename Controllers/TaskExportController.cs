using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TaskFlowBackend.DTOs.Export;
using TaskFlowBackend.Enums;
using TaskFlowBackend.Services.Interfaces;

namespace TaskFlowBackend.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/tasks/export")]
    public class TaskExportController : ControllerBase
    {
        private readonly ITaskExportService _taskExportService;

        public TaskExportController(ITaskExportService taskExportService)
        {
            _taskExportService = taskExportService;
        }

        [HttpPost]
        public async Task<IActionResult> Export([FromBody] TaskExportRequestDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var fileBytes = await _taskExportService.ExportTeamTasksAsync(dto, userId);

            var (contentType, extension) = dto.Format == TaskExportFormat.Xlsx
                ? ("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", ".xlsx")
                : ("text/csv", ".csv");

            var fileName = BuildSafeFileName(dto.FileName, extension);

            return File(fileBytes, contentType, fileName);
        }

        private static string BuildSafeFileName(string fileName, string extension)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var cleaned = new string(fileName.Where(c => !invalidChars.Contains(c)).ToArray()).Trim();

            foreach (var knownExtension in new[] { ".csv", ".xlsx" })
            {
                if (cleaned.EndsWith(knownExtension, StringComparison.OrdinalIgnoreCase))
                    cleaned = cleaned[..^knownExtension.Length];
            }

            if (string.IsNullOrWhiteSpace(cleaned))
                cleaned = "tasks-export";

            return $"{cleaned}{extension}";
        }
    }
}
