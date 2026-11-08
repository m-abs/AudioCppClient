using System;
using System.Collections.Generic;
using System.Globalization;

using Mabs.AudioCpp.Enums;

namespace Mabs.AudioCpp.Models.Tts;

/// <summary>Convenience request for the <c>voxtral_tts</c> spec, tts task (model default <c>voxtral_4b_tts_2603_bf16</c>).</summary>
public sealed record VoxtralTtsRequest : TtsRequest
{
    /// <summary>Embedded preset voice; the released checkpoint does not contain the encoder needed for reference-audio cloning.</summary>
    public new VoxtralTtsVoiceId? VoiceId { get; init; } = VoxtralTtsVoiceId.CasualMale;

    /// <summary>Maximum generated audio frames per text chunk; each frame represents 80 ms.</summary>
    public long? MaxTokens { get; init; } = 2048;

    /// <summary>Euler integration steps for the per-frame acoustic flow model.</summary>
    public long? NumInferenceSteps { get; init; } = 7;

    /// <summary>Acoustic classifier-free guidance scale; the AR backbone does not use CFG.</summary>
    public double? GuidanceScale { get; init; } = 1.2;

    /// <summary>Acoustic noise seed; omitted requests select a random seed.</summary>
    public string? Seed { get; init; }

    /// <summary>Maximum Unicode codepoints per framework long-form text chunk.</summary>
    public long? TextChunkSize { get; init; } = 300;

    /// <summary>Framework text chunking mode.</summary>
    public VoxtralTtsTextChunkMode? TextChunkMode { get; init; } = VoxtralTtsTextChunkMode.Default;

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (MaxTokens != null && (MaxTokens.Value < 1))
        {
            throw new ArgumentException("voxtral_tts tts max_tokens must be at least 1", "MaxTokens");
        }

        if (NumInferenceSteps != null && (NumInferenceSteps.Value < 1))
        {
            throw new ArgumentException("voxtral_tts tts num_inference_steps must be at least 1", "NumInferenceSteps");
        }

        if (GuidanceScale != null && (GuidanceScale.Value < 0))
        {
            throw new ArgumentException("voxtral_tts tts guidance_scale must be at least 0", "GuidanceScale");
        }

        if (TextChunkSize != null && (TextChunkSize.Value < 1))
        {
            throw new ArgumentException("voxtral_tts tts text_chunk_size must be at least 1", "TextChunkSize");
        }
    }

    /// <inheritdoc />
    public override Dictionary<string, string> BuildOptions()
    {
        Dictionary<string, string> options = [];

        if (VoiceId != null && VoiceId != VoxtralTtsVoiceId.CasualMale)
        {
            options["voice_id"] = VoiceId switch
            {
            VoxtralTtsVoiceId.CasualFemale => "casual_female",
            VoxtralTtsVoiceId.CasualMale => "casual_male",
            VoxtralTtsVoiceId.CheerfulFemale => "cheerful_female",
            VoxtralTtsVoiceId.NeutralFemale => "neutral_female",
            VoxtralTtsVoiceId.NeutralMale => "neutral_male",
            VoxtralTtsVoiceId.PtMale => "pt_male",
            VoxtralTtsVoiceId.PtFemale => "pt_female",
            VoxtralTtsVoiceId.NlMale => "nl_male",
            VoxtralTtsVoiceId.NlFemale => "nl_female",
            VoxtralTtsVoiceId.ItMale => "it_male",
            VoxtralTtsVoiceId.ItFemale => "it_female",
            VoxtralTtsVoiceId.FrMale => "fr_male",
            VoxtralTtsVoiceId.FrFemale => "fr_female",
            VoxtralTtsVoiceId.EsMale => "es_male",
            VoxtralTtsVoiceId.EsFemale => "es_female",
            VoxtralTtsVoiceId.DeMale => "de_male",
            VoxtralTtsVoiceId.DeFemale => "de_female",
            VoxtralTtsVoiceId.ArMale => "ar_male",
            VoxtralTtsVoiceId.HiMale => "hi_male",
            VoxtralTtsVoiceId.HiFemale => "hi_female",
            _ => throw new ArgumentException("voxtral_tts tts voice_id must be one of: casual_female, casual_male, cheerful_female, neutral_female, neutral_male, pt_male, pt_female, nl_male, nl_female, it_male, it_female, fr_male, fr_female, es_male, es_female, de_male, de_female, ar_male, hi_male, hi_female"),
            };
        }

        if (MaxTokens != null && MaxTokens != 2048)
        {
            options["max_tokens"] = MaxTokens.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (NumInferenceSteps != null && NumInferenceSteps != 7)
        {
            options["num_inference_steps"] = NumInferenceSteps.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (GuidanceScale != null && GuidanceScale != 1.2)
        {
            options["guidance_scale"] = GuidanceScale.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Seed != null)
        {
            options["seed"] = Seed;
        }

        if (TextChunkSize != null && TextChunkSize != 300)
        {
            options["text_chunk_size"] = TextChunkSize.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TextChunkMode != null && TextChunkMode != VoxtralTtsTextChunkMode.Default)
        {
            options["text_chunk_mode"] = TextChunkMode switch
            {
            VoxtralTtsTextChunkMode.Default => "default",
            VoxtralTtsTextChunkMode.TagAware => "tag_aware",
            VoxtralTtsTextChunkMode.Japanese => "japanese",
            VoxtralTtsTextChunkMode.Endline => "endline",
            _ => throw new ArgumentException("voxtral_tts tts text_chunk_mode must be one of: default, tag_aware, japanese, endline"),
            };
        }

        return options;
    }
}