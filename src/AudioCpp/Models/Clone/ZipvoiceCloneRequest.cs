using System;
using System.Collections.Generic;
using System.Globalization;

namespace Mabs.AudioCpp.Models.Clone;

/// <summary>Convenience request for the <c>zipvoice</c> spec, clone task (model default <c>zipvoice_distill_gguf</c>).</summary>
public sealed record ZipvoiceCloneRequest : CloneRequest
{
    /// <summary>Transcript matching the reference voice audio; required for zero-shot cloning.</summary>
    public string? ReferenceText { get; init; }

    /// <summary>Maximum characters per generated text chunk; default 128.</summary>
    public long? TextChunkSize { get; init; } = 128;

    /// <summary>Framework text chunking mode.</summary>
    public string? TextChunkMode { get; init; } = "word_budget";

    /// <summary>Guidance scale; inherits the session default (3.0 for both variants). ZipVoice-Distill uses a guidance-scale embedding; ZipVoice uses batched CFG. Zero disables guidance. Any finite nonnegative value is accepted.</summary>
    public double? GuidanceScale { get; init; } = 3;

    /// <summary>Euler ODE steps; inherits the session default (8 for both variants).</summary>
    public long? NumInferenceSteps { get; init; } = 8;

    /// <summary>Shift timesteps toward low SNR (smaller = stronger shift); default 0.5. Must be finite and positive.</summary>
    public double? Shift { get; init; } = 0.5;

    /// <summary>Speech speed multiplier applied through the prompt-duration ratio; default 1.0. Must be finite and positive.</summary>
    public double? Speed { get; init; } = 1;

    /// <summary>Feature scale applied to log-mel features; default 0.1 (reference default). Must be finite and positive.</summary>
    public double? FeatScale { get; init; } = 0.1;

    /// <summary>Target RMS for prompt loudness normalization; 0 disables. Default 0.1. Negative values disable normalization; values above 1 are accepted.</summary>
    public double? TargetRms { get; init; } = 0.1;

    /// <summary>Noise seed; default 666 (reference default).</summary>
    public string? Seed { get; init; } = "666";

    /// <summary>espeak language for phonemization; default en-us.</summary>
    public new string? Language { get; init; } = "en-us";

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (ReferenceText == null)
        {
            throw new ArgumentException("zipvoice clone requires reference_text", "ReferenceText");
        }

        if (TextChunkSize != null && (TextChunkSize.Value < 1))
        {
            throw new ArgumentException("zipvoice clone text_chunk_size must be at least 1", "TextChunkSize");
        }

        if (GuidanceScale != null && (GuidanceScale.Value < 0))
        {
            throw new ArgumentException("zipvoice clone guidance_scale must be at least 0", "GuidanceScale");
        }

        if (NumInferenceSteps != null && (NumInferenceSteps.Value < 1 || NumInferenceSteps.Value > 64))
        {
            throw new ArgumentException("zipvoice clone num_inference_steps must be between 1 and 64", "NumInferenceSteps");
        }
    }

    /// <inheritdoc />
    public override Dictionary<string, string> BuildOptions()
    {
        Dictionary<string, string> options = [];

        if (ReferenceText != null)
        {
            options["reference_text"] = ReferenceText;
        }

        if (TextChunkSize != null && TextChunkSize != 128)
        {
            options["text_chunk_size"] = TextChunkSize.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TextChunkMode != null && TextChunkMode != "word_budget")
        {
            options["text_chunk_mode"] = TextChunkMode;
        }

        if (GuidanceScale != null && GuidanceScale != 3)
        {
            options["guidance_scale"] = GuidanceScale.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (NumInferenceSteps != null && NumInferenceSteps != 8)
        {
            options["num_inference_steps"] = NumInferenceSteps.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Shift != null && Shift != 0.5)
        {
            options["shift"] = Shift.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Speed != null && Speed != 1)
        {
            options["speed"] = Speed.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (FeatScale != null && FeatScale != 0.1)
        {
            options["feat_scale"] = FeatScale.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TargetRms != null && TargetRms != 0.1)
        {
            options["target_rms"] = TargetRms.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Seed != null && Seed != "666")
        {
            options["seed"] = Seed;
        }

        if (Language != null && Language != "en-us")
        {
            options["language"] = Language;
        }

        return options;
    }
}