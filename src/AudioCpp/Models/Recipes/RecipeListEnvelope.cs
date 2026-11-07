using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mabs.AudioCpp.Models.Recipes;

/// <summary>Wire envelope of <c>GET /v1/recipes</c>: <c>{"object":"list","data":[...]}</c>.</summary>
internal sealed record RecipeListEnvelope
{
    /// <summary>Envelope object name, <c>list</c>.</summary>
    [JsonPropertyName("object")]
    public string? Object { get; init; }

    /// <summary>Recipe entries.</summary>
    [JsonPropertyName("data")]
    public IReadOnlyList<RecipeEntry>? Data { get; init; }
}