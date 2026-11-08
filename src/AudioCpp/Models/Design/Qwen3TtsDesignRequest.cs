using System;
using System.Collections.Generic;
using System.Globalization;

using Mabs.AudioCpp.Enums;

namespace Mabs.AudioCpp.Models.Design;

/// <summary>Convenience request for the <c>qwen3_tts</c> spec, design task (model default <c>qwen3_tts_1_7b_base_q8_0</c>).</summary>
public sealed record Qwen3TtsDesignRequest : DesignRequest
{
    /// <summary>Streaming codec chunk schedule: grow starts with one frame and doubles to stream_frames_per_event; fixed uses that size throughout.</summary>
    public Qwen3TtsStreamChunkPolicy? StreamChunkPolicy { get; init; } = Qwen3TtsStreamChunkPolicy.Grow;

    /// <summary>Codec frames per fixed streaming event, or the maximum for growing events. Smaller values reduce chunk latency but increase decoding overhead.</summary>
    public long? StreamFramesPerEvent { get; init; } = 12;

    /// <summary>Target synthesis language code or label; default auto lets the model infer from text and voice settings.</summary>
    public new string? Language { get; init; } = "auto";

    /// <summary>Maximum generated codec frames per text chunk; omitted uses the checkpoint generation_config.json (2048 when absent).</summary>
    public long? MaxTokens { get; init; }

    /// <summary>Sample the main codec stream instead of greedy decoding; default true.</summary>
    public bool? DoSample { get; init; } = true;

    /// <summary>Main codec sampling temperature; must be &gt; 0 when do_sample=true. Ignored when do_sample=false; default 0.9.</summary>
    public double? Temperature { get; init; } = 0.9;

    /// <summary>Main codec top-k sampling limit; default 50, 0 disables top-k filtering.</summary>
    public long? TopK { get; init; } = 50;

    /// <summary>Main codec nucleus sampling limit; default 1.0.</summary>
    public double? TopP { get; init; } = 1;

    /// <summary>Main codec repetition penalty; must be &gt; 0, default 1.05.</summary>
    public double? RepetitionPenalty { get; init; } = 1.05;

    /// <summary>Enable subtalker sampling for the remaining audio codebooks instead of greedy decoding; default true.</summary>
    public bool? SubtalkerDoSample { get; init; } = true;

    /// <summary>Subtalker sampling temperature; must be &gt; 0 when subtalker_do_sample=true. Ignored when subtalker_do_sample=false; default 0.9.</summary>
    public double? SubtalkerTemperature { get; init; } = 0.9;

    /// <summary>Subtalker top-k sampling limit; default 50, 0 disables top-k filtering.</summary>
    public long? SubtalkerTopK { get; init; } = 50;

    /// <summary>Subtalker nucleus sampling limit; default 1.0.</summary>
    public double? SubtalkerTopP { get; init; } = 1;

    /// <summary>Codec sampling seed; omitted requests choose a random seed.</summary>
    public string? Seed { get; init; }

    /// <summary>Transcript matching the reference audio for voice cloning; required unless x_vector_only_mode=true.</summary>
    public string? ReferenceText { get; init; }

    /// <summary>Use only the speaker embedding for voice cloning instead of ICL reference codec prompting; default false.</summary>
    public bool? XVectorOnlyMode { get; init; } = false;

    /// <summary>Named preset speaker from the CustomVoice checkpoint. Legacy speaker remains accepted.</summary>
    public new Qwen3TtsVoiceId? VoiceId { get; init; }

    /// <summary>Maximum UTF-8 codepoints per long-form text chunk; default 8192.</summary>
    public long? TextChunkSize { get; init; } = 8192;

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (VoiceId != null)
        {
            throw new ArgumentException("qwen3_tts design does not accept voice_id (capability built_in_voices is false)");
        }

        if (StreamFramesPerEvent != null && (StreamFramesPerEvent.Value < 1))
        {
            throw new ArgumentException("qwen3_tts design stream_frames_per_event must be at least 1", "StreamFramesPerEvent");
        }

        if (MaxTokens != null && (MaxTokens.Value < 1))
        {
            throw new ArgumentException("qwen3_tts design max_tokens must be at least 1", "MaxTokens");
        }

        if (Temperature != null && (Temperature.Value < 0))
        {
            throw new ArgumentException("qwen3_tts design temperature must be at least 0", "Temperature");
        }

        if (TopK != null && (TopK.Value < 0))
        {
            throw new ArgumentException("qwen3_tts design top_k must be at least 0", "TopK");
        }

        if (TopP != null && (TopP.Value < 0 || TopP.Value > 1))
        {
            throw new ArgumentException("qwen3_tts design top_p must be between 0 and 1", "TopP");
        }

        if (RepetitionPenalty != null && (RepetitionPenalty.Value < 0))
        {
            throw new ArgumentException("qwen3_tts design repetition_penalty must be at least 0", "RepetitionPenalty");
        }

        if (SubtalkerTemperature != null && (SubtalkerTemperature.Value < 0))
        {
            throw new ArgumentException("qwen3_tts design subtalker_temperature must be at least 0", "SubtalkerTemperature");
        }

        if (SubtalkerTopK != null && (SubtalkerTopK.Value < 0))
        {
            throw new ArgumentException("qwen3_tts design subtalker_top_k must be at least 0", "SubtalkerTopK");
        }

        if (SubtalkerTopP != null && (SubtalkerTopP.Value < 0 || SubtalkerTopP.Value > 1))
        {
            throw new ArgumentException("qwen3_tts design subtalker_top_p must be between 0 and 1", "SubtalkerTopP");
        }

        if (TextChunkSize != null && (TextChunkSize.Value < 1))
        {
            throw new ArgumentException("qwen3_tts design text_chunk_size must be at least 1", "TextChunkSize");
        }
    }

    /// <inheritdoc />
    public override Dictionary<string, string> BuildOptions()
    {
        Dictionary<string, string> options = [];

        if (StreamChunkPolicy != null && StreamChunkPolicy != Qwen3TtsStreamChunkPolicy.Grow)
        {
            options["stream_chunk_policy"] = StreamChunkPolicy switch
            {
            Qwen3TtsStreamChunkPolicy.Grow => "grow",
            Qwen3TtsStreamChunkPolicy.Fixed => "fixed",
            _ => throw new ArgumentException("qwen3_tts design stream_chunk_policy must be one of: grow, fixed"),
            };
        }

        if (StreamFramesPerEvent != null && StreamFramesPerEvent != 12)
        {
            options["stream_frames_per_event"] = StreamFramesPerEvent.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Language != null && Language != "auto")
        {
            options["language"] = Language;
        }

        if (MaxTokens != null)
        {
            options["max_tokens"] = MaxTokens.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (DoSample != null && DoSample != true)
        {
            options["do_sample"] = DoSample.Value ? "true" : "false";
        }

        if (Temperature != null && Temperature != 0.9)
        {
            options["temperature"] = Temperature.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TopK != null && TopK != 50)
        {
            options["top_k"] = TopK.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TopP != null && TopP != 1)
        {
            options["top_p"] = TopP.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (RepetitionPenalty != null && RepetitionPenalty != 1.05)
        {
            options["repetition_penalty"] = RepetitionPenalty.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (SubtalkerDoSample != null && SubtalkerDoSample != true)
        {
            options["subtalker_do_sample"] = SubtalkerDoSample.Value ? "true" : "false";
        }

        if (SubtalkerTemperature != null && SubtalkerTemperature != 0.9)
        {
            options["subtalker_temperature"] = SubtalkerTemperature.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (SubtalkerTopK != null && SubtalkerTopK != 50)
        {
            options["subtalker_top_k"] = SubtalkerTopK.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (SubtalkerTopP != null && SubtalkerTopP != 1)
        {
            options["subtalker_top_p"] = SubtalkerTopP.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Seed != null)
        {
            options["seed"] = Seed;
        }

        if (ReferenceText != null)
        {
            options["reference_text"] = ReferenceText;
        }

        if (XVectorOnlyMode != null && XVectorOnlyMode != false)
        {
            options["x_vector_only_mode"] = XVectorOnlyMode.Value ? "true" : "false";
        }

        if (Instruction != null)
        {
            options["instruct"] = Instruction;
        }

        if (VoiceId != null)
        {
            options["voice_id"] = VoiceId switch
            {
            Qwen3TtsVoiceId.Aiden => "aiden",
            Qwen3TtsVoiceId.Dylan => "dylan",
            Qwen3TtsVoiceId.Eric => "eric",
            Qwen3TtsVoiceId.OnoAnna => "ono_anna",
            Qwen3TtsVoiceId.Ryan => "ryan",
            Qwen3TtsVoiceId.Serena => "serena",
            Qwen3TtsVoiceId.Sohee => "sohee",
            Qwen3TtsVoiceId.UncleFu => "uncle_fu",
            Qwen3TtsVoiceId.Vivian => "vivian",
            _ => throw new ArgumentException("qwen3_tts design voice_id must be one of: aiden, dylan, eric, ono_anna, ryan, serena, sohee, uncle_fu, vivian"),
            };
        }

        if (TextChunkSize != null && TextChunkSize != 8192)
        {
            options["text_chunk_size"] = TextChunkSize.Value.ToString(CultureInfo.InvariantCulture);
        }

        return options;
    }
}