using System;
using System.Collections.Generic;
using System.Globalization;

namespace Mabs.AudioCpp.Models.Tts;

/// <summary>Convenience request for the <c>voxcpm1</c> spec, tts task (model default <c>voxcpm1_0_5b_q8_0</c>).</summary>
public sealed record Voxcpm1TtsRequest : TtsRequest
{
    /// <summary>Text chunking mode; default tag_aware.</summary>
    public string? TextChunkMode { get; init; } = "tag_aware";

    /// <summary>Random seed for MiniCPM and diffusion sampling.</summary>
    public string? Seed { get; init; }

    /// <summary>Maximum MiniCPM output tokens; default 4096. Zero selects model max_length; negative counts are rejected.</summary>
    public long? MaxTokens { get; init; } = 4096;

    /// <summary>Minimum MiniCPM output tokens before an EOS stop is honored; default 2.</summary>
    public long? MinTokens { get; init; } = 2;

    /// <summary>CFM diffusion sampling steps; default 10. Must be positive.</summary>
    public long? NumInferenceSteps { get; init; } = 10;

    /// <summary>CFM classifier-free guidance rate. Must be nonnegative.</summary>
    public double? GuidanceScale { get; init; } = 2;

    /// <summary>Retry when generation is detected as a bad case; defaults to true offline and false in streaming. Streaming ignores an explicit true value with a warning.</summary>
    public bool? RetryBadcase { get; init; } = true;

    /// <summary>Maximum total generation attempts when retrying bad cases, including the initial attempt; default 3. Must be positive.</summary>
    public long? RetryBadcaseMaxTimes { get; init; } = 3;

    /// <summary>Bad-case ratio threshold for retry decisions. Must be positive.</summary>
    public double? RetryBadcaseRatioThreshold { get; init; }

    /// <summary>Text prompt for prompt-continuation voice cloning.</summary>
    public string? PromptText { get; init; }

    /// <summary>Alias for prompt_text; text prompt for prompt-continuation voice cloning.</summary>
    public string? ReferenceText { get; init; }

    /// <summary>Maximum characters per offline text chunk; must be positive. Does not control streaming chunk cadence.</summary>
    public long? TextChunkSize { get; init; } = 2048;

    /// <summary>Language hint for Chinese-script normalization; Cantonese/yue preserves Traditional characters. Inherits structured transcript or voice language when provided.</summary>
    public new string? Language { get; init; }

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (TextChunkSize != null && (TextChunkSize.Value < 1))
        {
            throw new ArgumentException("voxcpm1 tts text_chunk_size must be at least 1", "TextChunkSize");
        }
    }

    /// <inheritdoc />
    public override Dictionary<string, string> BuildOptions()
    {
        Dictionary<string, string> options = [];

        if (TextChunkMode != null && TextChunkMode != "tag_aware")
        {
            options["text_chunk_mode"] = TextChunkMode;
        }

        if (Seed != null)
        {
            options["seed"] = Seed;
        }

        if (MaxTokens != null && MaxTokens != 4096)
        {
            options["max_tokens"] = MaxTokens.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (MinTokens != null && MinTokens != 2)
        {
            options["min_tokens"] = MinTokens.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (NumInferenceSteps != null && NumInferenceSteps != 10)
        {
            options["num_inference_steps"] = NumInferenceSteps.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (GuidanceScale != null && GuidanceScale != 2)
        {
            options["guidance_scale"] = GuidanceScale.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (RetryBadcase != null && RetryBadcase != false)
        {
            options["retry_badcase"] = RetryBadcase.Value ? "true" : "false";
        }

        if (RetryBadcaseMaxTimes != null && RetryBadcaseMaxTimes != 3)
        {
            options["retry_badcase_max_times"] = RetryBadcaseMaxTimes.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (RetryBadcaseRatioThreshold != null)
        {
            options["retry_badcase_ratio_threshold"] = RetryBadcaseRatioThreshold.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (PromptText != null)
        {
            options["prompt_text"] = PromptText;
        }

        if (ReferenceText != null)
        {
            options["reference_text"] = ReferenceText;
        }

        if (TextChunkSize != null && TextChunkSize != 2048)
        {
            options["text_chunk_size"] = TextChunkSize.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Language != null)
        {
            options["language"] = Language;
        }

        return options;
    }
}