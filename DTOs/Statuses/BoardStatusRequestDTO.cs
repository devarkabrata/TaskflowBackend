using System.ComponentModel.DataAnnotations;

namespace TaskFlowBackend.DTOs
{
    public class BoardStatusRequestDTO
    {
        [Required(ErrorMessage = "Status name is required.")]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        [Required(ErrorMessage = "Position is required.")]
        public int Position { get; set; }
        [Required(ErrorMessage = "TeamId is required.")]
        public Guid TeamId { get; set; }
        [Required(ErrorMessage = "Archievable status is required.")]
        public bool? IsArchievable { get; set; }
        public bool? IsDeletable { get; set; }
    }
}