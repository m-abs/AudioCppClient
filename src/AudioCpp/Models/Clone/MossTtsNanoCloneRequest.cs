using System;
using System.Collections.Generic;
using System.Globalization;

using Mabs.AudioCpp.Enums;

namespace Mabs.AudioCpp.Models.Clone;

/// <summary>Convenience request for the <c>moss_tts_nano</c> spec, clone task (model default <c>moss_tts_nano_100m_q8_0</c>).</summary>
public sealed record MossTtsNanoCloneRequest : CloneRequest
{
    /// <summary>Transcript for reference audio; a nonempty value requires voice reference audio.</summary>
    public string? ReferenceText { get; init; } = string.Empty;

    /// <summary>Maximum generated audio frames.</summary>
    public long? MaxTokens { get; init; } = 300;

    /// <summary>Active codebooks; defaults to the checkpoint n_vq. Must not exceed checkpoint n_vq; omitted uses n_vq.</summary>
    public long? ActiveCodebooks { get; init; }

    /// <summary>Enable sampling.</summary>
    public bool? DoSample { get; init; } = true;

    /// <summary>Audio sampling temperature. Must be positive, including when do_sample=false.</summary>
    public double? Temperature { get; init; } = 1.7;

    /// <summary>Audio nucleus sampling probability.</summary>
    public double? TopP { get; init; } = 0.8;

    /// <summary>Audio top-k sampling limit.</summary>
    public long? TopK { get; init; } = 25;

    /// <summary>Audio repetition penalty. Must be positive.</summary>
    public double? RepetitionPenalty { get; init; } = 1;

    /// <summary>Text sampling temperature. Must be positive, including when do_sample=false.</summary>
    public double? TextTemperature { get; init; } = 1.5;

    /// <summary>Text nucleus sampling probability.</summary>
    public double? TextTopP { get; init; } = 1;

    /// <summary>Text top-k sampling limit.</summary>
    public long? TextTopK { get; init; } = 50;

    /// <summary>Sampling seed; omitted chooses a random seed.</summary>
    public string? Seed { get; init; }

    /// <summary>Framework long-form text chunk size; default 50 when hanzi, kana and Hangul outnumber other letters and digits, otherwise 256.</summary>
    public long? TextChunkSize { get; init; }

    /// <summary>Framework text chunking mode.</summary>
    public MossTtsNanoTextChunkMode? TextChunkMode { get; init; } = MossTtsNanoTextChunkMode.Default;

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (MaxTokens != null && (MaxTokens.Value < 1))
        {
            throw new ArgumentException("moss_tts_nano clone max_tokens must be at least 1", "MaxTokens");
        }

        if (ActiveCodebooks != null && (ActiveCodebooks.Value < 1))
        {
            throw new ArgumentException("moss_tts_nano clone active_codebooks must be at least 1", "ActiveCodebooks");
        }

        if (TopP != null && (TopP.Value < 0 || TopP.Value > 1))
        {
            throw new ArgumentException("moss_tts_nano clone top_p must be between 0 and 1", "TopP");
        }

        if (TopK != null && (TopK.Value < 0))
        {
            throw new ArgumentException("moss_tts_nano clone top_k must be at least 0", "TopK");
        }

        if (TextTopP != null && (TextTopP.Value < 0 || TextTopP.Value > 1))
        {
            throw new ArgumentException("moss_tts_nano clone text_top_p must be between 0 and 1", "TextTopP");
        }

        if (TextTopK != null && (TextTopK.Value < 0))
        {
            throw new ArgumentException("moss_tts_nano clone text_top_k must be at least 0", "TextTopK");
        }

        if (TextChunkSize != null && (TextChunkSize.Value < 1))
        {
            throw new ArgumentException("moss_tts_nano clone text_chunk_size must be at least 1", "TextChunkSize");
        }
    }

    /// <inheritdoc />
    public override Dictionary<string, string> BuildOptions()
    {
        Dictionary<string, string> options = [];

        if (ReferenceText != null && ReferenceText != string.Empty)
        {
            options["reference_text"] = ReferenceText;
        }

        if (MaxTokens != null && MaxTokens != 300)
        {
            options["max_tokens"] = MaxTokens.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (ActiveCodebooks != null)
        {
            options["active_codebooks"] = ActiveCodebooks.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (DoSample != null && DoSample != true)
        {
            options["do_sample"] = DoSample.Value ? "true" : "false";
        }

        if (Temperature != null && Temperature != 1.7)
        {
            options["temperature"] = Temperature.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TopP != null && TopP != 0.8)
        {
            options["top_p"] = TopP.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TopK != null && TopK != 25)
        {
            options["top_k"] = TopK.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (RepetitionPenalty != null && RepetitionPenalty != 1)
        {
            options["repetition_penalty"] = RepetitionPenalty.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TextTemperature != null && TextTemperature != 1.5)
        {
            options["text_temperature"] = TextTemperature.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TextTopP != null && TextTopP != 1)
        {
            options["text_top_p"] = TextTopP.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TextTopK != null && TextTopK != 50)
        {
            options["text_top_k"] = TextTopK.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Seed != null)
        {
            options["seed"] = Seed;
        }

        if (TextChunkSize != null)
        {
            options["text_chunk_size"] = TextChunkSize.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TextChunkMode != null && TextChunkMode != MossTtsNanoTextChunkMode.Default)
        {
            options["text_chunk_mode"] = TextChunkMode switch
            {
            MossTtsNanoTextChunkMode.Default => "default",
            MossTtsNanoTextChunkMode.TagAware => "tag_aware",
            MossTtsNanoTextChunkMode.Japanese => "japanese",
            MossTtsNanoTextChunkMode.Endline => "endline",
            _ => throw new ArgumentException("moss_tts_nano clone text_chunk_mode must be one of: default, tag_aware, japanese, endline"),
            };
        }

        return options;
    }
}