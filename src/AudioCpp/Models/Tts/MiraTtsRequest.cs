using System;
using System.Collections.Generic;
using System.Globalization;

using Mabs.AudioCpp.Enums;

namespace Mabs.AudioCpp.Models.Tts;

/// <summary>Convenience request for the <c>mira_tts</c> spec, tts task (model default <c>mira_tts</c>).</summary>
public sealed record MiraTtsRequest : TtsRequest
{
    /// <summary>Maximum autoregressive speech tokens; default 1024.</summary>
    public long? MaxTokens { get; init; } = 1024;

    /// <summary>Sampling temperature; default 0.8. Must be positive.</summary>
    public double? Temperature { get; init; } = 0.8;

    /// <summary>Top-k sampling cutoff; default 50.</summary>
    public long? TopK { get; init; } = 50;

    /// <summary>Nucleus sampling probability; default 0.95. Must be positive.</summary>
    public double? TopP { get; init; } = 0.95;

    /// <summary>Minimum probability relative to the most likely token; default 0.05.</summary>
    public double? MinP { get; init; } = 0.05;

    /// <summary>Autoregressive repetition penalty; default 1.2.</summary>
    public double? RepetitionPenalty { get; init; } = 1.2;

    /// <summary>Maximum codepoints per progressively emitted streaming segment; default 160.</summary>
    public long? TextChunkSize { get; init; } = 160;

    /// <summary>Framework text chunking mode used by streaming synthesis.</summary>
    public MiraTtsTextChunkMode? TextChunkMode { get; init; } = MiraTtsTextChunkMode.Default;

    /// <summary>Sampling seed; omitted requests choose a random seed.</summary>
    public string? Seed { get; init; }

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (MaxTokens != null && (MaxTokens.Value < 1))
        {
            throw new ArgumentException("mira_tts tts max_tokens must be at least 1", "MaxTokens");
        }

        if (TopK != null && (TopK.Value < 1))
        {
            throw new ArgumentException("mira_tts tts top_k must be at least 1", "TopK");
        }

        if (TopP != null && (TopP.Value > 1))
        {
            throw new ArgumentException("mira_tts tts top_p must be at most 1", "TopP");
        }

        if (MinP != null && (MinP.Value < 0 || MinP.Value > 1))
        {
            throw new ArgumentException("mira_tts tts min_p must be between 0 and 1", "MinP");
        }

        if (RepetitionPenalty != null && (RepetitionPenalty.Value < 1))
        {
            throw new ArgumentException("mira_tts tts repetition_penalty must be at least 1", "RepetitionPenalty");
        }

        if (TextChunkSize != null && (TextChunkSize.Value < 1))
        {
            throw new ArgumentException("mira_tts tts text_chunk_size must be at least 1", "TextChunkSize");
        }
    }

    /// <inheritdoc />
    public override Dictionary<string, string> BuildOptions()
    {
        Dictionary<string, string> options = [];

        if (MaxTokens != null && MaxTokens != 1024)
        {
            options["max_tokens"] = MaxTokens.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Temperature != null && Temperature != 0.8)
        {
            options["temperature"] = Temperature.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TopK != null && TopK != 50)
        {
            options["top_k"] = TopK.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TopP != null && TopP != 0.95)
        {
            options["top_p"] = TopP.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (MinP != null && MinP != 0.05)
        {
            options["min_p"] = MinP.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (RepetitionPenalty != null && RepetitionPenalty != 1.2)
        {
            options["repetition_penalty"] = RepetitionPenalty.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TextChunkSize != null && TextChunkSize != 160)
        {
            options["text_chunk_size"] = TextChunkSize.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TextChunkMode != null && TextChunkMode != MiraTtsTextChunkMode.Default)
        {
            options["text_chunk_mode"] = TextChunkMode switch
            {
            MiraTtsTextChunkMode.Default => "default",
            MiraTtsTextChunkMode.TagAware => "tag_aware",
            MiraTtsTextChunkMode.Japanese => "japanese",
            MiraTtsTextChunkMode.Endline => "endline",
            _ => throw new ArgumentException("mira_tts tts text_chunk_mode must be one of: default, tag_aware, japanese, endline"),
            };
        }

        if (Seed != null)
        {
            options["seed"] = Seed;
        }

        return options;
    }
}