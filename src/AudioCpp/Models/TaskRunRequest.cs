using System;
using System.Text.Json.Serialization;

namespace Mabs.AudioCpp.Models;

/// <summary>Body of <c>POST /v1/tasks/run</c>.</summary>
public sealed record TaskRunRequest
{
    /// <summary>Configured model id. Required.</summary>
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>Busy-wait bound for this run, in milliseconds.</summary>
    [JsonPropertyName("busy_timeout_ms")]
    public long? BusyTimeoutMs { get; set; }

    /// <summary>The framework request. Required.</summary>
    [JsonPropertyName("request")]
    public TaskRequestBody Request { get; set; } = new TaskRequestBody();

    /// <summary>Validates the model id and the inner request.</summary>
    /// <exception cref="ArgumentException">The model id is missing.</exception>
    public void Validate()
    {
        if (string.IsNullOrEmpty(Model))
        {
            throw new ArgumentException("task run requires a model id", nameof(Model));
        }

        Request.Validate();
    }
}