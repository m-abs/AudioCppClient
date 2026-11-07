using System.Text.Json.Serialization;

namespace Mabs.AudioCpp.Models;

/// <summary>Server readiness from <c>GET /health</c>.</summary>
public sealed record AudioCppHealth
{
    /// <summary>Status; <c>ok</c> when the server is up.</summary>
    [JsonPropertyName("status")]
    public string? Status { get; init; }

    /// <summary>Active backend: <c>cpu</c>, <c>cuda</c>, <c>hip</c>, ….</summary>
    [JsonPropertyName("backend")]
    public string? Backend { get; init; }

    /// <summary>Number of configured models.</summary>
    [JsonPropertyName("models")]
    public int Models { get; init; }

    /// <summary>Whether the embedded WebUI is served.</summary>
    [JsonPropertyName("ui")]
    public bool Ui { get; init; }

    /// <summary>Whether WebUI model management is enabled.</summary>
    [JsonPropertyName("ui_management")]
    public bool UiManagement { get; init; }

    /// <summary>Build version, <c>dev</c> unless the binary was stamped.</summary>
    [JsonPropertyName("version")]
    public string? Version { get; init; }

    /// <summary>Short git commit of the build.</summary>
    [JsonPropertyName("git")]
    public string? Git { get; init; }

    /// <summary>Commit date of the build.</summary>
    [JsonPropertyName("git_date")]
    public string? GitDate { get; init; }
}