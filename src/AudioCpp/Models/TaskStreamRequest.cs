using System;
using System.Text.Json.Serialization;

namespace Mabs.AudioCpp.Models;

/// <summary>Body of <c>POST /v1/tasks/stream</c>.</summary>
public sealed record TaskStreamRequest
{
    /// <summary>Configured model id. Required.</summary>
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>
    /// When <c>sse</c>, the response is server-sent events written as the model produces
    /// them; when <see langword="null"/> the response is one JSON body (the client parses
    /// it and replays it as the same event sequence). Any other string is a server 400.
    /// </summary>
    [JsonPropertyName("stream_format")]
    public string? StreamFormat { get; set; } = Models.StreamFormat.Sse.Value;

    /// <summary>Ask the server to include streamed artifacts (forwarded; builds without artifact streaming ignore it).</summary>
    [JsonPropertyName("include_artifacts")]
    public bool? IncludeArtifacts { get; set; }

    /// <summary>Busy-wait bound for this run, in milliseconds.</summary>
    [JsonPropertyName("busy_timeout_ms")]
    public long? BusyTimeoutMs { get; set; }

    /// <summary>The framework request. Audio must use <see cref="TaskRequestBody.AudioPath"/>: the stream route ignores <c>audio_base64</c>.</summary>
    [JsonPropertyName("request")]
    public TaskRequestBody Request { get; set; } = new TaskRequestBody();

    /// <summary>Validates the model id, the stream format, and the inner request.</summary>
    /// <exception cref="ArgumentException">The model id is missing, or <see cref="StreamFormat"/> is neither <c>sse</c> nor null.</exception>
    public void Validate()
    {
        if (string.IsNullOrEmpty(Model))
        {
            throw new ArgumentException("task stream requires a model id", nameof(Model));
        }

        if (StreamFormat != null && StreamFormat != "sse")
        {
            throw new ArgumentException("task stream stream_format must be \"sse\" or null (JSON response)");
        }

        Request.Validate();
    }
}