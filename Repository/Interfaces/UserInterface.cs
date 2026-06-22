using TaskFlowBackend.DTOs.Auth;
using TaskFlowBackend.DTOs.Users;
using TaskFlowBackend.Models;

namespace TaskFlowBackend.Repository.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByIdAsync(Guid id);
        Task<User?> CreateUserAsync(CreateUserRequestDto user);
        Task<User?> UpdateUserAsync(Guid id, UpdateUserRequestDto user);
        Task<bool> DeleteUserAsync(Guid id);
        Task<User?> GetUserSearchAsync(string searchTerm);
    }
}