using System;
using System.Collections.Generic;
using System.Text.Json;

using mabs.AudioCpp.Models;

namespace mabs.AudioCpp.Http;

/// <summary>Shared JSON response parsers.</summary>
internal static class SseParsers
{
    internal static UnloadModelsResult UnloadResult(JsonElement root)
    {
        return new UnloadModelsResult
        {
            Unloaded = ReadStringArray(root, "unloaded"),
            NotFound = ReadStringArray(root, "not_found"),
        };
    }

    internal static ModelLoadResult ModelLoadResult(JsonElement root)
    {
        return new ModelLoadResult
        {
            Id = SseReader.ReadString(root, "id"),
            Loaded = SseReader.ReadBool(root, "loaded") ?? false,
            Reconfigured = SseReader.ReadBool(root, "reconfigured") ?? false,
        };
    }

    internal static TranscriptionResult Transcription(JsonElement root)
    {
        return new TranscriptionResult
        {
            Text = SseReader.ReadString(root, "text"),
            Timing = SseReader.ReadTiming(root, "timing"),
        };
    }

    internal static TaskResult TaskResult(JsonElement root)
    {
        return SseReader.ReadTaskResult(root);
    }

    internal static SpeechJsonResult SpeechJson(JsonElement root)
    {
        return new SpeechJsonResult
        {
            AudioBase64 = SseReader.ReadString(root, "audio") ?? string.Empty,
            Format = SseReader.ReadString(root, "format"),
            Timing = SseReader.ReadTiming(root, "timing"),
        };
    }

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