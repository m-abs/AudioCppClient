using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mabs.AudioCpp.Models;

/// <summary>A model entry from <c>GET /v1/models</c>.</summary>
public sealed record AudioCppModel
{
    /// <summary>Model id; use it as the <c>model</c> field of requests.</summary>
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    /// <summary>OpenAI-style object name, always <c>model</c>.</summary>
    [JsonPropertyName("object")]
    public string? Object { get; init; }

    /// <summary><see langword="true"/> for a config-defined id; <see langword="false"/> for one loaded through <c>POST /v1/models/load</c>.</summary>
    [JsonPropertyName("configured")]
    public bool Configured { get; init; }

    /// <summary>Effective backend device index.</summary>
    [JsonPropertyName("device")]
    public int? Device { get; init; }

    /// <summary><see langword="true"/> when the model is currently loaded in memory.</summary>
    [JsonPropertyName("loaded")]
    public bool Loaded { get; init; }

    /// <summary>Model family, e.g. <c>kokoro</c> or <c>parakeet</c>.</summary>
    [JsonPropertyName("family")]
    public string? Family { get; init; }

    /// <summary>Path the server resolves the model from; the registry always reports <c>path</c>, even for package-configured models.</summary>
    [JsonPropertyName("path")]
    public string? Path { get; init; }

    /// <summary>Task the model serves, e.g. <c>tts</c> or <c>asr</c>.</summary>
    [JsonPropertyName("task")]
    public string? Task { get; init; }

    /// <summary>Configured session options, when requested with <c>include_session_options=true</c>.</summary>
    [JsonPropertyName("session_options")]
    public IReadOnlyDictionary<string, string>? SessionOptions { get; init; }

    /// <summary>Declared request options, when requested with <c>include_params=true</c>.</summary>
    [JsonPropertyName("params")]
    public IReadOnlyList<AudioCppModelParam>? Params { get; init; }
}