using System;
using System.Collections.Generic;
using System.Globalization;

using Mabs.AudioCpp.Enums;

namespace Mabs.AudioCpp.Models.Tts;

/// <summary>Convenience request for the <c>supertonic</c> spec, tts task (model default <c>supertonic_3_orig</c>).</summary>
public sealed record SupertonicTtsRequest : TtsRequest
{
    /// <summary>Built-in voice preset; --voice-id takes precedence.</summary>
    public new SupertonicVoiceId? VoiceId { get; init; } = SupertonicVoiceId.M1;

    /// <summary>Flow denoising steps.</summary>
    public long? NumInferenceSteps { get; init; } = 8;

    /// <summary>Positive speech speed multiplier; voice.style.speaking_rate takes precedence.</summary>
    public double? Speed { get; init; } = 1.05;

    /// <summary>Noise seed.</summary>
    public string? Seed { get; init; } = "1234";

    /// <summary>Framework text chunk size; defaults to 120 for Korean/Japanese, otherwise 300.</summary>
    public long? TextChunkSize { get; init; }

    /// <summary>Framework text chunking mode.</summary>
    public SupertonicTextChunkMode? TextChunkMode { get; init; } = SupertonicTextChunkMode.Default;

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (NumInferenceSteps != null && (NumInferenceSteps.Value < 1))
        {
            throw new ArgumentException("supertonic tts num_inference_steps must be at least 1", "NumInferenceSteps");
        }

        if (Speed != null && (Speed.Value < 0))
        {
            throw new ArgumentException("supertonic tts speed must be at least 0", "Speed");
        }
    }

    /// <inheritdoc />
    public override Dictionary<string, string> BuildOptions()
    {
        Dictionary<string, string> options = [];

        if (VoiceId != null && VoiceId != SupertonicVoiceId.M1)
        {
            options["voice_id"] = VoiceId switch
            {
            SupertonicVoiceId.M1 => "M1",
            SupertonicVoiceId.M2 => "M2",
            SupertonicVoiceId.M3 => "M3",
            SupertonicVoiceId.M4 => "M4",
            SupertonicVoiceId.M5 => "M5",
            SupertonicVoiceId.F1 => "F1",
            SupertonicVoiceId.F2 => "F2",
            SupertonicVoiceId.F3 => "F3",
            SupertonicVoiceId.F4 => "F4",
            SupertonicVoiceId.F5 => "F5",
            _ => throw new ArgumentException("supertonic tts voice_id must be one of: M1, M2, M3, M4, M5, F1, F2, F3, F4, F5"),
            };
        }

        if (NumInferenceSteps != null && NumInferenceSteps != 8)
        {
            options["num_inference_steps"] = NumInferenceSteps.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Speed != null && Speed != 1.05)
        {
            options["speed"] = Speed.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Seed != null && Seed != "1234")
        {
            options["seed"] = Seed;
        }

        if (TextChunkSize != null)
        {
            options["text_chunk_size"] = TextChunkSize.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TextChunkMode != null && TextChunkMode != SupertonicTextChunkMode.Default)
        {
            options["text_chunk_mode"] = TextChunkMode switch
            {
            SupertonicTextChunkMode.Default => "default",
            SupertonicTextChunkMode.TagAware => "tag_aware",
            SupertonicTextChunkMode.Japanese => "japanese",
            SupertonicTextChunkMode.Endline => "endline",
            _ => throw new ArgumentException("supertonic tts text_chunk_mode must be one of: default, tag_aware, japanese, endline"),
            };
        }

        return options;
    }
}