using System.Text.Json.Serialization;

namespace Mabs.AudioCpp.Models;

/// <summary>Result of <c>POST /v1/models/load</c>.</summary>
public sealed record ModelLoadResult
{
    /// <summary>Loaded model id.</summary>
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    /// <summary>Whether the model was loaded by this call (as opposed to already resident and unchanged).</summary>
    [JsonPropertyName("loaded")]
    public bool Loaded { get; init; }

    /// <summary>Whether an already-registered model was reconfigured by this call.</summary>
    [JsonPropertyName("reconfigured")]
    public bool Reconfigured { get; init; }
}