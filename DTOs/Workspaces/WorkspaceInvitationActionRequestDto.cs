using System.ComponentModel.DataAnnotations;

namespace TaskFlowBackend.DTOs.Workspaces
{
    public class WorkspaceInvitationActionRequestDto
    {
        [Required(ErrorMessage = "WorkspaceId is required.")]
        public Guid WorkspaceId { get; set; }

        [Required(ErrorMessage = "UserId is required.")]
        public Guid UserId { get; set; }
    }
}
