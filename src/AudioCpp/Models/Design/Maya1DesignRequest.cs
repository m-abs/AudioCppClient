using System;
using System.Collections.Generic;
using System.Globalization;

using Mabs.AudioCpp.Enums;

namespace Mabs.AudioCpp.Models.Design;

/// <summary>Convenience request for the <c>maya1</c> spec, design task (model default <c>maya1_orig</c>).</summary>
public sealed record Maya1DesignRequest : DesignRequest
{
    /// <summary>Maximum generated audio-token count per text chunk; default 2048.</summary>
    public long? MaxTokens { get; init; }

    /// <summary>Minimum generated audio-token count before EOS may stop generation; default 28.</summary>
    public long? MinTokens { get; init; }

    /// <summary>Autoregressive sampling temperature; default 0.4. Must be &gt; 0.</summary>
    public double? Temperature { get; init; }

    /// <summary>Autoregressive nucleus sampling probability; must be in (0, 1], default 0.9.</summary>
    public double? TopP { get; init; }

    /// <summary>Autoregressive repetition penalty; default 1.1. Must be &gt; 0.</summary>
    public double? RepetitionPenalty { get; init; }

    /// <summary>Sampling and SNAC decoder noise seed; omitted requests choose a random seed.</summary>
    public string? Seed { get; init; }

    /// <summary>Maximum Unicode codepoints per long-form text chunk; default 300.</summary>
    public long? TextChunkSize { get; init; }

    /// <summary>Framework long-form text chunking mode; tag_aware preserves inline emotion tags.</summary>
    public Maya1TextChunkMode? TextChunkMode { get; init; }

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (Instruction == null)
        {
            throw new ArgumentException("maya1 design requires instruct", "Instruction");
        }

        if (MaxTokens != null && (MaxTokens.Value < 1))
        {
            throw new ArgumentException("maya1 design max_tokens must be at least 1", "MaxTokens");
        }

        if (MinTokens != null && (MinTokens.Value < 0))
        {
            throw new ArgumentException("maya1 design min_tokens must be at least 0", "MinTokens");
        }

        if (Temperature != null && (Temperature.Value < 0))
        {
            throw new ArgumentException("maya1 design temperature must be at least 0", "Temperature");
        }

        if (TopP != null && (TopP.Value < 0 || TopP.Value > 1))
        {
            throw new ArgumentException("maya1 design top_p must be between 0 and 1", "TopP");
        }

        if (RepetitionPenalty != null && (RepetitionPenalty.Value < 0))
        {
            throw new ArgumentException("maya1 design repetition_penalty must be at least 0", "RepetitionPenalty");
        }

        if (TextChunkSize != null && (TextChunkSize.Value < 1))
        {
            throw new ArgumentException("maya1 design text_chunk_size must be at least 1", "TextChunkSize");
        }
    }

    /// <inheritdoc />
    public override Dictionary<string, string> BuildOptions()
    {
        Dictionary<string, string> options = [];

        if (Instruction != null && Instruction != "Warm female voice with an American accent and conversational pacing.")
        {
            options["instruct"] = Instruction;
        }

        if (MaxTokens != null && MaxTokens != 2048)
        {
            options["max_tokens"] = MaxTokens.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (MinTokens != null && MinTokens != 28)
        {
            options["min_tokens"] = MinTokens.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Temperature != null && Temperature != 0.4)
        {
            options["temperature"] = Temperature.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TopP != null && TopP != 0.9)
        {
            options["top_p"] = TopP.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (RepetitionPenalty != null && RepetitionPenalty != 1.1)
        {
            options["repetition_penalty"] = RepetitionPenalty.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Seed != null)
        {
            options["seed"] = Seed;
        }

        if (TextChunkSize != null && TextChunkSize != 300)
        {
            options["text_chunk_size"] = TextChunkSize.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TextChunkMode != null && TextChunkMode != Maya1TextChunkMode.TagAware)
        {
            options["text_chunk_mode"] = TextChunkMode switch
            {
            Maya1TextChunkMode.Default => "default",
            Maya1TextChunkMode.TagAware => "tag_aware",
            Maya1TextChunkMode.Japanese => "japanese",
            Maya1TextChunkMode.Endline => "endline",
            _ => throw new ArgumentException("maya1 design text_chunk_mode must be one of: default, tag_aware, japanese, endline"),
            };
        }

        return options;
    }
}