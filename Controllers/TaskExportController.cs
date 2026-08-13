using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TaskFlowBackend.DTOs.Export;
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

        [HttpPost("csv")]
        public async Task<IActionResult> ExportCsv([FromBody] TaskCsvExportRequestDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var csvBytes = await _taskExportService.ExportTeamTasksToCsvAsync(dto, userId);
            var fileName = BuildSafeFileName(dto.FileName);

            return File(csvBytes, "text/csv", fileName);
        }

        private static string BuildSafeFileName(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var cleaned = new string(fileName.Where(c => !invalidChars.Contains(c)).ToArray()).Trim();

            if (string.IsNullOrWhiteSpace(cleaned))
                cleaned = "tasks-export";

            return cleaned.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)
                ? cleaned
                : $"{cleaned}.csv";
        }
    }
}
