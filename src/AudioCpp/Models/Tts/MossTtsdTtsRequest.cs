using System;
using System.Collections.Generic;
using System.Globalization;

namespace Mabs.AudioCpp.Models.Tts;

/// <summary>Convenience request for the <c>moss_ttsd</c> spec, tts task (model default <c>moss_ttsd_q8_0_codec_f16</c>).</summary>
public sealed record MossTtsdTtsRequest : TtsRequest
{
    /// <summary>Per-speaker reference WAV paths, comma separated and positional: the first is [S1], the second [S2]. Leave an entry empty to let that speaker be invented rather than cloned. Cannot be combined with voice_ref.</summary>
    public string? VoiceSamples { get; init; }

    /// <summary>What the reference recordings say, speaker-tagged. Prepended to the dialogue so the continuation carries on from words it has already spoken.</summary>
    public string? ReferenceText { get; init; }

    /// <summary>Full language name; the model does not understand codes like 'en'.</summary>
    public new string? Language { get; init; }

    /// <summary>Free-text direction for the delivery.</summary>
    public string? Instruct { get; init; }

    /// <summary>Reproduces a take exactly. Parsed as signed 32-bit integer then converted to uint32; negative values wrap.</summary>
    public string? Seed { get; init; }

    /// <summary>Text-token sampling temperature. Must be finite and positive when sampling.</summary>
    public double? TextTemperature { get; init; }

    /// <summary>Audio sampling temperature; must be &gt; 0, default 1.5.</summary>
    public double? Temperature { get; init; }

    /// <summary>Audio nucleus sampling cutoff.</summary>
    public double? TopP { get; init; }

    /// <summary>Audio top-k. Nonpositive values disable filtering.</summary>
    public long? TopK { get; init; }

    /// <summary>Audio repetition penalty; must be &gt; 0. Checked when token history is present; default 1.1.</summary>
    public double? RepetitionPenalty { get; init; }

    /// <summary>Ceiling on generated codec frames at 12.5/s; 0 derives one from the dialogue length. Nonpositive values derive an automatic ceiling.</summary>
    public long? MaxFrames { get; init; }

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (Temperature != null && (Temperature.Value < 0))
        {
            throw new ArgumentException("moss_ttsd tts temperature must be at least 0", "Temperature");
        }

        if (TopP != null && (TopP.Value < 0))
        {
            throw new ArgumentException("moss_ttsd tts top_p must be at least 0", "TopP");
        }

        if (RepetitionPenalty != null && (RepetitionPenalty.Value < 0))
        {
            throw new ArgumentException("moss_ttsd tts repetition_penalty must be at least 0", "RepetitionPenalty");
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

        if (Instruct != null && Instruct != string.Empty)
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

        if (MaxFrames != null && MaxFrames != 0)
        {
            options["max_frames"] = MaxFrames.Value.ToString(CultureInfo.InvariantCulture);
        }

        return options;
    }
}