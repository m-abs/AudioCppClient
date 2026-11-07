using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

using Mabs.AudioCpp.Utils;

namespace Mabs.AudioCpp.Models.Transcription;

/// <summary>Rich transcription response with the detail arrays the model produced.</summary>
public sealed record TranscriptionDetailsResult
{
    /// <summary>Recognized text.</summary>
    [JsonPropertyName("text")]
    public string? Text { get; init; }

    /// <summary>Detected or configured language.</summary>
    [JsonPropertyName("language")]
    public string? Language { get; init; }

    /// <summary>Speech segments, when the model produced them.</summary>
    [JsonPropertyName("segments")]
    public IReadOnlyList<AudioCppSegment>? Segments { get; init; }

    /// <summary>Speaker turns, when the model diarized.</summary>
    [JsonPropertyName("speaker_turns")]
    public IReadOnlyList<AudioCppSpeakerTurn>? SpeakerTurns { get; init; }

    /// <summary>Word timestamps, when the model aligned words.</summary>
    [JsonPropertyName("words")]
    public IReadOnlyList<AudioCppWord>? Words { get; init; }

    /// <summary>Sample rate the offsets are counted in; present alongside the arrays.</summary>
    [JsonPropertyName("sample_rate")]
    public int? SampleRate { get; init; }

    /// <summary>Run timing.</summary>
    [JsonPropertyName("timing")]
    public AudioCppTiming? Timing { get; init; }

    /// <summary>Parses a cached response JSON (a disk cache hit rebuilds the result).</summary>
    /// <param name="json">The raw JSON response body.</param>
    /// <returns>The parsed result; throws <see cref="JsonException"/> on null, invalid JSON, or a non-object root.</returns>
    public static TranscriptionDetailsResult Parse(string? json)
    {
        return Json.ParseObject<TranscriptionDetailsResult>(json);
    }
}