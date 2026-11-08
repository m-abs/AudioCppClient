using System;
using System.Collections.Generic;
using System.Globalization;

namespace Mabs.AudioCpp.Models.Clone;

/// <summary>Convenience request for the <c>chatterbox</c> spec, clone task (model default <c>chatterbox_q8_0</c>).</summary>
public sealed record ChatterboxCloneRequest : CloneRequest
{
    /// <summary>Emotion exaggeration. Captured during clone preparation; changing it on the same prepared session is rejected.</summary>
    public double? Exaggeration { get; init; } = 0.5;

    /// <summary>T3 classifier-free guidance scale. Captured during clone preparation; changing it on the same prepared session is rejected.</summary>
    public double? GuidanceScale { get; init; } = 0.5;

    /// <summary>T3 sampling temperature. Captured during clone preparation; changing it on the same prepared session is rejected.</summary>
    public double? Temperature { get; init; } = 0.8;

    /// <summary>T3 repetition penalty. Captured during clone preparation; changing it on the same prepared session is rejected.</summary>
    public double? RepetitionPenalty { get; init; } = 1.2;

    /// <summary>Minimum sampling probability. Captured during clone preparation; changing it on the same prepared session is rejected.</summary>
    public double? MinP { get; init; } = 0.05;

    /// <summary>Nucleus sampling probability. Captured during clone preparation; changing it on the same prepared session is rejected.</summary>
    public double? TopP { get; init; } = 1;

    /// <summary>S3Gen flow guidance scale. Captured during clone preparation; changing it on the same prepared session is rejected.</summary>
    public double? FlowGuidanceScale { get; init; } = 0.7;

    /// <summary>Maximum generated speech tokens. Captured during clone preparation; changing it on the same prepared session is rejected.</summary>
    public long? MaxTokens { get; init; } = 384;

    /// <summary>Enable sampling. Captured during clone preparation; changing it on the same prepared session is rejected.</summary>
    public bool? DoSample { get; init; } = true;

    /// <summary>Stop token generation on EOS. Captured during clone preparation; changing it on the same prepared session is rejected.</summary>
    public bool? StopOnEos { get; init; } = true;

    /// <summary>Voice conversion flow steps; max_steps is also accepted.</summary>
    public long? NumInferenceSteps { get; init; } = 10;

    /// <summary>Sampling seed; omitted chooses a random seed. Captured during clone preparation; changing it on the same prepared session is rejected.</summary>
    public string? Seed { get; init; }

    /// <summary>Voice conversion source WAV path; overrides audio_input.</summary>
    public string? SourceAudio { get; init; }

    /// <summary>Voice conversion target WAV path; overrides the speaker reference.</summary>
    public string? TargetVoice { get; init; }

    /// <summary>Framework long-form text chunk size.</summary>
    public long? TextChunkSize { get; init; } = 128;

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (MaxTokens != null && (MaxTokens.Value < 1))
        {
            throw new ArgumentException("chatterbox clone max_tokens must be at least 1", "MaxTokens");
        }

        if (NumInferenceSteps != null && (NumInferenceSteps.Value < 1))
        {
            throw new ArgumentException("chatterbox clone num_inference_steps must be at least 1", "NumInferenceSteps");
        }
    }

    /// <inheritdoc />
    public override Dictionary<string, string> BuildOptions()
    {
        Dictionary<string, string> options = [];

        if (Exaggeration != null && Exaggeration != 0.5)
        {
            options["exaggeration"] = Exaggeration.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (GuidanceScale != null && GuidanceScale != 0.5)
        {
            options["guidance_scale"] = GuidanceScale.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Temperature != null && Temperature != 0.8)
        {
            options["temperature"] = Temperature.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (RepetitionPenalty != null && RepetitionPenalty != 1.2)
        {
            options["repetition_penalty"] = RepetitionPenalty.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (MinP != null && MinP != 0.05)
        {
            options["min_p"] = MinP.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TopP != null && TopP != 1)
        {
            options["top_p"] = TopP.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (FlowGuidanceScale != null && FlowGuidanceScale != 0.7)
        {
            options["flow_guidance_scale"] = FlowGuidanceScale.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (MaxTokens != null && MaxTokens != 384)
        {
            options["max_tokens"] = MaxTokens.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (DoSample != null && DoSample != true)
        {
            options["do_sample"] = DoSample.Value ? "true" : "false";
        }

        if (StopOnEos != null && StopOnEos != true)
        {
            options["stop_on_eos"] = StopOnEos.Value ? "true" : "false";
        }

        if (NumInferenceSteps != null && NumInferenceSteps != 10)
        {
            options["num_inference_steps"] = NumInferenceSteps.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Seed != null)
        {
            options["seed"] = Seed;
        }

        if (SourceAudio != null)
        {
            options["source_audio"] = SourceAudio;
        }

        if (TargetVoice != null)
        {
            options["target_voice"] = TargetVoice;
        }

        if (TextChunkSize != null && TextChunkSize != 128)
        {
            options["text_chunk_size"] = TextChunkSize.Value.ToString(CultureInfo.InvariantCulture);
        }

        return options;
    }
}