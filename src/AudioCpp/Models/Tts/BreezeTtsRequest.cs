using System;
using System.Collections.Generic;
using System.Globalization;

using Mabs.AudioCpp.Enums;

namespace Mabs.AudioCpp.Models.Tts;

/// <summary>Convenience request for the <c>breeze_tts</c> spec, tts task (model default <c>breeze_tts_2_q8_0</c>).</summary>
public sealed record BreezeTtsRequest : TtsRequest
{
    /// <summary>BreezeTTS generation instruction.</summary>
    public string? Instruct { get; init; }

    /// <summary>Transcript for prompt/reference audio; required when reference audio codes are used, for either tts or clon.</summary>
    public string? ReferenceText { get; init; }

    /// <summary>Maximum Unicode codepoints per long-form text chunk; default 600.</summary>
    public long? TextChunkSize { get; init; }

    /// <summary>Framework text chunking mode.</summary>
    public BreezeTtsTextChunkMode? TextChunkMode { get; init; }

    /// <summary>Maximum generated BreezeTTS acoustic frames.</summary>
    public long? MaxTokens { get; init; }

    /// <summary>Classifier-free guidance scale.</summary>
    public double? GuidanceScale { get; init; }

    /// <summary>Backbone first-codebook sampling temperature. Must be &gt; 0.</summary>
    public double? Temperature { get; init; }

    /// <summary>Depth decoder codebook sampling temperature. Must be &gt; 0.</summary>
    public double? DepthTemperature { get; init; }

    /// <summary>Top-k sampling limit; 0 disables top-k filtering. Nonpositive values disable top-k filtering.</summary>
    public long? TopK { get; init; }

    /// <summary>Positive nucleus sampling cutoff; values &gt;=1 disable nucleus filtering.</summary>
    public double? TopP { get; init; }

    /// <summary>Generation seed.</summary>
    public string? Seed { get; init; }

    /// <summary>Maximum generated codec frames per streaming audio event; fixed size with stream_chunk_policy=fixed.</summary>
    public long? StreamFramesPerEvent { get; init; }

    /// <summary>Additional trailing codec frames buffered before streaming emission. The decoder is causal; zero minimizes latency.</summary>
    public long? StreamLookaheadMargin { get; init; }

    /// <summary>Streaming chunk schedule: grow emits 1, 2, 4, ... frames up to stream_frames_per_event; fixed uses that size from the first event.</summary>
    public BreezeTtsStreamChunkPolicy? StreamChunkPolicy { get; init; }

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (TextChunkSize != null && (TextChunkSize.Value < 1))
        {
            throw new ArgumentException("breeze_tts tts text_chunk_size must be at least 1", "TextChunkSize");
        }

        if (MaxTokens != null && (MaxTokens.Value < 1))
        {
            throw new ArgumentException("breeze_tts tts max_tokens must be at least 1", "MaxTokens");
        }

        if (GuidanceScale != null && (GuidanceScale.Value < 0))
        {
            throw new ArgumentException("breeze_tts tts guidance_scale must be at least 0", "GuidanceScale");
        }

        if (Temperature != null && (Temperature.Value < 0))
        {
            throw new ArgumentException("breeze_tts tts temperature must be at least 0", "Temperature");
        }

        if (DepthTemperature != null && (DepthTemperature.Value < 0))
        {
            throw new ArgumentException("breeze_tts tts depth_temperature must be at least 0", "DepthTemperature");
        }

        if (TopP != null && (TopP.Value < 0))
        {
            throw new ArgumentException("breeze_tts tts top_p must be at least 0", "TopP");
        }

        if (StreamFramesPerEvent != null && (StreamFramesPerEvent.Value < 1))
        {
            throw new ArgumentException("breeze_tts tts stream_frames_per_event must be at least 1", "StreamFramesPerEvent");
        }

        if (StreamLookaheadMargin != null && (StreamLookaheadMargin.Value < 0))
        {
            throw new ArgumentException("breeze_tts tts stream_lookahead_margin must be at least 0", "StreamLookaheadMargin");
        }
    }

    /// <inheritdoc />
    public override Dictionary<string, string> BuildOptions()
    {
        Dictionary<string, string> options = [];

        if (Instruct != null && Instruct != "Speak clearly and naturally.")
        {
            options["instruct"] = Instruct;
        }

        if (ReferenceText != null)
        {
            options["reference_text"] = ReferenceText;
        }

        if (TextChunkSize != null && TextChunkSize != 600)
        {
            options["text_chunk_size"] = TextChunkSize.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TextChunkMode != null && TextChunkMode != BreezeTtsTextChunkMode.Default)
        {
            options["text_chunk_mode"] = TextChunkMode switch
            {
            BreezeTtsTextChunkMode.Default => "default",
            BreezeTtsTextChunkMode.TagAware => "tag_aware",
            BreezeTtsTextChunkMode.Japanese => "japanese",
            BreezeTtsTextChunkMode.Endline => "endline",
            _ => throw new ArgumentException("breeze_tts tts text_chunk_mode must be one of: default, tag_aware, japanese, endline"),
            };
        }

        if (MaxTokens != null && MaxTokens != 1500)
        {
            options["max_tokens"] = MaxTokens.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (GuidanceScale != null && GuidanceScale != 1)
        {
            options["guidance_scale"] = GuidanceScale.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Temperature != null && Temperature != 0.9)
        {
            options["temperature"] = Temperature.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (DepthTemperature != null && DepthTemperature != 0.9)
        {
            options["depth_temperature"] = DepthTemperature.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TopK != null && TopK != 50)
        {
            options["top_k"] = TopK.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TopP != null && TopP != 1)
        {
            options["top_p"] = TopP.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Seed != null && Seed != "0")
        {
            options["seed"] = Seed;
        }

        if (StreamFramesPerEvent != null && StreamFramesPerEvent != 16)
        {
            options["stream_frames_per_event"] = StreamFramesPerEvent.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (StreamLookaheadMargin != null && StreamLookaheadMargin != 0)
        {
            options["stream_lookahead_margin"] = StreamLookaheadMargin.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (StreamChunkPolicy != null && StreamChunkPolicy != BreezeTtsStreamChunkPolicy.Grow)
        {
            options["stream_chunk_policy"] = StreamChunkPolicy switch
            {
            BreezeTtsStreamChunkPolicy.Grow => "grow",
            BreezeTtsStreamChunkPolicy.Fixed => "fixed",
            _ => throw new ArgumentException("breeze_tts tts stream_chunk_policy must be one of: grow, fixed"),
            };
        }

        return options;
    }
}