using TaskFlowBackend.Models;

namespace TaskFlowBackend.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(User user);
    }
}
