using Microsoft.EntityFrameworkCore;
using TaskFlowBackend.Data;
using TaskFlowBackend.Models;
using TaskFlowBackend.Repository.Interfaces;

namespace TaskFlowBackend.Repository
{
    public class CommentRepository : ICommentRepository
    {
        private readonly AppDBContext _context;

        public CommentRepository(AppDBContext context)
        {
            _context = context;
        }

        public async Task<Comment?> GetCommentsByIdAsync(Guid commentId)
        {
            return await _context.Comments
                .Include(c => c.Author)
                .FirstOrDefaultAsync(c => c.Id == commentId);
        }

        public async Task<Comment> AddCommentAsync(Comment comment)
        {
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();

            return comment;
        }

        public async Task<IEnumerable<Comment>> GetCommentsByTaskIdAsync(Guid taskId)
        {
            return await _context.Comments
                .Where(c => c.TaskId == taskId)
                .Include(c => c.Author)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Comment> UpdateCommentAsync(Comment comment)
        {
            comment.UpdatedAt = DateTime.UtcNow;
            _context.Comments.Update(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<bool> DeleteCommentAsync(Guid commentId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null) return false;

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
