using System;
using System.Collections.Generic;
using System.Globalization;

using Mabs.AudioCpp.Enums;

namespace Mabs.AudioCpp.Models.Tts;

/// <summary>Convenience request for the <c>moss_tts_v15</c> spec, tts task (model default <c>moss_tts_v15_q8_0_codec_f16</c>).</summary>
public sealed record MossTtsV15TtsRequest : TtsRequest
{
    /// <summary>Minimum generated audio frames per text chunk; nonpositive values derive a text-length-based floor. Separate from duration_frames conditioning.</summary>
    public long? MinFrames { get; init; }

    /// <summary>Generated audio-frame ceiling per text chunk; nonpositive values derive a text-length-based ceiling. Runtime raises it to at least the effective min_frames plus the number of codebooks.</summary>
    public long? MaxFrames { get; init; }

    /// <summary>Voice description. Followed only loosely on this checkpoint; a reference recording is far more reliable.</summary>
    public string? Instruct { get; init; }

    /// <summary>Duration budget in codec frames at 12.5 a second, the model's own '- Tokens:' field. Nonpositive values disable duration conditioning.</summary>
    public long? DurationFrames { get; init; }

    /// <summary>Full language name; the model does not understand codes like 'en'.</summary>
    public new string? Language { get; init; }

    /// <summary>Reproduces a take exactly; default 0. Parsed as signed 32-bit integer then converted to uint32; negative values wrap.</summary>
    public string? Seed { get; init; }

    /// <summary>Text-token sampling temperature. Must be finite and positive when sampling.</summary>
    public double? TextTemperature { get; init; }

    /// <summary>Audio sampling temperature; default 1.5. Must be finite and positive when sampling.</summary>
    public double? Temperature { get; init; }

    /// <summary>Audio nucleus sampling cutoff; default 0.6.</summary>
    public double? TopP { get; init; }

    /// <summary>Audio top-k; default 50.</summary>
    public long? TopK { get; init; }

    /// <summary>Audio repetition penalty; default 1.1. Must be finite and positive when history is nonempty.</summary>
    public double? RepetitionPenalty { get; init; }

    /// <summary>Maximum Unicode codepoints per long-form text chunk; default 200.</summary>
    public long? TextChunkSize { get; init; }

    /// <summary>Text chunking policy; word_budget also accepts default.</summary>
    public MossTtsV15TextChunkMode? TextChunkMode { get; init; }

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (TextChunkSize != null && (TextChunkSize.Value < 1))
        {
            throw new ArgumentException("moss_tts_v15 tts text_chunk_size must be at least 1", "TextChunkSize");
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

        if (Instruct != null && Instruct != string.Empty)
        {
            options["instruct"] = Instruct;
        }

        if (DurationFrames != null)
        {
            options["duration_frames"] = DurationFrames.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Language != null && Language != string.Empty)
        {
            options["language"] = Language;
        }

        if (Seed != null && Seed != "0")
        {
            options["seed"] = Seed;
        }

        if (TextTemperature != null && TextTemperature != 1.5)
        {
            options["text_temperature"] = TextTemperature.Value.ToString(CultureInfo.InvariantCulture);
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

        if (TextChunkMode != null && TextChunkMode != MossTtsV15TextChunkMode.WordBudget)
        {
            options["text_chunk_mode"] = TextChunkMode switch
            {
            MossTtsV15TextChunkMode.WordBudget => "word_budget",
            MossTtsV15TextChunkMode.TagAware => "tag_aware",
            MossTtsV15TextChunkMode.Japanese => "japanese",
            MossTtsV15TextChunkMode.Endline => "endline",
            _ => throw new ArgumentException("moss_tts_v15 tts text_chunk_mode must be one of: word_budget, tag_aware, japanese, endline"),
            };
        }

        return options;
    }
}