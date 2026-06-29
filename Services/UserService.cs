using TaskFlowBackend.DTOs.Auth;
using TaskFlowBackend.DTOs.Users;
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

        public UserService(IUserRepository userRepo, IWorkspaceService workspaceService)
        {
            _userRepo = userRepo;
            _workspaceService = workspaceService;
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

        public async Task<(User? createdUser, Workspace? workspace)> CreateUserWithWorkspace(CreateUserRequestDto user)
        {
            // Create the user first
            var createdUser = await CreateUser(user);

            if (createdUser == null)
            {
                return (null, null);
            }

            // Create and add that member to the owrkspace
            var workspace = await _workspaceService.CreateDefaultWorkspaceAsync(createdUser.Id, createdUser.Name);

            return (createdUser, workspace);
        }
    }
}