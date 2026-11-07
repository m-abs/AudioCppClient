using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;

namespace Mabs.AudioCpp.Models;

/// <summary>
/// Input for the transcription routes: either a server-local WAV path
/// (<see cref="AudioPath"/>, sent as JSON) or uploaded audio bytes
/// (<see cref="AudioFile"/> + <see cref="AudioFileName"/>, sent as multipart).
/// Exactly one form must be set.
/// </summary>
public record TranscriptionInputBase
{
    /// <summary>Server-local WAV path (JSON body form).</summary>
    [JsonPropertyName("audio")]
    public string? AudioPath { get; set; }

    /// <summary>Audio bytes to upload (multipart form). The client disposes this stream when the request completes.</summary>
    [JsonIgnore]
    public Stream? AudioFile { get; set; }

    /// <summary>Filename reported for <see cref="AudioFile"/>; must name a WAV for most builds.</summary>
    [JsonIgnore]
    public string? AudioFileName { get; set; }

    /// <summary>Configured model id. Required.</summary>
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>Language hint.</summary>
    [JsonPropertyName("language")]
    public string? Language { get; set; }

    /// <summary>Upper bound on waiting for a busy model, in milliseconds.</summary>
    [JsonPropertyName("busy_timeout_ms")]
    public long? BusyTimeoutMs { get; set; }

    /// <summary>Model-family-specific options (JSON form only; the multipart routes do not read an options map for single transcriptions).</summary>
    [JsonPropertyName("options")]
    public Dictionary<string, string>? Options { get; set; }

    /// <summary>Omits the options map when it is empty, as the wire format expects.</summary>
    /// <returns>Whether the options map has entries.</returns>
    public bool ShouldSerializeOptions()
    {
        return Options != null && Options.Count > 0;
    }

    /// <summary>Validates that exactly one audio form is set and a model id is present.</summary>
    public void Validate()
    {
        if (string.IsNullOrEmpty(Model))
        {
            throw new ArgumentException("transcription requires a model id", nameof(Model));
        }

        if (!string.IsNullOrEmpty(AudioPath) && AudioFile != null)
        {
            throw new ArgumentException("provide either AudioPath (server-local) or AudioFile (upload), not both");
        }

        if (string.IsNullOrEmpty(AudioPath) && AudioFile == null)
        {
            throw new ArgumentException("transcription requires AudioPath or AudioFile");
        }

        if (AudioFile != null && string.IsNullOrEmpty(AudioFileName))
        {
            throw new ArgumentException("AudioFileName is required when AudioFile is set", nameof(AudioFileName));
        }
    }

    /// <summary>Whether this request travels as a multipart upload (<see cref="AudioFile"/> form) rather than a JSON body.</summary>
    [JsonIgnore]
    internal bool IsMultipart => AudioFile != null;
}