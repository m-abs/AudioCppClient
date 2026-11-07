using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace Mabs.AudioCpp.Models;

/// <summary>
/// Input for the transcription routes: either a server-local WAV path
/// (<see cref="AudioPath"/>, sent as JSON) or uploaded audio bytes
/// (<see cref="AudioFile"/> + <see cref="AudioFileName"/>, sent as multipart).
/// Exactly one form must be set.
/// </summary>
public class TranscriptionInputBase
{
    /// <summary>Server-local WAV path (JSON body form).</summary>
    public string? AudioPath { get; set; }

    /// <summary>Audio bytes to upload (multipart form). The client disposes this stream when the request completes.</summary>
    public Stream? AudioFile { get; set; }

    /// <summary>Filename reported for <see cref="AudioFile"/>; must name a WAV for most builds.</summary>
    public string? AudioFileName { get; set; }

    /// <summary>Configured model id. Required.</summary>
    public string? Model { get; set; }

    /// <summary>Language hint.</summary>
    public string? Language { get; set; }

    /// <summary>Upper bound on waiting for a busy model, in milliseconds.</summary>
    public long? BusyTimeoutMs { get; set; }

    /// <summary>Model-family-specific options (JSON form only; the multipart routes do not read an options map for single transcriptions).</summary>
    public Dictionary<string, string>? Options { get; set; }

    /// <summary>Validates that exactly one audio form is set and a model id is present.</summary>
    public void Validate()
    {
        if (string.IsNullOrEmpty(Model))
        {
            throw new ArgumentException("transcription requires a model id", nameof(Model));
        }

        if (!string.IsNullOrEmpty(AudioPath) && AudioFile != null)
        {
            throw new ArgumentException("provide either AudioPath (server-local) or AudioFile (upload), not both");
        }

        if (string.IsNullOrEmpty(AudioPath) && AudioFile == null)
        {
            throw new ArgumentException("transcription requires AudioPath or AudioFile");
        }

        if (AudioFile != null && string.IsNullOrEmpty(AudioFileName))
        {
            throw new ArgumentException("AudioFileName is required when AudioFile is set", nameof(AudioFileName));
        }
    }

    /// <summary>Whether this request travels as a multipart upload (<see cref="AudioFile"/> form) rather than a JSON body.</summary>
    internal bool IsMultipart => AudioFile != null;
}

/// <summary>Request of <c>POST /v1/audio/transcriptions</c>.</summary>
public sealed class TranscriptionRequest : TranscriptionInputBase
{
    /// <summary>Recognition-context prompt (hotwords, spellings).</summary>
    public string? Prompt { get; set; }

    /// <summary>When <see langword="true"/>, the JSON form requests transcript SSE (<c>stream=true</c>); multipart uploads set the <c>stream</c> field instead.</summary>
    public bool? Stream { get; set; }
}

/// <summary>Request of <c>POST /v1/audio/transcriptions/details</c>. Streaming is rejected by the server on this route.</summary>
public sealed class TranscriptionDetailsRequest : TranscriptionInputBase
{
    /// <summary>Recognition-context prompt.</summary>
    public string? Prompt { get; set; }
}

/// <summary>Non-streaming transcription response (<c>{text, timing}</c>).</summary>
public sealed class TranscriptionResult
{
    /// <summary>Recognized text.</summary>
    public string? Text { get; internal set; }

    /// <summary>Run timing.</summary>
    public AudioCppTiming? Timing { get; internal set; }

    /// <summary>Parses a cached response JSON (a disk cache hit rebuilds the result).</summary>
    /// <param name="json">The raw JSON response body.</param>
    /// <returns>The parsed result; throws <see cref="JsonException"/> on null, invalid JSON, or a non-object root.</returns>
    public static TranscriptionResult Parse(string? json)
    {
        return Http.SseParsers.ParseRoot(json, Http.SseParsers.Transcription);
    }
}

/// <summary>Rich transcription response with the detail arrays the model produced.</summary>
public sealed class TranscriptionDetailsResult
{
    /// <summary>Recognized text.</summary>
    public string? Text { get; internal set; }

    /// <summary>Detected or configured language.</summary>
    public string? Language { get; internal set; }

    /// <summary>Speech segments, when the model produced them.</summary>
    public IReadOnlyList<AudioCppSegment>? Segments { get; internal set; }

    /// <summary>Speaker turns, when the model diarized.</summary>
    public IReadOnlyList<AudioCppSpeakerTurn>? SpeakerTurns { get; internal set; }

    /// <summary>Word timestamps, when the model aligned words.</summary>
    public IReadOnlyList<AudioCppWord>? Words { get; internal set; }

    /// <summary>Sample rate the offsets are counted in; present alongside the arrays.</summary>
    public int? SampleRate { get; internal set; }

    /// <summary>Run timing.</summary>
    public AudioCppTiming? Timing { get; internal set; }

    /// <summary>Parses a cached response JSON (a disk cache hit rebuilds the result).</summary>
    /// <param name="json">The raw JSON response body.</param>
    /// <returns>The parsed result; throws <see cref="JsonException"/> on null, invalid JSON, or a non-object root.</returns>
    public static TranscriptionDetailsResult Parse(string? json)
    {
        return Http.SseParsers.ParseRoot(json, Http.SseParsers.Details);
    }
}

/// <summary>Kind of a transcription stream event.</summary>
public enum TranscriptEventKind
{
    /// <summary>Finalized text appended to the transcript.</summary>
    TextDelta,

    /// <summary>Whole provisional transcript to replace, not append (revisable models, e.g. Phonon-2).</summary>
    TextPartial,

    /// <summary>Full transcript; the final text event.</summary>
    TextDone,

    /// <summary>Newly emitted speaker turns (diarization models).</summary>
    DiarizationDelta,

    /// <summary>Complete speaker turns; do not append to the deltas.</summary>
    DiarizationDone,
}

/// <summary>A transcription stream event.</summary>
public readonly struct TranscriptEvent
{
    private TranscriptEvent(TranscriptEventKind kind, string? text, IReadOnlyList<AudioCppSpeakerTurn>? turns, int? sampleRate, AudioCppTiming? timing)
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
        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;
        string? type = Http.SseReader.ReadString(root, "type");
        switch (type)
        {
            case "transcript.text.delta":
                return new TranscriptEvent(TranscriptEventKind.TextDelta, Http.SseReader.ReadString(root, "delta") ?? string.Empty, null, null, null);
            case "transcript.text.partial":
                return new TranscriptEvent(TranscriptEventKind.TextPartial, Http.SseReader.ReadString(root, "text") ?? string.Empty, null, null, null);
            case "transcript.text.done":
                return new TranscriptEvent(TranscriptEventKind.TextDone, Http.SseReader.ReadString(root, "text") ?? string.Empty, null, null, Http.SseReader.ReadTiming(root, "timing"));
            case "diarization.delta":
                return new TranscriptEvent(TranscriptEventKind.DiarizationDelta, null, Http.SseReader.ReadSpeakerTurns(root, "speaker_turns"), Http.SseReader.ReadInt(root, "sample_rate"), null);
            case "diarization.done":
                return new TranscriptEvent(TranscriptEventKind.DiarizationDone, null, Http.SseReader.ReadSpeakerTurns(root, "speaker_turns"), Http.SseReader.ReadInt(root, "sample_rate"), Http.SseReader.ReadTiming(root, "timing"));
            default:
                throw new JsonException("unknown transcription stream event type: " + (type ?? "<none>"));
        }
    }
}

/// <summary>
/// Query parameters of <c>POST /v1/audio/transcriptions/live</c>. The body is raw
/// interleaved PCM sent chunked; there is no multipart form and no JSON.
/// </summary>
public sealed class LiveTranscriptionRequest
{
    /// <summary>Streaming model id. Required.</summary>
    public string? Model { get; set; }

    /// <summary>Sample rate of the PCM; 1000–384000. Server default 16000.</summary>
    public int? SampleRate { get; set; } = 16000;

    /// <summary>Interleaved channels; 1–16. Server default 1.</summary>
    public int? Channels { get; set; } = 1;

    /// <summary><c>s16le</c> (default) or <c>f32le</c>.</summary>
    public string? SampleFormat { get; set; }

    /// <summary>Language hint.</summary>
    public string? Language { get; set; }

    /// <summary>Recognition-context prompt (hotwords, spellings).</summary>
    public string? Prompt { get; set; }

    /// <summary>Upper bound on waiting for a busy model, in milliseconds.</summary>
    public long? BusyTimeoutMs { get; set; }

    /// <summary>Validates and builds the query string.</summary>
    /// <returns>The encoded query string with a leading <c>?</c>.</returns>
    public string BuildQueryString()
    {
        if (string.IsNullOrEmpty(Model))
        {
            throw new ArgumentException("live transcription requires a model id", nameof(Model));
        }

        if (SampleRate is < 1000 or > 384000)
        {
            throw new ArgumentException("sample_rate must be between 1000 and 384000");
        }

        if (Channels is < 1 or > 16)
        {
            throw new ArgumentException("channels must be between 1 and 16");
        }

        QueryBuilder query = new();
        query.Add("model", Model);
        query.Add("sample_rate", SampleRate?.ToString(CultureInfo.InvariantCulture));
        query.Add("channels", Channels?.ToString(CultureInfo.InvariantCulture));
        query.Add("sample_format", SampleFormat);
        query.Add("language", Language);
        query.Add("prompt", Prompt);
        query.Add("busy_timeout_ms", BusyTimeoutMs?.ToString(CultureInfo.InvariantCulture));
        return query.ToString();
    }
}

/// <summary>
/// Request of <c>POST /v1/batches/transcriptions</c>: multiple uploaded WAV files through
/// one native offline batch. The model must implement native batching.
/// </summary>
public sealed class BatchTranscriptionRequest
{
    /// <summary>Native-batching model id. Required.</summary>
    public string? Model { get; set; }

    /// <summary>(Filename, bytes) per file. At least one entry is required; upload order defines event <c>index</c>.</summary>
    public IList<AudioUpload> Files { get; } = [];

    /// <summary>Language applied to every file.</summary>
    public string? Language { get; set; }

    /// <summary>Recognition-context prompt applied to every file.</summary>
    public string? Prompt { get; set; }

    /// <summary>Busy wait bound applied to every file.</summary>
    public long? BusyTimeoutMs { get; set; }

    /// <summary>Model options applied to every file.</summary>
    public Dictionary<string, string>? Options { get; set; }

    /// <summary>Validates the request.</summary>
    public void Validate()
    {
        if (string.IsNullOrEmpty(Model))
        {
            throw new ArgumentException("batch transcription requires a model id", nameof(Model));
        }

        if (Files.Count == 0)
        {
            throw new ArgumentException("batch transcription requires at least one file");
        }

        foreach (AudioUpload file in Files)
        {
            if (string.IsNullOrEmpty(file.FileName) || file.Content == null)
            {
                throw new ArgumentException("every batch file needs a filename and content");
            }
        }
    }
}

/// <summary>One uploaded file (multipart part).</summary>
public sealed class AudioUpload
{
    /// <summary>Creates an upload part from a filename and its bytes.</summary>
    /// <param name="fileName">Filename reported in the multipart form.</param>
    /// <param name="content">File bytes.</param>
    public AudioUpload(string fileName, byte[] content)
    {
        FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        Content = content ?? throw new ArgumentNullException(nameof(content));
    }

    /// <summary>Reported filename.</summary>
    public string FileName { get; }

    /// <summary>File bytes.</summary>
    public byte[] Content { get; }
}

/// <summary>Kind of a batch transcription stream event.</summary>
public enum BatchTranscriptionEventKind
{
    /// <summary>One file's result, published as the model finishes it.</summary>
    Result,

    /// <summary>Aggregate batch timing; the terminal event.</summary>
    Done,
}

/// <summary>A batch transcription stream event.</summary>
public readonly struct BatchTranscriptionEvent
{
    private BatchTranscriptionEvent(BatchTranscriptionEventKind kind, int index, string? filename, string? text, string? language, IReadOnlyList<AudioCppSpeakerTurn>? turns, IReadOnlyList<AudioCppWord>? words, IReadOnlyList<AudioCppSegment>? segments, int? sampleRate, int? resultCount, AudioCppTiming? timing)
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
        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;
        string? type = Http.SseReader.ReadString(root, "type");
        switch (type)
        {
            case "batch.transcription.result":
                return new BatchTranscriptionEvent(
                    BatchTranscriptionEventKind.Result,
                    Http.SseReader.ReadInt(root, "index") ?? -1,
                    Http.SseReader.ReadString(root, "filename"),
                    Http.SseReader.ReadString(root, "text"),
                    Http.SseReader.ReadString(root, "language"),
                    Http.SseReader.ReadSpeakerTurns(root, "speaker_turns"),
                    Http.SseReader.ReadWords(root, "words"),
                    Http.SseReader.ReadSegments(root, "segments"),
                    Http.SseReader.ReadInt(root, "sample_rate"),
                    null,
                    null);
            case "batch.transcription.done":
                return new BatchTranscriptionEvent(
                    BatchTranscriptionEventKind.Done,
                    -1,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    Http.SseReader.ReadInt(root, "result_count"),
                    Http.SseReader.ReadTiming(root, "timing"));
            default:
                throw new JsonException("unknown batch transcription event type: " + (type ?? "<none>"));
        }
    }
}