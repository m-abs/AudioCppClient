using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mabs.AudioCpp.Models;

/// <summary>Body of <c>POST /v1/models/load</c> (builds with native model management).</summary>
public sealed record ModelLoadRequest
{
    /// <summary>Id to register the model under. Required.</summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>Server-local path to the model file or directory. Mutually exclusive with <see cref="PackageId"/>.</summary>
    [JsonPropertyName("path")]
    public string? Path { get; set; }

    /// <summary>Catalog package id to resolve. Mutually exclusive with <see cref="Path"/>.</summary>
    [JsonPropertyName("package_id")]
    public string? PackageId { get; set; }

    /// <summary>Model family, for example <c>kokoro_tts</c>.</summary>
    [JsonPropertyName("family")]
    public string? Family { get; set; }

    /// <summary>Task, for example <c>tts</c> or <c>asr</c>.</summary>
    [JsonPropertyName("task")]
    public string? Task { get; set; }

    /// <summary>Run mode: <c>offline</c> or <c>streaming</c>.</summary>
    [JsonPropertyName("mode")]
    public string? Mode { get; set; }

    /// <summary>Backend device index override.</summary>
    [JsonPropertyName("device")]
    public int? Device { get; set; }

    /// <summary>Loader options (stringly-typed like other option maps).</summary>
    [JsonPropertyName("load_options")]
    public Dictionary<string, string>? LoadOptions { get; set; }

    /// <summary>Session options (stringly-typed like other option maps).</summary>
    [JsonPropertyName("session_options")]
    public Dictionary<string, string>? SessionOptions { get; set; }

    /// <summary>Validates the request, throwing when <see cref="Path"/> and <see cref="PackageId"/> are both set or neither is.</summary>
    public void Validate()
    {
        if (string.IsNullOrEmpty(Id))
        {
            throw new ArgumentException("model load requires an id", nameof(Id));
        }

        if (!string.IsNullOrEmpty(Path) && !string.IsNullOrEmpty(PackageId))
        {
            throw new ArgumentException("path and package_id are mutually exclusive; provide only one");
        }

        if (string.IsNullOrEmpty(Path) && string.IsNullOrEmpty(PackageId))
        {
            throw new ArgumentException("model load requires either path or package_id");
        }
    }
}