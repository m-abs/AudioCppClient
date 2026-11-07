using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Mabs.AudioCpp.Models;

/// <summary>Body of <c>POST /v1/recipes/run</c>.</summary>
public sealed record RecipeRunRequest
{
    /// <summary>Recipe id (from <c>GET /v1/recipes</c>). Required.</summary>
    [JsonPropertyName("recipe")]
    public string? Recipe { get; set; }

    /// <summary>When <see langword="true"/>, the response is the tasks-stream SSE envelope instead of one JSON result.</summary>
    [JsonPropertyName("stream")]
    public bool Stream { get; set; }

    /// <summary>Include streamed artifacts in the SSE response (accepted; honored by builds with artifact streaming).</summary>
    [JsonPropertyName("include_artifacts")]
    public bool IncludeArtifacts { get; set; }

    /// <summary>Busy-wait bound for the run, in milliseconds.</summary>
    [JsonPropertyName("busy_timeout_ms")]
    public long? BusyTimeoutMs { get; set; }

    /// <summary>Role name → configured model id. Every role the recipe requires must be present.</summary>
    [JsonPropertyName("models")]
    public IDictionary<string, string> Models { get; } = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>The shared request forwarded to the recipe stages.</summary>
    [JsonPropertyName("request")]
    public TaskRequestBody Request { get; set; } = new TaskRequestBody();

    /// <summary>Validates the recipe id, the models map, and the inner request.</summary>
    /// <exception cref="ArgumentException">The recipe id is missing or the models map is empty.</exception>
    public void Validate()
    {
        if (string.IsNullOrEmpty(Recipe))
        {
            throw new ArgumentException("recipe run requires a recipe id", nameof(Recipe));
        }

        if (Models.Count == 0)
        {
            throw new ArgumentException("recipe run requires a models map with an entry per role");
        }

        Request.Validate();
    }
}