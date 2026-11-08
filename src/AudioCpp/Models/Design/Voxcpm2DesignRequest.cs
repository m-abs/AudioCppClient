using System;
using System.Collections.Generic;
using System.Globalization;

using Mabs.AudioCpp.Enums;

namespace Mabs.AudioCpp.Models.Design;

/// <summary>Convenience request for the <c>voxcpm2</c> spec, design task (model default <c>voxcpm2_q8_0</c>).</summary>
public sealed record Voxcpm2DesignRequest : DesignRequest
{
    /// <summary>Minimum generated audio tokens.</summary>
    public long? MinTokens { get; init; }

    /// <summary>Maximum generated audio tokens. Zero resolves to model max_length; requires min_tokens &lt;= resolved max_tokens &lt;= model limit.</summary>
    public long? MaxTokens { get; init; }

    /// <summary>Flow sampling steps. Must be positive.</summary>
    public long? NumInferenceSteps { get; init; }

    /// <summary>Classifier-free guidance scale. Must be nonnegative.</summary>
    public double? GuidanceScale { get; init; }

    /// <summary>Retry detected bad generations. Offline only; streaming requires false.</summary>
    public bool? RetryBadcase { get; init; }

    /// <summary>Total attempts including the initial attempt, not extra retries; must be positive.</summary>
    public long? RetryBadcaseMaxTimes { get; init; }

    /// <summary>Bad-case audio/text length ratio threshold. Must be positive.</summary>
    public double? RetryBadcaseRatioThreshold { get; init; }

    /// <summary>Sampling seed.</summary>
    public string? Seed { get; init; }

    /// <summary>Precomputed flow noise file.</summary>
    public string? CfmNoiseFile { get; init; }

    /// <summary>Streaming decoder cadence: grow emits one patch first, then doubles toward stream_frames_per_event; fixed uses the configured size. Ignored offline.</summary>
    public Voxcpm2StreamChunkPolicy? StreamChunkPolicy { get; init; }

    /// <summary>Maximum generated latent patches per streaming decoder event. Set to 1 for one-patch emission. Ignored offline.</summary>
    public long? StreamFramesPerEvent { get; init; }

    /// <summary>Previous patches decoded as streaming left context.</summary>
    public long? StreamLeftContext { get; init; }

    /// <summary>Use the voice reference itself as the continuation prompt. Requires reference audio and reference_text or prompt_text; cannot be combined with explicit prompt audio.</summary>
    public bool? UltimateClone { get; init; }

    /// <summary>Transcript for continuation prompt audio, distinct from the base voice reference.</summary>
    public string? PromptText { get; init; }

    /// <summary>Long-form continuation or independent generation per chunk. Offline only.</summary>
    public Voxcpm2ChunkStrategy? ChunkStrategy { get; init; }

    /// <summary>Framework long-form text chunk size. Offline only.</summary>
    public long? TextChunkSize { get; init; }

    /// <summary>Framework text chunking mode. Offline only.</summary>
    public Voxcpm2TextChunkMode? TextChunkMode { get; init; }

    /// <inheritdoc />
    public override string? ComposedText()
    {
        return Instruction == null ? Text : $"({Instruction}) {Text}";
    }

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (MinTokens != null && (MinTokens.Value < 0))
        {
            throw new ArgumentException("voxcpm2 design min_tokens must be at least 0", "MinTokens");
        }

        if (MaxTokens != null && (MaxTokens.Value < 0))
        {
            throw new ArgumentException("voxcpm2 design max_tokens must be at least 0", "MaxTokens");
        }

        if (StreamFramesPerEvent != null && (StreamFramesPerEvent.Value < 1))
        {
            throw new ArgumentException("voxcpm2 design stream_frames_per_event must be at least 1", "StreamFramesPerEvent");
        }

        if (StreamLeftContext != null && (StreamLeftContext.Value < 0 || StreamLeftContext.Value > 8))
        {
            throw new ArgumentException("voxcpm2 design stream_left_context must be between 0 and 8", "StreamLeftContext");
        }
    }

    /// <inheritdoc />
    public override Dictionary<string, string> BuildOptions()
    {
        Dictionary<string, string> options = [];

        if (MinTokens != null && MinTokens != 2)
        {
            options["min_tokens"] = MinTokens.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (MaxTokens != null && MaxTokens != 4096)
        {
            options["max_tokens"] = MaxTokens.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (NumInferenceSteps != null && NumInferenceSteps != 10)
        {
            options["num_inference_steps"] = NumInferenceSteps.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (GuidanceScale != null && GuidanceScale != 2)
        {
            options["guidance_scale"] = GuidanceScale.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (RetryBadcase != null && RetryBadcase != true)
        {
            options["retry_badcase"] = RetryBadcase.Value ? "true" : "false";
        }

        if (RetryBadcaseMaxTimes != null && RetryBadcaseMaxTimes != 3)
        {
            options["retry_badcase_max_times"] = RetryBadcaseMaxTimes.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (RetryBadcaseRatioThreshold != null && RetryBadcaseRatioThreshold != 6)
        {
            options["retry_badcase_ratio_threshold"] = RetryBadcaseRatioThreshold.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Seed != null && Seed != "1234")
        {
            options["seed"] = Seed;
        }

        if (CfmNoiseFile != null)
        {
            options["cfm_noise_file"] = CfmNoiseFile;
        }

        if (StreamChunkPolicy != null && StreamChunkPolicy != Voxcpm2StreamChunkPolicy.Grow)
        {
            options["stream_chunk_policy"] = StreamChunkPolicy switch
            {
            Voxcpm2StreamChunkPolicy.Grow => "grow",
            Voxcpm2StreamChunkPolicy.Fixed => "fixed",
            _ => throw new ArgumentException("voxcpm2 design stream_chunk_policy must be one of: grow, fixed"),
            };
        }

        if (StreamFramesPerEvent != null && StreamFramesPerEvent != 4)
        {
            options["stream_frames_per_event"] = StreamFramesPerEvent.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (StreamLeftContext != null && StreamLeftContext != 3)
        {
            options["stream_left_context"] = StreamLeftContext.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (UltimateClone != null && UltimateClone != false)
        {
            options["ultimate_clone"] = UltimateClone.Value ? "true" : "false";
        }

        if (PromptText != null)
        {
            options["prompt_text"] = PromptText;
        }

        if (ChunkStrategy != null && ChunkStrategy != Voxcpm2ChunkStrategy.Continuation)
        {
            options["chunk_strategy"] = ChunkStrategy switch
            {
            Voxcpm2ChunkStrategy.Continuation => "continuation",
            Voxcpm2ChunkStrategy.Stateless => "stateless",
            _ => throw new ArgumentException("voxcpm2 design chunk_strategy must be one of: continuation, stateless"),
            };
        }

        if (TextChunkSize != null && TextChunkSize != 2048)
        {
            options["text_chunk_size"] = TextChunkSize.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TextChunkMode != null && TextChunkMode != Voxcpm2TextChunkMode.TagAware)
        {
            options["text_chunk_mode"] = TextChunkMode switch
            {
            Voxcpm2TextChunkMode.Default => "default",
            Voxcpm2TextChunkMode.TagAware => "tag_aware",
            Voxcpm2TextChunkMode.Japanese => "japanese",
            Voxcpm2TextChunkMode.Endline => "endline",
            _ => throw new ArgumentException("voxcpm2 design text_chunk_mode must be one of: default, tag_aware, japanese, endline"),
            };
        }

        return options;
    }
}