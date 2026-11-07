using System;
using System.Collections.Generic;
using System.Text.Json;

using Mabs.AudioCpp.Models;

namespace Mabs.AudioCpp.Http;

/// <summary>Shared JSON response parsers.</summary>
internal static class SseParsers
{
    /// <summary>Parses a cached response body and builds a result from its object root.</summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="json">The raw JSON response body.</param>
    /// <param name="parse">Parser applied to the object root.</param>
    /// <returns>The parsed result.</returns>
    /// <exception cref="JsonException">The body is null, invalid JSON, or not a JSON object.</exception>
    internal static T ParseRoot<T>(string? json, Func<JsonElement, T> parse)
    {
        if (json == null)
        {
            throw new JsonException("The cached JSON body is null.");
        }

        using JsonDocument doc = JsonDocument.Parse(json);
        if (doc.RootElement.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException("The cached JSON body is not a JSON object.");
        }

        return parse(doc.RootElement);
    }

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

    /// <summary>Parses the alignment response root.</summary>
    /// <param name="root">The JSON response root.</param>
    /// <returns>The parsed alignment result.</returns>
    internal static AlignmentResult Alignment(JsonElement root)
    {
        return new AlignmentResult
        {
            Text = SseReader.ReadString(root, "text"),
            Language = SseReader.ReadString(root, "language"),
            Words = SseReader.ReadWords(root, "words") ?? (IReadOnlyList<AudioCppWord>)Array.Empty<AudioCppWord>(),
            Timing = SseReader.ReadTiming(root, "timing"),
        };
    }

    /// <summary>Parses the details response root.</summary>
    /// <param name="root">The JSON response root.</param>
    /// <returns>The parsed details result; detail arrays are <see langword="null"/> when the model produced none.</returns>
    internal static TranscriptionDetailsResult Details(JsonElement root)
    {
        return new TranscriptionDetailsResult
        {
            Text = SseReader.ReadString(root, "text"),
            Language = SseReader.ReadString(root, "language"),
            Segments = SseReader.ReadSegments(root, "segments"),
            SpeakerTurns = SseReader.ReadSpeakerTurns(root, "speaker_turns"),
            Words = SseReader.ReadWords(root, "words"),
            SampleRate = SseReader.ReadInt(root, "sample_rate"),
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

        List<string> values = [];
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