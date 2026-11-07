using System;
using System.Globalization;

using Mabs.AudioCpp.Utils;

namespace Mabs.AudioCpp.Models;

/// <summary>
/// Query parameters of <c>POST /v1/audio/transcriptions/live</c>. The body is raw
/// interleaved PCM sent chunked; there is no multipart form and no JSON.
/// </summary>
public sealed class LiveTranscriptionRequest
{
    /// <summary>Streaming model id. Required.</summary>
    public string? Model { get; set; }

    /// <summary>Sample rate of the PCM; 1000–384000. Server default 16000.</summary>
    public int? SampleRate { get; set; } = 16000;

    /// <summary>Interleaved channels; 1–16. Server default 1.</summary>
    public int? Channels { get; set; } = 1;

    /// <summary><c>s16le</c> (default) or <c>f32le</c>.</summary>
    public string? SampleFormat { get; set; }

    /// <summary>Language hint.</summary>
    public string? Language { get; set; }

    /// <summary>Recognition-context prompt (hotwords, spellings).</summary>
    public string? Prompt { get; set; }

    /// <summary>Upper bound on waiting for a busy model, in milliseconds.</summary>
    public long? BusyTimeoutMs { get; set; }

    /// <summary>Validates and builds the query string.</summary>
    /// <returns>The encoded query string with a leading <c>?</c>.</returns>
    public string BuildQueryString()
    {
        if (string.IsNullOrEmpty(Model))
        {
            throw new ArgumentException("live transcription requires a model id", nameof(Model));
        }

        if (SampleRate is < 1000 or > 384000)
        {
            throw new ArgumentException("sample_rate must be between 1000 and 384000");
        }

        if (Channels is < 1 or > 16)
        {
            throw new ArgumentException("channels must be between 1 and 16");
        }

        QueryBuilder query = new();
        query.Add("model", Model);
        query.Add("sample_rate", SampleRate?.ToString(CultureInfo.InvariantCulture));
        query.Add("channels", Channels?.ToString(CultureInfo.InvariantCulture));
        query.Add("sample_format", SampleFormat);
        query.Add("language", Language);
        query.Add("prompt", Prompt);
        query.Add("busy_timeout_ms", BusyTimeoutMs?.ToString(CultureInfo.InvariantCulture));
        return query.ToString();
    }
}