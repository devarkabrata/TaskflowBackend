using System.ComponentModel.DataAnnotations;
using TaskFlowBackend.Enums;

namespace TaskFlowBackend.DTOs.Export
{
    public class TaskExportRequestDto
    {
        [Required]
        public Guid TeamId { get; set; }

        [Required]
        public string FileName { get; set; } = string.Empty;

        public bool IsIncludeArchiveTask { get; set; } = false;

        public TaskExportFormat Format { get; set; } = TaskExportFormat.Csv;
    }
}
