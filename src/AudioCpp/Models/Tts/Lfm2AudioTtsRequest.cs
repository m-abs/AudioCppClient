using System;
using System.Collections.Generic;
using System.Globalization;

using Mabs.AudioCpp.Enums;

namespace Mabs.AudioCpp.Models.Tts;

/// <summary>Convenience request for the <c>lfm2_audio</c> spec, tts task (model default <c>lfm2_audio_1_5b_q8_0</c>).</summary>
public sealed record Lfm2AudioTtsRequest : TtsRequest
{
    /// <summary>Must match the checkpoint's language (en or ja) when given; each published checkpoint transcribes and speaks one language.</summary>
    public new string? Language { get; init; } = "auto";

    /// <summary>ASR: transcript tokens per audio chunk, 512 by default. TTS: audio frames (80 ms each) per text chunk, 512 by default. S2S: text tokens and audio frames of the reply together, 1024 by default. A transcript, a text chunk's speech or a reply that reaches it is cut off there, with a warning on stderr.</summary>
    public long? MaxTokens { get; init; } = 512;

    /// <summary>Audio chunking mode. auto transcribes input up to audio_chunk_duration_sec whole and splits longer input at pauses found by the bundled VAD (fixed chunks when the VAD model is absent); vad always splits at pauses and skips silence; fixed cuts every audio_chunk_duration_sec; none transcribes the whole input at once.</summary>
    public Lfm2AudioAudioChunkMode? AudioChunkMode { get; init; } = Lfm2AudioAudioChunkMode.Auto;

    /// <summary>Longest chunk in seconds, at least 1. With fixed chunks, a final piece shorter than 1 second and the chunk before it split their audio evenly.</summary>
    public double? AudioChunkDurationSec { get; init; } = 30;

    /// <summary>TTS and S2S: audio code sampling temperature, 0.8 by default for TTS and 1.0 for S2S; 0 picks the most likely code.</summary>
    public double? Temperature { get; init; }

    /// <summary>TTS and S2S: sample audio codes from the k most likely, 64 by default for TTS and 4 for S2S; 0 keeps all, 1 is greedy.</summary>
    public long? TopK { get; init; }

    /// <summary>S2S: text token sampling temperature; 0, the default, picks the most likely token, as liquid-audio's README and demo do.</summary>
    public double? TextTemperature { get; init; } = 0;

    /// <summary>S2S: with a text_temperature above 0, sample text tokens from the k most likely; 0, the default, keeps all, and 1 is greedy.</summary>
    public long? TextTopK { get; init; } = 0;

    /// <summary>TTS and S2S: sampling seed; omitted requests choose a random seed. TTS text chunk i uses seed + i. S2S text sampling draws from a stream of its own, so with the same seed a reply follows the greedy-text reply until a text token differs.</summary>
    public string? Seed { get; init; }

    /// <summary>TTS: long-form text chunking mode; japanese for the Japanese checkpoint and default otherwise when omitted.</summary>
    public Lfm2AudioTextChunkMode? TextChunkMode { get; init; }

    /// <summary>TTS: maximum Unicode codepoints per text chunk, each spoken as its own turn.</summary>
    public long? TextChunkSize { get; init; } = 200;

    /// <summary>TTS and S2S streaming: audio frames (80 ms each) per event; each event's audio is final. ASR runs offline only.</summary>
    public long? StreamFramesPerEvent { get; init; } = 1;

    /// <summary>S2S: also return the reply's text tokens and audio codes as an lfm2_audio.reply artifact, to send back as history with the conversation's next turn.</summary>
    public bool? ReturnCodes { get; init; } = false;

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (MaxTokens != null && (MaxTokens.Value < 1))
        {
            throw new ArgumentException("lfm2_audio tts max_tokens must be at least 1", "MaxTokens");
        }

        if (AudioChunkDurationSec != null && (AudioChunkDurationSec.Value < 1))
        {
            throw new ArgumentException("lfm2_audio tts audio_chunk_duration_sec must be at least 1", "AudioChunkDurationSec");
        }

        if (Temperature != null && (Temperature.Value < 0))
        {
            throw new ArgumentException("lfm2_audio tts temperature must be at least 0", "Temperature");
        }

        if (TopK != null && (TopK.Value < 0))
        {
            throw new ArgumentException("lfm2_audio tts top_k must be at least 0", "TopK");
        }

        if (TextTemperature != null && (TextTemperature.Value < 0))
        {
            throw new ArgumentException("lfm2_audio tts text_temperature must be at least 0", "TextTemperature");
        }

        if (TextTopK != null && (TextTopK.Value < 0))
        {
            throw new ArgumentException("lfm2_audio tts text_top_k must be at least 0", "TextTopK");
        }

        if (TextChunkSize != null && (TextChunkSize.Value < 1))
        {
            throw new ArgumentException("lfm2_audio tts text_chunk_size must be at least 1", "TextChunkSize");
        }

        if (StreamFramesPerEvent != null && (StreamFramesPerEvent.Value < 1))
        {
            throw new ArgumentException("lfm2_audio tts stream_frames_per_event must be at least 1", "StreamFramesPerEvent");
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

        if (AudioChunkMode != null && AudioChunkMode != Lfm2AudioAudioChunkMode.Auto)
        {
            options["audio_chunk_mode"] = AudioChunkMode switch
            {
            Lfm2AudioAudioChunkMode.Auto => "auto",
            Lfm2AudioAudioChunkMode.Fixed => "fixed",
            Lfm2AudioAudioChunkMode.Vad => "vad",
            Lfm2AudioAudioChunkMode.None => "none",
            _ => throw new ArgumentException("lfm2_audio tts audio_chunk_mode must be one of: auto, fixed, vad, none"),
            };
        }

        if (AudioChunkDurationSec != null && AudioChunkDurationSec != 30)
        {
            options["audio_chunk_duration_sec"] = AudioChunkDurationSec.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Temperature != null)
        {
            options["temperature"] = Temperature.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TopK != null)
        {
            options["top_k"] = TopK.Value.ToString(CultureInfo.InvariantCulture);
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
            Lfm2AudioTextChunkMode.Default => "default",
            Lfm2AudioTextChunkMode.TagAware => "tag_aware",
            Lfm2AudioTextChunkMode.Japanese => "japanese",
            Lfm2AudioTextChunkMode.Endline => "endline",
            _ => throw new ArgumentException("lfm2_audio tts text_chunk_mode must be one of: default, tag_aware, japanese, endline"),
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

        return options;
    }
}