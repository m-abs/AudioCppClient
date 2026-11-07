using System.Text.Json.Serialization;

namespace Mabs.AudioCpp.Models.Tasks;

/// <summary>One named audio output of a result or stream event.</summary>
public sealed record NamedAudioOutput
{
    /// <summary>Output id, for example <c>vocals</c>.</summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>Decoded WAV bytes (results) or PCM; event payloads use <see cref="Format"/> to say which.</summary>
    [JsonPropertyName("audio")]
    public byte[]? Audio { get; init; }

    /// <summary>Sample rate of <see cref="Audio"/> (result payloads).</summary>
    [JsonPropertyName("sample_rate")]
    public int? SampleRate { get; init; }

    /// <summary>Channel count of <see cref="Audio"/> (result payloads).</summary>
    [JsonPropertyName("channels")]
    public int? Channels { get; init; }

    /// <summary>Format tag on stream events: <c>wav</c>.</summary>
    [JsonPropertyName("format")]
    public string? Format { get; init; }
}