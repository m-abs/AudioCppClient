using System;
using System.Collections.Generic;
using System.Globalization;

using Mabs.AudioCpp.Enums;

namespace Mabs.AudioCpp.Models.Tts;

/// <summary>Convenience request for the <c>echo_tts</c> spec, tts task (model default <c>echo_tts_q8_0</c>).</summary>
public sealed record EchoTtsRequest : TtsRequest
{
    /// <summary>Maximum Unicode codepoints per text chunk; must be positive, default 300.</summary>
    public long? TextChunkSize { get; init; } = 300;

    /// <summary>Classifier-free guidance scale on the text condition (independent-cfg only). Advertised range is recommended, not enforced by the parser.</summary>
    public double? TextGuidanceScale { get; init; } = 3;

    /// <summary>Classifier-free guidance scale on the speaker condition (independent-cfg only). Advertised range is recommended, not enforced by the parser.</summary>
    public double? SpeakerGuidanceScale { get; init; } = 8;

    /// <summary>Classifier-free guidance scale on the text and speaker condition (join-cfg only). Advertised range is recommended, not enforced by the parser.</summary>
    public double? GuidanceScale { get; init; } = 8;

    /// <summary>RF Euler timestep schedule: linear or sway; default linear.</summary>
    public EchoTtsTScheduleMode? TScheduleMode { get; init; } = EchoTtsTScheduleMode.Linear;

    /// <summary>Sway Sampling coefficient; must be finite and produce strictly decreasing timesteps. Default -1.0.</summary>
    public double? SwayCoeff { get; init; } = -1;

    /// <summary>Euler sampler steps. Default 40 for linear timestep schedule, 12 for sway.</summary>
    public long? NumInferenceSteps { get; init; } = 40;

    /// <summary>Initial-noise truncation factor. Advertised range is recommended, not enforced by the parser.</summary>
    public double? TruncationFactor { get; init; } = 0.8;

    /// <summary>Force-speaker KV scaling. 1.0 disables; 1.5 is the upstream default when enabled. Advertised range is recommended, not parser-enforced; zero is invalid for reciprocal scaling.</summary>
    public double? SpeakerKvScale { get; init; } = 1;

    /// <summary>RNG seed for the initial latent. Negative signed-integer values are treated as zero.</summary>
    public string? Seed { get; init; } = "0";

    /// <summary>Trim the speaker reference to at most this many seconds before encoding. Shorter references are cheaper and often clone better; upstream's guidance favours around 10 s. Defaults to 15 s. Values above the trained maximum of 297.1 s are clamped. Set per request, or as a session default from CLI or server config. Must be positive.</summary>
    public double? ReferenceDurationSec { get; init; } = 15;

    /// <summary>Cap the generation window, in seconds (up to 29.7215 s, the trained maximum; larger values are clamped to it). Quantised down to a whole latent frame of 46.44 ms. Left unset, the window is estimated per chunk from text length and widened automatically if the utterance does not finish inside it, which is substantially cheaper for short text. Must be positive; rounded to at least one latent frame.</summary>
    public double? MaxDurationSec { get; init; }

    /// <summary>Refresh the two unconditional CFG lanes only every Nth step inside the guidance window, reusing the previous guidance correction in between. 1 reproduces upstream exactly. Worth raising to 2 at 30+ steps, where the correction is still measured 8-10 times across the guided phase (roughly 1.3x fewer denoiser evaluations); not recommended below ~20 steps, where it is already coarsely sampled and staleness shows up as degraded timbre and text adherence.</summary>
    public long? GuidanceInterval { get; init; } = 1;

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (TextChunkSize != null && (TextChunkSize.Value < 1))
        {
            throw new ArgumentException("echo_tts tts text_chunk_size must be at least 1", "TextChunkSize");
        }

        if (TextGuidanceScale != null && (TextGuidanceScale.Value < 0))
        {
            throw new ArgumentException("echo_tts tts text_guidance_scale must be at least 0", "TextGuidanceScale");
        }

        if (SpeakerGuidanceScale != null && (SpeakerGuidanceScale.Value < 0))
        {
            throw new ArgumentException("echo_tts tts speaker_guidance_scale must be at least 0", "SpeakerGuidanceScale");
        }

        if (GuidanceScale != null && (GuidanceScale.Value < 0))
        {
            throw new ArgumentException("echo_tts tts guidance_scale must be at least 0", "GuidanceScale");
        }

        if (NumInferenceSteps != null && (NumInferenceSteps.Value < 1))
        {
            throw new ArgumentException("echo_tts tts num_inference_steps must be at least 1", "NumInferenceSteps");
        }

        if (TruncationFactor != null && (TruncationFactor.Value < 0 || TruncationFactor.Value > 1))
        {
            throw new ArgumentException("echo_tts tts truncation_factor must be between 0 and 1", "TruncationFactor");
        }

        if (SpeakerKvScale != null && (SpeakerKvScale.Value < 1))
        {
            throw new ArgumentException("echo_tts tts speaker_kv_scale must be at least 1", "SpeakerKvScale");
        }

        if (MaxDurationSec != null && (MaxDurationSec.Value > 29.7215))
        {
            throw new ArgumentException("echo_tts tts max_duration_sec must be at most 29.7215", "MaxDurationSec");
        }

        if (GuidanceInterval != null && (GuidanceInterval.Value < 1))
        {
            throw new ArgumentException("echo_tts tts guidance_interval must be at least 1", "GuidanceInterval");
        }
    }

    /// <inheritdoc />
    public override Dictionary<string, string> BuildOptions()
    {
        Dictionary<string, string> options = [];

        if (TextChunkSize != null && TextChunkSize != 300)
        {
            options["text_chunk_size"] = TextChunkSize.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TextGuidanceScale != null && TextGuidanceScale != 3)
        {
            options["text_guidance_scale"] = TextGuidanceScale.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (SpeakerGuidanceScale != null && SpeakerGuidanceScale != 8)
        {
            options["speaker_guidance_scale"] = SpeakerGuidanceScale.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (GuidanceScale != null && GuidanceScale != 8)
        {
            options["guidance_scale"] = GuidanceScale.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TScheduleMode != null && TScheduleMode != EchoTtsTScheduleMode.Linear)
        {
            options["t_schedule_mode"] = TScheduleMode switch
            {
            EchoTtsTScheduleMode.Linear => "linear",
            EchoTtsTScheduleMode.Sway => "sway",
            _ => throw new ArgumentException("echo_tts tts t_schedule_mode must be one of: linear, sway"),
            };
        }

        if (SwayCoeff != null && SwayCoeff != -1)
        {
            options["sway_coeff"] = SwayCoeff.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (NumInferenceSteps != null && NumInferenceSteps != 40)
        {
            options["num_inference_steps"] = NumInferenceSteps.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TruncationFactor != null && TruncationFactor != 0.8)
        {
            options["truncation_factor"] = TruncationFactor.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (SpeakerKvScale != null && SpeakerKvScale != 1)
        {
            options["speaker_kv_scale"] = SpeakerKvScale.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Seed != null && Seed != "0")
        {
            options["seed"] = Seed;
        }

        if (ReferenceDurationSec != null && ReferenceDurationSec != 15)
        {
            options["reference_duration_sec"] = ReferenceDurationSec.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (MaxDurationSec != null)
        {
            options["max_duration_sec"] = MaxDurationSec.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (GuidanceInterval != null && GuidanceInterval != 1)
        {
            options["guidance_interval"] = GuidanceInterval.Value.ToString(CultureInfo.InvariantCulture);
        }

        return options;
    }
}