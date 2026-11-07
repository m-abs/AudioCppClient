namespace Mabs.AudioCpp.Models;

/// <summary>Non-streaming speech result: audio bytes, content type, and timing headers.</summary>
public sealed record SpeechResult
{
    /// <summary>Audio bytes (WAV by default).</summary>
    public byte[] Audio { get; internal set; } = [];

    /// <summary>Response content type, for example <c>audio/wav</c>.</summary>
    public string? ContentType { get; internal set; }

    /// <summary>Timing reported by the <c>X-AudioCPP-Wall-Ms</c>, <c>X-AudioCPP-Audio-Duration-Ms</c> and <c>X-AudioCPP-RTF</c> headers.</summary>
    public AudioCppTiming? Timing { get; internal set; }
}