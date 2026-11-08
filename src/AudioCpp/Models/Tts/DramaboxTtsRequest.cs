using System;
using System.Collections.Generic;
using System.Globalization;

namespace Mabs.AudioCpp.Models.Tts;

/// <summary>Convenience request for the <c>dramabox</c> spec, tts task (model default <c>dramabox_q8_0</c>).</summary>
public sealed record DramaboxTtsRequest : TtsRequest
{
    /// <summary>Reference voice WAV path for voice cloning. If omitted, DramaBox generates from text without reference-audio conditioning.</summary>
    public string? TargetVoice { get; init; }

    /// <summary>Negative text conditioning used when guidance_scale enables classifier-free guidance; omitted uses the built-in quality prompt.</summary>
    public string? NegativePrompt { get; init; }

    /// <summary>Explicit target duration in seconds; default 0 uses the prompt-duration estimator. Above audio_chunk_threshold_sec, long-form generation uses estimated per-chunk durations instead of this explicit target.</summary>
    public double? DurationSec { get; init; }

    /// <summary>Diffusion sampling step count; default comes from config.json, 30 in the current package.</summary>
    public long? NumInferenceSteps { get; init; }

    /// <summary>Classifier-free guidance scale; default comes from config.json, 2.5 in the current package. Values greater than 1 enable CFG. Negative values disable this guidance branch.</summary>
    public double? GuidanceScale { get; init; }

    /// <summary>Spatio-temporal guidance scale; default comes from config.json, 1.5 in the current package. Values greater than 0 enable STG. Negative values disable this guidance branch.</summary>
    public double? SpatioTemporalGuidanceScale { get; init; }

    /// <summary>Multiplier applied to the estimated prompt duration when duration_sec is 0; default comes from config.json, 1.1 in the current package. The resulting duration has a 3-second floor.</summary>
    public double? DurationScale { get; init; }

    /// <summary>Reference voice crop/repeat duration in seconds; default comes from config.json, 10.0 in the current package. Must be &gt; 0.</summary>
    public double? ReferenceDurationSec { get; init; }

    /// <summary>Guidance rescale: auto or any negative value derives it from guidance_scale; nonnegative numeric values select an explicit rescale.</summary>
    public string? GuidanceRescale { get; init; }

    /// <summary>Estimated duration threshold that switches a request to long-form chunking; default 45.0 seconds. Must be &gt; 0.</summary>
    public double? AudioChunkThresholdSec { get; init; }

    /// <summary>Target estimated duration for each long-form chunk; default 37.0 seconds. Must be &gt; 0.</summary>
    public double? AudioChunkDurationSec { get; init; }

    /// <summary>Equal-power cross-fade between long-form chunks in seconds; default 0.05. Zero still overlaps one sample.</summary>
    public double? CrossFadeDurationSec { get; init; }

    /// <summary>Torch-compatible CUDA noise seed for diffusion sampling; default 42.</summary>
    public string? Seed { get; init; }

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (DurationSec != null && (DurationSec.Value < 0))
        {
            throw new ArgumentException("dramabox tts duration_sec must be at least 0", "DurationSec");
        }

        if (NumInferenceSteps != null && (NumInferenceSteps.Value < 1))
        {
            throw new ArgumentException("dramabox tts num_inference_steps must be at least 1", "NumInferenceSteps");
        }

        if (ReferenceDurationSec != null && (ReferenceDurationSec.Value < 0))
        {
            throw new ArgumentException("dramabox tts reference_duration_sec must be at least 0", "ReferenceDurationSec");
        }

        if (AudioChunkThresholdSec != null && (AudioChunkThresholdSec.Value < 0))
        {
            throw new ArgumentException("dramabox tts audio_chunk_threshold_sec must be at least 0", "AudioChunkThresholdSec");
        }

        if (AudioChunkDurationSec != null && (AudioChunkDurationSec.Value < 0))
        {
            throw new ArgumentException("dramabox tts audio_chunk_duration_sec must be at least 0", "AudioChunkDurationSec");
        }

        if (CrossFadeDurationSec != null && (CrossFadeDurationSec.Value < 0))
        {
            throw new ArgumentException("dramabox tts cross_fade_duration_sec must be at least 0", "CrossFadeDurationSec");
        }
    }

    /// <inheritdoc />
    public override Dictionary<string, string> BuildOptions()
    {
        Dictionary<string, string> options = [];

        if (TargetVoice != null)
        {
            options["target_voice"] = TargetVoice;
        }

        if (NegativePrompt != null && NegativePrompt != string.Empty)
        {
            options["negative_prompt"] = NegativePrompt;
        }

        if (DurationSec != null && DurationSec != 0)
        {
            options["duration_sec"] = DurationSec.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (NumInferenceSteps != null && NumInferenceSteps != 30)
        {
            options["num_inference_steps"] = NumInferenceSteps.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (GuidanceScale != null && GuidanceScale != 2.5)
        {
            options["guidance_scale"] = GuidanceScale.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (SpatioTemporalGuidanceScale != null && SpatioTemporalGuidanceScale != 1.5)
        {
            options["spatio_temporal_guidance_scale"] = SpatioTemporalGuidanceScale.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (DurationScale != null && DurationScale != 1.1)
        {
            options["duration_scale"] = DurationScale.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (ReferenceDurationSec != null && ReferenceDurationSec != 10)
        {
            options["reference_duration_sec"] = ReferenceDurationSec.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (GuidanceRescale != null && GuidanceRescale != "auto")
        {
            options["guidance_rescale"] = GuidanceRescale;
        }

        if (AudioChunkThresholdSec != null && AudioChunkThresholdSec != 45)
        {
            options["audio_chunk_threshold_sec"] = AudioChunkThresholdSec.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (AudioChunkDurationSec != null && AudioChunkDurationSec != 37)
        {
            options["audio_chunk_duration_sec"] = AudioChunkDurationSec.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (CrossFadeDurationSec != null && CrossFadeDurationSec != 0.05)
        {
            options["cross_fade_duration_sec"] = CrossFadeDurationSec.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Seed != null && Seed != "42")
        {
            options["seed"] = Seed;
        }

        return options;
    }
}