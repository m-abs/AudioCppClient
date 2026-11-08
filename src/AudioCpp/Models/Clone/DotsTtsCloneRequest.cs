using System;
using System.Collections.Generic;
using System.Globalization;

using Mabs.AudioCpp.Enums;

namespace Mabs.AudioCpp.Models.Clone;

/// <summary>Convenience request for the <c>dots_tts</c> spec, clone task (model default <c>dots_tts_soar_q8_0</c>).</summary>
public sealed record DotsTtsCloneRequest : CloneRequest
{
    /// <summary>Transcript for the prompt audio. When set with reference audio, DotTTS uses prompt prefill for higher quality voice cloning.</summary>
    public string? ReferenceText { get; init; }

    /// <summary>Optional prompt-audio trim duration before reference conditioning and prompt prefill.</summary>
    public double? ReferenceDurationSec { get; init; }

    /// <summary>DotTTS synthesis template. tts is normal text-to-speech; instruction_tts adds instruction conditioning; text_to_audio uses a sound-description template; tts_interleave uses the streaming interleave schedule; edit uses the DotTTS Edit source-audio editing schedule and is offline-only.</summary>
    public DotsTtsTemplateName? TemplateName { get; init; }

    /// <summary>Source WAV path for DotTTS Edit. Required when template_name=edit unless audio_input is supplied by the caller.</summary>
    public string? SourceAudio { get; init; }

    /// <summary>Structured DotTTS Edit instruction. If omitted with template_name=edit, the request text is used as the instruction.</summary>
    public string? Instruct { get; init; }

    /// <summary>Optional source transcript override for DotTTS Edit. If omitted, it is rendered from instruction tags.</summary>
    public string? SourceText { get; init; }

    /// <summary>Edit target transcript override. When omitted, explicit instruction plus text input uses that text; otherwise the target is rendered from instruction tags.</summary>
    public string? TargetText { get; init; }

    /// <summary>DotTTS Edit speaker conditioning mode. auto follows the upstream tag-based behavior; on/off force or disable source-audio speaker conditioning.</summary>
    public DotsTtsUseXvector? UseXvector { get; init; }

    /// <summary>Optional target language tag. none disables language tagging; otherwise pass a language code such as en or zh.</summary>
    public new string? Language { get; init; }

    /// <summary>SOAR flow-matching inference step count; default 10.</summary>
    public long? NumInferenceSteps { get; init; }

    /// <summary>Classifier-free guidance scale used by SOAR flow matching. Default 1.2.</summary>
    public double? GuidanceScale { get; init; }

    /// <summary>Multiplier applied to the prompt speaker embedding; default 1.5.</summary>
    public double? SpeakerScale { get; init; }

    /// <summary>SOAR ODE solver method for flow matching. Fixed-step euler is the default and parity baseline.</summary>
    public DotsTtsSamplerMode? SamplerMode { get; init; }

    /// <summary>Maximum generated audio patch count per generated segment; default 500. In Edit mode source patches consume this budget; it must exceed the source-patch count.</summary>
    public long? MaxTokens { get; init; }

    /// <summary>Maximum text codepoints per generated segment for long-form requests; default 320.</summary>
    public long? TextChunkSize { get; init; }

    /// <summary>Framework text chunking mode; default tag_aware preserves leading style/control tags across long-form chunks.</summary>
    public DotsTtsTextChunkMode? TextChunkMode { get; init; }

    /// <summary>Streaming vocoder latent patch merge size after the initial unmerged patches; default 4, matching the Python optimized streaming runtime default.</summary>
    public long? VocoderMergeSteps { get; init; }

    /// <summary>Seed for prompt latent sampling and flow noise initialization; default 42.</summary>
    public string? Seed { get; init; }

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (ReferenceDurationSec != null && (ReferenceDurationSec.Value < 0))
        {
            throw new ArgumentException("dots_tts clone reference_duration_sec must be at least 0", "ReferenceDurationSec");
        }

        if (NumInferenceSteps != null && (NumInferenceSteps.Value < 1))
        {
            throw new ArgumentException("dots_tts clone num_inference_steps must be at least 1", "NumInferenceSteps");
        }

        if (GuidanceScale != null && (GuidanceScale.Value < 0))
        {
            throw new ArgumentException("dots_tts clone guidance_scale must be at least 0", "GuidanceScale");
        }

        if (SpeakerScale != null && (SpeakerScale.Value < 0))
        {
            throw new ArgumentException("dots_tts clone speaker_scale must be at least 0", "SpeakerScale");
        }

        if (MaxTokens != null && (MaxTokens.Value < 1))
        {
            throw new ArgumentException("dots_tts clone max_tokens must be at least 1", "MaxTokens");
        }

        if (TextChunkSize != null && (TextChunkSize.Value < 1))
        {
            throw new ArgumentException("dots_tts clone text_chunk_size must be at least 1", "TextChunkSize");
        }

        if (VocoderMergeSteps != null && (VocoderMergeSteps.Value < 1))
        {
            throw new ArgumentException("dots_tts clone vocoder_merge_steps must be at least 1", "VocoderMergeSteps");
        }
    }

    /// <inheritdoc />
    public override Dictionary<string, string> BuildOptions()
    {
        Dictionary<string, string> options = [];

        if (ReferenceText != null)
        {
            options["reference_text"] = ReferenceText;
        }

        if (ReferenceDurationSec != null)
        {
            options["reference_duration_sec"] = ReferenceDurationSec.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TemplateName != null && TemplateName != DotsTtsTemplateName.Tts)
        {
            options["template_name"] = TemplateName switch
            {
            DotsTtsTemplateName.Tts => "tts",
            DotsTtsTemplateName.InstructionTts => "instruction_tts",
            DotsTtsTemplateName.TextToAudio => "text_to_audio",
            DotsTtsTemplateName.TtsInterleave => "tts_interleave",
            DotsTtsTemplateName.Edit => "edit",
            _ => throw new ArgumentException("dots_tts clone template_name must be one of: tts, instruction_tts, text_to_audio, tts_interleave, edit"),
            };
        }

        if (SourceAudio != null)
        {
            options["source_audio"] = SourceAudio;
        }

        if (Instruct != null)
        {
            options["instruct"] = Instruct;
        }

        if (SourceText != null)
        {
            options["source_text"] = SourceText;
        }

        if (TargetText != null)
        {
            options["target_text"] = TargetText;
        }

        if (UseXvector != null && UseXvector != DotsTtsUseXvector.Auto)
        {
            options["use_xvector"] = UseXvector switch
            {
            DotsTtsUseXvector.Auto => "auto",
            DotsTtsUseXvector.On => "on",
            DotsTtsUseXvector.Off => "off",
            _ => throw new ArgumentException("dots_tts clone use_xvector must be one of: auto, on, off"),
            };
        }

        if (Language != null && Language != "none")
        {
            options["language"] = Language;
        }

        if (NumInferenceSteps != null && NumInferenceSteps != 10)
        {
            options["num_inference_steps"] = NumInferenceSteps.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (GuidanceScale != null && GuidanceScale != 1.2)
        {
            options["guidance_scale"] = GuidanceScale.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (SpeakerScale != null && SpeakerScale != 1.5)
        {
            options["speaker_scale"] = SpeakerScale.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (SamplerMode != null && SamplerMode != DotsTtsSamplerMode.Euler)
        {
            options["sampler_mode"] = SamplerMode switch
            {
            DotsTtsSamplerMode.Euler => "euler",
            DotsTtsSamplerMode.Midpoint => "midpoint",
            DotsTtsSamplerMode.Rk4 => "rk4",
            _ => throw new ArgumentException("dots_tts clone sampler_mode must be one of: euler, midpoint, rk4"),
            };
        }

        if (MaxTokens != null && MaxTokens != 500)
        {
            options["max_tokens"] = MaxTokens.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TextChunkSize != null && TextChunkSize != 320)
        {
            options["text_chunk_size"] = TextChunkSize.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TextChunkMode != null && TextChunkMode != DotsTtsTextChunkMode.TagAware)
        {
            options["text_chunk_mode"] = TextChunkMode switch
            {
            DotsTtsTextChunkMode.Default => "default",
            DotsTtsTextChunkMode.TagAware => "tag_aware",
            DotsTtsTextChunkMode.Japanese => "japanese",
            DotsTtsTextChunkMode.Endline => "endline",
            _ => throw new ArgumentException("dots_tts clone text_chunk_mode must be one of: default, tag_aware, japanese, endline"),
            };
        }

        if (VocoderMergeSteps != null && VocoderMergeSteps != 4)
        {
            options["vocoder_merge_steps"] = VocoderMergeSteps.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Seed != null && Seed != "42")
        {
            options["seed"] = Seed;
        }

        return options;
    }
}