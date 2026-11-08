using System;
using System.Collections.Generic;
using System.Globalization;

namespace Mabs.AudioCpp.Models.Tts;

/// <summary>Convenience request for the <c>sopro_tts</c> spec, tts task (model default <c>sopro_v2_turbo_f16</c>).</summary>
public sealed record SoproTtsRequest : TtsRequest
{
    /// <summary>Language tag (en, pt, fr, de); omitted inherits transcript language, then the session default.</summary>
    public new string? Language { get; init; }

    /// <summary>Semantic sampling temperature; nonpositive selects greedy decoding. Omitted uses checkpoint generation.temperature.</summary>
    public double? Temperature { get; init; }

    /// <summary>Semantic nucleus cutoff; values outside [0,1] are disabled/clamped by the sampler. Omitted uses checkpoint generation.top_p.</summary>
    public double? TopP { get; init; }

    /// <summary>Semantic top-k; nonpositive disables filtering. Omitted uses checkpoint generation.top_k.</summary>
    public long? TopK { get; init; }

    /// <summary>Positive acoustic sampling steps; omitted uses checkpoint generation.steps.</summary>
    public long? NumInferenceSteps { get; init; }

    /// <summary>Positive maximum segment duration; must be &gt;= min_duration_sec. Omitted uses checkpoint generation.max_seconds.</summary>
    public double? MaxDurationSec { get; init; }

    /// <summary>Nonnegative minimum duration; must be &lt;= max_duration_sec. Omitted uses checkpoint generation.min_seconds.</summary>
    public double? MinDurationSec { get; init; }

    /// <summary>Positive reference window in seconds; omitted uses checkpoint generation.ref_seconds.</summary>
    public double? ReferenceDurationSec { get; init; }

    /// <summary>Positive codepoint budget per segment; omitted uses checkpoint generation.max_segment_chars.</summary>
    public long? TextChunkSize { get; init; }

    /// <summary>Non-negative seed for semantic sampling and the acoustic noise prior; omit for a random seed.</summary>
    public string? Seed { get; init; }

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (NumInferenceSteps != null && (NumInferenceSteps.Value < 1))
        {
            throw new ArgumentException("sopro_tts tts num_inference_steps must be at least 1", "NumInferenceSteps");
        }

        if (MinDurationSec != null && (MinDurationSec.Value < 0))
        {
            throw new ArgumentException("sopro_tts tts min_duration_sec must be at least 0", "MinDurationSec");
        }

        if (TextChunkSize != null && (TextChunkSize.Value < 1))
        {
            throw new ArgumentException("sopro_tts tts text_chunk_size must be at least 1", "TextChunkSize");
        }
    }

    /// <inheritdoc />
    public override Dictionary<string, string> BuildOptions()
    {
        Dictionary<string, string> options = [];

        if (Language != null && Language != string.Empty)
        {
            options["language"] = Language;
        }

        if (Temperature != null && Temperature != 0.8)
        {
            options["temperature"] = Temperature.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TopP != null && TopP != 0.9)
        {
            options["top_p"] = TopP.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TopK != null && TopK != 25)
        {
            options["top_k"] = TopK.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (NumInferenceSteps != null && NumInferenceSteps != 2)
        {
            options["num_inference_steps"] = NumInferenceSteps.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (MaxDurationSec != null)
        {
            options["max_duration_sec"] = MaxDurationSec.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (MinDurationSec != null)
        {
            options["min_duration_sec"] = MinDurationSec.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (ReferenceDurationSec != null)
        {
            options["reference_duration_sec"] = ReferenceDurationSec.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TextChunkSize != null && TextChunkSize != 300)
        {
            options["text_chunk_size"] = TextChunkSize.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Seed != null)
        {
            options["seed"] = Seed;
        }

        return options;
    }
}