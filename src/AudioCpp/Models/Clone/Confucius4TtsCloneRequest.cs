using System;
using System.Collections.Generic;
using System.Globalization;

using Mabs.AudioCpp.Enums;

namespace Mabs.AudioCpp.Models.Clone;

/// <summary>Convenience request for the <c>confucius4_tts</c> spec, clone task (model default <c>confucius4_tts_orig</c>).</summary>
public sealed record Confucius4TtsCloneRequest : CloneRequest
{
    /// <summary>Target synthesis language code used by the text frontend; default zh when no request transcript language or style language is provided.</summary>
    public new string? Language { get; init; } = "zh";

    /// <summary>T2S sampling temperature; must be &gt; 0, default 0.8.</summary>
    public double? Temperature { get; init; } = 0.8;

    /// <summary>T2S nucleus sampling probability; must be in (0, 1], default 0.8.</summary>
    public double? TopP { get; init; } = 0.8;

    /// <summary>T2S top-k sampling limit; must be positive, default 30.</summary>
    public long? TopK { get; init; } = 30;

    /// <summary>T2S beam count; default 3. Set 1 for single-beam sampling.</summary>
    public long? NumBeams { get; init; } = 3;

    /// <summary>T2S repetition penalty; must be &gt; 0, default 10.0.</summary>
    public double? RepetitionPenalty { get; init; } = 10;

    /// <summary>Maximum T2S semantic sequence length including prompt tokens; default 1520.</summary>
    public long? MaxTokens { get; init; } = 1520;

    /// <summary>S2A flow-matching step count; default 25.</summary>
    public long? NumInferenceSteps { get; init; } = 25;

    /// <summary>S2A classifier-free guidance scale; default 0.7. Negative values select the no-CFG path.</summary>
    public double? GuidanceScale { get; init; } = 0.7;

    /// <summary>Maximum text tokens per generated segment; default 80.</summary>
    public long? TextChunkSize { get; init; } = 80;

    /// <summary>word_budget/default selects model-local official_segments. Other modes use the framework splitter with a codepoint budget.</summary>
    public Confucius4TtsTextChunkMode? TextChunkMode { get; init; } = Confucius4TtsTextChunkMode.Default;

    /// <summary>Cross-fade duration between generated segments in seconds; default 0.3.</summary>
    public double? CrossFadeDurationSec { get; init; } = 0.3;

    /// <summary>Fade duration applied at segment edges in seconds; default 0.1. Currently validated but not applied to generated audio.</summary>
    public double? EdgeFadeDurationSec { get; init; } = 0.1;

    /// <summary>Silence padding applied at segment edges in seconds; default 0.1. Currently validated but not applied to generated audio.</summary>
    public double? EdgePadDurationSec { get; init; } = 0.1;

    /// <summary>Seed for T2S sampling and S2A noise initialization; default 1234.</summary>
    public string? Seed { get; init; } = "1234";

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (Temperature != null && (Temperature.Value < 0))
        {
            throw new ArgumentException("confucius4_tts clone temperature must be at least 0", "Temperature");
        }

        if (TopP != null && (TopP.Value < 0 || TopP.Value > 1))
        {
            throw new ArgumentException("confucius4_tts clone top_p must be between 0 and 1", "TopP");
        }

        if (TopK != null && (TopK.Value < 1))
        {
            throw new ArgumentException("confucius4_tts clone top_k must be at least 1", "TopK");
        }

        if (NumBeams != null && (NumBeams.Value < 1))
        {
            throw new ArgumentException("confucius4_tts clone num_beams must be at least 1", "NumBeams");
        }

        if (RepetitionPenalty != null && (RepetitionPenalty.Value < 0))
        {
            throw new ArgumentException("confucius4_tts clone repetition_penalty must be at least 0", "RepetitionPenalty");
        }

        if (MaxTokens != null && (MaxTokens.Value < 1))
        {
            throw new ArgumentException("confucius4_tts clone max_tokens must be at least 1", "MaxTokens");
        }

        if (NumInferenceSteps != null && (NumInferenceSteps.Value < 1))
        {
            throw new ArgumentException("confucius4_tts clone num_inference_steps must be at least 1", "NumInferenceSteps");
        }

        if (TextChunkSize != null && (TextChunkSize.Value < 1))
        {
            throw new ArgumentException("confucius4_tts clone text_chunk_size must be at least 1", "TextChunkSize");
        }

        if (CrossFadeDurationSec != null && (CrossFadeDurationSec.Value < 0))
        {
            throw new ArgumentException("confucius4_tts clone cross_fade_duration_sec must be at least 0", "CrossFadeDurationSec");
        }

        if (EdgeFadeDurationSec != null && (EdgeFadeDurationSec.Value < 0))
        {
            throw new ArgumentException("confucius4_tts clone edge_fade_duration_sec must be at least 0", "EdgeFadeDurationSec");
        }

        if (EdgePadDurationSec != null && (EdgePadDurationSec.Value < 0))
        {
            throw new ArgumentException("confucius4_tts clone edge_pad_duration_sec must be at least 0", "EdgePadDurationSec");
        }
    }

    /// <inheritdoc />
    public override Dictionary<string, string> BuildOptions()
    {
        Dictionary<string, string> options = [];

        if (Language != null && Language != "zh")
        {
            options["language"] = Language;
        }

        if (Temperature != null && Temperature != 0.8)
        {
            options["temperature"] = Temperature.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TopP != null && TopP != 0.8)
        {
            options["top_p"] = TopP.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TopK != null && TopK != 30)
        {
            options["top_k"] = TopK.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (NumBeams != null && NumBeams != 3)
        {
            options["num_beams"] = NumBeams.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (RepetitionPenalty != null && RepetitionPenalty != 10)
        {
            options["repetition_penalty"] = RepetitionPenalty.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (MaxTokens != null && MaxTokens != 1520)
        {
            options["max_tokens"] = MaxTokens.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (NumInferenceSteps != null && NumInferenceSteps != 25)
        {
            options["num_inference_steps"] = NumInferenceSteps.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (GuidanceScale != null && GuidanceScale != 0.7)
        {
            options["guidance_scale"] = GuidanceScale.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TextChunkSize != null && TextChunkSize != 80)
        {
            options["text_chunk_size"] = TextChunkSize.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TextChunkMode != null && TextChunkMode != Confucius4TtsTextChunkMode.Default)
        {
            options["text_chunk_mode"] = TextChunkMode switch
            {
            Confucius4TtsTextChunkMode.Default => "default",
            Confucius4TtsTextChunkMode.TagAware => "tag_aware",
            Confucius4TtsTextChunkMode.Japanese => "japanese",
            Confucius4TtsTextChunkMode.Endline => "endline",
            _ => throw new ArgumentException("confucius4_tts clone text_chunk_mode must be one of: default, tag_aware, japanese, endline"),
            };
        }

        if (CrossFadeDurationSec != null && CrossFadeDurationSec != 0.3)
        {
            options["cross_fade_duration_sec"] = CrossFadeDurationSec.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (EdgeFadeDurationSec != null && EdgeFadeDurationSec != 0.1)
        {
            options["edge_fade_duration_sec"] = EdgeFadeDurationSec.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (EdgePadDurationSec != null && EdgePadDurationSec != 0.1)
        {
            options["edge_pad_duration_sec"] = EdgePadDurationSec.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Seed != null && Seed != "1234")
        {
            options["seed"] = Seed;
        }

        return options;
    }
}