using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using Mabs.AudioCpp.Models.Transcription;

namespace Mabs.AudioCpp.Utils.Converters;

/// <summary>Reads a transcription stream event payload, dispatching on <c>type</c>; writing is not supported.</summary>
internal sealed class TranscriptEventConverter : JsonConverter<TranscriptEvent>
{
    /// <inheritdoc/>
    public override TranscriptEvent Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        TranscriptEventDto dto = JsonSerializer.Deserialize<TranscriptEventDto>(ref reader, options)!;
        return dto.Type switch
        {
            "transcript.text.delta" => new TranscriptEvent(TranscriptEventKind.TextDelta, dto.Delta ?? string.Empty, null, null, null),
            "transcript.text.partial" => new TranscriptEvent(TranscriptEventKind.TextPartial, dto.Text ?? string.Empty, null, null, null),
            "transcript.text.done" => new TranscriptEvent(TranscriptEventKind.TextDone, dto.Text ?? string.Empty, null, null, dto.Timing),
            "diarization.delta" => new TranscriptEvent(TranscriptEventKind.DiarizationDelta, null, dto.SpeakerTurns, dto.SampleRate, null),
            "diarization.done" => new TranscriptEvent(TranscriptEventKind.DiarizationDone, null, dto.SpeakerTurns, dto.SampleRate, dto.Timing),
            _ => throw new JsonException("unknown transcription stream event type: " + (dto.Type ?? "<none>")),
        };
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, TranscriptEvent value, JsonSerializerOptions options)
    {
        throw new NotSupportedException("TranscriptEvent can only be deserialized.");
    }
}