using System;
using System.Collections.Generic;
using System.Globalization;

namespace Mabs.AudioCpp.Models.Tts;

/// <summary>Convenience request for the <c>vieneu_v3_turbo</c> spec, tts task (model default <c>vieneu_v3_turbo_q8_0</c>).</summary>
public sealed record VieneuV3TurboTtsRequest : TtsRequest
{
    /// <summary>Cap the generation budget using the chunk's phoneme count. Legacy frame_cap remains accepted.</summary>
    public bool? AutoFrameCap { get; init; } = true;

    /// <summary>Phoneme-string chunks shorter than this character count join a neighbour. Legacy text_chunk_min remains accepted.</summary>
    public long? TextChunkMinSize { get; init; } = 20;

    /// <summary>File containing a comma-separated 192-dimensional speaker embedding.</summary>
    public string? SpeakerEmbeddingFile { get; init; }

    /// <summary>Inline comma-separated 192-dimensional speaker embedding.</summary>
    public string? SpeakerEmbedding { get; init; }

    /// <summary>Pre-encoded reference codes in row-major order, comma- or space-separated; codebooks per frame come from the checkpoint.</summary>
    public string? ReferenceCodes { get; init; }

    /// <summary>Pre-encoded reference codes, one frame per line. Replaces audio encoding and can be used with a speaker embedding instead of reference audio.</summary>
    public string? ReferenceCodesFile { get; init; }

    /// <summary>Clone using only the speaker embedding. Defaults to true for embedding-only input, otherwise false.</summary>
    public bool? XVectorOnlyMode { get; init; } = false;

    /// <summary>Encode reference audio and return an acoustic-tokens artifact without generating speech.</summary>
    public bool? EncodeReferenceOnly { get; init; } = false;

    /// <summary>Maximum generated frames per text chunk; uses the checkpoint max_new_tokens setting (300 fallback). Each frame represents 80 ms.</summary>
    public long? MaxTokens { get; init; }

    /// <summary>Enable sampling for all codebooks unless subtalker_do_sample overrides later codebooks.</summary>
    public bool? DoSample { get; init; } = true;

    /// <summary>Sampling temperature for all codebooks unless overridden for the subtalker.</summary>
    public double? Temperature { get; init; } = 0.8;

    /// <summary>Top-k sampling limit for all codebooks unless overridden for the subtalker.</summary>
    public long? TopK { get; init; } = 25;

    /// <summary>Nucleus cutoff within top-k candidates, unless overridden for the subtalker.</summary>
    public double? TopP { get; init; } = 0.95;

    /// <summary>Repetition penalty applied independently to each codebook.</summary>
    public double? RepetitionPenalty { get; init; } = 1.2;

    /// <summary>Recent frames remembered per codebook for repetition penalties; zero uses all history.</summary>
    public long? RepetitionWindow { get; init; } = 64;

    /// <summary>Regenerations for short chunks that exceed their text; zero disables the guard.</summary>
    public long? BabbleRetries { get; init; } = 2;

    /// <summary>Temperature override for later codebooks; omitted or negative inherits temperature.</summary>
    public double? SubtalkerTemperature { get; init; }

    /// <summary>Top-k override for later codebooks; omitted or negative inherits top_k.</summary>
    public long? SubtalkerTopK { get; init; }

    /// <summary>Nucleus cutoff override for later codebooks; omitted or negative inherits top_p.</summary>
    public double? SubtalkerTopP { get; init; }

    /// <summary>Sampling override for later codebooks; inherits do_sample when omitted.</summary>
    public bool? SubtalkerDoSample { get; init; } = false;

    /// <summary>Sampling seed; omitted draws a random seed.</summary>
    public string? Seed { get; init; }

    /// <summary>Maximum character budget per phoneme-string chunk; must be positive.</summary>
    public long? TextChunkSize { get; init; } = 200;

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (MaxTokens != null && (MaxTokens.Value < 1))
        {
            throw new ArgumentException("vieneu_v3_turbo tts max_tokens must be at least 1", "MaxTokens");
        }

        if (RepetitionWindow != null && (RepetitionWindow.Value < 0))
        {
            throw new ArgumentException("vieneu_v3_turbo tts repetition_window must be at least 0", "RepetitionWindow");
        }

        if (BabbleRetries != null && (BabbleRetries.Value < 0))
        {
            throw new ArgumentException("vieneu_v3_turbo tts babble_retries must be at least 0", "BabbleRetries");
        }

        if (TextChunkSize != null && (TextChunkSize.Value < 1))
        {
            throw new ArgumentException("vieneu_v3_turbo tts text_chunk_size must be at least 1", "TextChunkSize");
        }
    }

    /// <inheritdoc />
    public override Dictionary<string, string> BuildOptions()
    {
        Dictionary<string, string> options = [];

        if (AutoFrameCap != null && AutoFrameCap != true)
        {
            options["auto_frame_cap"] = AutoFrameCap.Value ? "true" : "false";
        }

        if (TextChunkMinSize != null && TextChunkMinSize != 20)
        {
            options["text_chunk_min_size"] = TextChunkMinSize.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (SpeakerEmbeddingFile != null)
        {
            options["speaker_embedding_file"] = SpeakerEmbeddingFile;
        }

        if (SpeakerEmbedding != null)
        {
            options["speaker_embedding"] = SpeakerEmbedding;
        }

        if (ReferenceCodes != null)
        {
            options["reference_codes"] = ReferenceCodes;
        }

        if (ReferenceCodesFile != null)
        {
            options["reference_codes_file"] = ReferenceCodesFile;
        }

        if (XVectorOnlyMode != null && XVectorOnlyMode != false)
        {
            options["x_vector_only_mode"] = XVectorOnlyMode.Value ? "true" : "false";
        }

        if (EncodeReferenceOnly != null && EncodeReferenceOnly != false)
        {
            options["encode_reference_only"] = EncodeReferenceOnly.Value ? "true" : "false";
        }

        if (MaxTokens != null)
        {
            options["max_tokens"] = MaxTokens.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (DoSample != null && DoSample != true)
        {
            options["do_sample"] = DoSample.Value ? "true" : "false";
        }

        if (Temperature != null && Temperature != 0.8)
        {
            options["temperature"] = Temperature.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TopK != null && TopK != 25)
        {
            options["top_k"] = TopK.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TopP != null && TopP != 0.95)
        {
            options["top_p"] = TopP.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (RepetitionPenalty != null && RepetitionPenalty != 1.2)
        {
            options["repetition_penalty"] = RepetitionPenalty.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (RepetitionWindow != null && RepetitionWindow != 64)
        {
            options["repetition_window"] = RepetitionWindow.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (BabbleRetries != null && BabbleRetries != 2)
        {
            options["babble_retries"] = BabbleRetries.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (SubtalkerTemperature != null)
        {
            options["subtalker_temperature"] = SubtalkerTemperature.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (SubtalkerTopK != null)
        {
            options["subtalker_top_k"] = SubtalkerTopK.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (SubtalkerTopP != null)
        {
            options["subtalker_top_p"] = SubtalkerTopP.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (SubtalkerDoSample != null && SubtalkerDoSample != false)
        {
            options["subtalker_do_sample"] = SubtalkerDoSample.Value ? "true" : "false";
        }

        if (Seed != null)
        {
            options["seed"] = Seed;
        }

        if (TextChunkSize != null && TextChunkSize != 200)
        {
            options["text_chunk_size"] = TextChunkSize.Value.ToString(CultureInfo.InvariantCulture);
        }

        return options;
    }
}