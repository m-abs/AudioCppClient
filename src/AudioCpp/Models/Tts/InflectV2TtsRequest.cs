using System;
using System.Collections.Generic;
using System.Globalization;

using Mabs.AudioCpp.Enums;

namespace Mabs.AudioCpp.Models.Tts;

/// <summary>Convenience request for the <c>inflect_v2</c> spec, tts task (model default <c>inflect_micro_v2_orig</c>).</summary>
public sealed record InflectV2TtsRequest : TtsRequest
{
    /// <summary>Speech speed multiplier mapped directly to Inflect speed; default 1.0.</summary>
    public double? Speed { get; init; }

    /// <summary>Latent Gaussian variation; default 0.667.</summary>
    public double? Variation { get; init; }

    /// <summary>Non-negative latent noise seed; default 0.</summary>
    public string? Seed { get; init; }

    /// <summary>Long-form text chunking mode.</summary>
    public InflectV2TextChunkMode? TextChunkMode { get; init; }

    /// <summary>Maximum Unicode codepoints per long-form text chunk; default 280.</summary>
    public long? TextChunkSize { get; init; }

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (Speed != null && (Speed.Value < 0.5 || Speed.Value > 2))
        {
            throw new ArgumentException("inflect_v2 tts speed must be between 0.5 and 2", "Speed");
        }

        if (Variation != null && (Variation.Value < 0 || Variation.Value > 1))
        {
            throw new ArgumentException("inflect_v2 tts variation must be between 0 and 1", "Variation");
        }

        if (TextChunkSize != null && (TextChunkSize.Value < 1))
        {
            throw new ArgumentException("inflect_v2 tts text_chunk_size must be at least 1", "TextChunkSize");
        }
    }

    /// <inheritdoc />
    public override Dictionary<string, string> BuildOptions()
    {
        Dictionary<string, string> options = [];

        if (Speed != null && Speed != 1)
        {
            options["speed"] = Speed.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Variation != null && Variation != 0.667)
        {
            options["variation"] = Variation.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Seed != null && Seed != "0")
        {
            options["seed"] = Seed;
        }

        if (TextChunkMode != null && TextChunkMode != InflectV2TextChunkMode.WordBudget)
        {
            options["text_chunk_mode"] = TextChunkMode switch
            {
            InflectV2TextChunkMode.WordBudget => "word_budget",
            _ => throw new ArgumentException("inflect_v2 tts text_chunk_mode must be one of: word_budget"),
            };
        }

        if (TextChunkSize != null && TextChunkSize != 280)
        {
            options["text_chunk_size"] = TextChunkSize.Value.ToString(CultureInfo.InvariantCulture);
        }

        return options;
    }
}