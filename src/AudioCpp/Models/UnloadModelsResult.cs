using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mabs.AudioCpp.Models;

/// <summary>Result of <c>POST /v1/tasks/unload_models</c> and <c>/v1/tasks/unload_all_models</c>.</summary>
public sealed record UnloadModelsResult
{
    /// <summary>Ids that were resident and are now unloaded.</summary>
    [JsonPropertyName("unloaded")]
    public IReadOnlyList<string> Unloaded { get; init; } = Array.Empty<string>();

    /// <summary>Ids the server does not know (only reported by the selective endpoint).</summary>
    [JsonPropertyName("not_found")]
    public IReadOnlyList<string> NotFound { get; init; } = Array.Empty<string>();
}