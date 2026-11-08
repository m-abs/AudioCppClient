using System;
using System.Collections.Generic;
using System.Globalization;

namespace Mabs.AudioCpp.Models.Tts;

/// <summary>Convenience request for the <c>xtts_v2</c> spec, tts task (model default <c>xtts_v2_gguf_q8_0</c>).</summary>
public sealed record XttsV2TtsRequest : TtsRequest
{
    /// <summary>Source language for text conditioning (e.g. 'en'); also selects tokenizer normalization.</summary>
    public new string? Language { get; init; } = "en";

    /// <summary>AR sampling temperature; official default 0.75.</summary>
    public double? Temperature { get; init; } = 0.75;

    /// <summary>AR top-k sampling; official default 50.</summary>
    public long? TopK { get; init; } = 50;

    /// <summary>AR nucleus sampling threshold; official default 0.85.</summary>
    public double? TopP { get; init; } = 0.85;

    /// <summary>AR repetition penalty; official default 10.0.</summary>
    public double? RepetitionPenalty { get; init; } = 10;

    /// <summary>Maximum mel codes to generate; official default 605.</summary>
    public long? MaxMelTokens { get; init; } = 605;

    /// <summary>Sampling seed; omitted selects a random seed.</summary>
    public string? Seed { get; init; }

    /// <summary>Maximum Unicode codepoints per framework text chunk; default 256.</summary>
    public long? TextChunkSize { get; init; } = 256;

    /// <summary>Framework text chunking mode.</summary>
    public string? TextChunkMode { get; init; } = "word_budget";

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (Temperature != null && (Temperature.Value < 0))
        {
            throw new ArgumentException("xtts_v2 tts temperature must be at least 0", "Temperature");
        }

        if (TopK != null && (TopK.Value < 0))
        {
            throw new ArgumentException("xtts_v2 tts top_k must be at least 0", "TopK");
        }

        if (TopP != null && (TopP.Value < 0 || TopP.Value > 1))
        {
            throw new ArgumentException("xtts_v2 tts top_p must be between 0 and 1", "TopP");
        }

        if (RepetitionPenalty != null && (RepetitionPenalty.Value < 1))
        {
            throw new ArgumentException("xtts_v2 tts repetition_penalty must be at least 1", "RepetitionPenalty");
        }

        if (MaxMelTokens != null && (MaxMelTokens.Value < 1))
        {
            throw new ArgumentException("xtts_v2 tts max_mel_tokens must be at least 1", "MaxMelTokens");
        }

        if (TextChunkSize != null && (TextChunkSize.Value < 1))
        {
            throw new ArgumentException("xtts_v2 tts text_chunk_size must be at least 1", "TextChunkSize");
        }
    }

    /// <inheritdoc />
    public override Dictionary<string, string> BuildOptions()
    {
        Dictionary<string, string> options = [];

        if (Language != null && Language != "en")
        {
            options["language"] = Language;
        }

        if (Temperature != null && Temperature != 0.75)
        {
            options["temperature"] = Temperature.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TopK != null && TopK != 50)
        {
            options["top_k"] = TopK.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TopP != null && TopP != 0.85)
        {
            options["top_p"] = TopP.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (RepetitionPenalty != null && RepetitionPenalty != 10)
        {
            options["repetition_penalty"] = RepetitionPenalty.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (MaxMelTokens != null && MaxMelTokens != 605)
        {
            options["max_mel_tokens"] = MaxMelTokens.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Seed != null)
        {
            options["seed"] = Seed;
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