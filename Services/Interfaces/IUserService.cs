using TaskFlowBackend.DTOs.Auth;
using TaskFlowBackend.DTOs.Users;
using TaskFlowBackend.Helpers.Pagination;
using TaskFlowBackend.Models;

namespace TaskFlowBackend.Services.Interfaces
{
    public interface IUserService
    {
        Task<User?> CreateUser(CreateUserRequestDto user);
        Task<(User? createdUser, Workspace? workspace)> CreateUserWithWorkspace(CreateUserRequestDto user);
        Task<User?> UpdateUser(Guid id, UpdateUserRequestDto user);
        Task<User?> GetUser(Guid id);
        Task<bool> DeleteUser(Guid id);
        Task<List<User>> GetAllUsers(string? search = null, PaginationParams? paginationParams = null, Guid? workspaceId = null);
        Task<User> UpdateAvatarAsync(Guid callerUserId, IFormFile file);
        Task<string> DeleteAvatarAsync(Guid callerUserId);
    }
}