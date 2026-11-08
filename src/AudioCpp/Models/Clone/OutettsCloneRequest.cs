using System;
using System.Collections.Generic;
using System.Globalization;

namespace Mabs.AudioCpp.Models.Clone;

/// <summary>Convenience request for the <c>outetts</c> spec, clone task (model default <c>outetts_1_0_1b_q8_0</c>).</summary>
public sealed record OutettsCloneRequest : CloneRequest
{
    /// <summary>Maximum generated audio tokens per chunk. When omitted, OuteTTS estimates a safe value from each chunk.</summary>
    public long? MaxTokens { get; init; }

    /// <summary>Sampling temperature; default 0.4 for cloning, otherwise model config default.</summary>
    public double? Temperature { get; init; }

    /// <summary>Top-k sampling; default 40 for cloning, otherwise model config default.</summary>
    public long? TopK { get; init; }

    /// <summary>Nucleus sampling in (0, 1]; default 0.9 for cloning, otherwise model config default.</summary>
    public double? TopP { get; init; }

    /// <summary>Minimum probability relative to the best token; default 0.05 for cloning, otherwise model config default.</summary>
    public double? MinP { get; init; }

    /// <summary>Windowed repetition penalty; must be &gt; 0, default 1.1.</summary>
    public double? RepetitionPenalty { get; init; }

    /// <summary>Recent-token penalty window; default 64.</summary>
    public long? RepetitionWindow { get; init; }

    /// <summary>Sampling seed; cloning defaults to 4099 for native weights and 42 for quantized weights.</summary>
    public string? Seed { get; init; }

    /// <summary>Transcript matching the reference voice audio for voice cloning.</summary>
    public string? ReferenceText { get; init; }

    /// <summary>Language code used to align the reference transcript; omitted inherits the request text language, then falls back to en.</summary>
    public string? ReferenceLanguage { get; init; }

    /// <summary>Maximum UTF-8 codepoints per long-form text chunk; default 256. Chunks are split further when required by max_tokens or context budget.</summary>
    public long? TextChunkSize { get; init; }

    /// <summary>Framework long-form text chunking mode; default word_budget.</summary>
    public string? TextChunkMode { get; init; }

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (MaxTokens != null && (MaxTokens.Value < 1))
        {
            throw new ArgumentException("outetts clone max_tokens must be at least 1", "MaxTokens");
        }

        if (Temperature != null && (Temperature.Value < 0))
        {
            throw new ArgumentException("outetts clone temperature must be at least 0", "Temperature");
        }

        if (TopK != null && (TopK.Value < 0))
        {
            throw new ArgumentException("outetts clone top_k must be at least 0", "TopK");
        }

        if (TopP != null && (TopP.Value < 0 || TopP.Value > 1))
        {
            throw new ArgumentException("outetts clone top_p must be between 0 and 1", "TopP");
        }

        if (MinP != null && (MinP.Value < 0 || MinP.Value > 1))
        {
            throw new ArgumentException("outetts clone min_p must be between 0 and 1", "MinP");
        }

        if (RepetitionPenalty != null && (RepetitionPenalty.Value < 0))
        {
            throw new ArgumentException("outetts clone repetition_penalty must be at least 0", "RepetitionPenalty");
        }

        if (RepetitionWindow != null && (RepetitionWindow.Value < 0))
        {
            throw new ArgumentException("outetts clone repetition_window must be at least 0", "RepetitionWindow");
        }

        if (TextChunkSize != null && (TextChunkSize.Value < 1))
        {
            throw new ArgumentException("outetts clone text_chunk_size must be at least 1", "TextChunkSize");
        }
    }

    /// <inheritdoc />
    public override Dictionary<string, string> BuildOptions()
    {
        Dictionary<string, string> options = [];

        if (MaxTokens != null)
        {
            options["max_tokens"] = MaxTokens.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Temperature != null)
        {
            options["temperature"] = Temperature.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TopK != null)
        {
            options["top_k"] = TopK.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TopP != null)
        {
            options["top_p"] = TopP.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (MinP != null)
        {
            options["min_p"] = MinP.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (RepetitionPenalty != null && RepetitionPenalty != 1.1)
        {
            options["repetition_penalty"] = RepetitionPenalty.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (RepetitionWindow != null && RepetitionWindow != 64)
        {
            options["repetition_window"] = RepetitionWindow.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Seed != null)
        {
            options["seed"] = Seed;
        }

        if (ReferenceText != null)
        {
            options["reference_text"] = ReferenceText;
        }

        if (ReferenceLanguage != null)
        {
            options["reference_language"] = ReferenceLanguage;
        }

        if (TextChunkSize != null && TextChunkSize != 256)
        {
            options["text_chunk_size"] = TextChunkSize.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TextChunkMode != null && TextChunkMode != "word_budget")
        {
            options["text_chunk_mode"] = TextChunkMode;
        }

        return options;
    }
}