using System;
using System.Collections.Generic;
using System.Globalization;

namespace Mabs.AudioCpp.Models.Tts;

/// <summary>Convenience request for the <c>pocket_tts_small</c> spec, tts task (model default <c>pocket_tts_small_english_18m_refl_f32</c>).</summary>
public sealed record PocketTtsSmallTtsRequest : TtsRequest
{
    /// <summary>Extra frames after EOS; -1 uses the model-derived value.</summary>
    public long? FramesAfterEos { get; init; }

    /// <summary>Maximum generated audio frames; zero estimates the cap from text length. Captured during session preparation.</summary>
    public long? MaxFrames { get; init; }

    /// <summary>Tokenizer tokens per text chunk; must be positive, default 50. Set during preparation and overridable per request.</summary>
    public long? TextChunkTokens { get; init; }

    /// <summary>Positive flow noise temperature; defaults to the model configuration. Captured during session preparation.</summary>
    public double? Temperature { get; init; }

    /// <summary>Noise seed; omitted chooses a random seed. Captured during session preparation.</summary>
    public string? Seed { get; init; }

    /// <summary>Noise clamp; a negative value disables clamping. Captured during session preparation.</summary>
    public double? NoiseClamp { get; init; }

    /// <summary>EOS detection threshold; defaults to the checkpoint recommendation.</summary>
    public double? EosThreshold { get; init; }

    /// <summary>Precomputed noise schedule file. Captured during session preparation.</summary>
    public string? NoiseFile { get; init; }

    /// <summary>Precomputed voice-state embedding path. Captured during session preparation.</summary>
    public string? VoiceEmbeddingFile { get; init; }

    /// <summary>Truncate voice clone audio. Captured during session preparation.</summary>
    public bool? TruncateCloneAudio { get; init; }

    /// <summary>Optional per-request character chunk size; otherwise use sentence/token budgeting.</summary>
    public long? TextChunkSize { get; init; }

    /// <summary>Export the prepared voice state during session preparation.</summary>
    public string? ExportVoiceStateFile { get; init; }

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (FramesAfterEos != null && (FramesAfterEos.Value < -1))
        {
            throw new ArgumentException("pocket_tts_small tts frames_after_eos must be at least -1", "FramesAfterEos");
        }

        if (MaxFrames != null && (MaxFrames.Value < 0))
        {
            throw new ArgumentException("pocket_tts_small tts max_frames must be at least 0", "MaxFrames");
        }

        if (TextChunkTokens != null && (TextChunkTokens.Value < 1))
        {
            throw new ArgumentException("pocket_tts_small tts text_chunk_tokens must be at least 1", "TextChunkTokens");
        }
    }

    /// <inheritdoc />
    public override Dictionary<string, string> BuildOptions()
    {
        Dictionary<string, string> options = [];

        if (FramesAfterEos != null && FramesAfterEos != -1)
        {
            options["frames_after_eos"] = FramesAfterEos.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (MaxFrames != null && MaxFrames != 0)
        {
            options["max_frames"] = MaxFrames.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TextChunkTokens != null && TextChunkTokens != 50)
        {
            options["text_chunk_tokens"] = TextChunkTokens.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Temperature != null)
        {
            options["temperature"] = Temperature.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Seed != null)
        {
            options["seed"] = Seed;
        }

        if (NoiseClamp != null && NoiseClamp != -1)
        {
            options["noise_clamp"] = NoiseClamp.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (EosThreshold != null && EosThreshold != -1)
        {
            options["eos_threshold"] = EosThreshold.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (NoiseFile != null)
        {
            options["noise_file"] = NoiseFile;
        }

        if (VoiceEmbeddingFile != null)
        {
            options["voice_embedding_file"] = VoiceEmbeddingFile;
        }

        if (TruncateCloneAudio != null && TruncateCloneAudio != false)
        {
            options["truncate_clone_audio"] = TruncateCloneAudio.Value ? "true" : "false";
        }

        if (TextChunkSize != null)
        {
            options["text_chunk_size"] = TextChunkSize.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (ExportVoiceStateFile != null)
        {
            options["export_voice_state_file"] = ExportVoiceStateFile;
        }

        return options;
    }
}