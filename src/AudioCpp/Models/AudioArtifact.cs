using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mabs.AudioCpp.Models;

/// <summary>An opaque model artifact (speaker embedding, acoustic tokens, …) returned by tasks routes.</summary>
public sealed record AudioArtifact
{
    /// <summary>Artifact id; ids may repeat.</summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Artifact kind: one of <see cref="ArtifactKind"/> constants, <c>custom</c> when the server
    /// reports an unknown one.
    /// </summary>
    [JsonPropertyName("kind")]
    public string Kind { get; init; } = ArtifactKind.Custom;

    /// <summary>Decoded payload bytes.</summary>
    [JsonPropertyName("payload")]
    public byte[] Payload { get; init; } = [];

    /// <summary>String metadata attached to the artifact, when present.</summary>
    [JsonPropertyName("meta")]
    public IReadOnlyDictionary<string, string>? Meta { get; init; }
}