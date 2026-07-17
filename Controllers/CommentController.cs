using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TaskFlowBackend.DTOs.Comments;
using TaskFlowBackend.Helpers.API;
using TaskFlowBackend.Services.Interfaces;

namespace TaskFlowBackend.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/comments")]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpGet]
        public async Task<ApiResponse<List<CommentResponseDto>>> GetComments([FromQuery] Guid taskId)
        {
            var userId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var result = await _commentService.GetCommentsByTaskId(taskId, userId);
            return ApiResponse<List<CommentResponseDto>>.Success(result, "Comments fetched successfully.");
        }

        [HttpPost]
        public async Task<ApiResponse<CommentResponseDto>> CreateComment([FromBody] CreateCommentRequestDto dto, [FromQuery] Guid taskId)
        {
            var userId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var result = await _commentService.CreateNewComment(dto, userId, taskId);
            return ApiResponse<CommentResponseDto>.Success(result, "Comment created successfully.", 201);
        }

        [HttpPut("{id:guid}")]
        public async Task<ApiResponse<CommentResponseDto>> UpdateComment(Guid id, [FromBody] UpdateCommentRequestDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var result = await _commentService.UpdateCommentAsync(dto, id, userId);
            return ApiResponse<CommentResponseDto>.Success(result, "Comment updated successfully.");
        }

        [HttpDelete("{id:guid}")]
        public async Task<ApiResponse<object>> DeleteComment(Guid id)
        {
            var userId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            await _commentService.DeleteCommentAsync(id, userId);
            return ApiResponse<object>.Success(null!, "Comment deleted successfully.", 204);
        }
    }
}
