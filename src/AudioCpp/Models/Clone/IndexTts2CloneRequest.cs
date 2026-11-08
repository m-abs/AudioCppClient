using System;
using System.Collections.Generic;
using System.Globalization;

using Mabs.AudioCpp.Enums;

namespace Mabs.AudioCpp.Models.Clone;

/// <summary>Convenience request for the <c>index_tts2</c> spec, clone task (model default <c>index_tts2_q8_0</c>).</summary>
public sealed record IndexTts2CloneRequest : CloneRequest
{
    /// <summary>Fallback target text when text_input is absent; prompt is also accepted.</summary>
    public new string? Text { get; init; }

    /// <summary>IndexTTS2.5 text language; empty or auto selects from text.</summary>
    public new string? Language { get; init; }

    /// <summary>Emotion conditioning blend strength.</summary>
    public double? EmotionAlpha { get; init; } = 1;

    /// <summary>Eight comma-separated finite emotion strengths.</summary>
    public string? EmotionVector { get; init; }

    /// <summary>Use text-derived emotion conditioning.</summary>
    public bool? UseEmotionText { get; init; } = false;

    /// <summary>Emotion description.</summary>
    public string? EmotionText { get; init; } = string.Empty;

    /// <summary>Use random emotion conditioning.</summary>
    public bool? UseRandomEmotion { get; init; } = false;

    /// <summary>Silence between generated segments in milliseconds.</summary>
    public long? IntervalSilenceMs { get; init; } = 200;

    /// <summary>Enable sampling.</summary>
    public bool? DoSample { get; init; } = true;

    /// <summary>Nucleus sampling probability. Must be in (0, 1].</summary>
    public double? TopP { get; init; } = 0.8;

    /// <summary>Top-k sampling limit.</summary>
    public long? TopK { get; init; } = 30;

    /// <summary>Sampling temperature. Must be positive.</summary>
    public double? Temperature { get; init; } = 0.8;

    /// <summary>Beam search length penalty.</summary>
    public double? LengthPenalty { get; init; } = 0;

    /// <summary>Beam search width.</summary>
    public long? NumBeams { get; init; } = 3;

    /// <summary>Sampling repetition penalty.</summary>
    public double? RepetitionPenalty { get; init; } = 10;

    /// <summary>Maximum generated mel tokens.</summary>
    public long? MaxTokens { get; init; } = 1500;

    /// <summary>Positive waveform duration rescaling, supported by both IndexTTS2 and IndexTTS2.5.</summary>
    public double? DurationScale { get; init; } = 1;

    /// <summary>Sampling seed; omitted chooses a random seed.</summary>
    public string? Seed { get; init; }

    /// <summary>Optional framework long-form text chunk size; omitted keeps the full input together. Must be positive.</summary>
    public long? TextChunkSize { get; init; }

    /// <summary>Framework text chunking mode.</summary>
    public IndexTts2TextChunkMode? TextChunkMode { get; init; } = IndexTts2TextChunkMode.Default;

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (EmotionAlpha != null && (EmotionAlpha.Value < 0 || EmotionAlpha.Value > 1))
        {
            throw new ArgumentException("index_tts2 clone emotion_alpha must be between 0 and 1", "EmotionAlpha");
        }

        if (IntervalSilenceMs != null && (IntervalSilenceMs.Value < 0))
        {
            throw new ArgumentException("index_tts2 clone interval_silence_ms must be at least 0", "IntervalSilenceMs");
        }

        if (TopK != null && (TopK.Value < 1))
        {
            throw new ArgumentException("index_tts2 clone top_k must be at least 1", "TopK");
        }

        if (NumBeams != null && (NumBeams.Value < 1))
        {
            throw new ArgumentException("index_tts2 clone num_beams must be at least 1", "NumBeams");
        }

        if (MaxTokens != null && (MaxTokens.Value < 1))
        {
            throw new ArgumentException("index_tts2 clone max_tokens must be at least 1", "MaxTokens");
        }
    }

    /// <inheritdoc />
    public override Dictionary<string, string> BuildOptions()
    {
        Dictionary<string, string> options = [];

        if (Text != null)
        {
            options["text"] = Text;
        }

        if (Language != null)
        {
            options["language"] = Language;
        }

        if (EmotionAlpha != null && EmotionAlpha != 1)
        {
            options["emotion_alpha"] = EmotionAlpha.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (EmotionVector != null)
        {
            options["emotion_vector"] = EmotionVector;
        }

        if (UseEmotionText != null && UseEmotionText != false)
        {
            options["use_emotion_text"] = UseEmotionText.Value ? "true" : "false";
        }

        if (EmotionText != null)
        {
            options["emotion_text"] = EmotionText;
        }

        if (UseRandomEmotion != null && UseRandomEmotion != false)
        {
            options["use_random_emotion"] = UseRandomEmotion.Value ? "true" : "false";
        }

        if (IntervalSilenceMs != null && IntervalSilenceMs != 200)
        {
            options["interval_silence_ms"] = IntervalSilenceMs.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (DoSample != null && DoSample != true)
        {
            options["do_sample"] = DoSample.Value ? "true" : "false";
        }

        if (TopP != null && TopP != 0.8)
        {
            options["top_p"] = TopP.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TopK != null && TopK != 30)
        {
            options["top_k"] = TopK.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Temperature != null && Temperature != 0.8)
        {
            options["temperature"] = Temperature.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (LengthPenalty != null && LengthPenalty != 0)
        {
            options["length_penalty"] = LengthPenalty.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (NumBeams != null && NumBeams != 3)
        {
            options["num_beams"] = NumBeams.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (RepetitionPenalty != null && RepetitionPenalty != 10)
        {
            options["repetition_penalty"] = RepetitionPenalty.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (MaxTokens != null && MaxTokens != 1500)
        {
            options["max_tokens"] = MaxTokens.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (DurationScale != null && DurationScale != 1)
        {
            options["duration_scale"] = DurationScale.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Seed != null)
        {
            options["seed"] = Seed;
        }

        if (TextChunkSize != null)
        {
            options["text_chunk_size"] = TextChunkSize.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TextChunkMode != null && TextChunkMode != IndexTts2TextChunkMode.Default)
        {
            options["text_chunk_mode"] = TextChunkMode switch
            {
            IndexTts2TextChunkMode.Default => "default",
            IndexTts2TextChunkMode.TagAware => "tag_aware",
            IndexTts2TextChunkMode.Japanese => "japanese",
            IndexTts2TextChunkMode.Endline => "endline",
            _ => throw new ArgumentException("index_tts2 clone text_chunk_mode must be one of: default, tag_aware, japanese, endline"),
            };
        }

        return options;
    }
}