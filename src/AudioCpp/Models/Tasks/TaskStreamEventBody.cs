using System.Collections.Generic;
using System.Text.Json.Serialization;

using Mabs.AudioCpp.Models.Transcription;
using Mabs.AudioCpp.Utils.Converters;

namespace Mabs.AudioCpp.Models.Tasks;

/// <summary>One event of a task stream (both the JSON <c>events[]</c> rows and the SSE <c>task.stream.event</c> payload).</summary>
[JsonConverter(typeof(TaskStreamEventBodyConverter))]
public sealed record TaskStreamEventBody
{
    /// <summary>Text produced since the previous event, with its language.</summary>
    [JsonPropertyName("text")]
    public string? PartialText { get; init; }

    /// <summary>Whole provisional text to replace, not append.</summary>
    [JsonPropertyName("partial_text_snapshot")]
    public string? PartialTextSnapshot { get; init; }

    /// <summary>Language of the partial text.</summary>
    [JsonPropertyName("language")]
    public string? Language { get; init; }

    /// <summary>WAV bytes of this event's audio.</summary>
    [JsonPropertyName("audio")]
    public byte[]? Audio { get; init; }

    /// <summary>Named audio outputs of this event (WAV, <c>format=wav</c>).</summary>
    [JsonPropertyName("named_audio_outputs")]
    public IReadOnlyList<NamedAudioOutput>? NamedAudioOutputs { get; init; }

    /// <summary>Word timestamps attached to this event.</summary>
    [JsonPropertyName("word_timestamps")]
    public IReadOnlyList<AudioCppWord>? WordTimestamps { get; init; }

    /// <summary>Speaker turns (diarization runs).</summary>
    [JsonPropertyName("speaker_turns")]
    public IReadOnlyList<AudioCppSpeakerTurn>? SpeakerTurns { get; init; }

    /// <summary>Artifacts carried by this event, when the build streams them.</summary>
    [JsonPropertyName("artifacts")]
    public IReadOnlyList<AudioArtifact>? Artifacts { get; init; }

    /// <summary>Whether this event is the final one.</summary>
    [JsonPropertyName("is_final")]
    public bool IsFinal { get; init; }
}