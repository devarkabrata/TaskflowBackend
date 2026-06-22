using TaskFlowBackend.DTOs.Auth;
using TaskFlowBackend.Models;

namespace TaskFlowBackend.Services.Interfaces
{
    public interface IAuthService
    {
        Task<User?> SignupAsync(SignupRequestDto dto);
        Task<AuthResponseDto> LoginAsync(LoginRequestDto dto);
    }
}
