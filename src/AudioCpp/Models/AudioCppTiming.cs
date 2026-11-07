using System.Text.Json.Serialization;

namespace Mabs.AudioCpp.Models;

/// <summary>
/// Inference timing. Different endpoints populate different members: non-streaming runs
/// report <see cref="WallMs"/>, <see cref="AudioDurationMs"/> and <see cref="Rtf"/>;
/// streaming runs report <see cref="TtftMs"/>. Live speech adds the remaining members.
/// </summary>
public sealed record AudioCppTiming
{
    /// <summary>Inference wall time in milliseconds.</summary>
    [JsonPropertyName("wall_ms")]
    public double? WallMs { get; init; }

    /// <summary>Audio duration in milliseconds (generated audio for speech, input audio for transcription).</summary>
    [JsonPropertyName("audio_duration_ms")]
    public double? AudioDurationMs { get; init; }

    /// <summary>Real-time factor: <c>wall_ms / audio_duration_ms</c>.</summary>
    [JsonPropertyName("rtf")]
    public double? Rtf { get; init; }

    /// <summary>Time to first output, in milliseconds; <see langword="null"/> when absent or explicitly null.</summary>
    [JsonPropertyName("ttft_ms")]
    public double? TtftMs { get; init; }

    /// <summary>Whether the timing reported a time to first output, i.e. <see cref="TtftMs"/> is not null.</summary>
    [JsonIgnore]
    public bool TtftPresent => TtftMs.HasValue;

    /// <summary>Live speech: whether first output audio arrived before the input stream closed.</summary>
    [JsonPropertyName("first_audio_before_input_end")]
    public bool? FirstAudioBeforeInputEnd { get; init; }

    /// <summary>Live speech: milliseconds of output that overlaps the still-open input stream.</summary>
    [JsonPropertyName("overlap_ms")]
    public double? OverlapMs { get; init; }

    /// <summary>Live speech: milliseconds from request start to first output audio.</summary>
    [JsonPropertyName("request_start_to_first_audio_ms")]
    public double? RequestStartToFirstAudioMs { get; init; }

    /// <summary>Live speech: milliseconds from request start to the end of the input stream.</summary>
    [JsonPropertyName("input_end_ms")]
    public double? InputEndMs { get; init; }
}