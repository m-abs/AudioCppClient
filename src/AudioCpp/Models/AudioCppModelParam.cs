using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mabs.AudioCpp.Models;

/// <summary>A request option declared by a model spec, from <c>GET /v1/models?include_params=true</c>.</summary>
public sealed record AudioCppModelParam
{
    /// <summary>Option name; pass it as a key of an <c>options</c> map.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>Declared type, for example <c>string</c>, <c>int</c>, <c>float</c>, <c>bool</c>, <c>enum</c>.</summary>
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    /// <summary>Whether the model requires the option.</summary>
    [JsonPropertyName("required")]
    public bool Required { get; init; }

    /// <summary>Human-readable description.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>Declared default value, as raw JSON (string/number/bool), when given.</summary>
    [JsonPropertyName("default")]
    public JsonElement? Default { get; init; }

    /// <summary>Declared minimum, as raw JSON, when given.</summary>
    [JsonPropertyName("min")]
    public JsonElement? Min { get; init; }

    /// <summary>Declared maximum, as raw JSON, when given.</summary>
    [JsonPropertyName("max")]
    public JsonElement? Max { get; init; }

    /// <summary>Allowed values for an enum row that names a preset.</summary>
    [JsonPropertyName("values")]
    public IReadOnlyList<string>? Values { get; init; }
}