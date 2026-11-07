using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mabs.AudioCpp.Models.Tasks;

/// <summary>An artifact sent in a task request (<c>request.artifacts[]</c>).</summary>
public sealed record AudioArtifactInput
{
    /// <summary>Non-empty artifact id; ids may repeat.</summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>One of the <see cref="ArtifactKind"/> constants.</summary>
    [JsonPropertyName("kind")]
    public string? Kind { get; set; } = ArtifactKind.Custom;

    /// <summary>Payload bytes, Base64-encoded on the wire. Mutually exclusive with <see cref="Path"/>.</summary>
    [JsonPropertyName("payload")]
    public byte[]? Payload { get; set; }

    /// <summary>Server-local file whose bytes are the payload. Mutually exclusive with <see cref="Path"/>.</summary>
    [JsonPropertyName("path")]
    public string? Path { get; set; }

    /// <summary>Optional string metadata (numbers/bools become text like the option map).</summary>
    [JsonPropertyName("meta")]
    public Dictionary<string, string>? Meta { get; set; }

    /// <summary>Validates the artifact (id, kind membership, payload XOR path).</summary>
    /// <exception cref="ArgumentException">The artifact lacks id/kind/payload-or-path, or the kind is unknown.</exception>
    public void Validate()
    {
        if (string.IsNullOrEmpty(Id))
        {
            throw new ArgumentException("artifact requires a non-empty id");
        }

        if (string.IsNullOrEmpty(Kind))
        {
            throw new ArgumentException("artifact requires a kind");
        }

        ArtifactKind.Validate(Kind!);
        if (Payload != null && Path != null)
        {
            throw new ArgumentException("artifact payload and path are mutually exclusive; provide only one");
        }

        if (Payload == null && Path == null)
        {
            throw new ArgumentException("artifact requires payload or path");
        }
    }
}