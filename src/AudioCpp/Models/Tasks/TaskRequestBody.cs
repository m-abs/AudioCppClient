using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

using Mabs.AudioCpp.Models.Speech;

namespace Mabs.AudioCpp.Models.Tasks;

/// <summary>
/// The <c>request</c> object accepted by <c>POST /v1/tasks/run</c>, <c>/v1/tasks/stream</c>
/// (as each entry of <c>/v1/tasks/batch</c>). It mirrors the audio.cpp CLI request-sequence
/// format: exactly one of <see cref="AudioPath"/> / <see cref="AudioBase64"/> for audio
/// input; family-specific parameters go in <see cref="Options"/> (values serialize the way
/// the server's option map does: bools <c>true</c>/<c>false</c>, integers without decimals,
/// doubles invariant).
/// </summary>
public sealed record TaskRequestBody
{
    /// <summary>Text input.</summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>Language code.</summary>
    [JsonPropertyName("language")]
    public string? Language { get; set; }

    /// <summary>Server-local WAV path. Mutually exclusive with <see cref="AudioBase64"/>.</summary>
    [JsonPropertyName("audio")]
    public string? AudioPath { get; set; }

    /// <summary>Inline WAV bytes, Base64-encoded on the wire. Mutually exclusive with <see cref="AudioPath"/>; ignored by <c>/v1/tasks/stream</c> per the server contract.</summary>
    [JsonPropertyName("audio_base64")]
    public byte[]? AudioBase64 { get; set; }

    /// <summary>Cached/native voice id.</summary>
    [JsonPropertyName("voice_id")]
    public string? VoiceId { get; set; }

    /// <summary>Voice reference: server-local path string or <see cref="VoiceRefObject"/> (path form only in CLI-shaped requests; object form is accepted too).</summary>
    [JsonPropertyName("voice_ref")]
    public object? VoiceRef { get; set; }

    /// <summary>Input artifacts, delivered to the model in array order.</summary>
    [JsonPropertyName("artifacts")]
    public IReadOnlyList<AudioArtifactInput>? Artifacts { get; set; }

    /// <summary>Family-specific parameters (the stringly-typed option map).</summary>
    [JsonPropertyName("options")]
    public Dictionary<string, string>? Options { get; set; }

    /// <summary>Omits the options map when it is empty, as the wire format expects.</summary>
    /// <returns>Whether the options map has entries.</returns>
    public bool ShouldSerializeOptions()
    {
        return Options != null && Options.Count > 0;
    }

    /// <summary>Validates the one-of audio rule, the voice-ref type, and each artifact.</summary>
    public void Validate()
    {
        if (AudioPath != null && AudioBase64 != null)
        {
            throw new ArgumentException("provide only one of audio path and audio_base64");
        }

        if (VoiceRef != null && VoiceRef is not string && VoiceRef is not VoiceRefObject)
        {
            throw new ArgumentException("VoiceRef must be a string path or a VoiceRefObject", nameof(VoiceRef));
        }

        if (Artifacts != null)
        {
            foreach (AudioArtifactInput artifact in Artifacts)
            {
                artifact.Validate();
            }
        }
    }
}