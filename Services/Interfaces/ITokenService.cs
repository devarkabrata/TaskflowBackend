using TaskFlowBackend.Models;

namespace TaskFlowBackend.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
        Task<string?> GenerateNewAccessToken(string refreshToken);
        bool VerifyToken(string token);
    }
}
