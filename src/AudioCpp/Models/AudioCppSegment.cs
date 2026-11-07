using System.Text.Json.Serialization;

namespace Mabs.AudioCpp.Models;

/// <summary>A speech segment reported by a model (timestamps are sample offsets).</summary>
public sealed record AudioCppSegment
{
    /// <summary>First sample of the segment.</summary>
    [JsonPropertyName("start_sample")]
    public long? StartSample { get; init; }

    /// <summary>End sample (exclusive) of the segment.</summary>
    [JsonPropertyName("end_sample")]
    public long? EndSample { get; init; }

    /// <summary>Model confidence, when reported.</summary>
    [JsonPropertyName("confidence")]
    public double? Confidence { get; init; }

    /// <summary>Segment text, when the segment carries one.</summary>
    [JsonPropertyName("text")]
    public string? Text { get; init; }
}