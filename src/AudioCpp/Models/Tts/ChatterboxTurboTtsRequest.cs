using System;
using System.Collections.Generic;
using System.Globalization;

namespace Mabs.AudioCpp.Models.Tts;

/// <summary>Convenience request for the <c>chatterbox_turbo</c> spec, tts task (model default <c>chatterbox_turbo_q8_0</c>).</summary>
public sealed record ChatterboxTurboTtsRequest : TtsRequest
{
    /// <summary>Maximum generated tokens; legacy max_new_tokens remains accepted.</summary>
    public long? MaxTokens { get; init; } = 1000;

    /// <summary>T3 sampling temperature.</summary>
    public double? Temperature { get; init; } = 0.8;

    /// <summary>T3 nucleus sampling cutoff.</summary>
    public double? TopP { get; init; } = 0.95;

    /// <summary>T3 top-k sampling limit.</summary>
    public long? TopK { get; init; } = 1000;

    /// <summary>T3 token repetition penalty.</summary>
    public double? RepetitionPenalty { get; init; } = 1.2;

    /// <summary>Generation seed.</summary>
    public string? Seed { get; init; } = "0";

    /// <summary>Maximum characters per text chunk; must be positive.</summary>
    public long? TextChunkSize { get; init; } = 128;

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (MaxTokens != null && (MaxTokens.Value < 0))
        {
            throw new ArgumentException("chatterbox_turbo tts max_tokens must be at least 0", "MaxTokens");
        }

        if (TopK != null && (TopK.Value < 0))
        {
            throw new ArgumentException("chatterbox_turbo tts top_k must be at least 0", "TopK");
        }

        if (TextChunkSize != null && (TextChunkSize.Value < 1))
        {
            throw new ArgumentException("chatterbox_turbo tts text_chunk_size must be at least 1", "TextChunkSize");
        }
    }

    /// <inheritdoc />
    public override Dictionary<string, string> BuildOptions()
    {
        Dictionary<string, string> options = [];

        if (MaxTokens != null && MaxTokens != 1000)
        {
            options["max_tokens"] = MaxTokens.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Temperature != null && Temperature != 0.8)
        {
            options["temperature"] = Temperature.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TopP != null && TopP != 0.95)
        {
            options["top_p"] = TopP.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TopK != null && TopK != 1000)
        {
            options["top_k"] = TopK.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (RepetitionPenalty != null && RepetitionPenalty != 1.2)
        {
            options["repetition_penalty"] = RepetitionPenalty.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Seed != null && Seed != "0")
        {
            options["seed"] = Seed;
        }

        if (TextChunkSize != null && TextChunkSize != 128)
        {
            options["text_chunk_size"] = TextChunkSize.Value.ToString(CultureInfo.InvariantCulture);
        }

        return options;
    }
}