using System.Text.Json.Serialization;

namespace Mabs.AudioCpp.Models.Transcription;

/// <summary>Request of <c>POST /v1/audio/transcriptions</c>.</summary>
public sealed record TranscriptionRequest : TranscriptionInputBase
{
    /// <summary>Recognition-context prompt (hotwords, spellings).</summary>
    [JsonPropertyName("prompt")]
    public string? Prompt { get; set; }

    /// <summary>When <see langword="true"/>, the JSON form requests transcript SSE (<c>stream=true</c>); multipart uploads set the <c>stream</c> field instead.</summary>
    [JsonPropertyName("stream")]
    public bool? Stream { get; set; }
}