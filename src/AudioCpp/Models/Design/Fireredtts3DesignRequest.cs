using System;
using System.Collections.Generic;
using System.Globalization;

using Mabs.AudioCpp.Enums;

namespace Mabs.AudioCpp.Models.Design;

/// <summary>Convenience request for the <c>fireredtts3</c> spec, design task (model default <c>fireredtts3_instruct_q8_0</c>).</summary>
public sealed record Fireredtts3DesignRequest : DesignRequest
{
    /// <summary>Instruct template; defaults to acoustic_edit with input audio, otherwise instruct_tts.</summary>
    public Fireredtts3TemplateName? TemplateName { get; init; }

    /// <summary>Transcript for the prompt audio.</summary>
    public string? ReferenceText { get; init; }

    /// <summary>Language tag used by FireRedTTS3.</summary>
    public new string? Language { get; init; }

    /// <summary>Maximum Unicode codepoints per long-form text chunk; default 80 when hanzi, kana and Hangul outnumber other letters and digits (except edits), otherwise 600.</summary>
    public long? TextChunkSize { get; init; }

    /// <summary>Framework text chunking mode.</summary>
    public Fireredtts3TextChunkMode? TextChunkMode { get; init; }

    /// <summary>FireRed DiT flow steps per generated latent patch.</summary>
    public long? NumInferenceSteps { get; init; }

    /// <summary>Positive guidance scale; defaults to 1.2 for voice-design/semantic-edit/acoustic-edit templates and 2 otherwise.</summary>
    public double? GuidanceScale { get; init; }

    /// <summary>Stop probability threshold for AR patch generation.</summary>
    public double? StopThreshold { get; init; }

    /// <summary>Unsigned 32-bit sampling seed in [0, 4294967295]; defaults to 2 for voice design and 1234 otherwise.</summary>
    public string? Seed { get; init; }

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (TextChunkSize != null && (TextChunkSize.Value < 1))
        {
            throw new ArgumentException("fireredtts3 design text_chunk_size must be at least 1", "TextChunkSize");
        }

        if (NumInferenceSteps != null && (NumInferenceSteps.Value < 1))
        {
            throw new ArgumentException("fireredtts3 design num_inference_steps must be at least 1", "NumInferenceSteps");
        }

        if (GuidanceScale != null && (GuidanceScale.Value < 0))
        {
            throw new ArgumentException("fireredtts3 design guidance_scale must be at least 0", "GuidanceScale");
        }

        if (StopThreshold != null && (StopThreshold.Value < 0 || StopThreshold.Value > 1))
        {
            throw new ArgumentException("fireredtts3 design stop_threshold must be between 0 and 1", "StopThreshold");
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
            Fireredtts3TemplateName.InstructTts => "instruct_tts",
            Fireredtts3TemplateName.VoiceDesign => "voice_design",
            Fireredtts3TemplateName.SemanticEdit => "semantic_edit",
            Fireredtts3TemplateName.AcousticEdit => "acoustic_edit",
            _ => throw new ArgumentException("fireredtts3 design template_name must be one of: instruct_tts, voice_design, semantic_edit, acoustic_edit"),
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

        if (Language != null && Language != "Chinese")
        {
            options["language"] = Language;
        }

        if (TextChunkSize != null)
        {
            options["text_chunk_size"] = TextChunkSize.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TextChunkMode != null && TextChunkMode != Fireredtts3TextChunkMode.Default)
        {
            options["text_chunk_mode"] = TextChunkMode switch
            {
            Fireredtts3TextChunkMode.Default => "default",
            Fireredtts3TextChunkMode.TagAware => "tag_aware",
            Fireredtts3TextChunkMode.Japanese => "japanese",
            Fireredtts3TextChunkMode.Endline => "endline",
            _ => throw new ArgumentException("fireredtts3 design text_chunk_mode must be one of: default, tag_aware, japanese, endline"),
            };
        }

        if (NumInferenceSteps != null && NumInferenceSteps != 10)
        {
            options["num_inference_steps"] = NumInferenceSteps.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (GuidanceScale != null)
        {
            options["guidance_scale"] = GuidanceScale.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (StopThreshold != null && StopThreshold != 0.5)
        {
            options["stop_threshold"] = StopThreshold.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Seed != null)
        {
            options["seed"] = Seed;
        }

        return options;
    }
}