using System.Text.Json.Serialization;

namespace Mabs.AudioCpp.Models.Tasks;

/// <summary>Wire shape of a task batch stream event payload.</summary>
internal sealed record TaskBatchEventDto
{
    /// <summary>The event type discriminator.</summary>
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    /// <summary>Original request index (Result events).</summary>
    [JsonPropertyName("index")]
    public int? Index { get; init; }

    /// <summary>The entry's result.</summary>
    [JsonPropertyName("result")]
    public TaskResult? Result { get; init; }

    /// <summary>Number of results (Done event).</summary>
    [JsonPropertyName("result_count")]
    public int? ResultCount { get; init; }

    /// <summary>Aggregate timing (Done event).</summary>
    [JsonPropertyName("timing")]
    public AudioCppTiming? Timing { get; init; }
}