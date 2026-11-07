using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using Mabs.AudioCpp.Models.Tasks;

namespace Mabs.AudioCpp.Utils.Converters;

/// <summary>Reads a task stream event payload, dispatching on <c>type</c>; writing is not supported.</summary>
internal sealed class TaskStreamEventConverter : JsonConverter<TaskStreamEvent>
{
    /// <inheritdoc/>
    public override TaskStreamEvent Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        JsonElement root = JsonSerializer.Deserialize<JsonElement>(ref reader, options)!;
        string? type = root.TryGetProperty("type", out JsonElement typeElement) ? typeElement.GetString() : null;
        switch (type)
        {
            case "task.stream.event":
                return TaskStreamEvent.FromEvent(JsonSerializer.Deserialize<TaskStreamEventBody>(root.GetRawText(), options)!);
            case "task.stream.done":
                if (root.TryGetProperty("result", out JsonElement result) && result.ValueKind == JsonValueKind.Object)
                {
                    return TaskStreamEvent.FromDone(JsonSerializer.Deserialize<TaskResult>(result.GetRawText(), options)!);
                }

                return TaskStreamEvent.FromDone(JsonSerializer.Deserialize<TaskResult>(root.GetRawText(), options)!);
            default:
                throw new JsonException("unknown task stream event type: " + (type ?? "<none>"));
        }
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, TaskStreamEvent value, JsonSerializerOptions options)
    {
        throw new NotSupportedException("TaskStreamEvent can only be deserialized.");
    }
}