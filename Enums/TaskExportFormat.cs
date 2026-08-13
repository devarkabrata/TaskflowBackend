using System.Text.Json.Serialization;

namespace TaskFlowBackend.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TaskExportFormat
    {
        Csv,
        Xlsx
    }
}
