using TaskFlowBackend.Helpers.Pagination;
using TaskFlowBackend.Models;

namespace TaskFlowBackend.Repository.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByIdAsync(Guid id);
        Task<Settings?> GetUserSettingsByIdAsync(Guid id);
        Task<User?> CreateUserAsync(User user);
        Task<User?> UpdateUserAsync(User user);
        Task<Settings?> UpdateUserSettingsAsync(Settings settings);
        Task<Settings?> CreateUserSettingsAsync(Settings settings);
        Task<bool> DeleteUserAsync(Guid id);
        Task<User?> GetUserSearchAsync(string searchTerm);
        Task<List<User>> GetAllUsersAsync(string? search = null, PaginationParams? paginationParams = null, Guid? workspaceId = null);
    }
}