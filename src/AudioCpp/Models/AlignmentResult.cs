using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

using Mabs.AudioCpp.Utils;

namespace Mabs.AudioCpp.Models;

/// <summary>Result of <c>POST /v1/audio/alignments</c>.</summary>
public sealed record AlignmentResult
{
    /// <summary>Aligned text, when echoed back.</summary>
    [JsonPropertyName("text")]
    public string? Text { get; init; }

    /// <summary>Language, when reported.</summary>
    [JsonPropertyName("language")]
    public string? Language { get; init; }

    /// <summary>Word timestamps in seconds plus sample offsets.</summary>
    [JsonPropertyName("words")]
    public IReadOnlyList<AudioCppWord> Words { get; init; } = Array.Empty<AudioCppWord>();

    /// <summary>Run timing.</summary>
    [JsonPropertyName("timing")]
    public AudioCppTiming? Timing { get; init; }

    /// <summary>Parses a cached response JSON (a disk cache hit rebuilds the result).</summary>
    /// <param name="json">The raw JSON response body.</param>
    /// <returns>The parsed result; throws <see cref="JsonException"/> on null, invalid JSON, or a non-object root.</returns>
    public static AlignmentResult Parse(string? json)
    {
        return Json.ParseObject<AlignmentResult>(json);
    }
}