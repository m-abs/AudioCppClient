using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mabs.AudioCpp.Models;

/// <summary>
/// Reads a task stream event body from either the SSE envelope (the <c>event</c> member) or a
/// plain event object, mapping the nested <c>partial_text</c> and
/// <c>partial_text_snapshot</c> shapes; writing is not supported.
/// </summary>
internal sealed class TaskStreamEventBodyConverter : JsonConverter<TaskStreamEventBody>
{
    /// <inheritdoc/>
    public override TaskStreamEventBody Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        TaskStreamEventBodyDto dto = JsonSerializer.Deserialize<TaskStreamEventBodyDto>(ref reader, options)!;
        if (dto.Event is not null)
        {
            dto = dto.Event;
        }

        return new TaskStreamEventBody
        {
            PartialText = dto.Text ?? dto.PartialText?.Text,
            PartialTextSnapshot = dto.PartialTextSnapshot?.Text,
            Language = dto.Language ?? dto.PartialText?.Language,
            Audio = dto.Audio,
            NamedAudioOutputs = dto.NamedAudioOutputs,
            WordTimestamps = dto.WordTimestamps,
            SpeakerTurns = dto.SpeakerTurns,
            Artifacts = dto.Artifacts,
            IsFinal = dto.IsFinal,
        };
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, TaskStreamEventBody value, JsonSerializerOptions options)
    {
        throw new NotSupportedException("TaskStreamEventBody can only be deserialized.");
    }
}