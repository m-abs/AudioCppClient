using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mabs.AudioCpp.Models;

/// <summary>Wire envelope of <c>GET /v1/models</c>: <c>{"object":"list","data":[...]}</c>.</summary>
internal sealed record ModelListEnvelope
{
    /// <summary>Envelope object name, <c>list</c>.</summary>
    [JsonPropertyName("object")]
    public string? Object { get; init; }

    /// <summary>Model entries.</summary>
    [JsonPropertyName("data")]
    public IReadOnlyList<AudioCppModel>? Data { get; init; }
}