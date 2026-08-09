using TaskFlowBackend.DTOs.Auth;
using TaskFlowBackend.DTOs.Tasks;
using TaskFlowBackend.DTOs.Users;
using TaskFlowBackend.Helpers.CustomException;
using TaskFlowBackend.Helpers.Pagination;
using TaskFlowBackend.Models;
using TaskFlowBackend.Repository.Interfaces;
using TaskFlowBackend.Services.Interfaces;

namespace TaskFlowBackend.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;
        private readonly IWorkspaceService _workspaceService;
        private readonly IAvatarStorageService _avatarStorageService;
        private readonly ITaskService _taskService;
        private readonly ITeamService _teamService;

        public UserService(IUserRepository userRepo, IWorkspaceService workspaceService, IAvatarStorageService avatarStorageService, ITaskService taskService, ITeamService teamService)
        {
            _userRepo = userRepo;
            _workspaceService = workspaceService;
            _avatarStorageService = avatarStorageService;
            _taskService = taskService;
            _teamService = teamService;
        }

        // Method for computing User Initials
        private static string ComputeInitials(string name)
        {
            var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return "?";
            if (parts.Length == 1) return parts[0][0].ToString().ToUpper();
            return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
        }

        public async Task<User?> CreateUser(CreateUserRequestDto user)
        {
            // Making the password hash using BCrypt
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password);

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Name = user.Name,
                Title = user.Title ?? string.Empty,
                Email = user.Email,
                AvatarUrl = user.AvatarUrl ?? string.Empty,
                AvatarPublicId = user.AvatarPublicId ?? string.Empty,
                AvatarInitials = user.AvatarInitials ?? ComputeInitials(user.Name),
                PasswordHash = hashedPassword,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await _userRepo.CreateUserAsync(newUser);

            return result;
        }

        public async Task<Settings?> CreateDefaultUserSettings(Guid id)
        {
            var payload = new Settings
            {
                Id = Guid.NewGuid(),
                UserId = id,
                DaysToArchieve = 2,
                NotificationOnMemberAddToWorkspace = false,
                NotificationOnMemberAddToTeam = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await _userRepo.CreateUserSettingsAsync(payload);

            return result;
        }

        public async Task<Settings?> GetUserSettings(Guid id)
        {
            var result = await _userRepo.GetUserSettingsByIdAsync(id);
            return result;
        }

        public async Task<User?> UpdateUser(Guid id, UpdateUserRequestDto user)
        {
            var existingUser = await _userRepo.GetUserByIdAsync(id);
            if (existingUser == null)
            {
                return null;
            }

            existingUser.Name = user.Name ?? existingUser.Name;
            existingUser.Title = user.Title ?? existingUser.Title;
            existingUser.AvatarUrl = user.AvatarUrl ?? existingUser.AvatarUrl;
            existingUser.AvatarPublicId = user.AvatarPublicId ?? existingUser.AvatarPublicId;
            existingUser.UpdatedAt = DateTime.UtcNow;

            var result = await _userRepo.UpdateUserAsync(existingUser);

            return result;
        }

        public async Task<User?> UpdateUserPassword(string email, string newPassword)
        {
            var existingUser = await _userRepo.GetUserByEmailAsync(email);
            if (existingUser == null)
            {
                throw new NotFoundException("There is no user with the provided email.");
            }

            // Hash the new password
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);
            existingUser.PasswordHash = hashedPassword;

            var result = await _userRepo.UpdateUserAsync(existingUser);

            return result;
        }

        public async Task<Settings?> UpdateUserSettings(Guid id, UpdateUserSettingsRequestDto settings)
        {
            var existingUserSettings = await _userRepo.GetUserSettingsByIdAsync(id);
            if (existingUserSettings == null)
            {
                return null;
            }

            existingUserSettings.DaysToArchieve = settings.DaysToArchieve;
            existingUserSettings.NotificationOnMemberAddToWorkspace = settings.NotificationOnMemberAddToWorkspace;
            existingUserSettings.NotificationOnMemberAddToTeam = settings.NotificationOnMemberAddToTeam;
            existingUserSettings.UpdatedAt = DateTime.UtcNow;

            var result = await _userRepo.UpdateUserSettingsAsync(existingUserSettings);

            return result;
        } 

        public async Task<User> UpdateAvatarAsync(Guid callerUserId, IFormFile file)
        {
            var user = await _userRepo.GetUserByIdAsync(callerUserId) ?? throw new NotFoundException("User not found.");

            var (url, storagePath) = await _avatarStorageService.UploadAsync(file, callerUserId);
            var previousStoragePath = user.AvatarPublicId;

            user.AvatarUrl = url;
            user.AvatarPublicId = storagePath;
            user.UpdatedAt = DateTime.UtcNow;

            var updated = await _userRepo.UpdateUserAsync(user);

            if (!string.IsNullOrEmpty(previousStoragePath))
                await _avatarStorageService.DeleteAsync(previousStoragePath);

            return updated!;
        }

        public async Task<string> DeleteAvatarAsync(Guid callerUserId)
        {
            var user = await _userRepo.GetUserByIdAsync(callerUserId) ?? throw new NotFoundException("User not found.");

            if (!string.IsNullOrEmpty(user.AvatarPublicId))
                await _avatarStorageService.DeleteAsync(user.AvatarPublicId);

            user.AvatarUrl = null;
            user.AvatarPublicId = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepo.UpdateUserAsync(user);
            return user.AvatarInitials!;
        }

        public async Task<User?> GetUser(Guid id)
        {
            var result = await _userRepo.GetUserByIdAsync(id);
            return result;
        }

        public async Task<bool> DeleteUser(Guid id)
        {
            bool result = await _userRepo.DeleteUserAsync(id);
            return result;
        }

        public async Task<List<User>> GetAllUsers(string? search = null, PaginationParams? paginationParams = null, Guid? workspaceId = null)
        {
            var result = await _userRepo.GetAllUsersAsync(search, paginationParams, workspaceId);
            return result;
        }

        public async Task<(User? createdUser, Workspace? workspace)> CreateUserWithWorkspace(CreateUserRequestDto user, string workspace_name)
        {
            // Create the user first
            var createdUser = await CreateUser(user);

            if (createdUser == null)
            {
                return (null, null);
            }

            // Create a default settings
            await CreateDefaultUserSettings(createdUser.Id);

            // Create and add that member to the owrkspace
            var workspace = await _workspaceService.CreateDefaultWorkspaceAsync(createdUser.Id, workspace_name);

            return (createdUser, workspace);
        }

        public async Task<StatResponseDto> GetUserStatsAsync(Guid user_id)
        {
            // Get Workspace count
            int workspace_count = await _workspaceService.GetWorkspaceCountAsync(user_id);

            // Get Tasks Count
            TaskCountDTO task = await _taskService.GetTaskCountByUser(user_id);

            // Get Teams count
            int teams_count = await _teamService.GetTeamCountAsync(user_id);

            return new StatResponseDto
            {
                TaskCount=task,
                TeamCount=teams_count,
                WorkspaceCount=workspace_count
            };
        }
    }
}