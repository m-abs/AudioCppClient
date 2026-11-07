using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

using Mabs.AudioCpp.Models.Transcription;
using Mabs.AudioCpp.Utils;

namespace Mabs.AudioCpp.Models.Tasks;

/// <summary>A result of <c>POST /v1/tasks/run</c>, <c>/v1/tasks/stream</c> or a recipe run.</summary>
public sealed record TaskResult
{
    /// <summary>Transcript or generated text, when the task produced one.</summary>
    [JsonPropertyName("text")]
    public string? Text { get; init; }

    /// <summary>Language reported with <see cref="Text"/>.</summary>
    [JsonPropertyName("language")]
    public string? Language { get; init; }

    /// <summary>Primary audio output, decoded from the Base64 WAV on the wire.</summary>
    [JsonPropertyName("audio")]
    public byte[]? Audio { get; init; }

    /// <summary>Sample rate of <see cref="Audio"/>.</summary>
    [JsonPropertyName("sample_rate")]
    public int? SampleRate { get; init; }

    /// <summary>Channel count of <see cref="Audio"/>.</summary>
    [JsonPropertyName("channels")]
    public int? Channels { get; init; }

    /// <summary>Additional named audio outputs.</summary>
    [JsonPropertyName("named_audio_outputs")]
    public IReadOnlyList<NamedAudioOutput>? NamedAudioOutputs { get; init; }

    /// <summary>Output artifacts; feed them back via <see cref="TaskRequestBody.Artifacts"/>.</summary>
    [JsonPropertyName("artifacts")]
    public IReadOnlyList<AudioArtifact>? Artifacts { get; init; }

    /// <summary>Speech segments, when the model produced them.</summary>
    [JsonPropertyName("segments")]
    public IReadOnlyList<AudioCppSegment>? Segments { get; init; }

    /// <summary>Speaker turns, when the model diarized.</summary>
    [JsonPropertyName("speaker_turns")]
    public IReadOnlyList<AudioCppSpeakerTurn>? SpeakerTurns { get; init; }

    /// <summary>Word timestamps, when the model aligned words.</summary>
    [JsonPropertyName("words")]
    public IReadOnlyList<AudioCppWord>? Words { get; init; }

    /// <summary>Run timing.</summary>
    [JsonPropertyName("timing")]
    public AudioCppTiming? Timing { get; init; }

    /// <summary>Parses a cached response JSON (a disk cache hit rebuilds the result).</summary>
    /// <param name="json">The raw JSON response body.</param>
    /// <returns>The parsed result; throws <see cref="JsonException"/> on null, invalid JSON, or a non-object root.</returns>
    public static TaskResult Parse(string? json)
    {
        return Json.ParseObject<TaskResult>(json);
    }
}