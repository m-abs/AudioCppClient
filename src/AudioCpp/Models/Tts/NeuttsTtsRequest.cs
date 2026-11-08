using System;
using System.Collections.Generic;
using System.Globalization;

using Mabs.AudioCpp.Enums;

namespace Mabs.AudioCpp.Models.Tts;

/// <summary>Convenience request for the <c>neutts</c> spec, tts task (model default <c>neutts_2e_orig</c>).</summary>
public sealed record NeuttsTtsRequest : TtsRequest
{
    /// <summary>Built-in NeuTTS speaker prompt; default emily.</summary>
    public new NeuttsVoiceId? VoiceId { get; init; } = NeuttsVoiceId.Emily;

    /// <summary>Optional emotion token inserted between reference text and target text; neutral inserts no emotion token.</summary>
    public NeuttsEmotion? Emotion { get; init; } = NeuttsEmotion.Neutral;

    /// <summary>Maximum generated speech-token count for the autoregressive generator; 0 uses the remaining model context, default 0. Total generation context is capped at min(checkpoint context, 2048).</summary>
    public long? MaxTokens { get; init; } = 0;

    /// <summary>Minimum generated speech-token count before EOS may stop generation; default 50.</summary>
    public long? MinTokens { get; init; } = 50;

    /// <summary>Autoregressive sampling temperature; must be &gt; 0, default 1.0.</summary>
    public double? Temperature { get; init; } = 1;

    /// <summary>Autoregressive top-k sampling limit; default 50.</summary>
    public long? TopK { get; init; } = 50;

    /// <summary>Autoregressive sampling seed; omitted requests choose a random seed.</summary>
    public string? Seed { get; init; }

    /// <summary>Framework text chunking mode for long-form synthesis.</summary>
    public NeuttsTextChunkMode? TextChunkMode { get; init; } = NeuttsTextChunkMode.Default;

    /// <summary>Maximum Unicode codepoints per long-form text chunk; default 600.</summary>
    public long? TextChunkSize { get; init; } = 600;

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (MaxTokens != null && (MaxTokens.Value < 0))
        {
            throw new ArgumentException("neutts tts max_tokens must be at least 0", "MaxTokens");
        }

        if (MinTokens != null && (MinTokens.Value < 0))
        {
            throw new ArgumentException("neutts tts min_tokens must be at least 0", "MinTokens");
        }

        if (Temperature != null && (Temperature.Value < 0))
        {
            throw new ArgumentException("neutts tts temperature must be at least 0", "Temperature");
        }

        if (TopK != null && (TopK.Value < 1))
        {
            throw new ArgumentException("neutts tts top_k must be at least 1", "TopK");
        }

        if (TextChunkSize != null && (TextChunkSize.Value < 1))
        {
            throw new ArgumentException("neutts tts text_chunk_size must be at least 1", "TextChunkSize");
        }
    }

    /// <inheritdoc />
    public override Dictionary<string, string> BuildOptions()
    {
        Dictionary<string, string> options = [];

        if (VoiceId != null && VoiceId != NeuttsVoiceId.Emily)
        {
            options["voice_id"] = VoiceId switch
            {
            NeuttsVoiceId.Dave => "dave",
            NeuttsVoiceId.Emily => "emily",
            NeuttsVoiceId.Greta => "greta",
            NeuttsVoiceId.Jo => "jo",
            NeuttsVoiceId.Juliette => "juliette",
            NeuttsVoiceId.Mateo => "mateo",
            NeuttsVoiceId.Paul => "paul",
            NeuttsVoiceId.Sophie => "sophie",
            NeuttsVoiceId.Steven => "steven",
            _ => throw new ArgumentException("neutts tts voice_id must be one of: dave, emily, greta, jo, juliette, mateo, paul, sophie, steven"),
            };
        }

        if (Emotion != null && Emotion != NeuttsEmotion.Neutral)
        {
            options["emotion"] = Emotion switch
            {
            NeuttsEmotion.Angry => "angry",
            NeuttsEmotion.Disgusted => "disgusted",
            NeuttsEmotion.Sad => "sad",
            NeuttsEmotion.Happy => "happy",
            NeuttsEmotion.Fearful => "fearful",
            NeuttsEmotion.Neutral => "neutral",
            NeuttsEmotion.Surprised => "surprised",
            _ => throw new ArgumentException("neutts tts emotion must be one of: angry, disgusted, sad, happy, fearful, neutral, surprised"),
            };
        }

        if (MaxTokens != null && MaxTokens != 0)
        {
            options["max_tokens"] = MaxTokens.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (MinTokens != null && MinTokens != 50)
        {
            options["min_tokens"] = MinTokens.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Temperature != null && Temperature != 1)
        {
            options["temperature"] = Temperature.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TopK != null && TopK != 50)
        {
            options["top_k"] = TopK.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Seed != null)
        {
            options["seed"] = Seed;
        }

        if (TextChunkMode != null && TextChunkMode != NeuttsTextChunkMode.Default)
        {
            options["text_chunk_mode"] = TextChunkMode switch
            {
            NeuttsTextChunkMode.Default => "default",
            NeuttsTextChunkMode.TagAware => "tag_aware",
            NeuttsTextChunkMode.Japanese => "japanese",
            NeuttsTextChunkMode.Endline => "endline",
            _ => throw new ArgumentException("neutts tts text_chunk_mode must be one of: default, tag_aware, japanese, endline"),
            };
        }

        if (TextChunkSize != null && TextChunkSize != 600)
        {
            options["text_chunk_size"] = TextChunkSize.Value.ToString(CultureInfo.InvariantCulture);
        }

        return options;
    }
}