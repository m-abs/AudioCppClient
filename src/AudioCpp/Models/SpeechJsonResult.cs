using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using Mabs.AudioCpp.Utils;

namespace Mabs.AudioCpp.Models;

/// <summary>Parsed <c>response_format=json</c> speech response.</summary>
public sealed record SpeechJsonResult
{
    /// <summary>Base64 WAV text as delivered.</summary>
    [JsonPropertyName("audio")]
    public string AudioBase64 { get; init; } = string.Empty;

    /// <summary>Decoded WAV bytes.</summary>
    [JsonIgnore]
    public byte[] Audio => Convert.FromBase64String(AudioBase64);

    /// <summary>Format tag; the server writes <c>wav</c>.</summary>
    [JsonPropertyName("format")]
    public string? Format { get; init; }

    /// <summary>Run timing (<c>wall_ms</c>, <c>audio_duration_ms</c>, <c>rtf</c>).</summary>
    [JsonPropertyName("timing")]
    public AudioCppTiming? Timing { get; init; }

    /// <summary>Parses a cached response JSON (a disk cache hit rebuilds the result).</summary>
    /// <param name="json">The raw JSON response body.</param>
    /// <returns>The parsed result; throws <see cref="JsonException"/> on null, invalid JSON, or a non-object root.</returns>
    public static SpeechJsonResult Parse(string? json)
    {
        return Json.ParseObject<SpeechJsonResult>(json);
    }
}