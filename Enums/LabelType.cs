using System.Text.Json.Serialization;

namespace TaskFlowBackend.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum LabelType
    {
        Feature,
        Bug,
        Design,
        Docs,
        Infra,
        Refactor
    }
}
