using System;
using System.Collections.Generic;
using System.Globalization;

using Mabs.AudioCpp.Enums;

namespace Mabs.AudioCpp.Models.Tts;

/// <summary>Convenience request for the <c>irodori_tts</c> spec, tts task (model default <c>irodori_tts_v4_small_q8_0</c>).</summary>
public sealed record IrodoriTtsRequest : TtsRequest
{
    /// <summary>Codec latent frames per streaming audio event; default 12 (0.48 seconds at 25 Hz).</summary>
    public long? StreamFramesPerEvent { get; init; } = 12;

    /// <summary>Text language code; Irodori-TTS supports Japanese only.</summary>
    public new string? Language { get; init; } = "ja";

    /// <summary>Natural-language voice or style instruction describing target voice identity, style, or emotion; supported only by caption-conditioned checkpoints. Legacy instruction and caption are accepted as aliases.</summary>
    public string? Instruct { get; init; }

    /// <summary>Use no-reference generation; default true unless a speaker reference is provided.</summary>
    public bool? NoRef { get; init; } = true;

    /// <summary>Speaker Inversion embedding (*.speaker.safetensors written by Irodori-TTS training) used as the speaker condition instead of reference audio; use it with the base checkpoint it was trained on. Cannot be combined with reference audio or no_ref=true.</summary>
    public string? SpeakerEmbeddingFile { get; init; }

    /// <summary>Trim reference audio to its first reference_duration_sec seconds before encoding it, as Python Irodori-TTS does; default is the checkpoint's ref_max_seconds (30 when it has none); 0 keeps the whole reference.</summary>
    public double? ReferenceDurationSec { get; init; }

    /// <summary>RF Euler timestep schedule: linear or sway; default linear.</summary>
    public string? TScheduleMode { get; init; } = "linear";

    /// <summary>Sway Sampling coefficient; must be finite and produce strictly decreasing timesteps. Default -1.0.</summary>
    public double? SwayCoeff { get; init; } = -1;

    /// <summary>Sampling steps; omitted uses 40 for RF checkpoints or 4 for MeanFlow checkpoints.</summary>
    public long? NumInferenceSteps { get; init; } = 40;

    /// <summary>Target duration in seconds; omitted or nonpositive values select predicted duration.</summary>
    public double? DurationSec { get; init; } = 0;

    /// <summary>Predicted-duration multiplier; must be &gt; 0 when set; default 1.0.</summary>
    public double? DurationScale { get; init; } = 1;

    /// <summary>Text chunk policy; word_budget also accepts the legacy spelling default.</summary>
    public IrodoriTtsTextChunkMode? TextChunkMode { get; init; } = IrodoriTtsTextChunkMode.Endline;

    /// <summary>Maximum characters per text chunk; default 100, or the model text-token window if smaller.</summary>
    public long? TextChunkSize { get; init; }

    /// <summary>Minimum generated duration in seconds; must be &gt; 0 and no greater than max_duration_sec; default 0.5.</summary>
    public double? MinDurationSec { get; init; } = 0.5;

    /// <summary>Maximum generated duration in seconds; must be at least min_duration_sec; default 30. Must be &gt; 0.</summary>
    public double? MaxDurationSec { get; init; } = 30;

    /// <summary>Text classifier-free guidance scale; default 3.0. Nonpositive values disable the corresponding CFG branch.</summary>
    public double? TextGuidanceScale { get; init; } = 3;

    /// <summary>Speaker classifier-free guidance scale; default 5.0. Nonpositive values disable the corresponding CFG branch.</summary>
    public double? SpeakerGuidanceScale { get; init; } = 5;

    /// <summary>Caption classifier-free guidance scale; default 3.0. Nonpositive values disable the corresponding CFG branch.</summary>
    public double? CaptionGuidanceScale { get; init; } = 3;

    /// <summary>Override all classifier-free guidance scales when set. Nonpositive values disable the corresponding CFG branch.</summary>
    public double? GuidanceScale { get; init; }

    /// <summary>Classifier-free guidance combination mode; default independent.</summary>
    public IrodoriTtsGuidanceMode? GuidanceMode { get; init; } = IrodoriTtsGuidanceMode.Independent;

    /// <summary>Minimum diffusion timestep value where guidance is active; default 0.5. The parser does not enforce a nonnegative bound.</summary>
    public double? GuidanceMinT { get; init; } = 0.5;

    /// <summary>Maximum diffusion timestep value where guidance is active; default 1.0. The parser does not enforce a nonnegative bound.</summary>
    public double? GuidanceMaxT { get; init; } = 1;

    /// <summary>Unsigned 32-bit generation seed in [0, 4294967295]; omitted uses a random seed.</summary>
    public string? Seed { get; init; }

    /// <summary>Trim trailing silence-like samples; default true.</summary>
    public bool? TrimTail { get; init; } = true;

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (StreamFramesPerEvent != null && (StreamFramesPerEvent.Value < 1))
        {
            throw new ArgumentException("irodori_tts tts stream_frames_per_event must be at least 1", "StreamFramesPerEvent");
        }

        if (ReferenceDurationSec != null && (ReferenceDurationSec.Value < 0))
        {
            throw new ArgumentException("irodori_tts tts reference_duration_sec must be at least 0", "ReferenceDurationSec");
        }

        if (NumInferenceSteps != null && (NumInferenceSteps.Value < 1))
        {
            throw new ArgumentException("irodori_tts tts num_inference_steps must be at least 1", "NumInferenceSteps");
        }

        if (DurationScale != null && (DurationScale.Value < 0))
        {
            throw new ArgumentException("irodori_tts tts duration_scale must be at least 0", "DurationScale");
        }

        if (TextChunkSize != null && (TextChunkSize.Value < 1))
        {
            throw new ArgumentException("irodori_tts tts text_chunk_size must be at least 1", "TextChunkSize");
        }

        if (MinDurationSec != null && (MinDurationSec.Value < 0))
        {
            throw new ArgumentException("irodori_tts tts min_duration_sec must be at least 0", "MinDurationSec");
        }

        if (MaxDurationSec != null && (MaxDurationSec.Value < 0))
        {
            throw new ArgumentException("irodori_tts tts max_duration_sec must be at least 0", "MaxDurationSec");
        }
    }

    /// <inheritdoc />
    public override Dictionary<string, string> BuildOptions()
    {
        Dictionary<string, string> options = [];

        if (StreamFramesPerEvent != null && StreamFramesPerEvent != 12)
        {
            options["stream_frames_per_event"] = StreamFramesPerEvent.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Language != null && Language != "ja")
        {
            options["language"] = Language;
        }

        if (Instruct != null)
        {
            options["instruct"] = Instruct;
        }

        if (NoRef != null && NoRef != true)
        {
            options["no_ref"] = NoRef.Value ? "true" : "false";
        }

        if (SpeakerEmbeddingFile != null)
        {
            options["speaker_embedding_file"] = SpeakerEmbeddingFile;
        }

        if (ReferenceDurationSec != null)
        {
            options["reference_duration_sec"] = ReferenceDurationSec.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TScheduleMode != null && TScheduleMode != "linear")
        {
            options["t_schedule_mode"] = TScheduleMode;
        }

        if (SwayCoeff != null && SwayCoeff != -1)
        {
            options["sway_coeff"] = SwayCoeff.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (NumInferenceSteps != null)
        {
            options["num_inference_steps"] = NumInferenceSteps.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (DurationSec != null)
        {
            options["duration_sec"] = DurationSec.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (DurationScale != null && DurationScale != 1)
        {
            options["duration_scale"] = DurationScale.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TextChunkMode != null && TextChunkMode != IrodoriTtsTextChunkMode.Endline)
        {
            options["text_chunk_mode"] = TextChunkMode switch
            {
            IrodoriTtsTextChunkMode.WordBudget => "word_budget",
            IrodoriTtsTextChunkMode.TagAware => "tag_aware",
            IrodoriTtsTextChunkMode.Japanese => "japanese",
            IrodoriTtsTextChunkMode.Endline => "endline",
            _ => throw new ArgumentException("irodori_tts tts text_chunk_mode must be one of: word_budget, tag_aware, japanese, endline"),
            };
        }

        if (TextChunkSize != null)
        {
            options["text_chunk_size"] = TextChunkSize.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (MinDurationSec != null && MinDurationSec != 0.5)
        {
            options["min_duration_sec"] = MinDurationSec.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (MaxDurationSec != null && MaxDurationSec != 30)
        {
            options["max_duration_sec"] = MaxDurationSec.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TextGuidanceScale != null && TextGuidanceScale != 3)
        {
            options["text_guidance_scale"] = TextGuidanceScale.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (SpeakerGuidanceScale != null && SpeakerGuidanceScale != 5)
        {
            options["speaker_guidance_scale"] = SpeakerGuidanceScale.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (CaptionGuidanceScale != null && CaptionGuidanceScale != 3)
        {
            options["caption_guidance_scale"] = CaptionGuidanceScale.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (GuidanceScale != null)
        {
            options["guidance_scale"] = GuidanceScale.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (GuidanceMode != null && GuidanceMode != IrodoriTtsGuidanceMode.Independent)
        {
            options["guidance_mode"] = GuidanceMode switch
            {
            IrodoriTtsGuidanceMode.Independent => "independent",
            IrodoriTtsGuidanceMode.Joint => "joint",
            IrodoriTtsGuidanceMode.Alternating => "alternating",
            _ => throw new ArgumentException("irodori_tts tts guidance_mode must be one of: independent, joint, alternating"),
            };
        }

        if (GuidanceMinT != null && GuidanceMinT != 0.5)
        {
            options["guidance_min_t"] = GuidanceMinT.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (GuidanceMaxT != null && GuidanceMaxT != 1)
        {
            options["guidance_max_t"] = GuidanceMaxT.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Seed != null)
        {
            options["seed"] = Seed;
        }

        if (TrimTail != null && TrimTail != true)
        {
            options["trim_tail"] = TrimTail.Value ? "true" : "false";
        }

        return options;
    }
}