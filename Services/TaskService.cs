using TaskFlowBackend.DTOs.Board;
using TaskFlowBackend.DTOs.Tasks;
using TaskFlowBackend.DTOs.Tasks.Archive;
using TaskFlowBackend.Helpers.API;
using TaskFlowBackend.Helpers.CustomException;
using TaskFlowBackend.Helpers.Pagination;
using TaskFlowBackend.Models;
using TaskFlowBackend.Repository.Archive.Interfaces;
using TaskFlowBackend.Repository.Interfaces;
using TaskFlowBackend.Services.Interfaces;

namespace TaskFlowBackend.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepo;
        private readonly ITeamRepository _teamRepo;
        private readonly IBoardStatusRepository _boardStatusRepo;
        private readonly IMigrateTasksRepository _migrateTasksRepo;

        public TaskService(ITaskRepository taskRepo, ITeamRepository teamRepo, IBoardStatusRepository boardStatusRepo, IMigrateTasksRepository migrateTasksRepo)
        {
            _taskRepo = taskRepo;
            _teamRepo = teamRepo;
            _boardStatusRepo = boardStatusRepo;
            _migrateTasksRepo = migrateTasksRepo;
        }

        public async Task<TaskResponseDto> CreateTaskAsync(CreateTaskRequestDto dto, Guid userId)
        {
            var team = await GetTeamOrThrowAsync(dto.TeamId);
            EnsureMembership(team, userId);

            await ValidateStatusBelongsToTeamAsync(dto.StatusId, dto.TeamId);
            ValidateAssigneesAreTeamMembers(team, dto.AssigneeIds);

            var taskNumber = await _taskRepo.GetNextTaskNumberAsync(dto.TeamId);

            var task = new TaskItem
            {
                Id = Guid.NewGuid(),
                TaskNumber = taskNumber,
                Title = dto.Title,
                Description = dto.Description,
                Priority = dto.Priority,
                Label = dto.Label,
                StatusId = dto.StatusId,
                TeamId = dto.TeamId,
                AssigneeIds = dto.AssigneeIds.ToArray(),
                ExpectedCompletion = ToUtc(dto.ExpectedCompletion),
                Progress = dto.Progress,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var created = await _taskRepo.CreateAsync(task);
            var reloaded = await _taskRepo.GetByIdAsync(created.Id);
            return await MapToDtoAsync(reloaded!);
        }

        public async Task<TaskResponseDto> GetTaskAsync(Guid taskId, Guid userId)
        {
            var task = await _taskRepo.GetByIdAsync(taskId) ?? throw new NotFoundException("Task not found.");
            var team = await GetTeamOrThrowAsync(task.TeamId);
            EnsureMembership(team, userId);
            return await MapToDtoAsync(task);
        }

        public async Task<TaskResponseDto> UpdateTaskAsync(Guid taskId, UpdateTaskRequestDto dto, Guid userId)
        {
            var task = await _taskRepo.GetByIdAsync(taskId) ?? throw new NotFoundException("Task not found.");
            var team = await GetTeamOrThrowAsync(task.TeamId);
            EnsureMembership(team, userId);

            if (dto.StatusId != null)
            {
                await ValidateStatusBelongsToTeamAsync(dto.StatusId.Value, task.TeamId);
                task.StatusId = dto.StatusId.Value;
            }

            if (dto.AssigneeIds != null)
            {
                ValidateAssigneesAreTeamMembers(team, dto.AssigneeIds);
                task.AssigneeIds = dto.AssigneeIds.ToArray();
            }

            if (dto.Title != null) task.Title = dto.Title;
            if (dto.Description != null) task.Description = dto.Description;
            if (dto.Priority.HasValue) task.Priority = dto.Priority.Value;
            if (dto.Label.HasValue) task.Label = dto.Label;
            if (dto.ExpectedCompletion.HasValue) task.ExpectedCompletion = ToUtc(dto.ExpectedCompletion);
            if (dto.Progress.HasValue) task.Progress = dto.Progress.Value;
            task.UpdatedAt = DateTime.UtcNow;

            var updated = await _taskRepo.UpdateAsync(task);
            var reloaded = await _taskRepo.GetByIdAsync(updated.Id);
            return await MapToDtoAsync(reloaded!);
        }

        public async Task DeleteTaskAsync(Guid taskId, Guid userId)
        {
            var task = await _taskRepo.GetByIdAsync(taskId) ?? throw new NotFoundException("Task not found.");
            var team = await GetTeamOrThrowAsync(task.TeamId);
            EnsureMembership(team, userId);

            await _taskRepo.DeleteAsync(task);
        }

        public async Task<PagedResult<TaskResponseDto>> ListTasksAsync(Guid userId, string? search, Guid? teamId, Guid? assigneeId, int page, int limit)
        {
            if (teamId.HasValue)
            {
                var team = await GetTeamOrThrowAsync(teamId.Value);
                EnsureMembership(team, userId);
            }

            var pagination = new PaginationParams { Page = page, Limit = limit };
            var (items, total) = await _taskRepo.SearchAsync(userId, teamId, search, assigneeId, pagination);

            var userLookup = await BuildUserLookupAsync(items.SelectMany(t => t.AssigneeIds));
            var dtos = items.Select(t => MapToDto(t, userLookup)).ToList();

            return new PagedResult<TaskResponseDto> { Data = dtos, Total = total, Page = page, Limit = limit };
        }

        public async Task<BoardResponseDto> GetBoardAsync(Guid teamId, Guid userId)
        {
            var team = await GetTeamOrThrowAsync(teamId);
            EnsureMembership(team, userId);

            var statuses = await _boardStatusRepo.GetByTeamIdAsync(teamId);
            var tasks = await _taskRepo.GetByTeamIdAsync(teamId);
            var userLookup = await BuildUserLookupAsync(tasks.SelectMany(t => t.AssigneeIds));

            var columns = statuses.Select(status => new BoardStatusResponseDto
            {
                Id = status.Id,
                Name = status.Name,
                Description = status.Description,
                Position = status.Position,
                TotalTasks = tasks.Count(t => t.StatusId == status.Id),
                IsArchievable = status.IsArchievable,
                IsDeletable = status.IsDeletable,
                Tasks = tasks.Where(t => t.StatusId == status.Id).Select(t => MapToDto(t, userLookup, false)).ToList()
            }).ToList();

            return new BoardResponseDto { TeamId = teamId, Columns = columns };
        }

        public async Task<TaskResponseDto> ChangeStatusAsync(Guid taskId, Guid statusId, Guid userId)
        {
            var task = await _taskRepo.GetByIdAsync(taskId) ?? throw new NotFoundException("Task not found.");
            var team = await GetTeamOrThrowAsync(task.TeamId);
            EnsureMembership(team, userId);

            await ValidateStatusBelongsToTeamAsync(statusId, task.TeamId);

            task.StatusId = statusId;
            task.UpdatedAt = DateTime.UtcNow;

            var updated = await _taskRepo.UpdateAsync(task);
            var reloaded = await _taskRepo.GetByIdAsync(updated.Id);
            return await MapToDtoAsync(reloaded!);
        }

        public async Task<List<TaskItem>> MarkAndCopyEligibleTasksAsync(int batchSize, int olderThanDays, CancellationToken ct = default)
        {
            // Calculate the cutoff date based on the provided olderThanDays
            var cutoff = DateTime.UtcNow.AddDays(-olderThanDays);

            // Fetch eligible tasks from the main database
            var eligible = await _taskRepo.GetUnarchivedTasksOlderthanThresold(Guid.Empty, cutoff, batchSize, ct);

            if (eligible.Count == 0) return eligible;

            // Mark tasks as archived in the main database
            await _taskRepo.UpdateTasksAsArchivedAsync(eligible, ct);

            // Get user information that are assigned to that task
            var userIds = eligible.SelectMany(t => t.AssigneeIds).Distinct();
            var users = await _taskRepo.GetUsersByIdsAsync(userIds);

            // Copy the eligible tasks to the archive database
            var archivedTasks = eligible.Select(task => new ArchivedTaskItem
            {
                Id = task.Id,
                TaskNumber = task.TaskNumber,
                Title = task.Title,
                Description = task.Description,
                Priority = task.Priority,
                Label = task.Label,
                StatusId = task.StatusId,
                TeamId = task.TeamId,
                AssigneeDetails = users.Select(u => new TaskAssigneeDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    AvatarInitials = u.AvatarInitials,
                    AvatarUrl = u.AvatarUrl ?? ""
                }).ToList(),
                ExpectedCompletion = task.ExpectedCompletion,
                Progress = task.Progress,
                CreatedBy = task.CreatedBy,
                DeletedAt = task.DeletedAt,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt
            }).ToList();

            await _migrateTasksRepo.MigrateTasksToArchiveAsync(archivedTasks, ct);

            return eligible;
        }

        public async Task<int> DeleteConfirmedArchivedTasksAsync(CancellationToken ct)
        {
            // Get all archieved tasks
            var ids = await _taskRepo.GetArchievedTasks();

            if(!ids.Any()) return 0;

            // Get confirmed tasks from the archive database
            var confirmedIds = await _migrateTasksRepo.GetConfirmedTaskIds(ids, ct);

            var confirmedTasks = await _taskRepo.GetTasksByIdsAsync(confirmedIds, ct);

            // Delete confirmed tasks from the main database
            var deletedCount = await _taskRepo.DeleteRangeAsync(confirmedTasks);

            return deletedCount;
        }

        private static DateTime? ToUtc(DateTime? value) => value?.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.Value.ToUniversalTime(),
            _ => value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null
        };

        private async Task<Team> GetTeamOrThrowAsync(Guid teamId)
            => await _teamRepo.GetByIdAsync(teamId) ?? throw new NotFoundException("Team not found.");

        private static void EnsureMembership(Team team, Guid userId)
        {
            if (!team.Members.Any(m => m.UserId == userId))
                throw new ForbiddenException("You are not a member of this team.");
        }

        private async Task ValidateStatusBelongsToTeamAsync(Guid statusId, Guid teamId)
        {
            var status = await _boardStatusRepo.GetByIdAsync(statusId);
            if (status == null || status.TeamId != teamId)
                throw new ValidationException("Validation failed.", new List<ApiError>
                {
                    new ApiError { Field = "statusId", Code = "INVALID_STATUS", Message = "StatusId does not belong to this team." }
                });
        }

        private static void ValidateAssigneesAreTeamMembers(Team team, List<Guid> assigneeIds)
        {
            var invalid = assigneeIds.Where(id => !team.Members.Any(m => m.UserId == id)).ToList();
            if (invalid.Any())
                throw new ValidationException("Validation failed.", new List<ApiError>
                {
                    new ApiError { Field = "assigneeIds", Code = "NOT_TEAM_MEMBER", Message = "One or more assignees are not members of this team." }
                });
        }

        private async Task<Dictionary<Guid, User>> BuildUserLookupAsync(IEnumerable<Guid> assigneeIds)
        {
            var distinctIds = assigneeIds.Distinct().ToList();
            var users = await _taskRepo.GetUsersByIdsAsync(distinctIds);
            return users.ToDictionary(u => u.Id);
        }

        private async Task<TaskResponseDto> MapToDtoAsync(TaskItem task)
        {
            var userLookup = await BuildUserLookupAsync(task.AssigneeIds);
            return MapToDto(task, userLookup);
        }

        private static TaskResponseDto MapToDto(TaskItem task, Dictionary<Guid, User> userLookup, bool includeDescription = true) => new()
        {
            Id = task.Id,
            TaskNumber = task.TaskNumber,
            Title = task.Title,
            Description = includeDescription ? task.Description : null,
            Priority = task.Priority.ToString(),
            Label = task.Label?.ToString(),
            StatusId = task.StatusId,
            TeamId = task.TeamId,
            Assignees = task.AssigneeIds
                .Where(id => userLookup.ContainsKey(id))
                .Select(id => new AssigneeSummaryDto
                {
                    UserId = id,
                    Name = userLookup[id].Name,
                    AvatarInitials = userLookup[id].AvatarInitials,
                    AvatarUrl = userLookup[id].AvatarUrl
                }).ToList(),
            ExpectedCompletion = task.ExpectedCompletion,
            Progress = task.Progress,
            CreatedBy = task.CreatedBy,
            DeletedAt = task.DeletedAt,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt
        };
    }
}
