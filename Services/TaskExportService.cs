using System.Text;
using TaskFlowBackend.DTOs.Export;
using TaskFlowBackend.Helpers.CustomException;
using TaskFlowBackend.Models;
using TaskFlowBackend.Repository.Archive.Interfaces;
using TaskFlowBackend.Repository.Interfaces;
using TaskFlowBackend.Services.Interfaces;

namespace TaskFlowBackend.Services
{
    public class TaskExportService : ITaskExportService
    {
        private static readonly string[] CsvHeader =
        {
            "Task Number", "Title", "Status", "Creation Date", "Updation Date", "Assignees"
        };

        private readonly ITaskRepository _taskRepository;
        private readonly IMigrateTasksRepository _migrateTasksRepository;
        private readonly ITeamRepository _teamRepository;

        public TaskExportService(
            ITaskRepository taskRepository,
            IMigrateTasksRepository migrateTasksRepository,
            ITeamRepository teamRepository)
        {
            _taskRepository = taskRepository;
            _migrateTasksRepository = migrateTasksRepository;
            _teamRepository = teamRepository;
        }

        public async Task<byte[]> ExportTeamTasksToCsvAsync(TaskCsvExportRequestDto request, Guid userId)
        {
            var team = await _teamRepository.GetByIdAsync(request.TeamId)
                ?? throw new NotFoundException("Team not found.");

            if (!team.Members.Any(m => m.UserId == userId))
                throw new ForbiddenException("You are not a member of this team.");

            var rows = new List<TaskCsvRowDto>();

            var activeTasks = await _taskRepository.GetByTeamIdAsync(request.TeamId);
            var userLookup = await BuildUserLookupAsync(activeTasks.SelectMany(t => t.AssigneeIds));

            rows.AddRange(activeTasks.Select(t => new TaskCsvRowDto
            {
                TaskNumber = t.TaskNumber,
                Title = t.Title,
                Status = t.Status?.Name ?? string.Empty,
                AssigneeNames = t.AssigneeIds
                    .Where(id => userLookup.ContainsKey(id))
                    .Select(id => userLookup[id].Name)
                    .ToList(),
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            }));

            if (request.IsIncludeArchiveTask)
            {
                var (archivedTasks, _) = await _migrateTasksRepository.GetArchivedTasksAsync(request.TeamId, null, null, null);

                rows.AddRange(archivedTasks.Select(t => new TaskCsvRowDto
                {
                    TaskNumber = t.TaskNumber,
                    Title = t.Title,
                    Status = t.Status ?? string.Empty,
                    AssigneeNames = t.AssigneeDetails.Select(a => a.Name).ToList(),
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt
                }));
            }

            var orderedRows = rows.OrderBy(r => r.TaskNumber).ToList();

            return BuildCsvBytes(orderedRows);
        }

        private async Task<Dictionary<Guid, User>> BuildUserLookupAsync(IEnumerable<Guid> assigneeIds)
        {
            var distinctIds = assigneeIds.Distinct().ToList();
            var users = await _taskRepository.GetUsersByIdsAsync(distinctIds);
            return users.ToDictionary(u => u.Id);
        }

        private static byte[] BuildCsvBytes(List<TaskCsvRowDto> rows)
        {
            var sb = new StringBuilder();

            AppendCsvLine(sb, CsvHeader);

            foreach (var row in rows)
            {
                AppendCsvLine(sb, new[]
                {
                    row.TaskNumber.ToString(),
                    row.Title,
                    row.Status,
                    row.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                    row.UpdatedAt.ToString("yyyy-MM-dd HH:mm"),
                    string.Join(", ", row.AssigneeNames)
                });
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        private static void AppendCsvLine(StringBuilder sb, IEnumerable<string> fields)
        {
            sb.Append(string.Join(",", fields.Select(EscapeCsvField)));
            sb.Append("\r\n");
        }

        private static string EscapeCsvField(string field)
        {
            if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
                return $"\"{field.Replace("\"", "\"\"")}\"";

            return field;
        }
    }
}
