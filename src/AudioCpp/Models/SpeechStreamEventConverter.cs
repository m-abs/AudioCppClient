using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using Mabs.AudioCpp.Enums;

namespace Mabs.AudioCpp.Models;

/// <summary>Reads a speech stream event payload, dispatching on <c>type</c>; writing is not supported.</summary>
internal sealed class SpeechStreamEventConverter : JsonConverter<SpeechStreamEvent>
{
    /// <inheritdoc/>
    public override SpeechStreamEvent Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        SpeechStreamEventDto dto = JsonSerializer.Deserialize<SpeechStreamEventDto>(ref reader, options)!;
        return dto.Type switch
        {
            "speech.audio.delta" => new SpeechStreamEvent(SpeechStreamEventKind.AudioDelta, dto.Audio ?? [], null, null),
            "speech.audio.done" => new SpeechStreamEvent(SpeechStreamEventKind.AudioDone, null, null, dto.Timing),
            "speech.text.delta" => new SpeechStreamEvent(SpeechStreamEventKind.TextDelta, null, dto.Delta ?? string.Empty, null),
            "speech.text.done" => new SpeechStreamEvent(SpeechStreamEventKind.TextDone, null, dto.Text ?? string.Empty, null),
            _ => throw new JsonException("unknown speech stream event type: " + (dto.Type ?? "<none>")),
        };
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, SpeechStreamEvent value, JsonSerializerOptions options)
    {
        throw new NotSupportedException("SpeechStreamEvent can only be deserialized.");
    }
}