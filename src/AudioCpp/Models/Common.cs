using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Mabs.AudioCpp.Models;

/// <summary>
/// Inference timing. Different endpoints populate different members: non-streaming runs
/// report <see cref="WallMs"/>, <see cref="AudioDurationMs"/> and <see cref="Rtf"/>;
/// streaming runs report <see cref="TtftMs"/> (when the server sent a <c>ttft_ms</c> key
/// — check <see cref="TtftPresent"/> because a live-speech timing can carry an explicit
/// JSON <c>null</c>); live speech adds the remaining members.
/// </summary>
public sealed class AudioCppTiming
{
    /// <summary>Inference wall time in milliseconds.</summary>
    public double? WallMs { get; internal set; }

    /// <summary>Audio duration in milliseconds (generated audio for speech, input audio for transcription).</summary>
    public double? AudioDurationMs { get; internal set; }

    /// <summary>Real-time factor: <c>wall_ms / audio_duration_ms</c>.</summary>
    public double? Rtf { get; internal set; }

    /// <summary>Time to first output, in milliseconds; <see langword="null"/> when absent or explicitly null.</summary>
    public double? TtftMs { get; internal set; }

    /// <summary>
    /// <see langword="true"/> when the timing object contained a <c>ttft_ms</c> key with a
    /// non-null value. A live-speech timing sets <see cref="TtftMs"/> to
    /// <see langword="null"/> while this is <see langword="true"/> when the first output
    /// audio arrived before the input stream ended.
    /// </summary>
    public bool TtftPresent { get; internal set; }

    /// <summary>Live speech: whether first output audio arrived before the input stream closed.</summary>
    public bool? FirstAudioBeforeInputEnd { get; internal set; }

    /// <summary>Live speech: milliseconds of output that overlaps the still-open input stream.</summary>
    public double? OverlapMs { get; internal set; }

    /// <summary>Live speech: milliseconds from request start to first output audio.</summary>
    public double? RequestStartToFirstAudioMs { get; internal set; }

    /// <summary>Live speech: milliseconds from request start to the end of the input stream.</summary>
    public double? InputEndMs { get; internal set; }
}

/// <summary>A speech segment reported by a model (timestamps are sample offsets).</summary>
public sealed class AudioCppSegment
{
    /// <summary>First sample of the segment.</summary>
    public long? StartSample { get; internal set; }

    /// <summary>End sample (exclusive) of the segment.</summary>
    public long? EndSample { get; internal set; }

    /// <summary>Model confidence, when reported.</summary>
    public double? Confidence { get; internal set; }

    /// <summary>Segment text, when the segment carries one.</summary>
    public string? Text { get; internal set; }
}

/// <summary>A word with timestamps, from alignment or word-level transcription output.</summary>
public sealed class AudioCppWord
{
    /// <summary>The word text.</summary>
    public string? Word { get; internal set; }

    /// <summary>Start time in seconds (alignment responses only).</summary>
    public double? Start { get; internal set; }

    /// <summary>End time in seconds (alignment responses only).</summary>
    public double? End { get; internal set; }

    /// <summary>First sample of the word.</summary>
    public long? StartSample { get; internal set; }

    /// <summary>End sample (exclusive) of the word.</summary>
    public long? EndSample { get; internal set; }

    /// <summary>Model confidence, when reported.</summary>
    public double? Confidence { get; internal set; }
}

/// <summary>A speaker turn from a diarization model.</summary>
public sealed class AudioCppSpeakerTurn
{
    /// <summary>First sample of the turn.</summary>
    public long? StartSample { get; internal set; }

    /// <summary>End sample (exclusive) of the turn.</summary>
    public long? EndSample { get; internal set; }

    /// <summary>Speaker label, for example <c>speaker_0</c>.</summary>
    public string? SpeakerId { get; internal set; }

    /// <summary>Model confidence, when reported.</summary>
    public double? Confidence { get; internal set; }

    /// <summary>Turn text, when the model attaches one.</summary>
    public string? Text { get; internal set; }
}

/// <summary>An opaque model artifact (speaker embedding, acoustic tokens, …) returned by tasks routes.</summary>
public sealed class AudioArtifact
{
    /// <summary>Artifact id; ids may repeat.</summary>
    public string Id { get; internal set; } = string.Empty;

    /// <summary>
    /// Artifact kind: one of <c>speaker_embedding</c>, <c>style_embedding</c>,
    /// <c>prompt_embedding</c>, <c>acoustic_tokens</c>, <c>midi</c>,
    /// <c>transcript_alignment</c>, <c>diarization_state</c>, <c>vad_state</c>, <c>custom</c>.
    /// </summary>
    public string Kind { get; internal set; } = "custom";

    /// <summary>Decoded payload bytes.</summary>
    public byte[] Payload { get; internal set; } = Array.Empty<byte>();

    /// <summary>String metadata attached to the artifact, when present.</summary>
    public IReadOnlyDictionary<string, string>? Meta { get; internal set; }
}

/// <summary>One named audio output of a result or stream event.</summary>
public sealed class NamedAudioOutput
{
    /// <summary>Output id, for example <c>vocals</c>.</summary>
    public string Id { get; internal set; } = string.Empty;

    /// <summary>Decoded WAV bytes (results) or PCM; event payloads use <c>format</c> to say which.</summary>
    public byte[]? Audio { get; internal set; }

    /// <summary>Sample rate of <see cref="Audio"/> (result payloads).</summary>
    public int? SampleRate { get; internal set; }

    /// <summary>Channel count of <see cref="Audio"/> (result payloads).</summary>
    public int? Channels { get; internal set; }

    /// <summary>Format tag on stream events: <c>wav</c>.</summary>
    public string? Format { get; internal set; }
}

/// <summary>A result of <c>POST /v1/tasks/run</c>, <c>/v1/tasks/stream</c> or a recipe run.</summary>
public sealed class TaskResult
{
    /// <summary>Transcript or generated text, when the task produced one.</summary>
    public string? Text { get; internal set; }

    /// <summary>Language reported with <see cref="Text"/>.</summary>
    public string? Language { get; internal set; }

    /// <summary>Primary audio output, decoded from the Base64 WAV on the wire.</summary>
    public byte[]? Audio { get; internal set; }

    /// <summary>Sample rate of <see cref="Audio"/>.</summary>
    public int? SampleRate { get; internal set; }

    /// <summary>Channel count of <see cref="Audio"/>.</summary>
    public int? Channels { get; internal set; }

    /// <summary>Additional named audio outputs.</summary>
    public IReadOnlyList<NamedAudioOutput>? NamedAudioOutputs { get; internal set; }

    /// <summary>Output artifacts; feed them back via <see cref="TaskRequestBody.Artifacts"/>.</summary>
    public IReadOnlyList<AudioArtifact>? Artifacts { get; internal set; }

    /// <summary>Speech segments, when the model produced them.</summary>
    public IReadOnlyList<AudioCppSegment>? Segments { get; internal set; }

    /// <summary>Speaker turns, when the model diarized.</summary>
    public IReadOnlyList<AudioCppSpeakerTurn>? SpeakerTurns { get; internal set; }

    /// <summary>Word timestamps, when the model aligned words.</summary>
    public IReadOnlyList<AudioCppWord>? Words { get; internal set; }

    /// <summary>Run timing.</summary>
    public AudioCppTiming? Timing { get; internal set; }
}

/// <summary>Server readiness from <c>GET /health</c>.</summary>
public sealed class AudioCppHealth
{
    /// <summary>Status; <c>ok</c> when the server is up.</summary>
    public string? Status { get; internal set; }

    /// <summary>Active backend: <c>cpu</c>, <c>cuda</c>, <c>hip</c>, ….</summary>
    public string? Backend { get; internal set; }

    /// <summary>Number of configured models.</summary>
    public int Models { get; internal set; }

    /// <summary>Whether the embedded WebUI is served.</summary>
    public bool Ui { get; internal set; }

    /// <summary>Whether WebUI model management is enabled.</summary>
    public bool UiManagement { get; internal set; }

    /// <summary>Build version, <c>dev</c> unless the binary was stamped.</summary>
    public string? Version { get; internal set; }

    /// <summary>Short git commit of the build.</summary>
    public string? Git { get; internal set; }

    /// <summary>Commit date of the build.</summary>
    public string? GitDate { get; internal set; }
}

/// <summary>A request option declared by a model spec, from <c>GET /v1/models?include_params=true</c>.</summary>
public sealed class AudioCppModelParam
{
    /// <summary>Option name; pass it as a key of an <c>options</c> map.</summary>
    public string? Name { get; internal set; }

    /// <summary>Declared type, for example <c>string</c>, <c>int</c>, <c>float</c>, <c>bool</c>, <c>enum</c>.</summary>
    public string? Type { get; internal set; }

    /// <summary>Whether the model requires the option.</summary>
    public bool Required { get; internal set; }

    /// <summary>Human-readable description.</summary>
    public string? Description { get; internal set; }

    /// <summary>Declared default value, as raw JSON (string/number/bool), when given.</summary>
    public JsonElement? Default { get; internal set; }

    /// <summary>Declared minimum, as raw JSON, when given.</summary>
    public JsonElement? Min { get; internal set; }

    /// <summary>Declared maximum, as raw JSON, when given.</summary>
    public JsonElement? Max { get; internal set; }

    /// <summary>Allowed values for an enum row that names a preset.</summary>
    public IReadOnlyList<string>? Values { get; internal set; }
}

/// <summary>A model entry from <c>GET /v1/models</c>.</summary>
public sealed class AudioCppModel
{
    /// <summary>Model id; use it as the <c>model</c> field of requests.</summary>
    public string? Id { get; internal set; }

    /// <summary>OpenAI-style object name, always <c>model</c>.</summary>
    public string? Object { get; internal set; }

    /// <summary><see langword="true"/> for a config-defined id; <see langword="false"/> for one loaded through <c>POST /v1/models/load</c>.</summary>
    public bool Configured { get; internal set; }

    /// <summary>Effective backend device index.</summary>
    public int? Device { get; internal set; }

    /// <summary>Configured session options, when requested with <c>include_session_options=true</c>.</summary>
    public IReadOnlyDictionary<string, string>? SessionOptions { get; internal set; }

    /// <summary>Declared request options, when requested with <c>include_params=true</c>.</summary>
    public IReadOnlyList<AudioCppModelParam>? Params { get; internal set; }
}

/// <summary>Result of <c>POST /v1/tasks/unload_models</c> and <c>/v1/tasks/unload_all_models</c>.</summary>
public sealed class UnloadModelsResult
{
    /// <summary>Ids that were resident and are now unloaded.</summary>
    public IReadOnlyList<string> Unloaded { get; internal set; } = Array.Empty<string>();

    /// <summary>Ids the server does not know (only reported by the selective endpoint).</summary>
    public IReadOnlyList<string> NotFound { get; internal set; } = Array.Empty<string>();
}

/// <summary>Body of <c>POST /v1/models/load</c> (builds with native model management).</summary>
public sealed class ModelLoadRequest
{
    /// <summary>Id to register the model under. Required.</summary>
    public string? Id { get; set; }

    /// <summary>Server-local path to the model file or directory. Mutually exclusive with <see cref="PackageId"/>.</summary>
    public string? Path { get; set; }

    /// <summary>Catalog package id to resolve. Mutually exclusive with <see cref="Path"/>.</summary>
    public string? PackageId { get; set; }

    /// <summary>Model family, for example <c>kokoro_tts</c>.</summary>
    public string? Family { get; set; }

    /// <summary>Task, for example <c>tts</c> or <c>asr</c>.</summary>
    public string? Task { get; set; }

    /// <summary>Run mode: <c>offline</c> or <c>streaming</c>.</summary>
    public string? Mode { get; set; }

    /// <summary>Backend device index override.</summary>
    public int? Device { get; set; }

    /// <summary>Loader options (stringly-typed like other option maps).</summary>
    public Dictionary<string, string>? LoadOptions { get; set; }

    /// <summary>Session options (stringly-typed like other option maps).</summary>
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

    /// <summary>Writes the load request object (call <see cref="Validate"/> first).</summary>
    /// <param name="writer">The JSON writer.</param>
    internal void WriteTo(Utf8JsonWriter writer)
    {
        writer.WriteStartObject();
        writer.WriteString("id", Id);
        RequestJson.WriteStringIfSet(writer, "path", Path);
        RequestJson.WriteStringIfSet(writer, "package_id", PackageId);
        RequestJson.WriteStringIfSet(writer, "family", Family);
        RequestJson.WriteStringIfSet(writer, "task", Task);
        RequestJson.WriteStringIfSet(writer, "mode", Mode);
        RequestJson.WriteNumberIfSet(writer, "device", Device);
        if (LoadOptions != null && LoadOptions.Count > 0)
        {
            writer.WriteStartObject("load_options");
            foreach (KeyValuePair<string, string> pair in LoadOptions)
            {
                writer.WriteString(pair.Key, pair.Value);
            }

            writer.WriteEndObject();
        }

        if (SessionOptions != null && SessionOptions.Count > 0)
        {
            writer.WriteStartObject("session_options");
            foreach (KeyValuePair<string, string> pair in SessionOptions)
            {
                writer.WriteString(pair.Key, pair.Value);
            }

            writer.WriteEndObject();
        }

        writer.WriteEndObject();
    }
}

/// <summary>Result of <c>POST /v1/models/load</c>.</summary>
public sealed class ModelLoadResult
{
    /// <summary>Loaded model id.</summary>
    public string? Id { get; internal set; }

    /// <summary>Whether the model was loaded by this call (as opposed to already resident and unchanged).</summary>
    public bool Loaded { get; internal set; }

    /// <summary>Whether an already-registered model was reconfigured by this call.</summary>
    public bool Reconfigured { get; internal set; }
}

/// <summary>A recipe entry from <c>GET /v1/recipes</c>.</summary>
public sealed class RecipeEntry
{
    /// <summary>Recipe id; use it as <see cref="RecipeRunRequest.Recipe"/>.</summary>
    public string? Id { get; internal set; }

    /// <summary>Human-readable description.</summary>
    public string? Description { get; internal set; }

    /// <summary>Model roles the recipe requires.</summary>
    public IReadOnlyList<RecipeRole> Roles { get; internal set; } = Array.Empty<RecipeRole>();
}

/// <summary>One required role of a recipe.</summary>
public sealed class RecipeRole
{
    /// <summary>Role name; a key of <see cref="RecipeRunRequest.Models"/>.</summary>
    public string? Name { get; internal set; }

    /// <summary>Task the role's model must implement.</summary>
    public string? Task { get; internal set; }

    /// <summary>Mode the role's model must be configured with.</summary>
    public string? Mode { get; internal set; }
}