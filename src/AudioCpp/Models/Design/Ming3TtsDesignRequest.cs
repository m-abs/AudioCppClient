using System;
using System.Collections.Generic;
using System.Globalization;

using Mabs.AudioCpp.Enums;

namespace Mabs.AudioCpp.Models.Design;

/// <summary>Convenience request for the <c>ming3_tts</c> spec, design task (model default <c>ming_3_0_tts_0_5b_bf16</c>).</summary>
public sealed record Ming3TtsDesignRequest : DesignRequest
{
    /// <summary>Conditioning template; defaults to voice_design on that route, otherwise tts.</summary>
    public Ming3TtsTemplateName? TemplateName { get; init; }

    /// <summary>Target language name or code; defaults to the request's language (the server's top-level language field, the CLI's --language). Required by multilingual; ignored by dialect.</summary>
    public new string? Language { get; init; }

    /// <summary>Structured instruction field language; defaults to en for dialect, otherwise zh.</summary>
    public Ming3TtsInstructionSchema? InstructionSchema { get; init; }

    /// <summary>Categorical speaking speed for the basic template, not waveform resampling.</summary>
    public Ming3TtsSpeed? Speed { get; init; }

    /// <summary>Categorical pitch for the basic template.</summary>
    public Ming3TtsPitch? Pitch { get; init; }

    /// <summary>Categorical volume for the basic template.</summary>
    public Ming3TtsVolume? Volume { get; init; }

    /// <summary>Emotion label for the emotion template.</summary>
    public string? Emotion { get; init; }

    /// <summary>Dialect for the dialect template.</summary>
    public Ming3TtsDialect? Dialect { get; init; }

    /// <summary>Built-in English voice for the ip template.</summary>
    public new Ming3TtsVoiceId? VoiceId { get; init; }

    /// <summary>Transcript accompanying voice_ref; enables the acoustic reference prefix. Without a transcript, only speaker conditioning is used.</summary>
    public string? ReferenceText { get; init; }

    /// <summary>CAMPPlus embedding file or list, with shape [192] or [1, 192]. Supersedes voice_ref audio; zero_speaker_embedding takes precedence.</summary>
    public string? SpeakerEmbeddingFile { get; init; }

    /// <summary>Export the enabled voice_ref CAMPPlus speaker embedding as F32 Safetensors. Requires voice_ref, use_speaker_embedding=true, zero_speaker_embedding=false and no speaker_embedding_file. Does not export acoustic latents or the reference transcript.</summary>
    public string? ExportSpeakerEmbeddingFile { get; init; }

    /// <summary>Condition on the reference speaker embedding.</summary>
    public bool? UseSpeakerEmbedding { get; init; } = true;

    /// <summary>Use a zero projected speaker row instead of reference audio conditioning.</summary>
    public bool? ZeroSpeakerEmbedding { get; init; } = false;

    /// <summary>Maximum generated acoustic patches per text chunk, also limited by the 512-position generation context.</summary>
    public long? MaxTokens { get; init; } = 1000;

    /// <summary>Flow guidance: conditioned prediction plus guidance_scale times its difference from the null prediction.</summary>
    public double? GuidanceScale { get; init; } = 2;

    /// <summary>Flow SDE noise scale.</summary>
    public double? Sigma { get; init; } = 0.25;

    /// <summary>Flow sampling temperature; zero still uses seeded initial noise.</summary>
    public double? Temperature { get; init; } = 0;

    /// <summary>Reference-posterior and generation noise seed.</summary>
    public string? Seed { get; init; } = "1895";

    /// <summary>Send the text as one synthesis fragment without frontend chunking.</summary>
    public bool? RawText { get; init; } = false;

    /// <summary>Framework long-form chunk budget in Unicode codepoints; default 50 when hanzi, kana and Hangul outnumber other letters and digits, otherwise 300; multi-chunk generation without speaker conditioning is capped at 120, including explicit overrides, and anchors later chunks to the first generated voice. Smaller overrides, single-chunk inputs and explicitly conditioned voices retain their chunk size.</summary>
    public long? TextChunkSize { get; init; }

    /// <summary>Framework text chunking policy.</summary>
    public Ming3TtsTextChunkMode? TextChunkMode { get; init; } = Ming3TtsTextChunkMode.TagAware;

    /// <summary>Trim trailing low-energy 100-ms frames per fragment, retaining a 300-ms tail.</summary>
    public bool? TrimSilence { get; init; } = true;

    /// <summary>Maximum latent patches per streaming audio decode.</summary>
    public long? StreamFramesPerEvent { get; init; } = 4;

    /// <summary>Streaming audio chunk schedule.</summary>
    public Ming3TtsStreamChunkPolicy? StreamChunkPolicy { get; init; } = Ming3TtsStreamChunkPolicy.Grow;

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (VoiceId != null)
        {
            throw new ArgumentException("ming3_tts design does not accept voice_id (capability built_in_voices is false)");
        }

        if (MaxTokens != null && (MaxTokens.Value < 1))
        {
            throw new ArgumentException("ming3_tts design max_tokens must be at least 1", "MaxTokens");
        }

        if (Sigma != null && (Sigma.Value < 0))
        {
            throw new ArgumentException("ming3_tts design sigma must be at least 0", "Sigma");
        }

        if (Temperature != null && (Temperature.Value < 0))
        {
            throw new ArgumentException("ming3_tts design temperature must be at least 0", "Temperature");
        }

        if (TextChunkSize != null && (TextChunkSize.Value < 1))
        {
            throw new ArgumentException("ming3_tts design text_chunk_size must be at least 1", "TextChunkSize");
        }

        if (StreamFramesPerEvent != null && (StreamFramesPerEvent.Value < 1 || StreamFramesPerEvent.Value > 64))
        {
            throw new ArgumentException("ming3_tts design stream_frames_per_event must be between 1 and 64", "StreamFramesPerEvent");
        }
    }

    /// <inheritdoc />
    public override Dictionary<string, string> BuildOptions()
    {
        Dictionary<string, string> options = [];

        if (TemplateName != null)
        {
            options["template_name"] = TemplateName switch
            {
            Ming3TtsTemplateName.Tts => "tts",
            Ming3TtsTemplateName.Multilingual => "multilingual",
            Ming3TtsTemplateName.Basic => "basic",
            Ming3TtsTemplateName.Emotion => "emotion",
            Ming3TtsTemplateName.Dialect => "dialect",
            Ming3TtsTemplateName.Ip => "ip",
            Ming3TtsTemplateName.VoiceDesign => "voice_design",
            Ming3TtsTemplateName.Freeform => "freeform",
            _ => throw new ArgumentException("ming3_tts design template_name must be one of: tts, multilingual, basic, emotion, dialect, ip, voice_design, freeform"),
            };
        }

        if (Language != null)
        {
            options["language"] = Language;
        }

        if (Instruction != null)
        {
            options["instruct"] = Instruction;
        }

        if (InstructionSchema != null)
        {
            options["instruction_schema"] = InstructionSchema switch
            {
            Ming3TtsInstructionSchema.En => "en",
            Ming3TtsInstructionSchema.Zh => "zh",
            _ => throw new ArgumentException("ming3_tts design instruction_schema must be one of: en, zh"),
            };
        }

        if (Speed != null)
        {
            options["speed"] = Speed switch
            {
            Ming3TtsSpeed.慢速 => "慢速",
            Ming3TtsSpeed.中速 => "中速",
            Ming3TtsSpeed.快速 => "快速",
            _ => throw new ArgumentException("ming3_tts design speed must be one of: 慢速, 中速, 快速"),
            };
        }

        if (Pitch != null)
        {
            options["pitch"] = Pitch switch
            {
            Ming3TtsPitch.低 => "低",
            Ming3TtsPitch.中 => "中",
            Ming3TtsPitch.高 => "高",
            _ => throw new ArgumentException("ming3_tts design pitch must be one of: 低, 中, 高"),
            };
        }

        if (Volume != null)
        {
            options["volume"] = Volume switch
            {
            Ming3TtsVolume.低 => "低",
            Ming3TtsVolume.中 => "中",
            Ming3TtsVolume.高 => "高",
            _ => throw new ArgumentException("ming3_tts design volume must be one of: 低, 中, 高"),
            };
        }

        if (Emotion != null)
        {
            options["emotion"] = Emotion;
        }

        if (Dialect != null)
        {
            options["dialect"] = Dialect switch
            {
            Ming3TtsDialect.Chuanyudialect => "chuanyu dialect",
            Ming3TtsDialect.Cantonese => "cantonese",
            _ => throw new ArgumentException("ming3_tts design dialect must be one of: chuanyu dialect, cantonese"),
            };
        }

        if (VoiceId != null)
        {
            options["voice_id"] = VoiceId switch
            {
            Ming3TtsVoiceId.USEmma => "US_Emma",
            Ming3TtsVoiceId.USJames => "US_James",
            Ming3TtsVoiceId.UKGeorge => "UK_George",
            Ming3TtsVoiceId.UKCharlotte => "UK_Charlotte",
            _ => throw new ArgumentException("ming3_tts design voice_id must be one of: US_Emma, US_James, UK_George, UK_Charlotte"),
            };
        }

        if (ReferenceText != null)
        {
            options["reference_text"] = ReferenceText;
        }

        if (SpeakerEmbeddingFile != null)
        {
            options["speaker_embedding_file"] = SpeakerEmbeddingFile;
        }

        if (ExportSpeakerEmbeddingFile != null)
        {
            options["export_speaker_embedding_file"] = ExportSpeakerEmbeddingFile;
        }

        if (UseSpeakerEmbedding != null && UseSpeakerEmbedding != true)
        {
            options["use_speaker_embedding"] = UseSpeakerEmbedding.Value ? "true" : "false";
        }

        if (ZeroSpeakerEmbedding != null && ZeroSpeakerEmbedding != false)
        {
            options["zero_speaker_embedding"] = ZeroSpeakerEmbedding.Value ? "true" : "false";
        }

        if (MaxTokens != null && MaxTokens != 1000)
        {
            options["max_tokens"] = MaxTokens.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (GuidanceScale != null && GuidanceScale != 2)
        {
            options["guidance_scale"] = GuidanceScale.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Sigma != null && Sigma != 0.25)
        {
            options["sigma"] = Sigma.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Temperature != null && Temperature != 0)
        {
            options["temperature"] = Temperature.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Seed != null && Seed != "1895")
        {
            options["seed"] = Seed;
        }

        if (RawText != null && RawText != false)
        {
            options["raw_text"] = RawText.Value ? "true" : "false";
        }

        if (TextChunkSize != null)
        {
            options["text_chunk_size"] = TextChunkSize.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TextChunkMode != null && TextChunkMode != Ming3TtsTextChunkMode.TagAware)
        {
            options["text_chunk_mode"] = TextChunkMode switch
            {
            Ming3TtsTextChunkMode.Default => "default",
            Ming3TtsTextChunkMode.TagAware => "tag_aware",
            Ming3TtsTextChunkMode.Japanese => "japanese",
            Ming3TtsTextChunkMode.Endline => "endline",
            _ => throw new ArgumentException("ming3_tts design text_chunk_mode must be one of: default, tag_aware, japanese, endline"),
            };
        }

        if (TrimSilence != null && TrimSilence != true)
        {
            options["trim_silence"] = TrimSilence.Value ? "true" : "false";
        }

        if (StreamFramesPerEvent != null && StreamFramesPerEvent != 4)
        {
            options["stream_frames_per_event"] = StreamFramesPerEvent.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (StreamChunkPolicy != null && StreamChunkPolicy != Ming3TtsStreamChunkPolicy.Grow)
        {
            options["stream_chunk_policy"] = StreamChunkPolicy switch
            {
            Ming3TtsStreamChunkPolicy.Fixed => "fixed",
            Ming3TtsStreamChunkPolicy.Grow => "grow",
            _ => throw new ArgumentException("ming3_tts design stream_chunk_policy must be one of: fixed, grow"),
            };
        }

        return options;
    }
}