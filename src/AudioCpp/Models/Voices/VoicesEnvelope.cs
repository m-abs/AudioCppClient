using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mabs.AudioCpp.Models.Voices;

/// <summary>Wire envelope of <c>GET /v1/audio/voices</c>: <c>{"voices":[...]}</c>.</summary>
internal sealed record VoicesEnvelope
{
    /// <summary>Discoverable voice names.</summary>
    [JsonPropertyName("voices")]
    public IReadOnlyList<string>? Voices { get; init; }
}