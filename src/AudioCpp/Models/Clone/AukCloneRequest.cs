using System;
using System.Collections.Generic;
using System.Globalization;

namespace Mabs.AudioCpp.Models.Clone;

/// <summary>Convenience request for the <c>auk</c> spec, clone task (model default <c>auk_base_f32</c>).</summary>
public sealed record AukCloneRequest : CloneRequest
{
    /// <summary>TTS voice description. Without it, text carries the complete upstream instruction; editing instructions always go in text.</summary>
    public string? Instruct { get; init; }

    /// <summary>Positive output duration in seconds. Required without reference audio; otherwise defaults to the valid reference duration.</summary>
    public double? DurationSec { get; init; }

    /// <summary>Euler sampling steps for AuK base; AuK-Flash always uses its fixed four-step schedule.</summary>
    public long? NumInferenceSteps { get; init; }

    /// <summary>Batched guidance strength for AuK base; AuK-Flash always disables classifier-free guidance.</summary>
    public double? GuidanceScale { get; init; }

    /// <summary>Sway coefficient for AuK base; ignored by AuK-Flash.</summary>
    public double? SwaySamplingCoef { get; init; }

    /// <summary>Nonnegative noise seed, or -1 for a random seed.</summary>
    public string? Seed { get; init; }

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (NumInferenceSteps != null && (NumInferenceSteps.Value < 1))
        {
            throw new ArgumentException("auk clone num_inference_steps must be at least 1", "NumInferenceSteps");
        }
    }

    /// <inheritdoc />
    public override Dictionary<string, string> BuildOptions()
    {
        Dictionary<string, string> options = [];

        if (Instruct != null && Instruct != string.Empty)
        {
            options["instruct"] = Instruct;
        }

        if (DurationSec != null)
        {
            options["duration_sec"] = DurationSec.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (NumInferenceSteps != null && NumInferenceSteps != 32)
        {
            options["num_inference_steps"] = NumInferenceSteps.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (GuidanceScale != null && GuidanceScale != 2)
        {
            options["guidance_scale"] = GuidanceScale.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (SwaySamplingCoef != null && SwaySamplingCoef != -1)
        {
            options["sway_sampling_coef"] = SwaySamplingCoef.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Seed != null && Seed != "-1")
        {
            options["seed"] = Seed;
        }

        return options;
    }
}