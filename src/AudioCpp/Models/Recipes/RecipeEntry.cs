using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mabs.AudioCpp.Models.Recipes;

/// <summary>A recipe entry from <c>GET /v1/recipes</c>.</summary>
public sealed record RecipeEntry
{
    /// <summary>Recipe id; use it as <see cref="RecipeRunRequest.Recipe"/>.</summary>
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    /// <summary>Human-readable description.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>Model roles the recipe requires.</summary>
    [JsonPropertyName("roles")]
    public IReadOnlyList<RecipeRole> Roles { get; init; } = Array.Empty<RecipeRole>();
}