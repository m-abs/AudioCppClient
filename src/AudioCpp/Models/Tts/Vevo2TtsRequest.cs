using System;
using System.Collections.Generic;
using System.Globalization;

using Mabs.AudioCpp.Enums;

namespace Mabs.AudioCpp.Models.Tts;

/// <summary>Convenience request for the <c>vevo2</c> spec, tts task (model default <c>vevo2_q8_0</c>).</summary>
public sealed record Vevo2TtsRequest : TtsRequest
{
    /// <summary>Generation route; omitted selects the task default.</summary>
    public Vevo2Route? Route { get; init; }

    /// <summary>Source audio file for conversion or editing.</summary>
    public string? SourceAudio { get; init; }

    /// <summary>Target voice audio file; overrides structured speaker audio.</summary>
    public string? TargetVoice { get; init; }

    /// <summary>Prosody or melody reference audio file.</summary>
    public string? ProsodyRef { get; init; }

    /// <summary>Style reference audio file.</summary>
    public string? StyleRef { get; init; }

    /// <summary>Text or lyrics; overrides structured text input.</summary>
    public string? TargetText { get; init; }

    /// <summary>Style-reference transcript; reference_text is also accepted.</summary>
    public string? StyleRefText { get; init; }

    /// <summary>Use prosody codes; default depends on route and requires prosody audio when enabled.</summary>
    public bool? UseProsodyCode { get; init; } = false;

    /// <summary>Align reference pitch to the target voice; default depends on route.</summary>
    public bool? UsePitchShift { get; init; } = false;

    /// <summary>Manual source reference pitch shift in semitones.</summary>
    public long? SourceShiftSteps { get; init; } = 0;

    /// <summary>Manual prosody reference pitch shift in semitones.</summary>
    public long? ProsodyShiftSteps { get; init; } = 0;

    /// <summary>Manual style reference pitch shift in semitones.</summary>
    public long? StyleShiftSteps { get; init; } = 0;

    /// <summary>Rescale prosody-code conditioning to this duration; not a general output-length limit. Legacy target_duration_seconds is accepted.</summary>
    public double? DurationSec { get; init; }

    /// <summary>Trim target voice reference before conditioning. Legacy reference_duration_seconds is accepted.</summary>
    public double? ReferenceDurationSec { get; init; }

    /// <summary>Positive AR sampling temperature; default comes from the checkpoint.</summary>
    public double? Temperature { get; init; } = 0.7;

    /// <summary>AR top-k sampling; default comes from the checkpoint.</summary>
    public long? TopK { get; init; } = 20;

    /// <summary>AR nucleus sampling; default comes from the checkpoint.</summary>
    public double? TopP { get; init; } = 0.8;

    /// <summary>Positive AR repetition penalty; default comes from the checkpoint.</summary>
    public double? RepetitionPenalty { get; init; }

    /// <summary>Maximum generated AR tokens.</summary>
    public long? MaxTokens { get; init; } = 500;

    /// <summary>Flow-matching denoising steps.</summary>
    public long? NumInferenceSteps { get; init; } = 32;

    /// <summary>Sampling seed; random when omitted.</summary>
    public string? Seed { get; init; }

    /// <summary>Character budget per text chunk.</summary>
    public long? TextChunkSize { get; init; } = 128;

    /// <summary>Source-audio chunking for style_preserved_vc/svc requires audio_chunk_duration_sec. Auto uses fixed chunks; none disables chunking.</summary>
    public Vevo2AudioChunkMode? AudioChunkMode { get; init; } = Vevo2AudioChunkMode.Auto;

    /// <summary>Source-audio chunk duration for style_preserved_vc/svc; zero or omitted keeps one-shot processing.</summary>
    public double? AudioChunkDurationSec { get; init; } = 0;

    /// <summary>Source chunk overlap and output crossfade; must be smaller than chunk duration.</summary>
    public double? CrossFadeDurationSec { get; init; } = 1;

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (ReferenceDurationSec != null && (ReferenceDurationSec.Value < 0))
        {
            throw new ArgumentException("vevo2 tts reference_duration_sec must be at least 0", "ReferenceDurationSec");
        }

        if (MaxTokens != null && (MaxTokens.Value < 1))
        {
            throw new ArgumentException("vevo2 tts max_tokens must be at least 1", "MaxTokens");
        }

        if (NumInferenceSteps != null && (NumInferenceSteps.Value < 1))
        {
            throw new ArgumentException("vevo2 tts num_inference_steps must be at least 1", "NumInferenceSteps");
        }

        if (TextChunkSize != null && (TextChunkSize.Value < 1))
        {
            throw new ArgumentException("vevo2 tts text_chunk_size must be at least 1", "TextChunkSize");
        }

        if (AudioChunkDurationSec != null && (AudioChunkDurationSec.Value < 0))
        {
            throw new ArgumentException("vevo2 tts audio_chunk_duration_sec must be at least 0", "AudioChunkDurationSec");
        }
    }

    /// <inheritdoc />
    public override Dictionary<string, string> BuildOptions()
    {
        Dictionary<string, string> options = [];

        if (Route != null)
        {
            options["route"] = Route switch
            {
            Vevo2Route.ZeroShotTts => "zero_shot_tts",
            Vevo2Route.TextToSinging => "text_to_singing",
            Vevo2Route.Svs => "svs",
            Vevo2Route.StylePreservedVc => "style_preserved_vc",
            Vevo2Route.StylePreservedSvc => "style_preserved_svc",
            Vevo2Route.StyleConvertedVc => "style_converted_vc",
            Vevo2Route.StyleConvertedSvc => "style_converted_svc",
            Vevo2Route.Editing => "editing",
            Vevo2Route.SingingStyleConversion => "singing_style_conversion",
            Vevo2Route.HummingToSinging => "humming_to_singing",
            Vevo2Route.InstrumentToSinging => "instrument_to_singing",
            _ => throw new ArgumentException("vevo2 tts route must be one of: zero_shot_tts, text_to_singing, svs, style_preserved_vc, style_preserved_svc, style_converted_vc, style_converted_svc, editing, singing_style_conversion, humming_to_singing, instrument_to_singing"),
            };
        }

        if (SourceAudio != null)
        {
            options["source_audio"] = SourceAudio;
        }

        if (TargetVoice != null)
        {
            options["target_voice"] = TargetVoice;
        }

        if (ProsodyRef != null)
        {
            options["prosody_ref"] = ProsodyRef;
        }

        if (StyleRef != null)
        {
            options["style_ref"] = StyleRef;
        }

        if (TargetText != null)
        {
            options["target_text"] = TargetText;
        }

        if (StyleRefText != null)
        {
            options["style_ref_text"] = StyleRefText;
        }

        if (UseProsodyCode != null && UseProsodyCode != false)
        {
            options["use_prosody_code"] = UseProsodyCode.Value ? "true" : "false";
        }

        if (UsePitchShift != null && UsePitchShift != false)
        {
            options["use_pitch_shift"] = UsePitchShift.Value ? "true" : "false";
        }

        if (SourceShiftSteps != null && SourceShiftSteps != 0)
        {
            options["source_shift_steps"] = SourceShiftSteps.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (ProsodyShiftSteps != null && ProsodyShiftSteps != 0)
        {
            options["prosody_shift_steps"] = ProsodyShiftSteps.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (StyleShiftSteps != null && StyleShiftSteps != 0)
        {
            options["style_shift_steps"] = StyleShiftSteps.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (DurationSec != null)
        {
            options["duration_sec"] = DurationSec.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (ReferenceDurationSec != null)
        {
            options["reference_duration_sec"] = ReferenceDurationSec.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Temperature != null)
        {
            options["temperature"] = Temperature.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TopK != null)
        {
            options["top_k"] = TopK.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TopP != null)
        {
            options["top_p"] = TopP.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (RepetitionPenalty != null)
        {
            options["repetition_penalty"] = RepetitionPenalty.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (MaxTokens != null && MaxTokens != 500)
        {
            options["max_tokens"] = MaxTokens.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (NumInferenceSteps != null && NumInferenceSteps != 32)
        {
            options["num_inference_steps"] = NumInferenceSteps.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Seed != null)
        {
            options["seed"] = Seed;
        }

        if (TextChunkSize != null && TextChunkSize != 128)
        {
            options["text_chunk_size"] = TextChunkSize.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (AudioChunkMode != null && AudioChunkMode != Vevo2AudioChunkMode.Auto)
        {
            options["audio_chunk_mode"] = AudioChunkMode switch
            {
            Vevo2AudioChunkMode.Auto => "auto",
            Vevo2AudioChunkMode.Fixed => "fixed",
            Vevo2AudioChunkMode.None => "none",
            _ => throw new ArgumentException("vevo2 tts audio_chunk_mode must be one of: auto, fixed, none"),
            };
        }

        if (AudioChunkDurationSec != null)
        {
            options["audio_chunk_duration_sec"] = AudioChunkDurationSec.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (CrossFadeDurationSec != null && CrossFadeDurationSec != 1)
        {
            options["cross_fade_duration_sec"] = CrossFadeDurationSec.Value.ToString(CultureInfo.InvariantCulture);
        }

        return options;
    }
}