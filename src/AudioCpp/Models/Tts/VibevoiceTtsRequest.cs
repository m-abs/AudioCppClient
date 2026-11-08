using System;
using System.Collections.Generic;
using System.Globalization;

namespace Mabs.AudioCpp.Models.Tts;

/// <summary>Convenience request for the <c>vibevoice</c> spec, tts task (model default <c>vibevoice_1_5b_q8_0</c>).</summary>
public sealed record VibevoiceTtsRequest : TtsRequest
{
    /// <summary>Comma-separated reference WAV paths ordered by speaker ID; cannot be combined with voice_ref.</summary>
    public string? VoiceSamples { get; init; }

    /// <summary>Diffusion steps; defaults to the checkpoint configuration.</summary>
    public long? NumInferenceSteps { get; init; }

    /// <summary>Classifier-free guidance scale.</summary>
    public double? GuidanceScale { get; init; }

    /// <summary>Positive generation length multiplier.</summary>
    public double? MaxLengthTimes { get; init; }

    /// <summary>Maximum generated tokens; zero uses the model-derived limit.</summary>
    public long? MaxTokens { get; init; }

    /// <summary>Enable sampling.</summary>
    public bool? DoSample { get; init; }

    /// <summary>Positive sampling temperature.</summary>
    public double? Temperature { get; init; }

    /// <summary>Top-k sampling limit; zero disables filtering.</summary>
    public long? TopK { get; init; }

    /// <summary>Nucleus probability in (0, 1].</summary>
    public double? TopP { get; init; }

    /// <summary>Sampling seed.</summary>
    public string? Seed { get; init; }

    /// <summary>F32 prompt acoustic noise file.</summary>
    public string? PromptNoiseFile { get; init; }

    /// <summary>F32 initial diffusion noise file.</summary>
    public string? DiffusionNoiseFile { get; init; }

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (NumInferenceSteps != null && (NumInferenceSteps.Value < 1))
        {
            throw new ArgumentException("vibevoice tts num_inference_steps must be at least 1", "NumInferenceSteps");
        }

        if (GuidanceScale != null && (GuidanceScale.Value < 0))
        {
            throw new ArgumentException("vibevoice tts guidance_scale must be at least 0", "GuidanceScale");
        }

        if (MaxTokens != null && (MaxTokens.Value < 0))
        {
            throw new ArgumentException("vibevoice tts max_tokens must be at least 0", "MaxTokens");
        }

        if (TopK != null && (TopK.Value < 0))
        {
            throw new ArgumentException("vibevoice tts top_k must be at least 0", "TopK");
        }
    }

    /// <inheritdoc />
    public override Dictionary<string, string> BuildOptions()
    {
        Dictionary<string, string> options = [];

        if (VoiceSamples != null && VoiceSamples != string.Empty)
        {
            options["voice_samples"] = VoiceSamples;
        }

        if (NumInferenceSteps != null && NumInferenceSteps != 10)
        {
            options["num_inference_steps"] = NumInferenceSteps.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (GuidanceScale != null && GuidanceScale != 1.3)
        {
            options["guidance_scale"] = GuidanceScale.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (MaxLengthTimes != null && MaxLengthTimes != 2)
        {
            options["max_length_times"] = MaxLengthTimes.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (MaxTokens != null && MaxTokens != 0)
        {
            options["max_tokens"] = MaxTokens.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (DoSample != null && DoSample != false)
        {
            options["do_sample"] = DoSample.Value ? "true" : "false";
        }

        if (Temperature != null && Temperature != 1)
        {
            options["temperature"] = Temperature.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TopK != null && TopK != 50)
        {
            options["top_k"] = TopK.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TopP != null && TopP != 1)
        {
            options["top_p"] = TopP.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Seed != null && Seed != "1234")
        {
            options["seed"] = Seed;
        }

        if (PromptNoiseFile != null)
        {
            options["prompt_noise_file"] = PromptNoiseFile;
        }

        if (DiffusionNoiseFile != null)
        {
            options["diffusion_noise_file"] = DiffusionNoiseFile;
        }

        return options;
    }
}