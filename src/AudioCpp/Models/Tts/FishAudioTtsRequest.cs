using System;
using System.Collections.Generic;
using System.Globalization;

namespace Mabs.AudioCpp.Models.Tts;

/// <summary>Convenience request for the <c>fish_audio</c> spec, tts task (model default <c>fish_audio_s2_pro_q8_0</c>).</summary>
public sealed record FishAudioTtsRequest : TtsRequest
{
    /// <summary>Reference transcript; required with inline reference audio.</summary>
    public string? ReferenceText { get; init; }

    /// <summary>Fish Audio ordered JSON array of reference conditioning pairs for one generated speaker: [{"audio":"ref1.wav","text":"Transcript one."},{"audio":"ref2.wav","text":"Transcript two."}]. This is not a multi-speaker dialogue routing option.</summary>
    public string? MultiReferenceCond { get; init; }

    /// <summary>Maximum Fish Audio semantic tokens to generate; default 1024. Zero selects the default token budget.</summary>
    public long? MaxTokens { get; init; } = 1024;

    /// <summary>Maximum characters per generated text chunk; default 200.</summary>
    public long? TextChunkSize { get; init; } = 200;

    /// <summary>Framework text chunking mode; default word_budget.</summary>
    public string? TextChunkMode { get; init; } = "word_budget";

    /// <summary>Top-p sampling value in (0, 1]; default 0.8.</summary>
    public double? TopP { get; init; } = 0.8;

    /// <summary>Top-k sampling value; default 30.</summary>
    public long? TopK { get; init; } = 30;

    /// <summary>Semantic-token sampling temperature in (0, 2); default 0.8.</summary>
    public double? Temperature { get; init; } = 0.8;

    /// <summary>Sampling seed for reproducible output; omitted uses a random seed.</summary>
    public string? Seed { get; init; }

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (MaxTokens != null && (MaxTokens.Value < 0))
        {
            throw new ArgumentException("fish_audio tts max_tokens must be at least 0", "MaxTokens");
        }

        if (TextChunkSize != null && (TextChunkSize.Value < 1))
        {
            throw new ArgumentException("fish_audio tts text_chunk_size must be at least 1", "TextChunkSize");
        }

        if (TopP != null && (TopP.Value < 0 || TopP.Value > 1))
        {
            throw new ArgumentException("fish_audio tts top_p must be between 0 and 1", "TopP");
        }

        if (TopK != null && (TopK.Value < 1))
        {
            throw new ArgumentException("fish_audio tts top_k must be at least 1", "TopK");
        }

        if (Temperature != null && (Temperature.Value < 0 || Temperature.Value > 2))
        {
            throw new ArgumentException("fish_audio tts temperature must be between 0 and 2", "Temperature");
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

        return options;
    }
}