using System.Text.Json.Serialization;

namespace TaskFlowBackend.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum WorkspaceMemberStatus
    {
        Active,
        Pending
    }
}
