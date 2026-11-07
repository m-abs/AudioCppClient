using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mabs.AudioCpp.Models;

/// <summary>JSON-mode task stream response (<c>{"events":[…],"result":{…}}</c>) read when <see cref="TaskStreamRequest.StreamFormat"/> is null.</summary>
public sealed record TaskStreamJsonResponse
{
    /// <summary>The event rows, in production order.</summary>
    [JsonPropertyName("events")]
    public List<TaskStreamEventBody>? Events { get; init; }

    /// <summary>The final result.</summary>
    [JsonPropertyName("result")]
    public TaskResult? Result { get; init; }
}