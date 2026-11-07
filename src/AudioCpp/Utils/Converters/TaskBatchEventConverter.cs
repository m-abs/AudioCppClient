using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using Mabs.AudioCpp.Models.Tasks;

namespace Mabs.AudioCpp.Utils.Converters;

/// <summary>Reads a task batch event payload, dispatching on <c>type</c>; writing is not supported.</summary>
internal sealed class TaskBatchEventConverter : JsonConverter<TaskBatchEvent>
{
    /// <inheritdoc/>
    public override TaskBatchEvent Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        TaskBatchEventDto dto = JsonSerializer.Deserialize<TaskBatchEventDto>(ref reader, options)!;
        return dto.Type switch
        {
            "task.batch.result" => new TaskBatchEvent(TaskBatchEventKind.Result, dto.Index ?? -1, dto.Result, null, null),
            "task.batch.done" => new TaskBatchEvent(TaskBatchEventKind.Done, -1, null, dto.ResultCount, dto.Timing),
            _ => throw new JsonException("unknown task batch event type: " + (dto.Type ?? "<none>")),
        };
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, TaskBatchEvent value, JsonSerializerOptions options)
    {
        throw new NotSupportedException("TaskBatchEvent can only be deserialized.");
    }
}