using System;
using System.Collections.Generic;
using System.Globalization;

using Mabs.AudioCpp.Enums;

namespace Mabs.AudioCpp.Models.Clone;

/// <summary>Convenience request for the <c>plapre</c> spec, clone task (model default <c>plapre_3_bf16</c>).</summary>
public sealed record PlapreCloneRequest : CloneRequest
{
    /// <summary>Built-in speaker.</summary>
    public new PlapreVoiceId? VoiceId { get; init; }

    /// <summary>Transcript of the reference audio, used for voice cloning.</summary>
    public string? ReferenceText { get; init; } = string.Empty;

    /// <summary>Use only the reference speaker embedding, without the reference transcript/audio-token prefix.</summary>
    public bool? XVectorOnlyMode { get; init; } = false;

    /// <summary>JSON array of per-word integer durations, from 0 to 399 frames at 25 Hz. Duration-controlled text is generated in one chunk.</summary>
    public string? WordDurationFrames { get; init; }

    /// <summary>Previous utterance for prosodic context.</summary>
    public string? ContextText { get; init; }

    /// <summary>Optional previous-utterance WAV accompanying context_text.</summary>
    public string? ContextAudioFile { get; init; }

    /// <summary>Whether context_text belongs to the target speaker.</summary>
    public bool? SameSpeaker { get; init; } = true;

    /// <summary>Ordered JSON array of pronunciation references: [{"word":"morgen","audio_file":"morgen.wav"}]. Up to 10 references, 60 codes each and 200 total codes are used.</summary>
    public string? Pronunciations { get; init; }

    /// <summary>Start of the masked span in voice_ref, in 25-Hz codec frames. Supply both edit boundaries and the complete desired text.</summary>
    public long? EditStartFrame { get; init; }

    /// <summary>Exclusive end of the masked span in voice_ref. Editing runs in one text chunk.</summary>
    public long? EditEndFrame { get; init; }

    /// <summary>Maximum generated tokens per text chunk; each audio token represents 40 ms.</summary>
    public long? MaxTokens { get; init; } = 500;

    /// <summary>Minimum generated tokens before a stop token can be selected.</summary>
    public long? MinTokens { get; init; } = 0;

    /// <summary>Sampling temperature; zero selects greedy decoding.</summary>
    public double? Temperature { get; init; } = 0.8;

    /// <summary>Nucleus sampling probability in (0, 1].</summary>
    public double? TopP { get; init; } = 0.95;

    /// <summary>Top-k sampling; -1 or zero disables filtering.</summary>
    public long? TopK { get; init; } = 50;

    /// <summary>Sampling and vocoder noise seed.</summary>
    public string? Seed { get; init; } = "0";

    /// <summary>Maximum Unicode codepoints per long-form text chunk.</summary>
    public long? TextChunkSize { get; init; } = 300;

    /// <summary>Framework text chunking policy.</summary>
    public PlapreTextChunkMode? TextChunkMode { get; init; } = PlapreTextChunkMode.Default;

    /// <summary>Silence inserted between text chunks.</summary>
    public double? SilenceDurationSec { get; init; } = 0.1;

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (VoiceId != null)
        {
            throw new ArgumentException("plapre clone does not accept voice_id (capability built_in_voices is false)");
        }

        if (EditStartFrame != null && (EditStartFrame.Value < 0))
        {
            throw new ArgumentException("plapre clone edit_start_frame must be at least 0", "EditStartFrame");
        }

        if (EditEndFrame != null && (EditEndFrame.Value < 0))
        {
            throw new ArgumentException("plapre clone edit_end_frame must be at least 0", "EditEndFrame");
        }

        if (MaxTokens != null && (MaxTokens.Value < 1))
        {
            throw new ArgumentException("plapre clone max_tokens must be at least 1", "MaxTokens");
        }

        if (MinTokens != null && (MinTokens.Value < 0))
        {
            throw new ArgumentException("plapre clone min_tokens must be at least 0", "MinTokens");
        }

        if (Temperature != null && (Temperature.Value < 0))
        {
            throw new ArgumentException("plapre clone temperature must be at least 0", "Temperature");
        }

        if (TopP != null && (TopP.Value < 0 || TopP.Value > 1))
        {
            throw new ArgumentException("plapre clone top_p must be between 0 and 1", "TopP");
        }

        if (TopK != null && (TopK.Value < -1))
        {
            throw new ArgumentException("plapre clone top_k must be at least -1", "TopK");
        }

        if (TextChunkSize != null && (TextChunkSize.Value < 1))
        {
            throw new ArgumentException("plapre clone text_chunk_size must be at least 1", "TextChunkSize");
        }

        if (SilenceDurationSec != null && (SilenceDurationSec.Value < 0))
        {
            throw new ArgumentException("plapre clone silence_duration_sec must be at least 0", "SilenceDurationSec");
        }
    }

    /// <inheritdoc />
    public override Dictionary<string, string> BuildOptions()
    {
        Dictionary<string, string> options = [];

        if (VoiceId != null && VoiceId != PlapreVoiceId.Tor)
        {
            options["voice_id"] = VoiceId switch
            {
            PlapreVoiceId.Tor => "tor",
            PlapreVoiceId.Ida => "ida",
            PlapreVoiceId.Liv => "liv",
            PlapreVoiceId.Ask => "ask",
            PlapreVoiceId.Kaj => "kaj",
            _ => throw new ArgumentException("plapre clone voice_id must be one of: tor, ida, liv, ask, kaj"),
            };
        }

        if (ReferenceText != null && ReferenceText != string.Empty)
        {
            options["reference_text"] = ReferenceText;
        }

        if (XVectorOnlyMode != null && XVectorOnlyMode != false)
        {
            options["x_vector_only_mode"] = XVectorOnlyMode.Value ? "true" : "false";
        }

        if (WordDurationFrames != null)
        {
            options["word_duration_frames"] = WordDurationFrames;
        }

        if (ContextText != null)
        {
            options["context_text"] = ContextText;
        }

        if (ContextAudioFile != null)
        {
            options["context_audio_file"] = ContextAudioFile;
        }

        if (SameSpeaker != null && SameSpeaker != true)
        {
            options["same_speaker"] = SameSpeaker.Value ? "true" : "false";
        }

        if (Pronunciations != null)
        {
            options["pronunciations"] = Pronunciations;
        }

        if (EditStartFrame != null)
        {
            options["edit_start_frame"] = EditStartFrame.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (EditEndFrame != null)
        {
            options["edit_end_frame"] = EditEndFrame.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (MaxTokens != null && MaxTokens != 500)
        {
            options["max_tokens"] = MaxTokens.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (MinTokens != null && MinTokens != 0)
        {
            options["min_tokens"] = MinTokens.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Temperature != null && Temperature != 0.8)
        {
            options["temperature"] = Temperature.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TopP != null && TopP != 0.95)
        {
            options["top_p"] = TopP.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TopK != null && TopK != 50)
        {
            options["top_k"] = TopK.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Seed != null && Seed != "0")
        {
            options["seed"] = Seed;
        }

        if (TextChunkSize != null && TextChunkSize != 300)
        {
            options["text_chunk_size"] = TextChunkSize.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TextChunkMode != null && TextChunkMode != PlapreTextChunkMode.Default)
        {
            options["text_chunk_mode"] = TextChunkMode switch
            {
            PlapreTextChunkMode.Default => "default",
            PlapreTextChunkMode.TagAware => "tag_aware",
            PlapreTextChunkMode.Japanese => "japanese",
            PlapreTextChunkMode.Endline => "endline",
            _ => throw new ArgumentException("plapre clone text_chunk_mode must be one of: default, tag_aware, japanese, endline"),
            };
        }

        if (SilenceDurationSec != null && SilenceDurationSec != 0.1)
        {
            options["silence_duration_sec"] = SilenceDurationSec.Value.ToString(CultureInfo.InvariantCulture);
        }

        return options;
    }
}