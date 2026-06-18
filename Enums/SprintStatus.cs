using System.Text.Json.Serialization;

namespace TaskFlowBackend.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SprintStatus
    {
        Planning,
        Active,
        Completed
    }
}
