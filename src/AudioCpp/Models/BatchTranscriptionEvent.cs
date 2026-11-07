using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

using Mabs.AudioCpp.Utils;

namespace Mabs.AudioCpp.Models;

/// <summary>A batch transcription stream event.</summary>
[JsonConverter(typeof(BatchTranscriptionEventConverter))]
public readonly struct BatchTranscriptionEvent
{
    /// <summary>Stores the parsed event fields.</summary>
    /// <param name="kind">The event kind.</param>
    /// <param name="index">Original request index for result events.</param>
    /// <param name="filename">Uploaded filename for result events.</param>
    /// <param name="text">Transcript text for result events.</param>
    /// <param name="language">Detected language for result events.</param>
    /// <param name="turns">Speaker turns for result events.</param>
    /// <param name="words">Word timestamps for result events.</param>
    /// <param name="segments">Segments for result events.</param>
    /// <param name="sampleRate">Sample rate reported for the entry.</param>
    /// <param name="resultCount">Number of results on the done event.</param>
    /// <param name="timing">Aggregate timing on the done event.</param>
    internal BatchTranscriptionEvent(BatchTranscriptionEventKind kind, int index, string? filename, string? text, string? language, IReadOnlyList<AudioCppSpeakerTurn>? turns, IReadOnlyList<AudioCppWord>? words, IReadOnlyList<AudioCppSegment>? segments, int? sampleRate, int? resultCount, AudioCppTiming? timing)
    {
        Kind = kind;
        Index = index;
        Filename = filename;
        Text = text;
        Language = language;
        SpeakerTurns = turns;
        Words = words;
        Segments = segments;
        SampleRate = sampleRate;
        ResultCount = resultCount;
        Timing = timing;
    }

    /// <summary>Which event this is.</summary>
    public BatchTranscriptionEventKind Kind { get; }

    /// <summary>Upload position this result maps back to.</summary>
    public int Index { get; }

    /// <summary>Sanitized upload filename.</summary>
    public string? Filename { get; }

    /// <summary>Transcript for this file.</summary>
    public string? Text { get; }

    /// <summary>Language, when the model reported one.</summary>
    public string? Language { get; }

    /// <summary>Speaker turns, for diarization models.</summary>
    public IReadOnlyList<AudioCppSpeakerTurn>? SpeakerTurns { get; }

    /// <summary>Word timestamps, when produced.</summary>
    public IReadOnlyList<AudioCppWord>? Words { get; }

    /// <summary>Segments, when produced.</summary>
    public IReadOnlyList<AudioCppSegment>? Segments { get; }

    /// <summary>Sample rate for the detail arrays.</summary>
    public int? SampleRate { get; }

    /// <summary>Number of results (Done event).</summary>
    public int? ResultCount { get; }

    /// <summary>Aggregate timing (Done event).</summary>
    public AudioCppTiming? Timing { get; }

    /// <summary>Parses one SSE data payload into a batch event.</summary>
    /// <param name="json">The event's JSON data payload.</param>
    /// <returns>The parsed event.</returns>
    /// <exception cref="JsonException">The payload carries an unknown event type.</exception>
    internal static BatchTranscriptionEvent Parse(string json)
    {
        return Json.Deserialize<BatchTranscriptionEvent>(json);
    }
}