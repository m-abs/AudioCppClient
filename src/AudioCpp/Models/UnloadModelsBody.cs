using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mabs.AudioCpp.Models;

/// <summary>Body DTO of <c>POST /v1/tasks/unload_models</c>: <c>{"model_ids":[...]}</c>.</summary>
internal sealed record UnloadModelsBody
{
    /// <summary>Model ids to unload.</summary>
    [JsonPropertyName("model_ids")]
    public IEnumerable<string>? ModelIds { get; set; }
}