using System.ComponentModel.DataAnnotations;

namespace TaskFlowBackend.DTOs.Export
{
    public class TaskCsvExportRequestDto
    {
        [Required]
        public Guid TeamId { get; set; }

        [Required]
        public string FileName { get; set; } = string.Empty;

        public bool IsIncludeArchiveTask { get; set; } = false;
    }
}
