using TaskFlowBackend.DTOs.Auth;
using TaskFlowBackend.DTOs.Users;
using TaskFlowBackend.Models;

namespace TaskFlowBackend.Services.Interfaces
{
    public interface IUserService
    {
        Task<User?> CreateUser(CreateUserRequestDto user);
        // Task<User?> CreateUserWithWorkspace(User user);
        Task<User?> UpdateUser(Guid id, UpdateUserRequestDto user);
        Task<User?> GetUser(Guid id);
        Task<bool> DeleteUser(Guid id);
    }
}