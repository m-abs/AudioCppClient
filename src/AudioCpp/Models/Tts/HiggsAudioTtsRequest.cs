using System;
using System.Collections.Generic;
using System.Globalization;

using Mabs.AudioCpp.Enums;

namespace Mabs.AudioCpp.Models.Tts;

/// <summary>Convenience request for the <c>higgs_audio_tts</c> spec, tts task (model default <c>higgs_audio_tts_4b_q8_0</c>).</summary>
public sealed record HiggsAudioTtsRequest : TtsRequest
{
    /// <summary>Streaming codec frames per event; grow starts at one and increases to this cap. Each frame is 40 ms of audio.</summary>
    public long? StreamFramesPerEvent { get; init; } = 32;

    /// <summary>Streaming chunk schedule. The non-causal codec always retains ten frames of left and right context.</summary>
    public HiggsAudioTtsStreamChunkPolicy? StreamChunkPolicy { get; init; } = HiggsAudioTtsStreamChunkPolicy.Grow;

    /// <summary>Maximum generated AR tokens; zero uses the default. Must not exceed the configured context length.</summary>
    public long? MaxTokens { get; init; } = 2048;

    /// <summary>Return generated audio with a warning instead of failing when max_tokens is reached before EOC. Audio may be truncated or contain silence/noise.</summary>
    public bool? AllowIncompleteGeneration { get; init; } = false;

    /// <summary>AR sampling temperature. Must be positive.</summary>
    public double? Temperature { get; init; } = 0.8;

    /// <summary>AR nucleus sampling probability. Must be positive.</summary>
    public double? TopP { get; init; } = 0.8;

    /// <summary>AR top-k sampling limit.</summary>
    public long? TopK { get; init; } = 30;

    /// <summary>Filter NaN and infinity during greedy and stochastic top-k selection. All-invalid logits still fail.</summary>
    public bool? FilterNanLogits { get; init; } = false;

    /// <summary>Accepted for Python API compatibility; audio sampling does not consume this value. Compatibility-only, but must be finite and positive.</summary>
    public double? RepetitionPenalty { get; init; } = 1.1;

    /// <summary>Torch sampling seed; omitted chooses a random seed.</summary>
    public string? Seed { get; init; }

    /// <summary>Transcript accompanying reference audio.</summary>
    public string? ReferenceText { get; init; } = string.Empty;

    /// <summary>Framework long-form text chunk size.</summary>
    public long? TextChunkSize { get; init; } = 1024;

    /// <summary>Framework text chunking mode.</summary>
    public HiggsAudioTtsTextChunkMode? TextChunkMode { get; init; } = HiggsAudioTtsTextChunkMode.Default;

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (StreamFramesPerEvent != null && (StreamFramesPerEvent.Value < 1))
        {
            throw new ArgumentException("higgs_audio_tts tts stream_frames_per_event must be at least 1", "StreamFramesPerEvent");
        }

        if (MaxTokens != null && (MaxTokens.Value < 0))
        {
            throw new ArgumentException("higgs_audio_tts tts max_tokens must be at least 0", "MaxTokens");
        }

        if (TopK != null && (TopK.Value < 0))
        {
            throw new ArgumentException("higgs_audio_tts tts top_k must be at least 0", "TopK");
        }

        if (TextChunkSize != null && (TextChunkSize.Value < 1))
        {
            throw new ArgumentException("higgs_audio_tts tts text_chunk_size must be at least 1", "TextChunkSize");
        }
    }

    /// <inheritdoc />
    public override Dictionary<string, string> BuildOptions()
    {
        Dictionary<string, string> options = [];

        if (StreamFramesPerEvent != null && StreamFramesPerEvent != 32)
        {
            options["stream_frames_per_event"] = StreamFramesPerEvent.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (StreamChunkPolicy != null && StreamChunkPolicy != HiggsAudioTtsStreamChunkPolicy.Grow)
        {
            options["stream_chunk_policy"] = StreamChunkPolicy switch
            {
            HiggsAudioTtsStreamChunkPolicy.Grow => "grow",
            HiggsAudioTtsStreamChunkPolicy.Fixed => "fixed",
            _ => throw new ArgumentException("higgs_audio_tts tts stream_chunk_policy must be one of: grow, fixed"),
            };
        }

        if (MaxTokens != null && MaxTokens != 2048)
        {
            options["max_tokens"] = MaxTokens.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (AllowIncompleteGeneration != null && AllowIncompleteGeneration != false)
        {
            options["allow_incomplete_generation"] = AllowIncompleteGeneration.Value ? "true" : "false";
        }

        if (Temperature != null && Temperature != 0.8)
        {
            options["temperature"] = Temperature.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TopP != null && TopP != 0.8)
        {
            options["top_p"] = TopP.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TopK != null && TopK != 30)
        {
            options["top_k"] = TopK.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (FilterNanLogits != null && FilterNanLogits != false)
        {
            options["filter_nan_logits"] = FilterNanLogits.Value ? "true" : "false";
        }

        if (RepetitionPenalty != null && RepetitionPenalty != 1.1)
        {
            options["repetition_penalty"] = RepetitionPenalty.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Seed != null)
        {
            options["seed"] = Seed;
        }

        if (ReferenceText != null && ReferenceText != string.Empty)
        {
            options["reference_text"] = ReferenceText;
        }

        if (TextChunkSize != null && TextChunkSize != 1024)
        {
            options["text_chunk_size"] = TextChunkSize.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TextChunkMode != null && TextChunkMode != HiggsAudioTtsTextChunkMode.Default)
        {
            options["text_chunk_mode"] = TextChunkMode switch
            {
            HiggsAudioTtsTextChunkMode.Default => "default",
            HiggsAudioTtsTextChunkMode.TagAware => "tag_aware",
            HiggsAudioTtsTextChunkMode.Japanese => "japanese",
            HiggsAudioTtsTextChunkMode.Endline => "endline",
            _ => throw new ArgumentException("higgs_audio_tts tts text_chunk_mode must be one of: default, tag_aware, japanese, endline"),
            };
        }

        return options;
    }
}