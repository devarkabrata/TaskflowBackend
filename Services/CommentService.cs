using TaskFlowBackend.DTOs.Comments;
using TaskFlowBackend.Helpers.CustomException;
using TaskFlowBackend.Models;
using TaskFlowBackend.Repository.Interfaces;
using TaskFlowBackend.Services.Interfaces;

namespace TaskFlowBackend.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepo;
        private readonly ITaskRepository _taskRepo;
        private readonly ITeamRepository _teamRepo;

        public CommentService(ICommentRepository commentRepo, ITaskRepository taskRepo, ITeamRepository teamRepo)
        {
            _commentRepo = commentRepo;
            _taskRepo = taskRepo;
            _teamRepo = teamRepo;
        }

        public async Task<List<CommentResponseDto>> GetCommentsByTaskId(Guid taskId, Guid userId)
        {
            await EnsureTaskMembershipAsync(taskId, userId);

            var comments = await _commentRepo.GetCommentsByTaskIdAsync(taskId);
            return comments.Select(MapToDto).ToList();
        }

        public async Task<CommentResponseDto> CreateNewComment(CreateCommentRequestDto dto, Guid userId, Guid taskId)
        {
            await EnsureTaskMembershipAsync(taskId, userId);

            var newComment = new Comment
            {
                Id = Guid.NewGuid(),
                AuthorId = userId,
                TaskId = taskId,
                Body = dto.Body,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var created = await _commentRepo.AddCommentAsync(newComment);
            var reloaded = await _commentRepo.GetCommentsByIdAsync(created.Id);
            return MapToDto(reloaded!);
        }

        public async Task<CommentResponseDto> UpdateCommentAsync(UpdateCommentRequestDto dto, Guid commentId, Guid userId)
        {
            var comment = await _commentRepo.GetCommentsByIdAsync(commentId)
                ?? throw new NotFoundException("Comment not found.");

            await EnsureTaskMembershipAsync(comment.TaskId, userId);
            EnsureAuthor(comment, userId);

            comment.Body = dto.Body;
            var updated = await _commentRepo.UpdateCommentAsync(comment);
            return MapToDto(updated);
        }

        public async Task DeleteCommentAsync(Guid commentId, Guid userId)
        {
            var comment = await _commentRepo.GetCommentsByIdAsync(commentId)
                ?? throw new NotFoundException("Comment not found.");

            await EnsureTaskMembershipAsync(comment.TaskId, userId);
            EnsureAuthor(comment, userId);

            await _commentRepo.DeleteCommentAsync(commentId);
        }

        private async Task EnsureTaskMembershipAsync(Guid taskId, Guid userId)
        {
            var task = await _taskRepo.GetByIdAsync(taskId) ?? throw new NotFoundException("Task not found.");
            var team = await _teamRepo.GetByIdAsync(task.TeamId) ?? throw new NotFoundException("Team not found.");

            if (!team.Members.Any(m => m.UserId == userId))
                throw new ForbiddenException("You are not a member of this team.");
        }

        private static void EnsureAuthor(Comment comment, Guid userId)
        {
            if (comment.AuthorId != userId)
                throw new ForbiddenException("You can only modify your own comments.");
        }

        private static CommentResponseDto MapToDto(Comment comment) => new()
        {
            Id = comment.Id,
            TaskId = comment.TaskId,
            Author = new CommentAuthorDto
            {
                Id = comment.Author.Id,
                Name = comment.Author.Name,
                Email = comment.Author.Email,
                AvatarUrl = comment.Author.AvatarUrl
            },
            Body = comment.Body,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt
        };
    }
}
