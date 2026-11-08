using System;
using System.Collections.Generic;
using System.Globalization;

using Mabs.AudioCpp.Enums;

namespace Mabs.AudioCpp.Models.Design;

/// <summary>Convenience request for the <c>firered_audio</c> spec, design task (model default <c>firered_audio_q8_0</c>).</summary>
public sealed record FireredAudioDesignRequest : DesignRequest
{
    /// <summary>Defaults to voice_design for design, acoustic_edit for generation with input audio, tts_clone for other generation, and asr for ASR.</summary>
    public FireredAudioTemplateName? TemplateName { get; init; }

    /// <summary>Transcript for the prompt audio used by tts_clone.</summary>
    public string? ReferenceText { get; init; }

    /// <summary>FireRedAudio generation language tag.</summary>
    public new string? Language { get; init; } = "zh";

    /// <summary>FireRedAudio DiT flow steps per generated latent patch.</summary>
    public long? NumInferenceSteps { get; init; } = 10;

    /// <summary>FireRedAudio DiT classifier-free guidance scale. Must be &gt; 0.</summary>
    public double? GuidanceScale { get; init; } = 2;

    /// <summary>Maximum generated RedAE latent patches.</summary>
    public long? MaxAudioSteps { get; init; } = 750;

    /// <summary>Minimum generated RedAE latent patches before eosp can stop generation.</summary>
    public long? MinAudioSteps { get; init; } = 6;

    /// <summary>Maximum text-mode tokens before audio mode.</summary>
    public long? MaxTextTokens { get; init; } = 512;

    /// <summary>Output token budget; defaults to 300, or 1024 for understand with enable_thinking=true.</summary>
    public long? MaxTokens { get; init; }

    /// <summary>Leave the understanding think block open for chain-of-thought style answers.</summary>
    public bool? EnableThinking { get; init; } = false;

    /// <summary>Text sampling top-k for understanding.</summary>
    public long? TopK { get; init; } = 20;

    /// <summary>Text sampling top-p for understanding.</summary>
    public double? TopP { get; init; } = 0.8;

    /// <summary>Text sampling temperature for understanding. Understanding-mode sampling requires a positive value; zero does not select greedy decoding.</summary>
    public double? Temperature { get; init; } = 0.7;

    /// <summary>Unsigned 32-bit generation seed in [0, 4294967295]; default 1234.</summary>
    public string? Seed { get; init; } = "1234";

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (NumInferenceSteps != null && (NumInferenceSteps.Value < 1))
        {
            throw new ArgumentException("firered_audio design num_inference_steps must be at least 1", "NumInferenceSteps");
        }

        if (GuidanceScale != null && (GuidanceScale.Value < 0))
        {
            throw new ArgumentException("firered_audio design guidance_scale must be at least 0", "GuidanceScale");
        }

        if (MaxAudioSteps != null && (MaxAudioSteps.Value < 1))
        {
            throw new ArgumentException("firered_audio design max_audio_steps must be at least 1", "MaxAudioSteps");
        }

        if (MinAudioSteps != null && (MinAudioSteps.Value < 0))
        {
            throw new ArgumentException("firered_audio design min_audio_steps must be at least 0", "MinAudioSteps");
        }

        if (MaxTextTokens != null && (MaxTextTokens.Value < 1))
        {
            throw new ArgumentException("firered_audio design max_text_tokens must be at least 1", "MaxTextTokens");
        }

        if (MaxTokens != null && (MaxTokens.Value < 1))
        {
            throw new ArgumentException("firered_audio design max_tokens must be at least 1", "MaxTokens");
        }

        if (TopK != null && (TopK.Value < 0))
        {
            throw new ArgumentException("firered_audio design top_k must be at least 0", "TopK");
        }

        if (TopP != null && (TopP.Value < 0 || TopP.Value > 1))
        {
            throw new ArgumentException("firered_audio design top_p must be between 0 and 1", "TopP");
        }

        if (Temperature != null && (Temperature.Value < 0))
        {
            throw new ArgumentException("firered_audio design temperature must be at least 0", "Temperature");
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
            FireredAudioTemplateName.TtsClone => "tts_clone",
            FireredAudioTemplateName.VoiceDesign => "voice_design",
            FireredAudioTemplateName.SemanticEdit => "semantic_edit",
            FireredAudioTemplateName.AcousticEdit => "acoustic_edit",
            FireredAudioTemplateName.Asr => "asr",
            FireredAudioTemplateName.Understand => "understand",
            _ => throw new ArgumentException("firered_audio design template_name must be one of: tts_clone, voice_design, semantic_edit, acoustic_edit, asr, understand"),
            };
        }

        if (ReferenceText != null)
        {
            options["reference_text"] = ReferenceText;
        }

        if (Instruction != null)
        {
            options["instruct"] = Instruction;
        }

        if (Language != null && Language != "zh")
        {
            options["language"] = Language;
        }

        if (NumInferenceSteps != null && NumInferenceSteps != 10)
        {
            options["num_inference_steps"] = NumInferenceSteps.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (GuidanceScale != null && GuidanceScale != 2)
        {
            options["guidance_scale"] = GuidanceScale.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (MaxAudioSteps != null && MaxAudioSteps != 750)
        {
            options["max_audio_steps"] = MaxAudioSteps.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (MinAudioSteps != null && MinAudioSteps != 6)
        {
            options["min_audio_steps"] = MinAudioSteps.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (MaxTextTokens != null && MaxTextTokens != 512)
        {
            options["max_text_tokens"] = MaxTextTokens.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (MaxTokens != null)
        {
            options["max_tokens"] = MaxTokens.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (EnableThinking != null && EnableThinking != false)
        {
            options["enable_thinking"] = EnableThinking.Value ? "true" : "false";
        }

        if (TopK != null && TopK != 20)
        {
            options["top_k"] = TopK.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TopP != null && TopP != 0.8)
        {
            options["top_p"] = TopP.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Temperature != null && Temperature != 0.7)
        {
            options["temperature"] = Temperature.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Seed != null && Seed != "1234")
        {
            options["seed"] = Seed;
        }

        return options;
    }
}