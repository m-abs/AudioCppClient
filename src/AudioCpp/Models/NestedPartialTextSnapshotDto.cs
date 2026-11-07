using System.Text.Json.Serialization;

namespace Mabs.AudioCpp.Models;

/// <summary>The nested <c>partial_text_snapshot</c> object shape.</summary>
internal sealed record NestedPartialTextSnapshotDto
{
    /// <summary>Whole provisional text to replace, not append.</summary>
    [JsonPropertyName("text")]
    public string? Text { get; init; }
}