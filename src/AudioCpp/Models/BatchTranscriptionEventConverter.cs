using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mabs.AudioCpp.Models;

/// <summary>Reads a batch transcription event payload, dispatching on <c>type</c>; writing is not supported.</summary>
internal sealed class BatchTranscriptionEventConverter : JsonConverter<BatchTranscriptionEvent>
{
    /// <inheritdoc/>
    public override BatchTranscriptionEvent Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        BatchTranscriptionEventDto dto = JsonSerializer.Deserialize<BatchTranscriptionEventDto>(ref reader, options)!;
        return dto.Type switch
        {
            "batch.transcription.result" => new BatchTranscriptionEvent(BatchTranscriptionEventKind.Result,
                                                                        dto.Index ?? -1,
                                                                        dto.Filename,
                                                                        dto.Text,
                                                                        dto.Language,
                                                                        dto.SpeakerTurns,
                                                                        dto.Words,
                                                                        dto.Segments,
                                                                        dto.SampleRate,
                                                                        null,
                                                                        null),
            "batch.transcription.done" => new BatchTranscriptionEvent(BatchTranscriptionEventKind.Done,
                                                                      -1,
                                                                      null,
                                                                      null,
                                                                      null,
                                                                      null,
                                                                      null,
                                                                      null,
                                                                      null,
                                                                      dto.ResultCount,
                                                                      dto.Timing),
            _ => throw new JsonException("unknown batch transcription event type: " + (dto.Type ?? "<none>")),
        };
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, BatchTranscriptionEvent value, JsonSerializerOptions options)
    {
        throw new NotSupportedException("BatchTranscriptionEvent can only be deserialized.");
    }
}