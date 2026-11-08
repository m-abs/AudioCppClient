using System;
using System.Collections.Generic;
using System.Globalization;

using Mabs.AudioCpp.Enums;

namespace Mabs.AudioCpp.Models.Tts;

/// <summary>Convenience request for the <c>cosyvoice3</c> spec, tts task (model default <c>cosyvoice3_q8_0</c>).</summary>
public sealed record Cosyvoice3TtsRequest : TtsRequest
{
    /// <summary>CosyVoice3 request template.</summary>
    public Cosyvoice3TemplateName? TemplateName { get; init; }

    /// <summary>Transcript for the prompt audio.</summary>
    public string? ReferenceText { get; init; }

    /// <summary>Instruction text for instruct mode.</summary>
    public string? Instruct { get; init; }

    /// <summary>Maximum Unicode codepoints per long-form text chunk; default 600.</summary>
    public long? TextChunkSize { get; init; }

    /// <summary>Framework text chunking mode.</summary>
    public Cosyvoice3TextChunkMode? TextChunkMode { get; init; }

    /// <summary>Maximum generated speech tokens; -1 (default) selects 20 times the target text token count. Explicit limits must be positive.</summary>
    public long? MaxTokens { get; init; }

    /// <summary>Minimum generated speech tokens before stop is accepted; -1 (default) selects twice the target text token count. Must not exceed the resolved maximum.</summary>
    public long? MinTokens { get; init; }

    /// <summary>AR speech-token top-k sampling limit; default 25.</summary>
    public long? TopK { get; init; }

    /// <summary>Flow decoder Euler steps; default 10.</summary>
    public long? NumInferenceSteps { get; init; }

    /// <summary>Generation seed.</summary>
    public string? Seed { get; init; }

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (TextChunkSize != null && (TextChunkSize.Value < 1))
        {
            throw new ArgumentException("cosyvoice3 tts text_chunk_size must be at least 1", "TextChunkSize");
        }

        if (MaxTokens != null && (MaxTokens.Value < -1))
        {
            throw new ArgumentException("cosyvoice3 tts max_tokens must be at least -1", "MaxTokens");
        }

        if (MinTokens != null && (MinTokens.Value < -1))
        {
            throw new ArgumentException("cosyvoice3 tts min_tokens must be at least -1", "MinTokens");
        }

        if (TopK != null && (TopK.Value < 1))
        {
            throw new ArgumentException("cosyvoice3 tts top_k must be at least 1", "TopK");
        }

        if (NumInferenceSteps != null && (NumInferenceSteps.Value < 1))
        {
            throw new ArgumentException("cosyvoice3 tts num_inference_steps must be at least 1", "NumInferenceSteps");
        }
    }

    /// <inheritdoc />
    public override Dictionary<string, string> BuildOptions()
    {
        Dictionary<string, string> options = [];

        if (TemplateName != null && TemplateName != Cosyvoice3TemplateName.ZeroShot)
        {
            options["template_name"] = TemplateName switch
            {
            Cosyvoice3TemplateName.ZeroShot => "zero_shot",
            Cosyvoice3TemplateName.CrossLingual => "cross_lingual",
            Cosyvoice3TemplateName.Instruct => "instruct",
            _ => throw new ArgumentException("cosyvoice3 tts template_name must be one of: zero_shot, cross_lingual, instruct"),
            };
        }

        if (ReferenceText != null)
        {
            options["reference_text"] = ReferenceText;
        }

        if (Instruct != null)
        {
            options["instruct"] = Instruct;
        }

        if (TextChunkSize != null && TextChunkSize != 600)
        {
            options["text_chunk_size"] = TextChunkSize.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TextChunkMode != null && TextChunkMode != Cosyvoice3TextChunkMode.Default)
        {
            options["text_chunk_mode"] = TextChunkMode switch
            {
            Cosyvoice3TextChunkMode.Default => "default",
            Cosyvoice3TextChunkMode.TagAware => "tag_aware",
            Cosyvoice3TextChunkMode.Japanese => "japanese",
            Cosyvoice3TextChunkMode.Endline => "endline",
            _ => throw new ArgumentException("cosyvoice3 tts text_chunk_mode must be one of: default, tag_aware, japanese, endline"),
            };
        }

        if (MaxTokens != null && MaxTokens != -1)
        {
            options["max_tokens"] = MaxTokens.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (MinTokens != null && MinTokens != 0)
        {
            options["min_tokens"] = MinTokens.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TopK != null && TopK != 25)
        {
            options["top_k"] = TopK.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (NumInferenceSteps != null && NumInferenceSteps != 10)
        {
            options["num_inference_steps"] = NumInferenceSteps.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Seed != null && Seed != "1986")
        {
            options["seed"] = Seed;
        }

        return options;
    }
}