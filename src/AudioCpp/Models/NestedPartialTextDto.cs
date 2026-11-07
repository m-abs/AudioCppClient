using System.Text.Json.Serialization;

namespace Mabs.AudioCpp.Models;

/// <summary>The nested <c>partial_text</c> object shape.</summary>
internal sealed record NestedPartialTextDto
{
    /// <summary>Text produced since the previous event.</summary>
    [JsonPropertyName("text")]
    public string? Text { get; init; }

    /// <summary>Language of the partial text.</summary>
    [JsonPropertyName("language")]
    public string? Language { get; init; }
}