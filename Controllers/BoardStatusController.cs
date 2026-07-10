using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TaskFlowBackend.DTOs.Board;
using TaskFlowBackend.Helpers.API;
using TaskFlowBackend.Services.Interfaces;

namespace TaskFlowBackend.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/board-statuses")]
    public class BoardStatusController : ControllerBase
    {
        private readonly IBoardStatusService _boardStatusService;

        public BoardStatusController(IBoardStatusService boardStatusService)
        {
            _boardStatusService = boardStatusService;
        }

        [HttpGet("team/{teamId:guid}")]
        public async Task<ApiResponse<List<BoardStatusCatalogDto>>> GetCatalog(Guid teamId)
        {
            var userId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var result = await _boardStatusService.GetCatalogAsync(teamId, userId);
            return ApiResponse<List<BoardStatusCatalogDto>>.Success(result, "Statuses fetched successfully.");
        }
    }
}
