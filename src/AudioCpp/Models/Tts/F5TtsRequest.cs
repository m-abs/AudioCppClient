using System;
using System.Collections.Generic;
using System.Globalization;

namespace Mabs.AudioCpp.Models.Tts;

/// <summary>Convenience request for the <c>f5_tts</c> spec, tts task (model default <c>habibi_unified</c>).</summary>
public sealed record F5TtsRequest : TtsRequest
{
    /// <summary>Transcript matching the reference voice audio; required by F5-TTS zero-shot cloning.</summary>
    public string? ReferenceText { get; init; }

    /// <summary>Habibi dialect token: UNK MSA SAU UAE ALG IRQ EGY MAR OMN TUN LEV SDN LBY; default UNK.</summary>
    public string? Dialect { get; init; } = "UNK";

    /// <summary>Sway sampling coefficient for inference timesteps; default -1.0. Advertised bounds are recommended settings, not parser-enforced limits.</summary>
    public double? SwaySamplingCoef { get; init; } = -1;

    /// <summary>Speech speed multiplier for duration estimation; default 1.0. Generated text chunks shorter than 10 characters use 0.3 instead. Advertised bounds are recommended settings, not parser-enforced limits.</summary>
    public double? Speed { get; init; } = 1;

    /// <summary>Non-negative noise seed; omitted selects a random seed. Explicit zero is deterministic.</summary>
    public string? Seed { get; init; }

    /// <summary>CFM Euler steps (NFE); default 32. The parser rejects nonpositive counts.</summary>
    public long? NumInferenceSteps { get; init; } = 32;

    /// <summary>Classifier-free guidance strength; default 2.0. Advertised bounds are recommended settings, not parser-enforced limits.</summary>
    public double? GuidanceScale { get; init; } = 2;

    /// <summary>Strip Arabic combining marks (harakat/tanwin/shadda/tatweel) before synthesis; Habibi is trained on undiacritized ASR transcripts and garbles diacritized input. Default true.</summary>
    public bool? StripDiacritics { get; init; } = true;

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (ReferenceText == null)
        {
            throw new ArgumentException("f5_tts tts requires reference_text", "ReferenceText");
        }

        if (SwaySamplingCoef != null && (SwaySamplingCoef.Value < -5 || SwaySamplingCoef.Value > 5))
        {
            throw new ArgumentException("f5_tts tts sway_sampling_coef must be between -5 and 5", "SwaySamplingCoef");
        }

        if (Speed != null && (Speed.Value < 0.5 || Speed.Value > 2))
        {
            throw new ArgumentException("f5_tts tts speed must be between 0.5 and 2", "Speed");
        }

        if (NumInferenceSteps != null && (NumInferenceSteps.Value < 1))
        {
            throw new ArgumentException("f5_tts tts num_inference_steps must be at least 1", "NumInferenceSteps");
        }

        if (GuidanceScale != null && (GuidanceScale.Value < 0 || GuidanceScale.Value > 10))
        {
            throw new ArgumentException("f5_tts tts guidance_scale must be between 0 and 10", "GuidanceScale");
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

        if (Dialect != null && Dialect != "UNK")
        {
            options["dialect"] = Dialect;
        }

        if (SwaySamplingCoef != null && SwaySamplingCoef != -1)
        {
            options["sway_sampling_coef"] = SwaySamplingCoef.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Speed != null && Speed != 1)
        {
            options["speed"] = Speed.Value.ToString(CultureInfo.InvariantCulture);
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

        if (StripDiacritics != null && StripDiacritics != true)
        {
            options["strip_diacritics"] = StripDiacritics.Value ? "true" : "false";
        }

        return options;
    }
}