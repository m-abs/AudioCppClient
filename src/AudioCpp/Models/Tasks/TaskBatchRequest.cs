using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mabs.AudioCpp.Models.Tasks;

/// <summary>Body of <c>POST /v1/tasks/batch</c>.</summary>
public sealed record TaskBatchRequest
{
    /// <summary>Native-batching model id. Required.</summary>
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>Busy-wait bound for the batch, in milliseconds.</summary>
    [JsonPropertyName("busy_timeout_ms")]
    public long? BusyTimeoutMs { get; set; }

    /// <summary>One entry per request; results map back by index. At least one entry is required.</summary>
    [JsonPropertyName("requests")]
    public IList<TaskRequestBody> Requests { get; } = [];

    /// <summary>Validates the model id, the requests array, and every entry.</summary>
    /// <exception cref="ArgumentException">The model id is missing or the requests array is empty.</exception>
    public void Validate()
    {
        if (string.IsNullOrEmpty(Model))
        {
            throw new ArgumentException("task batch requires a model id", nameof(Model));
        }

        if (Requests.Count == 0)
        {
            throw new ArgumentException("task batch requires a non-empty requests array");
        }

        foreach (TaskRequestBody request in Requests)
        {
            request.Validate();
        }
    }
}