using System.Collections.Generic;
using System.Text.Json.Serialization;

using Mabs.AudioCpp.Models.Transcription;

namespace Mabs.AudioCpp.Models.Tasks;

/// <summary>Wire shape of a task stream event body, including the nested partial-text shapes.</summary>
internal sealed record TaskStreamEventBodyDto
{
    /// <summary>Flat text produced since the previous event.</summary>
    [JsonPropertyName("text")]
    public string? Text { get; init; }

    /// <summary>Language of the partial text.</summary>
    [JsonPropertyName("language")]
    public string? Language { get; init; }

    /// <summary>Base64 audio payload of this event.</summary>
    [JsonPropertyName("audio")]
    public byte[]? Audio { get; init; }

    /// <summary>Named audio outputs of this event.</summary>
    [JsonPropertyName("named_audio_outputs")]
    public IReadOnlyList<NamedAudioOutput>? NamedAudioOutputs { get; init; }

    /// <summary>Word timestamps attached to this event.</summary>
    [JsonPropertyName("word_timestamps")]
    public IReadOnlyList<AudioCppWord>? WordTimestamps { get; init; }

    /// <summary>Speaker turns (diarization runs).</summary>
    [JsonPropertyName("speaker_turns")]
    public IReadOnlyList<AudioCppSpeakerTurn>? SpeakerTurns { get; init; }

    /// <summary>Artifacts carried by this event.</summary>
    [JsonPropertyName("artifacts")]
    public IReadOnlyList<AudioArtifact>? Artifacts { get; init; }

    /// <summary>Whether this event is the final one.</summary>
    [JsonPropertyName("is_final")]
    public bool IsFinal { get; init; }

    /// <summary>Nested <c>partial_text</c> object (SSE envelope and JSON events[] rows).</summary>
    [JsonPropertyName("partial_text")]
    public NestedPartialTextDto? PartialText { get; init; }

    /// <summary>Nested <c>partial_text_snapshot</c> object.</summary>
    [JsonPropertyName("partial_text_snapshot")]
    public NestedPartialTextSnapshotDto? PartialTextSnapshot { get; init; }

    /// <summary>The <c>event</c> member of a task stream envelope.</summary>
    [JsonPropertyName("event")]
    public TaskStreamEventBodyDto? Event { get; init; }
}