using System;
using System.Collections.Generic;
using System.Text.Json;

using Mabs.AudioCpp.Models;

namespace Mabs.AudioCpp.Http;

/// <summary>Shared JSON response parsers.</summary>
internal static class SseParsers
{
    /// <summary>Parses the unload response (<c>unloaded</c> and <c>not_found</c> id lists).</summary>
    /// <param name="root">The JSON response root.</param>
    /// <returns>The unload result; both lists are empty when absent.</returns>
    internal static UnloadModelsResult UnloadResult(JsonElement root)
    {
        return new UnloadModelsResult
        {
            Unloaded = ReadStringArray(root, "unloaded"),
            NotFound = ReadStringArray(root, "not_found"),
        };
    }

    /// <summary>Parses the <c>POST /v1/models/load</c> response.</summary>
    /// <param name="root">The JSON response root.</param>
    /// <returns>The load result.</returns>
    internal static ModelLoadResult ModelLoadResult(JsonElement root)
    {
        return new ModelLoadResult
        {
            Id = SseReader.ReadString(root, "id"),
            Loaded = SseReader.ReadBool(root, "loaded") ?? false,
            Reconfigured = SseReader.ReadBool(root, "reconfigured") ?? false,
        };
    }

    /// <summary>Parses the non-streaming transcription response (<c>text</c> and <c>timing</c>).</summary>
    /// <param name="root">The JSON response root.</param>
    /// <returns>The transcription result.</returns>
    internal static TranscriptionResult Transcription(JsonElement root)
    {
        return new TranscriptionResult
        {
            Text = SseReader.ReadString(root, "text"),
            Timing = SseReader.ReadTiming(root, "timing"),
        };
    }

    /// <summary>Parses a task result envelope.</summary>
    /// <param name="root">The JSON result object.</param>
    /// <returns>The task result.</returns>
    internal static TaskResult TaskResult(JsonElement root)
    {
        return SseReader.ReadTaskResult(root);
    }

    /// <summary>Parses the <c>response_format=json</c> speech envelope (<c>audio</c>, <c>format</c>, <c>timing</c>).</summary>
    /// <param name="root">The JSON response root.</param>
    /// <returns>The parsed speech JSON result.</returns>
    internal static SpeechJsonResult SpeechJson(JsonElement root)
    {
        return new SpeechJsonResult
        {
            AudioBase64 = SseReader.ReadString(root, "audio") ?? string.Empty,
            Format = SseReader.ReadString(root, "format"),
            Timing = SseReader.ReadTiming(root, "timing"),
        };
    }

    /// <summary>Reads a string array property as a list.</summary>
    /// <param name="root">The JSON object to read from.</param>
    /// <param name="property">The property name.</param>
    /// <returns>The strings found; empty when the property is absent or not an array.</returns>
    internal static IReadOnlyList<string> ReadStringArray(JsonElement root, string property)
    {
        if (!root.TryGetProperty(property, out JsonElement array) ||
            array.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<string>();
        }

        List<string> values = new();
        foreach (JsonElement element in array.EnumerateArray())
        {
            if (element.ValueKind == JsonValueKind.String)
            {
                values.Add(element.GetString()!);
            }
        }

        return values;
    }
}