using TaskFlowBackend.Models;

namespace TaskFlowBackend.Repository.Interfaces
{
    public interface ICommentRepository
    {
        Task<Comment?> GetCommentsByIdAsync(Guid commentId);
        Task<IEnumerable<Comment>> GetCommentsByTaskIdAsync(Guid taskId);
        Task<Comment> AddCommentAsync(Comment comment);
        Task<Comment> UpdateCommentAsync(Comment comment);
        Task<bool> DeleteCommentAsync(Guid commentId);
    }
}
