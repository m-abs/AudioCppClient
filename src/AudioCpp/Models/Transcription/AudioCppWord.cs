using System.Text.Json.Serialization;

namespace Mabs.AudioCpp.Models.Transcription;

/// <summary>A word with timestamps, from alignment or word-level transcription output.</summary>
public sealed record AudioCppWord
{
    /// <summary>The word text.</summary>
    [JsonPropertyName("word")]
    public string? Word { get; init; }

    /// <summary>Start time in seconds (alignment responses only).</summary>
    [JsonPropertyName("start")]
    public double? Start { get; init; }

    /// <summary>End time in seconds (alignment responses only).</summary>
    [JsonPropertyName("end")]
    public double? End { get; init; }

    /// <summary>First sample of the word.</summary>
    [JsonPropertyName("start_sample")]
    public long? StartSample { get; init; }

    /// <summary>End sample (exclusive) of the word.</summary>
    [JsonPropertyName("end_sample")]
    public long? EndSample { get; init; }

    /// <summary>Model confidence, when reported.</summary>
    [JsonPropertyName("confidence")]
    public double? Confidence { get; init; }
}