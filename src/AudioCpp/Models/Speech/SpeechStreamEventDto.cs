using System.Text.Json.Serialization;

namespace Mabs.AudioCpp.Models.Speech;

/// <summary>Wire shape of a speech stream event payload.</summary>
internal sealed record SpeechStreamEventDto
{
    /// <summary>The event type discriminator.</summary>
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    /// <summary>Base64 audio payload for <c>speech.audio.delta</c>.</summary>
    [JsonPropertyName("audio")]
    public byte[]? Audio { get; init; }

    /// <summary>Text delta for <c>speech.text.delta</c>.</summary>
    [JsonPropertyName("delta")]
    public string? Delta { get; init; }

    /// <summary>Final text for <c>speech.text.done</c>.</summary>
    [JsonPropertyName("text")]
    public string? Text { get; init; }

    /// <summary>Timing for <c>speech.audio.done</c>.</summary>
    [JsonPropertyName("timing")]
    public AudioCppTiming? Timing { get; init; }
}