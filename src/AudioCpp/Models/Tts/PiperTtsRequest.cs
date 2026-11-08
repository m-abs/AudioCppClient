using System;
using System.Collections.Generic;
using System.Globalization;

using Mabs.AudioCpp.Enums;

namespace Mabs.AudioCpp.Models.Tts;

/// <summary>Convenience request for the <c>piper_tts</c> spec, tts task (model default <c>piper_lessac_medium_orig</c>).</summary>
public sealed record PiperTtsRequest : TtsRequest
{
    /// <summary>Speech speed multiplier; larger values produce faster speech.</summary>
    public double? Speed { get; init; } = 1;

    /// <summary>Acoustic latent noise scale.</summary>
    public double? Variation { get; init; } = 0.667;

    /// <summary>Stochastic duration noise scale.</summary>
    public double? DurationVariation { get; init; } = 0.8;

    /// <summary>Non-negative generation seed.</summary>
    public string? Seed { get; init; } = "1234";

    /// <summary>Long-form text chunking mode.</summary>
    public PiperTtsTextChunkMode? TextChunkMode { get; init; } = PiperTtsTextChunkMode.Default;

    /// <summary>Maximum Unicode codepoints per long-form chunk.</summary>
    public long? TextChunkSize { get; init; } = 280;

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (Speed != null && (Speed.Value < 0.5 || Speed.Value > 2))
        {
            throw new ArgumentException("piper_tts tts speed must be between 0.5 and 2", "Speed");
        }

        if (Variation != null && (Variation.Value < 0 || Variation.Value > 1))
        {
            throw new ArgumentException("piper_tts tts variation must be between 0 and 1", "Variation");
        }

        if (DurationVariation != null && (DurationVariation.Value < 0 || DurationVariation.Value > 2))
        {
            throw new ArgumentException("piper_tts tts duration_variation must be between 0 and 2", "DurationVariation");
        }

        if (TextChunkSize != null && (TextChunkSize.Value < 1))
        {
            throw new ArgumentException("piper_tts tts text_chunk_size must be at least 1", "TextChunkSize");
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

        if (DurationVariation != null && DurationVariation != 0.8)
        {
            options["duration_variation"] = DurationVariation.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Seed != null && Seed != "1234")
        {
            options["seed"] = Seed;
        }

        if (TextChunkMode != null && TextChunkMode != PiperTtsTextChunkMode.Default)
        {
            options["text_chunk_mode"] = TextChunkMode switch
            {
            PiperTtsTextChunkMode.Default => "default",
            PiperTtsTextChunkMode.TagAware => "tag_aware",
            PiperTtsTextChunkMode.Japanese => "japanese",
            PiperTtsTextChunkMode.Endline => "endline",
            _ => throw new ArgumentException("piper_tts tts text_chunk_mode must be one of: default, tag_aware, japanese, endline"),
            };
        }

        if (TextChunkSize != null && TextChunkSize != 280)
        {
            options["text_chunk_size"] = TextChunkSize.Value.ToString(CultureInfo.InvariantCulture);
        }

        return options;
    }
}