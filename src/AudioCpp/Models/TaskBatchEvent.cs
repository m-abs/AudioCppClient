using System.Text.Json;
using System.Text.Json.Serialization;

using Mabs.AudioCpp.Utils;

namespace Mabs.AudioCpp.Models;

/// <summary>A task batch stream event.</summary>
[JsonConverter(typeof(TaskBatchEventConverter))]
public readonly struct TaskBatchEvent
{
    /// <summary>Stores the parsed event fields.</summary>
    /// <param name="kind">The event kind.</param>
    /// <param name="index">Original request index for result events.</param>
    /// <param name="result">The entry's result.</param>
    /// <param name="resultCount">Number of results on the done event.</param>
    /// <param name="timing">Aggregate timing on the done event.</param>
    internal TaskBatchEvent(TaskBatchEventKind kind, int index, TaskResult? result, int? resultCount, AudioCppTiming? timing)
    {
        Kind = kind;
        Index = index;
        Result = result;
        ResultCount = resultCount;
        Timing = timing;
    }

    /// <summary>Which event this is.</summary>
    public TaskBatchEventKind Kind { get; }

    /// <summary>Original request index (Result events).</summary>
    public int Index { get; }

    /// <summary>The entry's result; its own <c>timing</c> is null because fused execution has no per-request wall time.</summary>
    public TaskResult? Result { get; }

    /// <summary>Number of results (Done event).</summary>
    public int? ResultCount { get; }

    /// <summary>Aggregate timing (Done event).</summary>
    public AudioCppTiming? Timing { get; }

    /// <summary>Parses one SSE data payload into a batch event.</summary>
    /// <param name="json">The event's JSON data payload.</param>
    /// <returns>The parsed event.</returns>
    /// <exception cref="JsonException">The payload carries an unknown event type.</exception>
    internal static TaskBatchEvent Parse(string json)
    {
        return Json.Deserialize<TaskBatchEvent>(json);
    }
}