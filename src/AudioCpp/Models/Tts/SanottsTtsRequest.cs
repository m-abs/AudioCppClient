using System;
using System.Collections.Generic;
using System.Globalization;

using Mabs.AudioCpp.Enums;

namespace Mabs.AudioCpp.Models.Tts;

/// <summary>Convenience request for the <c>sanotts</c> spec, tts task (model default <c>sanotts_heart_nano_orig</c>).</summary>
public sealed record SanottsTtsRequest : TtsRequest
{
    /// <summary>Speech speed multiplier; larger is faster. Its reciprocal scales the voice's tuned length scale before the per-token clamp.</summary>
    public double? Speed { get; init; }

    /// <summary>Decoder noise seed. The decoder is noise-fed, so a given seed picks one of many valid renderings; 0 derives it from the text as sha256(text)[:8], which is what the reference implementations do. Piperlite voices are deterministic and ignore the seed.</summary>
    public string? Seed { get; init; }

    /// <summary>Long-form text chunking mode.</summary>
    public SanottsTextChunkMode? TextChunkMode { get; init; }

    /// <summary>Maximum Unicode codepoints per long-form text chunk; default 280.</summary>
    public long? TextChunkSize { get; init; }

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (Speed != null && (Speed.Value < 0.5 || Speed.Value > 2))
        {
            throw new ArgumentException("sanotts tts speed must be between 0.5 and 2", "Speed");
        }

        if (TextChunkSize != null && (TextChunkSize.Value < 1))
        {
            throw new ArgumentException("sanotts tts text_chunk_size must be at least 1", "TextChunkSize");
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

        if (Seed != null && Seed != "0")
        {
            options["seed"] = Seed;
        }

        if (TextChunkMode != null && TextChunkMode != SanottsTextChunkMode.WordBudget)
        {
            options["text_chunk_mode"] = TextChunkMode switch
            {
            SanottsTextChunkMode.WordBudget => "word_budget",
            _ => throw new ArgumentException("sanotts tts text_chunk_mode must be one of: word_budget"),
            };
        }

        if (TextChunkSize != null && TextChunkSize != 280)
        {
            options["text_chunk_size"] = TextChunkSize.Value.ToString(CultureInfo.InvariantCulture);
        }

        return options;
    }
}