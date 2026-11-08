using System;
using System.Collections.Generic;
using System.Globalization;

using Mabs.AudioCpp.Enums;

namespace Mabs.AudioCpp.Models.Tts;

/// <summary>Convenience request for the <c>kugelaudio</c> spec, tts task (model default <c>kugelaudio_0_open_q8_0</c>).</summary>
public sealed record KugelaudioTtsRequest : TtsRequest
{
    /// <summary>Bundled preset voice.</summary>
    public new KugelaudioVoiceId? VoiceId { get; init; }

    /// <summary>Diffusion steps per acoustic frame, from 1 to 1000; omitted uses the checkpoint's diffusion_head_config.ddpm_num_inference_steps.</summary>
    public long? NumInferenceSteps { get; init; }

    /// <summary>Diffusion classifier-free guidance; 1 disables the unconditional pass. Any finite value is accepted; negative values enter CFG arithmetic.</summary>
    public double? GuidanceScale { get; init; }

    /// <summary>Maximum autoregressive tokens per text chunk.</summary>
    public long? MaxTokens { get; init; }

    /// <summary>Sample autoregressive speech control tokens instead of greedy decoding.</summary>
    public bool? DoSample { get; init; }

    /// <summary>Autoregressive sampling temperature when do_sample is enabled. Any finite value is accepted; negative values disable sampling.</summary>
    public double? Temperature { get; init; }

    /// <summary>Random seed; -1 chooses a random seed.</summary>
    public string? Seed { get; init; }

    /// <summary>Maximum Unicode codepoints per long-form text chunk.</summary>
    public long? TextChunkSize { get; init; }

    /// <summary>Framework long-form text splitting mode.</summary>
    public KugelaudioTextChunkMode? TextChunkMode { get; init; }

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (VoiceId != null)
        {
            throw new ArgumentException("kugelaudio tts does not accept voice_id (capability built_in_voices is false)");
        }

        if (NumInferenceSteps != null && (NumInferenceSteps.Value < 1 || NumInferenceSteps.Value > 1000))
        {
            throw new ArgumentException("kugelaudio tts num_inference_steps must be between 1 and 1000", "NumInferenceSteps");
        }

        if (MaxTokens != null && (MaxTokens.Value < 1))
        {
            throw new ArgumentException("kugelaudio tts max_tokens must be at least 1", "MaxTokens");
        }

        if (TextChunkSize != null && (TextChunkSize.Value < 1))
        {
            throw new ArgumentException("kugelaudio tts text_chunk_size must be at least 1", "TextChunkSize");
        }
    }

    /// <inheritdoc />
    public override Dictionary<string, string> BuildOptions()
    {
        Dictionary<string, string> options = [];

        if (VoiceId != null && VoiceId != KugelaudioVoiceId.Default)
        {
            options["voice_id"] = VoiceId switch
            {
            KugelaudioVoiceId.Default => "default",
            KugelaudioVoiceId.Clear => "clear",
            KugelaudioVoiceId.EnglishFemale => "english_female",
            KugelaudioVoiceId.EnglishMale => "english_male",
            _ => throw new ArgumentException("kugelaudio tts voice_id must be one of: default, clear, english_female, english_male"),
            };
        }

        if (NumInferenceSteps != null && NumInferenceSteps != 20)
        {
            options["num_inference_steps"] = NumInferenceSteps.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (GuidanceScale != null && GuidanceScale != 3)
        {
            options["guidance_scale"] = GuidanceScale.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (MaxTokens != null && MaxTokens != 2048)
        {
            options["max_tokens"] = MaxTokens.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (DoSample != null && DoSample != false)
        {
            options["do_sample"] = DoSample.Value ? "true" : "false";
        }

        if (Temperature != null && Temperature != 1)
        {
            options["temperature"] = Temperature.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Seed != null && Seed != "1234")
        {
            options["seed"] = Seed;
        }

        if (TextChunkSize != null && TextChunkSize != 300)
        {
            options["text_chunk_size"] = TextChunkSize.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TextChunkMode != null && TextChunkMode != KugelaudioTextChunkMode.Default)
        {
            options["text_chunk_mode"] = TextChunkMode switch
            {
            KugelaudioTextChunkMode.Default => "default",
            KugelaudioTextChunkMode.TagAware => "tag_aware",
            KugelaudioTextChunkMode.Japanese => "japanese",
            KugelaudioTextChunkMode.Endline => "endline",
            _ => throw new ArgumentException("kugelaudio tts text_chunk_mode must be one of: default, tag_aware, japanese, endline"),
            };
        }

        return options;
    }
}