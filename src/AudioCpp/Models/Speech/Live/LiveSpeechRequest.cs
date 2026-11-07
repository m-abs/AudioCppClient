using System;
using System.Collections.Generic;
using System.Globalization;

using Mabs.AudioCpp.Utils;

namespace Mabs.AudioCpp.Models.Speech.Live;

/// <summary>
/// Parameters of <c>POST /v1/audio/speech/live</c>: chunked PCM body in, SSE or raw audio
/// out. All request fields travel as query parameters because the body carries audio.
/// </summary>
public sealed class LiveSpeechRequest
{
    /// <summary>Configured streaming model id. Required.</summary>
    public string? Model { get; set; }

    /// <summary>
    /// Text for a text-to-speech model (required for it); speech-to-speech, <c>vc</c> and
    /// <c>svc</c> models treat it as an optional prompt.
    /// </summary>
    public string? Input { get; set; }

    /// <summary>Language code.</summary>
    public string? Language { get; set; }

    /// <summary>Voice preset / native voice id.</summary>
    public string? Voice { get; set; }

    /// <summary>Sample rate of the PCM body; 1000–384000. Server default 16000.</summary>
    public int? SampleRate { get; set; } = 16000;

    /// <summary>Interleaved channel count of the PCM body; 1–16. Server default 1.</summary>
    public int? Channels { get; set; } = 1;

    /// <summary><c>s16le</c> (default) or <c>f32le</c>.</summary>
    public string? SampleFormat { get; set; }

    /// <summary><c>sse</c> (default) or <c>audio</c>. The route only supports <c>response_format=pcm</c>.</summary>
    public StreamFormat StreamFormat { get; set; } = StreamFormat.Sse;

    /// <summary>Stream the text the model writes as it speaks; requires <see cref="StreamFormat"/> = <c>sse</c>.</summary>
    public bool? ReturnText { get; set; }

    /// <summary>Upper bound on waiting for a busy model, in milliseconds.</summary>
    public long? BusyTimeoutMs { get; set; }

    /// <summary>Sampling seed as decimal text (full uint64 range).</summary>
    public string? Seed { get; set; }

    /// <summary>
    /// Extra query-passed options the live handler recognizes: <c>temperature</c>,
    /// <c>top_k</c>, <c>top_p</c>, <c>max_tokens</c>, <c>max_steps</c>,
    /// <c>repetition_penalty</c>, <c>guidance_scale</c>, <c>num_inference_steps</c>,
    /// <c>system_prompt</c>, <c>instructions</c> (maps to <c>system_prompt</c>),
    /// <c>voice_id</c>, <c>reference_text</c>, <c>text_temperature</c>, <c>text_top_k</c>,
    /// <c>do_sample</c>.
    /// </summary>
    public Dictionary<string, string>? Options { get; set; }

    /// <summary>Validates the request and builds the query string.</summary>
    /// <returns>The encoded query string with a leading <c>?</c>.</returns>
    public string BuildQueryString()
    {
        if (string.IsNullOrEmpty(Model))
        {
            throw new ArgumentException("live speech requires a model id", nameof(Model));
        }

        if (SampleRate is < 1000 or > 384000)
        {
            throw new ArgumentException("sample_rate must be between 1000 and 384000");
        }

        if (Channels is < 1 or > 16)
        {
            throw new ArgumentException("channels must be between 1 and 16");
        }

        if (ReturnText == true && StreamFormat != StreamFormat.Sse)
        {
            throw new ArgumentException("return_text needs stream_format=sse");
        }

        QueryBuilder query = new();
        query.Add("model", Model);
        query.Add("input", Input);
        query.Add("language", Language);
        query.Add("voice", Voice);
        query.Add("sample_rate", SampleRate?.ToString(CultureInfo.InvariantCulture));
        query.Add("channels", Channels?.ToString(CultureInfo.InvariantCulture));
        query.Add("sample_format", SampleFormat);
        query.Add("stream_format", StreamFormat.Value);
        if (ReturnText.HasValue)
        {
            query.Add("return_text", ReturnText.Value ? "true" : "false");
        }

        query.Add("busy_timeout_ms", BusyTimeoutMs?.ToString(CultureInfo.InvariantCulture));
        query.Add("seed", Seed);
        if (Options != null)
        {
            foreach (KeyValuePair<string, string> pair in Options)
            {
                query.Add(pair.Key, pair.Value);
            }
        }

        return query.ToString();
    }
}