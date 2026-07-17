using TaskFlowBackend.DTOs.Comments;

namespace TaskFlowBackend.Services.Interfaces
{
    public interface ICommentService
    {
        Task<List<CommentResponseDto>> GetCommentsByTaskId(Guid taskId, Guid userId);
        Task<CommentResponseDto> CreateNewComment(CreateCommentRequestDto dto, Guid userId, Guid taskId);
        Task<CommentResponseDto> UpdateCommentAsync(UpdateCommentRequestDto dto, Guid commentId, Guid userId);
        Task DeleteCommentAsync(Guid commentId, Guid userId);
    }
}
