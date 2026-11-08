using System;
using System.Collections.Generic;
using System.Globalization;

namespace Mabs.AudioCpp.Models.Design;

/// <summary>Convenience request for the <c>omnivoice</c> spec, design task (model default <c>omnivoice_q8_0</c>).</summary>
public sealed record OmnivoiceDesignRequest : DesignRequest
{
    /// <summary>Synthesis language code or label; omitted uses automatic language selection.</summary>
    public new string? Language { get; init; }

    /// <summary>Transcript for speaker reference audio used in voice cloning.</summary>
    public string? ReferenceText { get; init; }

    /// <summary>Generation seed for reproducible output; omitted value keeps the session RNG state.</summary>
    public string? Seed { get; init; }

    /// <summary>Diffusion language-model inference steps; default 32.</summary>
    public long? NumInferenceSteps { get; init; } = 32;

    /// <summary>Classifier-free guidance scale; default 2.0.</summary>
    public double? GuidanceScale { get; init; } = 2;

    /// <summary>Speech speed multiplier used for target-length estimation; default 1.0.</summary>
    public double? Speed { get; init; } = 1;

    /// <summary>Target output duration in seconds; omitted uses the model's estimated duration.</summary>
    public double? DurationSec { get; init; }

    /// <summary>Diffusion timestep schedule shift; default 0.1.</summary>
    public double? Shift { get; init; } = 0.1;

    /// <summary>Add the denoise control token when reference audio is present; default true.</summary>
    public bool? Denoise { get; init; } = true;

    /// <summary>Preprocess speaker reference audio before tokenization; default true.</summary>
    public bool? PreprocessPrompt { get; init; } = true;

    /// <summary>Apply output postprocessing and trimming; default true.</summary>
    public bool? PostprocessOutput { get; init; } = true;

    /// <summary>Layer-order penalty used while selecting masked audio-codebook positions; default 5.0.</summary>
    public double? LayerPenaltyFactor { get; init; } = 5;

    /// <summary>Gumbel temperature for masked-position selection; default 5.0.</summary>
    public double? PositionTemperature { get; init; } = 5;

    /// <summary>Audio-token class sampling temperature; default 0 for greedy class selection.</summary>
    public double? ClassTemperature { get; init; } = 0;

    /// <summary>Target chunk duration for automatic long-form splitting; default 15 seconds.</summary>
    public double? AudioChunkDurationSec { get; init; } = 15;

    /// <summary>Estimated duration above which long-form generation is split into chunks; default 30 seconds.</summary>
    public double? AudioChunkThresholdSec { get; init; } = 30;

    /// <summary>Explicit long-form text chunk size in Unicode codepoints. Omitted value uses duration-based automatic chunking.</summary>
    public long? TextChunkSize { get; init; }

    /// <summary>Text chunking mode; default tag_aware.</summary>
    public string? TextChunkMode { get; init; } = "tag_aware";

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (NumInferenceSteps != null && (NumInferenceSteps.Value < 1))
        {
            throw new ArgumentException("omnivoice design num_inference_steps must be at least 1", "NumInferenceSteps");
        }

        if (DurationSec != null && (DurationSec.Value < 0))
        {
            throw new ArgumentException("omnivoice design duration_sec must be at least 0", "DurationSec");
        }

        if (ClassTemperature != null && (ClassTemperature.Value < 0))
        {
            throw new ArgumentException("omnivoice design class_temperature must be at least 0", "ClassTemperature");
        }

        if (AudioChunkDurationSec != null && (AudioChunkDurationSec.Value < 0))
        {
            throw new ArgumentException("omnivoice design audio_chunk_duration_sec must be at least 0", "AudioChunkDurationSec");
        }

        if (AudioChunkThresholdSec != null && (AudioChunkThresholdSec.Value < 0))
        {
            throw new ArgumentException("omnivoice design audio_chunk_threshold_sec must be at least 0", "AudioChunkThresholdSec");
        }

        if (TextChunkSize != null && (TextChunkSize.Value < 1))
        {
            throw new ArgumentException("omnivoice design text_chunk_size must be at least 1", "TextChunkSize");
        }
    }

    /// <inheritdoc />
    public override Dictionary<string, string> BuildOptions()
    {
        Dictionary<string, string> options = [];

        if (Language != null)
        {
            options["language"] = Language;
        }

        if (ReferenceText != null)
        {
            options["reference_text"] = ReferenceText;
        }

        if (Instruction != null)
        {
            options["instruct"] = Instruction;
        }

        if (Seed != null)
        {
            options["seed"] = Seed;
        }

        if (NumInferenceSteps != null && NumInferenceSteps != 32)
        {
            options["num_inference_steps"] = NumInferenceSteps.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (GuidanceScale != null && GuidanceScale != 2)
        {
            options["guidance_scale"] = GuidanceScale.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Speed != null && Speed != 1)
        {
            options["speed"] = Speed.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (DurationSec != null)
        {
            options["duration_sec"] = DurationSec.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Shift != null && Shift != 0.1)
        {
            options["shift"] = Shift.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Denoise != null && Denoise != true)
        {
            options["denoise"] = Denoise.Value ? "true" : "false";
        }

        if (PreprocessPrompt != null && PreprocessPrompt != true)
        {
            options["preprocess_prompt"] = PreprocessPrompt.Value ? "true" : "false";
        }

        if (PostprocessOutput != null && PostprocessOutput != true)
        {
            options["postprocess_output"] = PostprocessOutput.Value ? "true" : "false";
        }

        if (LayerPenaltyFactor != null && LayerPenaltyFactor != 5)
        {
            options["layer_penalty_factor"] = LayerPenaltyFactor.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (PositionTemperature != null && PositionTemperature != 5)
        {
            options["position_temperature"] = PositionTemperature.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (ClassTemperature != null && ClassTemperature != 0)
        {
            options["class_temperature"] = ClassTemperature.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (AudioChunkDurationSec != null && AudioChunkDurationSec != 15)
        {
            options["audio_chunk_duration_sec"] = AudioChunkDurationSec.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (AudioChunkThresholdSec != null && AudioChunkThresholdSec != 30)
        {
            options["audio_chunk_threshold_sec"] = AudioChunkThresholdSec.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TextChunkSize != null)
        {
            options["text_chunk_size"] = TextChunkSize.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TextChunkMode != null && TextChunkMode != "tag_aware")
        {
            options["text_chunk_mode"] = TextChunkMode;
        }

        return options;
    }
}