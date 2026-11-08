using System;
using System.Collections.Generic;
using System.Globalization;

namespace Mabs.AudioCpp.Models.Clone;

/// <summary>Convenience request for the <c>glm_tts</c> spec, clone task (model default <c>glm_tts_q8_0</c>).</summary>
public sealed record GlmTtsCloneRequest : CloneRequest
{
    /// <summary>Transcript matching the reference voice audio; required by GLM-TTS zero-shot synthesis.</summary>
    public string? ReferenceText { get; init; }

    /// <summary>Maximum generated speech tokens; otherwise the official 2x-to-20x text-token bounds are used. Nonpositive values select the automatic text-derived budget.</summary>
    public long? MaxTokens { get; init; }

    /// <summary>Speech-token temperature; official default 1.0. Nonpositive values select greedy decoding.</summary>
    public double? Temperature { get; init; }

    /// <summary>Speech-token top-k; official default 25. Nonpositive values disable top-k filtering.</summary>
    public long? TopK { get; init; }

    /// <summary>Speech-token nucleus threshold; official default 0.8. Out-of-range values are not rejected by the parser.</summary>
    public double? TopP { get; init; }

    /// <summary>Speech-token, Flow-noise, and HiFT seed.</summary>
    public string? Seed { get; init; }

    /// <summary>Flow Euler steps; defaults to model config, usually official default 10.</summary>
    public long? NumInferenceSteps { get; init; }

    /// <summary>Flow classifier-free guidance rate; defaults to model config, usually official default 0.7. Negative values are accepted and enter the CFG formula.</summary>
    public double? FlowGuidanceScale { get; init; }

    /// <summary>Optional raw float32 initial Flow noise for parity tests.</summary>
    public string? FlowNoiseFile { get; init; }

    /// <summary>Optional raw float32 HiFT phase-uniform and Gaussian values for parity tests.</summary>
    public string? HiftSourceRandomFile { get; init; }

    /// <summary>Torch RNG value offset used before HiFT source generation; default 0.</summary>
    public long? HiftPriorNoiseCount { get; init; }

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (ReferenceText == null)
        {
            throw new ArgumentException("glm_tts clone requires reference_text", "ReferenceText");
        }

        if (NumInferenceSteps != null && (NumInferenceSteps.Value < 1))
        {
            throw new ArgumentException("glm_tts clone num_inference_steps must be at least 1", "NumInferenceSteps");
        }

        if (HiftPriorNoiseCount != null && (HiftPriorNoiseCount.Value < 0))
        {
            throw new ArgumentException("glm_tts clone hift_prior_noise_count must be at least 0", "HiftPriorNoiseCount");
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

        if (MaxTokens != null && MaxTokens != 0)
        {
            options["max_tokens"] = MaxTokens.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Temperature != null && Temperature != 1)
        {
            options["temperature"] = Temperature.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TopK != null && TopK != 25)
        {
            options["top_k"] = TopK.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TopP != null && TopP != 0.8)
        {
            options["top_p"] = TopP.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Seed != null && Seed != "0")
        {
            options["seed"] = Seed;
        }

        if (NumInferenceSteps != null)
        {
            options["num_inference_steps"] = NumInferenceSteps.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (FlowGuidanceScale != null)
        {
            options["flow_guidance_scale"] = FlowGuidanceScale.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (FlowNoiseFile != null)
        {
            options["flow_noise_file"] = FlowNoiseFile;
        }

        if (HiftSourceRandomFile != null)
        {
            options["hift_source_random_file"] = HiftSourceRandomFile;
        }

        if (HiftPriorNoiseCount != null && HiftPriorNoiseCount != 0)
        {
            options["hift_prior_noise_count"] = HiftPriorNoiseCount.Value.ToString(CultureInfo.InvariantCulture);
        }

        return options;
    }
}