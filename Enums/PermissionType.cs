using System.Text.Json.Serialization;

namespace TaskFlowBackend.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PermissionType
    {
        Read,
        Write,
        Delete,
        Manage,
        Comment
    }
}
