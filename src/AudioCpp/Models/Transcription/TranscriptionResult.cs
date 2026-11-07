using System.Text.Json;
using System.Text.Json.Serialization;

using Mabs.AudioCpp.Utils;

namespace Mabs.AudioCpp.Models.Transcription;

/// <summary>Non-streaming transcription response (<c>{text, timing}</c>).</summary>
public sealed record TranscriptionResult
{
    /// <summary>Recognized text.</summary>
    [JsonPropertyName("text")]
    public string? Text { get; init; }

    /// <summary>Run timing.</summary>
    [JsonPropertyName("timing")]
    public AudioCppTiming? Timing { get; init; }

    /// <summary>Parses a cached response JSON (a disk cache hit rebuilds the result).</summary>
    /// <param name="json">The raw JSON response body.</param>
    /// <returns>The parsed result; throws <see cref="JsonException"/> on null, invalid JSON, or a non-object root.</returns>
    public static TranscriptionResult Parse(string? json)
    {
        return Json.ParseObject<TranscriptionResult>(json);
    }
}