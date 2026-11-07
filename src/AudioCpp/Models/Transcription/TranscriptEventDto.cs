using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mabs.AudioCpp.Models.Transcription;

/// <summary>Wire shape of a transcription stream event payload.</summary>
internal sealed record TranscriptEventDto
{
    /// <summary>The event type discriminator.</summary>
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    /// <summary>Text delta for <c>transcript.text.delta</c>.</summary>
    [JsonPropertyName("delta")]
    public string? Delta { get; init; }

    /// <summary>Provisional or final text.</summary>
    [JsonPropertyName("text")]
    public string? Text { get; init; }

    /// <summary>Speaker turns for diarization events.</summary>
    [JsonPropertyName("speaker_turns")]
    public IReadOnlyList<AudioCppSpeakerTurn>? SpeakerTurns { get; init; }

    /// <summary>Sample rate the turn offsets use.</summary>
    [JsonPropertyName("sample_rate")]
    public int? SampleRate { get; init; }

    /// <summary>Timing on done events.</summary>
    [JsonPropertyName("timing")]
    public AudioCppTiming? Timing { get; init; }
}