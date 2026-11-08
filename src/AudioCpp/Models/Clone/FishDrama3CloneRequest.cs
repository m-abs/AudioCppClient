using System;
using System.Collections.Generic;
using System.Globalization;

namespace Mabs.AudioCpp.Models.Clone;

/// <summary>Convenience request for the <c>fish_drama3</c> spec, clone task (model default <c>fish_drama3_q4_experts_q8_backbone</c>).</summary>
public sealed record FishDrama3CloneRequest : CloneRequest
{
    /// <summary>Prepare reference VQ codes without generating speech. Use the task API to receive drama3_reference_v1 acoustic-token artifacts.</summary>
    public bool? EncodeReferenceOnly { get; init; }

    /// <summary>Transcript of inline reference audio.</summary>
    public string? ReferenceText { get; init; }

    /// <summary>Ordered references as JSON: [{text, audio_path or vq_codes}]. Speaker tags 0..15 select voices.</summary>
    public string? MultiReferenceCond { get; init; }

    /// <summary>Alias of multi_reference_cond.</summary>
    public string? References { get; init; }

    /// <summary>Continuation/edit history as a JSON array of {text, vq_codes or audio_path}; a string generates that earlier take first.</summary>
    public string? PreviousTakes { get; init; }

    /// <summary>JSON history file containing a take object or an ordered array of earlier takes. Avoids the Windows command-line length limit for VQ grids.</summary>
    public string? PreviousTakesFile { get; init; }

    /// <summary>Maximum generated acoustic frames per text chunk (about 21.5 frames/second).</summary>
    public long? MaxTokens { get; init; }

    /// <summary>Maximum characters per long-form chunk.</summary>
    public long? TextChunkSize { get; init; }

    /// <summary>Skip text chunks that emit EOS before producing audio, matching upstream. The request fails if all chunks are empty.</summary>
    public bool? SkipEmptyChunks { get; init; }

    /// <summary>Framework text chunking mode.</summary>
    public string? TextChunkMode { get; init; }

    /// <summary>Main and residual sampling temperature. Zero makes the initial draws greedy; semantic repeat redraws retain the fixed Drama 3 RAS settings.</summary>
    public double? Temperature { get; init; }

    /// <summary>Inclusive nucleus cutoff before top-k filtering, matching Drama 3.</summary>
    public double? TopP { get; init; }

    /// <summary>Top-k candidate count. The default 4097 keeps every candidate (no cap), as Fish's Drama 3 Space does; 30 can stall in long silences.</summary>
    public long? TopK { get; init; }

    /// <summary>Reproducible native sampling seed.</summary>
    public string? Seed { get; init; }

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (MaxTokens != null && (MaxTokens.Value < 1))
        {
            throw new ArgumentException("fish_drama3 clone max_tokens must be at least 1", "MaxTokens");
        }

        if (TextChunkSize != null && (TextChunkSize.Value < 1))
        {
            throw new ArgumentException("fish_drama3 clone text_chunk_size must be at least 1", "TextChunkSize");
        }

        if (Temperature != null && (Temperature.Value < 0))
        {
            throw new ArgumentException("fish_drama3 clone temperature must be at least 0", "Temperature");
        }

        if (TopP != null && (TopP.Value < 0 || TopP.Value > 1))
        {
            throw new ArgumentException("fish_drama3 clone top_p must be between 0 and 1", "TopP");
        }

        if (TopK != null && (TopK.Value < 1))
        {
            throw new ArgumentException("fish_drama3 clone top_k must be at least 1", "TopK");
        }
    }

    /// <inheritdoc />
    public override Dictionary<string, string> BuildOptions()
    {
        Dictionary<string, string> options = [];

        if (EncodeReferenceOnly != null && EncodeReferenceOnly != false)
        {
            options["encode_reference_only"] = EncodeReferenceOnly.Value ? "true" : "false";
        }

        if (ReferenceText != null)
        {
            options["reference_text"] = ReferenceText;
        }

        if (MultiReferenceCond != null)
        {
            options["multi_reference_cond"] = MultiReferenceCond;
        }

        if (References != null)
        {
            options["references"] = References;
        }

        if (PreviousTakes != null)
        {
            options["previous_takes"] = PreviousTakes;
        }

        if (PreviousTakesFile != null)
        {
            options["previous_takes_file"] = PreviousTakesFile;
        }

        if (MaxTokens != null && MaxTokens != 1024)
        {
            options["max_tokens"] = MaxTokens.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TextChunkSize != null && TextChunkSize != 128)
        {
            options["text_chunk_size"] = TextChunkSize.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (SkipEmptyChunks != null && SkipEmptyChunks != false)
        {
            options["skip_empty_chunks"] = SkipEmptyChunks.Value ? "true" : "false";
        }

        if (TextChunkMode != null && TextChunkMode != "tag_aware")
        {
            options["text_chunk_mode"] = TextChunkMode;
        }

        if (Temperature != null && Temperature != 1)
        {
            options["temperature"] = Temperature.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TopP != null && TopP != 0.9)
        {
            options["top_p"] = TopP.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TopK != null && TopK != 4097)
        {
            options["top_k"] = TopK.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Seed != null)
        {
            options["seed"] = Seed;
        }

        return options;
    }
}