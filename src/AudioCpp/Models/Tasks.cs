using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace Mabs.AudioCpp.Models;

/// <summary>
/// The <c>kind</c> of an artifact. Wire values accepted by the server.
/// </summary>
public static class ArtifactKind
{
    /// <summary>Speaker embedding bytes.</summary>
    public const string SpeakerEmbedding = "speaker_embedding";

    /// <summary>Style embedding bytes.</summary>
    public const string StyleEmbedding = "style_embedding";

    /// <summary>Prompt embedding bytes.</summary>
    public const string PromptEmbedding = "prompt_embedding";

    /// <summary>Acoustic tokens.</summary>
    public const string AcousticTokens = "acoustic_tokens";

    /// <summary>MIDI data.</summary>
    public const string Midi = "midi";

    /// <summary>Transcript alignment state.</summary>
    public const string TranscriptAlignment = "transcript_alignment";

    /// <summary>Diarization continuation state.</summary>
    public const string DiarizationState = "diarization_state";

    /// <summary>VAD continuation state.</summary>
    public const string VadState = "vad_state";

    /// <summary>Application-defined payload.</summary>
    public const string Custom = "custom";

    /// <summary>The kinds the server accepts on request artifacts.</summary>
    public static readonly IReadOnlyList<string> All = new[]
    {
        SpeakerEmbedding, StyleEmbedding, PromptEmbedding, AcousticTokens, Midi,
        TranscriptAlignment, DiarizationState, VadState, Custom,
    };

    /// <summary>Throws when <paramref name="kind"/> is not a server-accepted value.</summary>
    /// <param name="kind">The artifact kind to check.</param>
    /// <exception cref="ArgumentException">The kind is not in <see cref="All"/>.</exception>
    public static void Validate(string kind)
    {
        if (!All.Contains(kind))
        {
            throw new ArgumentException(
                "unknown artifact kind '" + kind + "'; expected one of " + string.Join(", ", All),
                nameof(kind));
        }
    }
}

/// <summary>An artifact sent in a task request (<c>request.artifacts[]</c>).</summary>
public sealed class AudioArtifactInput
{
    /// <summary>Non-empty artifact id; ids may repeat.</summary>
    public string? Id { get; set; }

    /// <summary>One of the <see cref="ArtifactKind"/> constants.</summary>
    public string? Kind { get; set; } = ArtifactKind.Custom;

    /// <summary>Payload bytes, Base64-encoded on the wire. Mutually exclusive with <see cref="Path"/>.</summary>
    public byte[]? Payload { get; set; }

    /// <summary>Server-local file whose bytes are the payload. Mutually exclusive with <see cref="Payload"/>.</summary>
    public string? Path { get; set; }

    /// <summary>Optional string metadata (numbers/bools become text like the option map).</summary>
    public Dictionary<string, string>? Meta { get; set; }

    /// <summary>Validates the artifact and writes its JSON object.</summary>
    /// <param name="writer">The JSON writer.</param>
    /// <exception cref="ArgumentException">The artifact lacks id/kind/payload-or-path, or the kind is unknown.</exception>
    internal void WriteTo(Utf8JsonWriter writer)
    {
        if (string.IsNullOrEmpty(Id))
        {
            throw new ArgumentException("artifact requires a non-empty id");
        }

        if (string.IsNullOrEmpty(Kind))
        {
            throw new ArgumentException("artifact requires a kind");
        }

        ArtifactKind.Validate(Kind!);
        if (Payload != null && Path != null)
        {
            throw new ArgumentException("artifact payload and path are mutually exclusive; provide only one");
        }

        if (Payload == null && Path == null)
        {
            throw new ArgumentException("artifact requires payload or path");
        }

        writer.WriteStartObject();
        writer.WriteString("id", Id);
        writer.WriteString("kind", Kind);
        if (Payload != null)
        {
            writer.WriteString("payload", Convert.ToBase64String(Payload));
        }
        else
        {
            writer.WriteString("path", Path);
        }

        if (Meta != null && Meta.Count > 0)
        {
            writer.WriteStartObject("meta");
            foreach (KeyValuePair<string, string> pair in Meta)
            {
                writer.WriteString(pair.Key, pair.Value);
            }

            writer.WriteEndObject();
        }

        writer.WriteEndObject();
    }
}

/// <summary>
/// The <c>request</c> object accepted by <c>POST /v1/tasks/run</c>, <c>/v1/tasks/stream</c>
/// (as each entry of <c>/v1/tasks/batch</c>). It mirrors the audio.cpp CLI request-sequence
/// format: exactly one of <see cref="AudioPath"/> / <see cref="AudioBase64"/> for audio
/// input; family-specific parameters go in <see cref="Options"/> (values serialize the way
/// the server's option map does: bools <c>true</c>/<c>false</c>, integers without decimals,
/// doubles invariant).
/// </summary>
public sealed class TaskRequestBody
{
    /// <summary>Text input.</summary>
    public string? Text { get; set; }

    /// <summary>Language code.</summary>
    public string? Language { get; set; }

    /// <summary>Server-local WAV path. Mutually exclusive with <see cref="AudioBase64"/>.</summary>
    public string? AudioPath { get; set; }

    /// <summary>Inline WAV bytes, Base64-encoded on the wire. Mutually exclusive with <see cref="AudioPath"/>; ignored by <c>/v1/tasks/stream</c> per the server contract.</summary>
    public byte[]? AudioBase64 { get; set; }

    /// <summary>Cached/native voice id.</summary>
    public string? VoiceId { get; set; }

    /// <summary>Voice reference: server-local path string or <see cref="VoiceRefObject"/> (path form only in CLI-shaped requests; object form is accepted too).</summary>
    public object? VoiceRef { get; set; }

    /// <summary>Input artifacts, delivered to the model in array order.</summary>
    public IReadOnlyList<AudioArtifactInput>? Artifacts { get; set; }

    /// <summary>Family-specific parameters (the stringly-typed option map).</summary>
    public Dictionary<string, string>? Options { get; set; }

    /// <summary>Validates the one-of audio rule.</summary>
    public void Validate()
    {
        if (AudioPath != null && AudioBase64 != null)
        {
            throw new ArgumentException("provide only one of audio path and audio_base64");
        }
    }

    /// <summary>Writes the request object (validates first).</summary>
    /// <param name="writer">The JSON writer.</param>
    internal void WriteTo(Utf8JsonWriter writer)
    {
        Validate();
        writer.WriteStartObject();
        RequestJson.WriteStringIfSet(writer, "text", Text);
        RequestJson.WriteStringIfSet(writer, "language", Language);
        RequestJson.WriteStringIfSet(writer, "audio", AudioPath);
        if (AudioBase64 != null)
        {
            writer.WriteString("audio_base64", Convert.ToBase64String(AudioBase64));
        }

        RequestJson.WriteStringIfSet(writer, "voice_id", VoiceId);
        switch (VoiceRef)
        {
            case null:
                break;
            case string path:
                writer.WriteString("voice_ref", path);
                break;
            case VoiceRefObject obj:
                writer.WritePropertyName("voice_ref");
                obj.WriteTo(writer);
                break;
            default:
                throw new ArgumentException("VoiceRef must be a string path or a VoiceRefObject", nameof(VoiceRef));
        }

        if (Artifacts != null && Artifacts.Count > 0)
        {
            writer.WriteStartArray("artifacts");
            foreach (AudioArtifactInput artifact in Artifacts)
            {
                artifact.WriteTo(writer);
            }

            writer.WriteEndArray();
        }

        RequestJson.WriteOptions(writer, Options);
        writer.WriteEndObject();
    }
}

/// <summary>Body of <c>POST /v1/tasks/run</c>.</summary>
public sealed class TaskRunRequest
{
    /// <summary>Configured model id. Required.</summary>
    public string? Model { get; set; }

    /// <summary>The framework request. Required.</summary>
    public TaskRequestBody Request { get; set; } = new TaskRequestBody();

    /// <summary>Busy-wait bound for this run, in milliseconds.</summary>
    public long? BusyTimeoutMs { get; set; }

    /// <summary>Writes the run request object.</summary>
    /// <param name="writer">The JSON writer.</param>
    /// <exception cref="ArgumentException">The model id is missing.</exception>
    internal void WriteTo(Utf8JsonWriter writer)
    {
        if (string.IsNullOrEmpty(Model))
        {
            throw new ArgumentException("task run requires a model id", nameof(Model));
        }

        writer.WriteStartObject();
        writer.WriteString("model", Model);
        RequestJson.WriteNumberIfSet(writer, "busy_timeout_ms", BusyTimeoutMs);
        writer.WritePropertyName("request");
        Request.WriteTo(writer);
        writer.WriteEndObject();
    }
}

/// <summary>Body of <c>POST /v1/tasks/stream</c>.</summary>
public sealed class TaskStreamRequest
{
    /// <summary>Configured model id. Required.</summary>
    public string? Model { get; set; }

    /// <summary>The framework request. Audio must use <see cref="TaskRequestBody.AudioPath"/>: the stream route ignores <c>audio_base64</c>.</summary>
    public TaskRequestBody Request { get; set; } = new TaskRequestBody();

    /// <summary>
    /// When <c>sse</c>, the response is server-sent events written as the model produces
    /// them; when <see langword="null"/> the response is one JSON body (the client parses
    /// it and replays it as the same event sequence). Any other string is a server 400.
    /// </summary>
    public string? StreamFormat { get; set; } = Models.StreamFormat.Sse.Value;

    /// <summary>Ask the server to include streamed artifacts (forwarded; builds without artifact streaming ignore it).</summary>
    public bool? IncludeArtifacts { get; set; }

    /// <summary>Busy-wait bound for this run, in milliseconds.</summary>
    public long? BusyTimeoutMs { get; set; }

    /// <summary>Writes the stream request object.</summary>
    /// <param name="writer">The JSON writer.</param>
    /// <exception cref="ArgumentException">The model id is missing, or <see cref="StreamFormat"/> is neither <c>sse</c> nor null.</exception>
    internal void WriteTo(Utf8JsonWriter writer)
    {
        if (string.IsNullOrEmpty(Model))
        {
            throw new ArgumentException("task stream requires a model id", nameof(Model));
        }

        if (StreamFormat != null && StreamFormat != "sse")
        {
            throw new ArgumentException("task stream stream_format must be \"sse\" or null (JSON response)");
        }

        writer.WriteStartObject();
        writer.WriteString("model", Model);
        if (StreamFormat != null)
        {
            writer.WriteString("stream_format", StreamFormat);
        }

        if (IncludeArtifacts.HasValue)
        {
            writer.WriteBoolean("include_artifacts", IncludeArtifacts.Value);
        }

        RequestJson.WriteNumberIfSet(writer, "busy_timeout_ms", BusyTimeoutMs);
        writer.WritePropertyName("request");
        Request.WriteTo(writer);
        writer.WriteEndObject();
    }
}

/// <summary>Body of <c>POST /v1/tasks/batch</c>.</summary>
public sealed class TaskBatchRequest
{
    /// <summary>Native-batching model id. Required.</summary>
    public string? Model { get; set; }

    /// <summary>One entry per request; results map back by index. At least one entry is required.</summary>
    public IList<TaskRequestBody> Requests { get; } = [];

    /// <summary>Busy-wait bound for the batch, in milliseconds.</summary>
    public long? BusyTimeoutMs { get; set; }

    /// <summary>Writes the batch request object.</summary>
    /// <param name="writer">The JSON writer.</param>
    /// <exception cref="ArgumentException">The model id is missing or the requests array is empty.</exception>
    internal void WriteTo(Utf8JsonWriter writer)
    {
        if (string.IsNullOrEmpty(Model))
        {
            throw new ArgumentException("task batch requires a model id", nameof(Model));
        }

        if (Requests.Count == 0)
        {
            throw new ArgumentException("task batch requires a non-empty requests array");
        }

        writer.WriteStartObject();
        writer.WriteString("model", Model);
        RequestJson.WriteNumberIfSet(writer, "busy_timeout_ms", BusyTimeoutMs);
        writer.WriteStartArray("requests");
        foreach (TaskRequestBody request in Requests)
        {
            request.WriteTo(writer);
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
    }
}

/// <summary>One event of a task stream (both the JSON <c>events[]</c> rows and the SSE <c>task.stream.event</c> payload).</summary>
public sealed class TaskStreamEventBody
{
    /// <summary>Text produced since the previous event, with its language.</summary>
    public string? PartialText { get; internal set; }

    /// <summary>Whole provisional text to replace, not append.</summary>
    public string? PartialTextSnapshot { get; internal set; }

    /// <summary>Language of the partial text.</summary>
    public string? Language { get; internal set; }

    /// <summary>WAV bytes of this event's audio.</summary>
    public byte[]? Audio { get; internal set; }

    /// <summary>Named audio outputs of this event (WAV, <c>format=wav</c>).</summary>
    public IReadOnlyList<NamedAudioOutput>? NamedAudioOutputs { get; internal set; }

    /// <summary>Word timestamps attached to this event.</summary>
    public IReadOnlyList<AudioCppWord>? WordTimestamps { get; internal set; }

    /// <summary>Speaker turns (diarization runs).</summary>
    public IReadOnlyList<AudioCppSpeakerTurn>? SpeakerTurns { get; internal set; }

    /// <summary>Artifacts carried by this event, when the build streams them.</summary>
    public IReadOnlyList<AudioArtifact>? Artifacts { get; internal set; }

    /// <summary>Whether this event is the final one.</summary>
    public bool IsFinal { get; internal set; }

    /// <summary>Parses a plain event object (flat field names).</summary>
    /// <param name="element">The JSON event object.</param>
    /// <returns>The parsed event body.</returns>
    internal static TaskStreamEventBody Parse(JsonElement element)
    {
        return new TaskStreamEventBody
        {
            PartialText = Http.SseReader.ReadString(element, "text"),
            Language = Http.SseReader.ReadString(element, "language"),
            Audio = Http.SseReader.ReadBase64(element, "audio"),
            NamedAudioOutputs = Http.SseReader.ReadNamedAudioOutputs(element, "named_audio_outputs"),
            WordTimestamps = Http.SseReader.ReadWords(element, "word_timestamps"),
            SpeakerTurns = Http.SseReader.ReadSpeakerTurns(element, "speaker_turns"),
            Artifacts = Http.SseReader.ReadArtifacts(element, "artifacts"),
            IsFinal = Http.SseReader.ReadBool(element, "is_final") ?? false,
        };
    }

    /// <summary>
    /// Parses an event with the nested <c>partial_text</c>/<c>partial_text_snapshot</c>
    /// shapes: the SSE envelope (<c>{"type":"task.stream.event","event":{…}}</c>) or a JSON
    /// <c>events[]</c> row.
    /// </summary>
    /// <param name="element">The JSON envelope or event object.</param>
    /// <returns>The parsed event body.</returns>
    internal static TaskStreamEventBody ParseEnvelope(JsonElement element)
    {
        // SSE: {"type":"task.stream.event","event":{…}}
        if (element.TryGetProperty("event", out JsonElement inner) &&
            inner.ValueKind == JsonValueKind.Object)
        {
            TaskStreamEventBody body = Parse(inner);
            if (body.PartialText == null &&
                inner.TryGetProperty("partial_text", out JsonElement partial) &&
                partial.ValueKind == JsonValueKind.Object)
            {
                body.PartialText = Http.SseReader.ReadString(partial, "text");
                body.Language = body.Language ?? Http.SseReader.ReadString(partial, "language");
            }

            if (inner.TryGetProperty("partial_text_snapshot", out JsonElement snapshot) &&
                snapshot.ValueKind == JsonValueKind.Object)
            {
                body.PartialTextSnapshot = Http.SseReader.ReadString(snapshot, "text");
            }

            return body;
        }

        // JSON events[] rows use the same nested shapes.
        if (element.TryGetProperty("partial_text", out JsonElement pt) &&
            pt.ValueKind == JsonValueKind.Object)
        {
            TaskStreamEventBody body = Parse(element);
            body.PartialText = Http.SseReader.ReadString(pt, "text");
            body.Language = body.Language ?? Http.SseReader.ReadString(pt, "language");
            if (element.TryGetProperty("partial_text_snapshot", out JsonElement snap) &&
                snap.ValueKind == JsonValueKind.Object)
            {
                body.PartialTextSnapshot = Http.SseReader.ReadString(snap, "text");
            }

            return body;
        }

        return Parse(element);
    }
}

/// <summary>Kind of a task stream event surfaced by the client.</summary>
public enum TaskStreamEventKind
{
    /// <summary>An incremental event.</summary>
    Event,

    /// <summary>The final result; the run succeeded when this arrives.</summary>
    Done,
}

/// <summary>A task stream event: an incremental <see cref="Event"/> or the terminal <see cref="Result"/>.</summary>
public readonly struct TaskStreamEvent
{
    private TaskStreamEvent(TaskStreamEventKind kind, TaskStreamEventBody? evt, TaskResult? result)
    {
        Kind = kind;
        Event = evt;
        Result = result;
    }

    /// <summary>Which event this is.</summary>
    public TaskStreamEventKind Kind { get; }

    /// <summary>The incremental event body (<see cref="TaskStreamEventKind.Event"/>).</summary>
    public TaskStreamEventBody? Event { get; }

    /// <summary>The final result (<see cref="TaskStreamEventKind.Done"/>).</summary>
    public TaskResult? Result { get; }

    /// <summary>Wraps an incremental event body.</summary>
    /// <param name="body">The event body.</param>
    /// <returns>The event.</returns>
    internal static TaskStreamEvent FromEvent(TaskStreamEventBody body) => new(TaskStreamEventKind.Event, body, null);

    /// <summary>Wraps the terminal result.</summary>
    /// <param name="result">The final result.</param>
    /// <returns>The event.</returns>
    internal static TaskStreamEvent FromDone(TaskResult result) => new(TaskStreamEventKind.Done, null, result);

    /// <summary>Parses one SSE data payload into a task stream event.</summary>
    /// <param name="json">The event's JSON data payload.</param>
    /// <returns>The parsed event.</returns>
    /// <exception cref="JsonException">The payload carries an unknown event type.</exception>
    internal static TaskStreamEvent ParseSse(string json)
    {
        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;
        string? type = Http.SseReader.ReadString(root, "type");
        switch (type)
        {
            case "task.stream.event":
                return FromEvent(TaskStreamEventBody.ParseEnvelope(root));
            case "task.stream.done":
                if (root.TryGetProperty("result", out JsonElement result) &&
                    result.ValueKind == JsonValueKind.Object)
                {
                    return FromDone(Http.SseReader.ReadTaskResult(result));
                }

                return FromDone(Http.SseReader.ReadTaskResult(root));
            default:
                throw new JsonException("unknown task stream event type: " + (type ?? "<none>"));
        }
    }
}

/// <summary>Kind of a task batch stream event.</summary>
public enum TaskBatchEventKind
{
    /// <summary>One entry's result, with its original request index.</summary>
    Result,

    /// <summary>Aggregate batch timing; the terminal event.</summary>
    Done,
}

/// <summary>A task batch stream event.</summary>
public readonly struct TaskBatchEvent
{
    private TaskBatchEvent(TaskBatchEventKind kind, int index, TaskResult? result, int? resultCount, AudioCppTiming? timing)
    {
        Kind = kind;
        Index = index;
        Result = result;
        ResultCount = resultCount;
        Timing = timing;
    }

    /// <summary>Which event this is.</summary>
    public TaskBatchEventKind Kind { get; }

    /// <summary>Original request index (Result events).</summary>
    public int Index { get; }

    /// <summary>The entry's result; its own <c>timing</c> is null because fused execution has no per-request wall time.</summary>
    public TaskResult? Result { get; }

    /// <summary>Number of results (Done event).</summary>
    public int? ResultCount { get; }

    /// <summary>Aggregate timing (Done event).</summary>
    public AudioCppTiming? Timing { get; }

    /// <summary>Parses one SSE data payload into a batch event.</summary>
    /// <param name="json">The event's JSON data payload.</param>
    /// <returns>The parsed event.</returns>
    /// <exception cref="JsonException">The payload carries an unknown event type.</exception>
    internal static TaskBatchEvent Parse(string json)
    {
        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;
        string? type = Http.SseReader.ReadString(root, "type");
        switch (type)
        {
            case "task.batch.result":
                {
                    TaskResult? result = root.TryGetProperty("result", out JsonElement resultElement) && resultElement.ValueKind == JsonValueKind.Object ? Http.SseReader.ReadTaskResult(resultElement) : null;
                    return new TaskBatchEvent(TaskBatchEventKind.Result, Http.SseReader.ReadInt(root, "index") ?? -1, result, null, null);
                }

            case "task.batch.done":
                return new TaskBatchEvent(TaskBatchEventKind.Done, -1, null, Http.SseReader.ReadInt(root, "result_count"), Http.SseReader.ReadTiming(root, "timing"));
            default:
                throw new JsonException("unknown task batch event type: " + (type ?? "<none>"));
        }
    }
}

/// <summary>Request of <c>POST /v1/audio/alignments</c>: forced alignment of a known transcript against uploaded audio.</summary>
public sealed class AlignmentRequest
{
    /// <summary>Alignment model id (configured with <c>task: "align"</c>, <c>mode: "offline"</c>). Required.</summary>
    public string? Model { get; set; }

    /// <summary>Known transcript. Required.</summary>
    public string? Text { get; set; }

    /// <summary>Language; model-dependent (Qwen3 Forced Aligner requires it).</summary>
    public string? Language { get; set; }

    /// <summary>Audio bytes to upload (multipart). Required.</summary>
    public byte[]? AudioFile { get; set; }

    /// <summary>Reported filename of <see cref="AudioFile"/>; must name a WAV.</summary>
    public string? AudioFileName { get; set; }

    /// <summary>Validates required fields.</summary>
    public void Validate()
    {
        if (string.IsNullOrEmpty(Model))
        {
            throw new ArgumentException("alignment requires a model id", nameof(Model));
        }

        if (string.IsNullOrEmpty(Text))
        {
            throw new ArgumentException("alignment requires the transcript text", nameof(Text));
        }

        if (AudioFile == null || string.IsNullOrEmpty(AudioFileName))
        {
            throw new ArgumentException("alignment requires an uploaded audio file (bytes + filename)");
        }
    }
}

/// <summary>Result of <c>POST /v1/audio/alignments</c>.</summary>
public sealed class AlignmentResult
{
    /// <summary>Aligned text, when echoed back.</summary>
    public string? Text { get; internal set; }

    /// <summary>Language, when reported.</summary>
    public string? Language { get; internal set; }

    /// <summary>Word timestamps in seconds plus sample offsets.</summary>
    public IReadOnlyList<AudioCppWord> Words { get; internal set; } = Array.Empty<AudioCppWord>();

    /// <summary>Run timing.</summary>
    public AudioCppTiming? Timing { get; internal set; }

    /// <summary>Parses a cached response JSON (a disk cache hit rebuilds the result).</summary>
    /// <param name="json">The raw JSON response body.</param>
    /// <returns>The parsed result; throws <see cref="JsonException"/> on null, invalid JSON, or a non-object root.</returns>
    public static AlignmentResult Parse(string? json)
    {
        return Http.SseParsers.ParseRoot(json, Http.SseParsers.Alignment);
    }
}

/// <summary>Body of <c>POST /v1/recipes/run</c>.</summary>
public sealed class RecipeRunRequest
{
    /// <summary>Recipe id (from <c>GET /v1/recipes</c>). Required.</summary>
    public string? Recipe { get; set; }

    /// <summary>Role name → configured model id. Every role the recipe requires must be present.</summary>
    public IDictionary<string, string> Models { get; } = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>The shared request forwarded to the recipe stages.</summary>
    public TaskRequestBody Request { get; set; } = new TaskRequestBody();

    /// <summary>When <see langword="true"/>, the response is the tasks-stream SSE envelope instead of one JSON result.</summary>
    public bool Stream { get; set; }

    /// <summary>Include streamed artifacts in the SSE response (accepted; honored by builds with artifact streaming).</summary>
    public bool IncludeArtifacts { get; set; }

    /// <summary>Busy-wait bound for the run, in milliseconds.</summary>
    public long? BusyTimeoutMs { get; set; }

    /// <summary>Writes the recipe run request object.</summary>
    /// <param name="writer">The JSON writer.</param>
    /// <exception cref="ArgumentException">The recipe id is missing or the models map is empty.</exception>
    internal void WriteTo(Utf8JsonWriter writer)
    {
        if (string.IsNullOrEmpty(Recipe))
        {
            throw new ArgumentException("recipe run requires a recipe id", nameof(Recipe));
        }

        if (Models.Count == 0)
        {
            throw new ArgumentException("recipe run requires a models map with an entry per role");
        }

        writer.WriteStartObject();
        writer.WriteString("recipe", Recipe);
        if (Stream)
        {
            writer.WriteBoolean("stream", true);
        }

        if (IncludeArtifacts)
        {
            writer.WriteBoolean("include_artifacts", true);
        }

        RequestJson.WriteNumberIfSet(writer, "busy_timeout_ms", BusyTimeoutMs);
        writer.WriteStartObject("models");
        foreach (KeyValuePair<string, string> pair in Models)
        {
            writer.WriteString(pair.Key, pair.Value);
        }

        writer.WriteEndObject();
        writer.WritePropertyName("request");
        Request.WriteTo(writer);
        writer.WriteEndObject();
    }
}