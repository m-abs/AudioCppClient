using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

using Mabs.AudioCpp.Utils;
using Mabs.AudioCpp.Utils.Converters;

namespace Mabs.AudioCpp.Models.Transcription;

/// <summary>A transcription stream event.</summary>
[JsonConverter(typeof(TranscriptEventConverter))]
public readonly struct TranscriptEvent
{
    /// <summary>Stores the parsed event fields.</summary>
    /// <param name="kind">The event kind.</param>
    /// <param name="text">Provisional or final text.</param>
    /// <param name="turns">Speaker turns for diarization events.</param>
    /// <param name="sampleRate">Sample rate the turn offsets use.</param>
    /// <param name="timing">Timing on done events.</param>
    internal TranscriptEvent(TranscriptEventKind kind, string? text, IReadOnlyList<AudioCppSpeakerTurn>? turns, int? sampleRate, AudioCppTiming? timing)
    {
        Kind = kind;
        Text = text;
        SpeakerTurns = turns;
        SampleRate = sampleRate;
        Timing = timing;
    }

    /// <summary>Which event this is.</summary>
    public TranscriptEventKind Kind { get; }

    /// <summary>Delta, provisional, or final text.</summary>
    public string? Text { get; }

    /// <summary>Speaker turns for diarization events.</summary>
    public IReadOnlyList<AudioCppSpeakerTurn>? SpeakerTurns { get; }

    /// <summary>Sample rate the turn offsets use.</summary>
    public int? SampleRate { get; }

    /// <summary>Timing on <see cref="TranscriptEventKind.TextDone"/> / <see cref="TranscriptEventKind.DiarizationDone"/>.</summary>
    public AudioCppTiming? Timing { get; }

    /// <summary>Parses one SSE data payload into a transcript event.</summary>
    /// <param name="json">The event's JSON data payload.</param>
    /// <returns>The parsed event.</returns>
    /// <exception cref="JsonException">The payload carries an unknown event type.</exception>
    internal static TranscriptEvent Parse(string json)
    {
        return Json.Deserialize<TranscriptEvent>(json);
    }
}