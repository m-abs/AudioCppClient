using System;
using System.Collections.Generic;
using System.Globalization;

namespace Mabs.AudioCpp.Models.Design;

/// <summary>Convenience request for the <c>moss_voicegen</c> spec, design task (model default <c>moss_voicegen_bf16_codec_f16_decode</c>).</summary>
public sealed record MossVoicegenDesignRequest : DesignRequest
{
    /// <summary>Minimum generated audio frames per text chunk; nonpositive values derive a text-length-based floor.</summary>
    public long? MinFrames { get; init; } = 0;

    /// <summary>Generated audio-frame ceiling per text chunk; nonpositive values derive a text-length-based ceiling. Runtime raises it to at least the effective min_frames plus the number of codebooks.</summary>
    public long? MaxFrames { get; init; } = 0;

    /// <summary>Text-token sampling temperature. Must be finite and positive when sampling.</summary>
    public double? TextTemperature { get; init; } = 1.5;

    /// <summary>Optional full language name, such as English or Chinese; not an ISO language code.</summary>
    public new string? Language { get; init; }

    /// <summary>Generation seed; later text chunks add their chunk index. Parsed as signed 32-bit integer then converted to uint32; negative values wrap.</summary>
    public string? Seed { get; init; } = "0";

    /// <summary>Audio-token sampling temperature. Must be finite and positive when sampling.</summary>
    public double? Temperature { get; init; } = 1.5;

    /// <summary>Audio-token nucleus sampling cutoff.</summary>
    public double? TopP { get; init; } = 0.6;

    /// <summary>Audio-token top-k sampling limit.</summary>
    public long? TopK { get; init; } = 50;

    /// <summary>Audio-token repetition penalty. Must be finite and positive when history is nonempty.</summary>
    public double? RepetitionPenalty { get; init; } = 1.1;

    /// <summary>Maximum characters per generated text chunk; must be positive.</summary>
    public long? TextChunkSize { get; init; } = 200;

    /// <summary>Text chunking policy.</summary>
    public string? TextChunkMode { get; init; } = "word_budget";

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (TextChunkSize != null && (TextChunkSize.Value < 1))
        {
            throw new ArgumentException("moss_voicegen design text_chunk_size must be at least 1", "TextChunkSize");
        }
    }

    /// <inheritdoc />
    public override Dictionary<string, string> BuildOptions()
    {
        Dictionary<string, string> options = [];

        if (MinFrames != null && MinFrames != 0)
        {
            options["min_frames"] = MinFrames.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (MaxFrames != null && MaxFrames != 0)
        {
            options["max_frames"] = MaxFrames.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Instruction != null)
        {
            options["instruct"] = Instruction;
        }

        if (TextTemperature != null && TextTemperature != 1.5)
        {
            options["text_temperature"] = TextTemperature.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Language != null)
        {
            options["language"] = Language;
        }

        if (Seed != null && Seed != "0")
        {
            options["seed"] = Seed;
        }

        if (Temperature != null && Temperature != 1.5)
        {
            options["temperature"] = Temperature.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TopP != null && TopP != 0.6)
        {
            options["top_p"] = TopP.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TopK != null && TopK != 50)
        {
            options["top_k"] = TopK.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (RepetitionPenalty != null && RepetitionPenalty != 1.1)
        {
            options["repetition_penalty"] = RepetitionPenalty.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TextChunkSize != null && TextChunkSize != 200)
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