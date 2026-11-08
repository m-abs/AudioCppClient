using System;
using System.Collections.Generic;
using System.Globalization;

using Mabs.AudioCpp.Enums;

namespace Mabs.AudioCpp.Models.Tts;

/// <summary>Convenience request for the <c>lfm3_audio</c> spec, tts task (model default <c>lfm3_audio_2b_q8_0</c>).</summary>
public sealed record Lfm3AudioTtsRequest : TtsRequest
{
    /// <summary>ASR: ISO code of the spoken language (prompt 'Perform ASR in &lt;English name&gt;.'); auto lets the model infer it ('Perform ASR.'). TTS: picks the default voice of that language when no voice is given.</summary>
    public new string? Language { get; init; }

    /// <summary>ASR: text tokens per audio chunk (max(512, 8 per second of audio); chat 512, or 2048 with enable_thinking). TTS: audio frames (80 ms) per text chunk (512). S2S: text tokens and frames of the reply together (1024).</summary>
    public long? MaxTokens { get; init; }

    /// <summary>ASR chunking, as lfm2_audio.</summary>
    public Lfm3AudioAudioChunkMode? AudioChunkMode { get; init; }

    /// <summary>ASR: longest chunk in seconds; auto keeps input up to this long whole and splits longer input at VAD pauses.</summary>
    public double? AudioChunkDurationSec { get; init; }

    /// <summary>S2S: most audio frames after the reply text ends before the turn is closed with an end-of-audio frame; 0 disables the cap.</summary>
    public long? MaxAudioTailFrames { get; init; }

    /// <summary>TTS and S2S: audio code temperature; 0 picks the most likely code. The default is the checkpoint's generation_config.json audio_temperature.</summary>
    public double? Temperature { get; init; }

    /// <summary>TTS and S2S: sample audio codes from the k most likely; 0 keeps all. The default is the checkpoint's audio_top_k.</summary>
    public long? TopK { get; init; }

    /// <summary>TTS and S2S: audio repetition penalty over the last 4096 frames (the checkpoint's audio_repetition_window) of each codebook; must be positive, 1 disables it. The default is the checkpoint's audio_repetition_penalty.</summary>
    public double? RepetitionPenalty { get; init; }

    /// <summary>ASR and S2S: text token temperature; 0 is greedy.</summary>
    public double? TextTemperature { get; init; }

    /// <summary>ASR and S2S: sample text tokens from the k most likely; 0 keeps all.</summary>
    public long? TextTopK { get; init; }

    /// <summary>Sampling seed (random when absent); TTS text chunk i uses seed + i.</summary>
    public string? Seed { get; init; }

    /// <summary>TTS long-form text chunking (values as text::parse_text_chunk_mode_override accepts).</summary>
    public Lfm3AudioTextChunkMode? TextChunkMode { get; init; }

    /// <summary>TTS: target characters per text chunk.</summary>
    public long? TextChunkSize { get; init; }

    /// <summary>TTS and S2S streaming: audio frames per event.</summary>
    public long? StreamFramesPerEvent { get; init; }

    /// <summary>S2S: return the lfm3_audio.reply artifact a client sends back with the next turn.</summary>
    public bool? ReturnCodes { get; init; }

    /// <summary>ASR task: answer the spoken input as a text chat reply instead of transcribing it.</summary>
    public bool? Chat { get; init; }

    /// <summary>ASR chat: let the model think first; only the text after &lt;/think&gt; is returned.</summary>
    public bool? EnableThinking { get; init; }

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (MaxTokens != null && (MaxTokens.Value < 1))
        {
            throw new ArgumentException("lfm3_audio tts max_tokens must be at least 1", "MaxTokens");
        }

        if (AudioChunkDurationSec != null && (AudioChunkDurationSec.Value < 1))
        {
            throw new ArgumentException("lfm3_audio tts audio_chunk_duration_sec must be at least 1", "AudioChunkDurationSec");
        }

        if (MaxAudioTailFrames != null && (MaxAudioTailFrames.Value < 0))
        {
            throw new ArgumentException("lfm3_audio tts max_audio_tail_frames must be at least 0", "MaxAudioTailFrames");
        }

        if (Temperature != null && (Temperature.Value < 0))
        {
            throw new ArgumentException("lfm3_audio tts temperature must be at least 0", "Temperature");
        }

        if (TopK != null && (TopK.Value < 0))
        {
            throw new ArgumentException("lfm3_audio tts top_k must be at least 0", "TopK");
        }

        if (RepetitionPenalty != null && (RepetitionPenalty.Value < 0.01))
        {
            throw new ArgumentException("lfm3_audio tts repetition_penalty must be at least 0.01", "RepetitionPenalty");
        }

        if (TextTemperature != null && (TextTemperature.Value < 0))
        {
            throw new ArgumentException("lfm3_audio tts text_temperature must be at least 0", "TextTemperature");
        }

        if (TextTopK != null && (TextTopK.Value < 0))
        {
            throw new ArgumentException("lfm3_audio tts text_top_k must be at least 0", "TextTopK");
        }

        if (TextChunkSize != null && (TextChunkSize.Value < 1))
        {
            throw new ArgumentException("lfm3_audio tts text_chunk_size must be at least 1", "TextChunkSize");
        }

        if (StreamFramesPerEvent != null && (StreamFramesPerEvent.Value < 1))
        {
            throw new ArgumentException("lfm3_audio tts stream_frames_per_event must be at least 1", "StreamFramesPerEvent");
        }
    }

    /// <inheritdoc />
    public override Dictionary<string, string> BuildOptions()
    {
        Dictionary<string, string> options = [];

        if (Language != null && Language != "auto")
        {
            options["language"] = Language;
        }

        if (MaxTokens != null)
        {
            options["max_tokens"] = MaxTokens.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (AudioChunkMode != null && AudioChunkMode != Lfm3AudioAudioChunkMode.Auto)
        {
            options["audio_chunk_mode"] = AudioChunkMode switch
            {
            Lfm3AudioAudioChunkMode.Auto => "auto",
            Lfm3AudioAudioChunkMode.Fixed => "fixed",
            Lfm3AudioAudioChunkMode.Vad => "vad",
            Lfm3AudioAudioChunkMode.None => "none",
            _ => throw new ArgumentException("lfm3_audio tts audio_chunk_mode must be one of: auto, fixed, vad, none"),
            };
        }

        if (AudioChunkDurationSec != null && AudioChunkDurationSec != 300)
        {
            options["audio_chunk_duration_sec"] = AudioChunkDurationSec.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (MaxAudioTailFrames != null && MaxAudioTailFrames != 375)
        {
            options["max_audio_tail_frames"] = MaxAudioTailFrames.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Temperature != null && Temperature != 1)
        {
            options["temperature"] = Temperature.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TopK != null && TopK != 128)
        {
            options["top_k"] = TopK.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (RepetitionPenalty != null && RepetitionPenalty != 1.1)
        {
            options["repetition_penalty"] = RepetitionPenalty.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TextTemperature != null && TextTemperature != 0)
        {
            options["text_temperature"] = TextTemperature.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TextTopK != null && TextTopK != 0)
        {
            options["text_top_k"] = TextTopK.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Seed != null)
        {
            options["seed"] = Seed;
        }

        if (TextChunkMode != null)
        {
            options["text_chunk_mode"] = TextChunkMode switch
            {
            Lfm3AudioTextChunkMode.Default => "default",
            Lfm3AudioTextChunkMode.TagAware => "tag_aware",
            Lfm3AudioTextChunkMode.Japanese => "japanese",
            Lfm3AudioTextChunkMode.Endline => "endline",
            _ => throw new ArgumentException("lfm3_audio tts text_chunk_mode must be one of: default, tag_aware, japanese, endline"),
            };
        }

        if (TextChunkSize != null && TextChunkSize != 200)
        {
            options["text_chunk_size"] = TextChunkSize.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (StreamFramesPerEvent != null && StreamFramesPerEvent != 1)
        {
            options["stream_frames_per_event"] = StreamFramesPerEvent.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (ReturnCodes != null && ReturnCodes != false)
        {
            options["return_codes"] = ReturnCodes.Value ? "true" : "false";
        }

        if (Chat != null && Chat != false)
        {
            options["chat"] = Chat.Value ? "true" : "false";
        }

        if (EnableThinking != null && EnableThinking != false)
        {
            options["enable_thinking"] = EnableThinking.Value ? "true" : "false";
        }

        return options;
    }
}