using System.Text.Json.Serialization;

namespace Mabs.AudioCpp.Models;

/// <summary>A speaker turn from a diarization model.</summary>
public sealed record AudioCppSpeakerTurn
{
    /// <summary>First sample of the turn.</summary>
    [JsonPropertyName("start_sample")]
    public long? StartSample { get; init; }

    /// <summary>End sample (exclusive) of the turn.</summary>
    [JsonPropertyName("end_sample")]
    public long? EndSample { get; init; }

    /// <summary>Speaker label, for example <c>speaker_0</c>.</summary>
    [JsonPropertyName("speaker_id")]
    public string? SpeakerId { get; init; }

    /// <summary>Model confidence, when reported.</summary>
    [JsonPropertyName("confidence")]
    public double? Confidence { get; init; }

    /// <summary>Turn text, when the model attaches one.</summary>
    [JsonPropertyName("text")]
    public string? Text { get; init; }
}