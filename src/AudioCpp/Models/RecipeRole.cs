using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

using Mabs.AudioCpp.Utils;

namespace Mabs.AudioCpp.Models;

/// <summary>One required role of a recipe.</summary>
public sealed record RecipeRole
{
    /// <summary>Role name; a key of <see cref="RecipeRunRequest.Models"/>.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>Task the role's model must implement.</summary>
    [JsonPropertyName("task")]
    public string? Task { get; init; }

    /// <summary>Mode the role's model must be configured with.</summary>
    [JsonPropertyName("mode")]
    public string? Mode { get; init; }
}