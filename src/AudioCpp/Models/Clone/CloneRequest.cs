using System;
using System.Collections.Generic;

using Mabs.AudioCpp.Models.Speech;
using Mabs.AudioCpp.Models.Tasks;

namespace Mabs.AudioCpp.Models.Clone;

/// <summary>
/// Base request record for the clone convenience clients. Family records add typed options;
/// these top-level fields mirror the <c>TaskRequestBody</c> shape. A field travels as a
/// top-level wire field only when the family's options map does not cover the same name.
/// </summary>
public record CloneRequest
{
    /// <summary>Text input.</summary>
    public string? Text { get; set; }

    /// <summary>Language code.</summary>
    public string? Language { get; set; }

    /// <summary>Cached/native voice id.</summary>
    public string? VoiceId { get; set; }

    /// <summary>Voice reference: server-local path string or <see cref="VoiceRefObject"/>.</summary>
    public object? VoiceRef { get; set; }

    /// <summary>Server-local WAV path. Mutually exclusive with <see cref="AudioBase64"/>.</summary>
    public string? AudioPath { get; set; }

    /// <summary>Inline WAV bytes, Base64-encoded on the wire. Mutually exclusive with <see cref="AudioPath"/>.</summary>
    public byte[]? AudioBase64 { get; set; }

    /// <summary>Input artifacts, delivered to the model in array order.</summary>
    public IReadOnlyList<AudioArtifactInput>? Artifacts { get; set; }

    /// <summary>
    /// shortcut: an explicit set equal to the spec default is indistinguishable from unset;
    /// values equal to the spec default are omitted — the server applies its own default, so
    /// the wire is equivalent. A web-ui default that conflicts with the spec default is
    /// serialized, so the documented default wins. Upgrade when a server stops applying spec
    /// defaults.
    /// </summary>
    /// <returns>The options set to a value different from the spec default.</returns>
    public virtual Dictionary<string, string> BuildOptions()
    {
        return [];
    }

    /// <summary>Validates the one-of audio rule, the voice-ref type, and each artifact.</summary>
    public virtual void Validate()
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

    /// <summary>Builds the generic task request body: top-level fields plus the options map.</summary>
    /// <returns>The task request body, validated.</returns>
    public virtual TaskRequestBody ToTaskRequestBody()
    {
        Validate();
        Dictionary<string, string> options = BuildOptions();
        TaskRequestBody body = new();
        if (!options.ContainsKey("text"))
        {
            body.Text = Text;
        }

        if (!options.ContainsKey("language"))
        {
            body.Language = Language;
        }

        if (!options.ContainsKey("voice_id"))
        {
            body.VoiceId = VoiceId;
        }

        if (!options.ContainsKey("voice_ref"))
        {
            body.VoiceRef = VoiceRef;
        }

        body.AudioPath = AudioPath;
        body.AudioBase64 = AudioBase64;
        body.Artifacts = Artifacts;

        if (options.Count > 0)
        {
            body.Options = options;
        }

        return body;
    }
}