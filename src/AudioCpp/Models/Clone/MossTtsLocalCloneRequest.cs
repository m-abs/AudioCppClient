using System;
using System.Collections.Generic;
using System.Globalization;

using Mabs.AudioCpp.Enums;

namespace Mabs.AudioCpp.Models.Clone;

/// <summary>Convenience request for the <c>moss_tts_local</c> spec, clone task (model default <c>moss_tts_local_v1_5_q8_0</c>).</summary>
public sealed record MossTtsLocalCloneRequest : CloneRequest
{
    /// <summary>Maximum generated audio frames.</summary>
    public long? MaxTokens { get; init; } = 4096;

    /// <summary>Enable sampling.</summary>
    public bool? DoSample { get; init; } = true;

    /// <summary>Audio sampling temperature. Must be finite and positive when sampling.</summary>
    public double? Temperature { get; init; } = 1.7;

    /// <summary>Audio nucleus sampling probability.</summary>
    public double? TopP { get; init; } = 0.8;

    /// <summary>Audio top-k sampling limit.</summary>
    public long? TopK { get; init; } = 25;

    /// <summary>Audio repetition penalty. Must be finite and positive when token history is nonempty.</summary>
    public double? RepetitionPenalty { get; init; } = 1;

    /// <summary>Text sampling temperature. Must be finite and positive when sampling.</summary>
    public double? TextTemperature { get; init; } = 1;

    /// <summary>Text nucleus sampling probability.</summary>
    public double? TextTopP { get; init; } = 1;

    /// <summary>Text top-k sampling limit.</summary>
    public long? TextTopK { get; init; } = 50;

    /// <summary>Sampling seed; omitted chooses a random seed.</summary>
    public string? Seed { get; init; }

    /// <summary>Framework long-form text chunk size.</summary>
    public long? TextChunkSize { get; init; } = 2048;

    /// <summary>Framework text chunking mode.</summary>
    public MossTtsLocalTextChunkMode? TextChunkMode { get; init; } = MossTtsLocalTextChunkMode.Default;

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (MaxTokens != null && (MaxTokens.Value < 1))
        {
            throw new ArgumentException("moss_tts_local clone max_tokens must be at least 1", "MaxTokens");
        }

        if (TextChunkSize != null && (TextChunkSize.Value < 1))
        {
            throw new ArgumentException("moss_tts_local clone text_chunk_size must be at least 1", "TextChunkSize");
        }
    }

    /// <inheritdoc />
    public override Dictionary<string, string> BuildOptions()
    {
        Dictionary<string, string> options = [];

        if (MaxTokens != null && MaxTokens != 4096)
        {
            options["max_tokens"] = MaxTokens.Value.ToString(CultureInfo.InvariantCulture);
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

        if (TextTemperature != null && TextTemperature != 1)
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

        if (TextChunkSize != null && TextChunkSize != 2048)
        {
            options["text_chunk_size"] = TextChunkSize.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TextChunkMode != null && TextChunkMode != MossTtsLocalTextChunkMode.Default)
        {
            options["text_chunk_mode"] = TextChunkMode switch
            {
            MossTtsLocalTextChunkMode.Default => "default",
            MossTtsLocalTextChunkMode.TagAware => "tag_aware",
            MossTtsLocalTextChunkMode.Japanese => "japanese",
            MossTtsLocalTextChunkMode.Endline => "endline",
            _ => throw new ArgumentException("moss_tts_local clone text_chunk_mode must be one of: default, tag_aware, japanese, endline"),
            };
        }

        return options;
    }
}