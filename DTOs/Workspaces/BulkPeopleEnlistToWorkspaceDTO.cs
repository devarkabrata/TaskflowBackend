using System.ComponentModel.DataAnnotations;

namespace TaskFlowBackend.DTOs.Workspaces
{
    public class BulkPeopleEnlistToWorkspaceDTO
    {
        [Required(ErrorMessage = "UserIds are required.")]
        public List<Guid> UserIds { get; set; } = new();
    }
}