using System.Text.Json;
using System.Text.Json.Serialization;

using Mabs.AudioCpp.Utils;

namespace Mabs.AudioCpp.Models;

/// <summary>A task stream event: an incremental <see cref="Event"/> or the terminal <see cref="Result"/>.</summary>
[JsonConverter(typeof(TaskStreamEventConverter))]
public readonly struct TaskStreamEvent
{
    /// <summary>Stores the parsed event fields.</summary>
    /// <param name="kind">The event kind.</param>
    /// <param name="evt">The event body for <see cref="TaskStreamEventKind.Event"/>.</param>
    /// <param name="result">The result for <see cref="TaskStreamEventKind.Done"/>.</param>
    internal TaskStreamEvent(TaskStreamEventKind kind, TaskStreamEventBody? evt, TaskResult? result)
    {
        Kind = kind;
        Event = evt;
        Result = result;
    }

    /// <summary>Which event this is.</summary>
    public TaskStreamEventKind Kind { get; }

    /// <summary>The incremental event body (<see cref="TaskStreamEventKind.Event"/>).</summary>
    public TaskStreamEventBody? Event { get; }

    /// <summary>The final result (<see cref="TaskStreamEventKind.Done"/>).</summary>
    public TaskResult? Result { get; }

    /// <summary>Wraps an incremental event body.</summary>
    /// <param name="body">The event body.</param>
    /// <returns>The event.</returns>
    internal static TaskStreamEvent FromEvent(TaskStreamEventBody body)
    {
        return new TaskStreamEvent(TaskStreamEventKind.Event, body, null);
    }

    /// <summary>Wraps the terminal result.</summary>
    /// <param name="result">The final result.</param>
    /// <returns>The event.</returns>
    internal static TaskStreamEvent FromDone(TaskResult result)
    {
        return new TaskStreamEvent(TaskStreamEventKind.Done, null, result);
    }

    /// <summary>Parses one SSE data payload into a task stream event.</summary>
    /// <param name="json">The event's JSON data payload.</param>
    /// <returns>The parsed event.</returns>
    /// <exception cref="JsonException">The payload carries an unknown event type.</exception>
    internal static TaskStreamEvent ParseSse(string json)
    {
        return Json.Deserialize<TaskStreamEvent>(json);
    }
}