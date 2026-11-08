using System;
using System.Collections.Generic;
using System.Globalization;

namespace Mabs.AudioCpp.Models.Clone;

/// <summary>Convenience request for the <c>moss_ttsd</c> spec, clone task (model default <c>moss_ttsd_q8_0_codec_f16</c>).</summary>
public sealed record MossTtsdCloneRequest : CloneRequest
{
    /// <summary>Per-speaker reference WAV paths, comma separated and positional: the first is [S1], the second [S2]. Leave an entry empty to let that speaker be invented rather than cloned. Cannot be combined with voice_ref.</summary>
    public string? VoiceSamples { get; init; }

    /// <summary>What the reference recordings say, speaker-tagged. Prepended to the dialogue so the continuation carries on from words it has already spoken.</summary>
    public string? ReferenceText { get; init; }

    /// <summary>Full language name; the model does not understand codes like 'en'.</summary>
    public new string? Language { get; init; }

    /// <summary>Free-text direction for the delivery.</summary>
    public string? Instruct { get; init; } = string.Empty;

    /// <summary>Reproduces a take exactly. Parsed as signed 32-bit integer then converted to uint32; negative values wrap.</summary>
    public string? Seed { get; init; }

    /// <summary>Text-token sampling temperature. Must be finite and positive when sampling.</summary>
    public double? TextTemperature { get; init; } = 1.5;

    /// <summary>Audio sampling temperature; must be &gt; 0, default 1.5.</summary>
    public double? Temperature { get; init; } = 1.5;

    /// <summary>Audio nucleus sampling cutoff.</summary>
    public double? TopP { get; init; } = 0.6;

    /// <summary>Audio top-k. Nonpositive values disable filtering.</summary>
    public long? TopK { get; init; } = 50;

    /// <summary>Audio repetition penalty; must be &gt; 0. Checked when token history is present; default 1.1.</summary>
    public double? RepetitionPenalty { get; init; } = 1.1;

    /// <summary>Ceiling on generated codec frames at 12.5/s; 0 derives one from the dialogue length. Nonpositive values derive an automatic ceiling.</summary>
    public long? MaxFrames { get; init; } = 0;

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (Temperature != null && (Temperature.Value < 0))
        {
            throw new ArgumentException("moss_ttsd clone temperature must be at least 0", "Temperature");
        }

        if (TopP != null && (TopP.Value < 0))
        {
            throw new ArgumentException("moss_ttsd clone top_p must be at least 0", "TopP");
        }

        if (RepetitionPenalty != null && (RepetitionPenalty.Value < 0))
        {
            throw new ArgumentException("moss_ttsd clone repetition_penalty must be at least 0", "RepetitionPenalty");
        }
    }

    /// <inheritdoc />
    public override Dictionary<string, string> BuildOptions()
    {
        Dictionary<string, string> options = [];

        if (VoiceSamples != null)
        {
            options["voice_samples"] = VoiceSamples;
        }

        if (ReferenceText != null)
        {
            options["reference_text"] = ReferenceText;
        }

        if (Language != null)
        {
            options["language"] = Language;
        }

        if (Instruct != null)
        {
            options["instruct"] = Instruct;
        }

        if (Seed != null)
        {
            options["seed"] = Seed;
        }

        if (TextTemperature != null && TextTemperature != 1.5)
        {
            options["text_temperature"] = TextTemperature.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Temperature != null && Temperature != 1.5)
        {
            options["temperature"] = Temperature.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TopP != null && TopP != 0.6)
        {
            options["top_p"] = TopP.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TopK != null && TopK != 50)
        {
            options["top_k"] = TopK.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (RepetitionPenalty != null && RepetitionPenalty != 1.1)
        {
            options["repetition_penalty"] = RepetitionPenalty.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (MaxFrames != null)
        {
            options["max_frames"] = MaxFrames.Value.ToString(CultureInfo.InvariantCulture);
        }

        return options;
    }
}