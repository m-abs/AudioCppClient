using System;
using System.Collections.Generic;
using System.Globalization;

namespace Mabs.AudioCpp.Models.Tts;

/// <summary>Convenience request for the <c>audio8_tts</c> spec, tts task (model default <c>audio8_tts_preview_0_6b_q8_0</c>).</summary>
public sealed record Audio8TtsRequest : TtsRequest
{
    /// <summary>Reference transcript used with speaker reference audio.</summary>
    public string? ReferenceText { get; init; }

    /// <summary>Ordered Audio8 TTS reference conditioning pairs as a JSON array: [{"audio":"ref.wav","text":"transcript"}, ...].</summary>
    public string? MultiReferenceCond { get; init; }

    /// <summary>Maximum semantic tokens to generate; default 1024. Zero selects the default.</summary>
    public long? MaxTokens { get; init; } = 1024;

    /// <summary>Maximum characters per generated text chunk; default 200.</summary>
    public long? TextChunkSize { get; init; } = 200;

    /// <summary>Framework text chunking mode.</summary>
    public string? TextChunkMode { get; init; } = "word_budget";

    /// <summary>Top-p sampling value in (0, 1]; default 0.8.</summary>
    public double? TopP { get; init; } = 0.9;

    /// <summary>Top-k sampling value; default 30.</summary>
    public long? TopK { get; init; } = 50;

    /// <summary>Semantic-token sampling temperature in (0, 2); default 0.8.</summary>
    public double? Temperature { get; init; } = 0.7;

    /// <summary>Sampling seed for reproducible output; omitted uses a random seed.</summary>
    public string? Seed { get; init; }

    /// <summary>Language hint for Chinese-script normalization; Cantonese/yue preserves Traditional characters. Inherits structured transcript or voice language when provided.</summary>
    public new string? Language { get; init; }

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (MaxTokens != null && (MaxTokens.Value < 0))
        {
            throw new ArgumentException("audio8_tts tts max_tokens must be at least 0", "MaxTokens");
        }

        if (TextChunkSize != null && (TextChunkSize.Value < 1))
        {
            throw new ArgumentException("audio8_tts tts text_chunk_size must be at least 1", "TextChunkSize");
        }

        if (TopP != null && (TopP.Value < 0 || TopP.Value > 1))
        {
            throw new ArgumentException("audio8_tts tts top_p must be between 0 and 1", "TopP");
        }

        if (TopK != null && (TopK.Value < 1))
        {
            throw new ArgumentException("audio8_tts tts top_k must be at least 1", "TopK");
        }

        if (Temperature != null && (Temperature.Value < 0 || Temperature.Value > 2))
        {
            throw new ArgumentException("audio8_tts tts temperature must be between 0 and 2", "Temperature");
        }
    }

    /// <inheritdoc />
    public override Dictionary<string, string> BuildOptions()
    {
        Dictionary<string, string> options = [];

        if (ReferenceText != null)
        {
            options["reference_text"] = ReferenceText;
        }

        if (MultiReferenceCond != null)
        {
            options["multi_reference_cond"] = MultiReferenceCond;
        }

        if (MaxTokens != null && MaxTokens != 1024)
        {
            options["max_tokens"] = MaxTokens.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TextChunkSize != null && TextChunkSize != 200)
        {
            options["text_chunk_size"] = TextChunkSize.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TextChunkMode != null && TextChunkMode != "word_budget")
        {
            options["text_chunk_mode"] = TextChunkMode;
        }

        if (TopP != null && TopP != 0.8)
        {
            options["top_p"] = TopP.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TopK != null && TopK != 30)
        {
            options["top_k"] = TopK.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Temperature != null && Temperature != 0.8)
        {
            options["temperature"] = Temperature.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Seed != null)
        {
            options["seed"] = Seed;
        }

        if (Language != null)
        {
            options["language"] = Language;
        }

        return options;
    }
}