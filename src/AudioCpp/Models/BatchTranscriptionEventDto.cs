using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mabs.AudioCpp.Models;

/// <summary>Wire shape of a batch transcription stream event payload.</summary>
internal sealed record BatchTranscriptionEventDto
{
    /// <summary>The event type discriminator.</summary>
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    /// <summary>Upload position this result maps back to.</summary>
    [JsonPropertyName("index")]
    public int? Index { get; init; }

    /// <summary>Sanitized upload filename.</summary>
    [JsonPropertyName("filename")]
    public string? Filename { get; init; }

    /// <summary>Transcript for this file.</summary>
    [JsonPropertyName("text")]
    public string? Text { get; init; }

    /// <summary>Language, when the model reported one.</summary>
    [JsonPropertyName("language")]
    public string? Language { get; init; }

    /// <summary>Speaker turns, for diarization models.</summary>
    [JsonPropertyName("speaker_turns")]
    public IReadOnlyList<AudioCppSpeakerTurn>? SpeakerTurns { get; init; }

    /// <summary>Word timestamps, when produced.</summary>
    [JsonPropertyName("words")]
    public IReadOnlyList<AudioCppWord>? Words { get; init; }

    /// <summary>Segments, when produced.</summary>
    [JsonPropertyName("segments")]
    public IReadOnlyList<AudioCppSegment>? Segments { get; init; }

    /// <summary>Sample rate for the detail arrays.</summary>
    [JsonPropertyName("sample_rate")]
    public int? SampleRate { get; init; }

    /// <summary>Number of results (Done event).</summary>
    [JsonPropertyName("result_count")]
    public int? ResultCount { get; init; }

    /// <summary>Aggregate timing (Done event).</summary>
    [JsonPropertyName("timing")]
    public AudioCppTiming? Timing { get; init; }
}