using System.Text.Json.Serialization;

namespace Mabs.AudioCpp.Models.Transcription;

/// <summary>Request of <c>POST /v1/audio/transcriptions/details</c>. Streaming is rejected by the server on this route.</summary>
public sealed record TranscriptionDetailsRequest : TranscriptionInputBase
{
    /// <summary>Recognition-context prompt.</summary>
    [JsonPropertyName("prompt")]
    public string? Prompt { get; set; }
}