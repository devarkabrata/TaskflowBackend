using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlowBackend.DTOs;
using TaskFlowBackend.Helpers.API;

namespace TaskFlowBackend.Controllers
{
    [ApiController]
    [Route("api/health")]
    public class HealthController : ControllerBase
    {
        public HealthController(){}

        [AllowAnonymous]
        [HttpHead]
        public async Task<ApiResponse<HealthResponseDTO>> CheckHealth()
        {
            var response = new HealthResponseDTO
            {
                Source = "Dotnet 8 Web API Backend",
                Timestamp = DateTime.UtcNow
            };

            return ApiResponse<HealthResponseDTO>.Success(response, "Server is Up and Running", 200);
        }
        
    }
}
