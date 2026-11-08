using System;
using System.Collections.Generic;
using System.Globalization;

namespace Mabs.AudioCpp.Models.Tts;

/// <summary>Convenience request for the <c>gradium_phonon</c> spec, tts task (model default <c>gradium_phonon_31m_en_q8_0</c>).</summary>
public sealed record GradiumPhononTtsRequest : TtsRequest
{
    /// <summary>Frames kept after EOS, counting the EOS frame; -1 keeps 4 for texts of up to four words and 2 otherwise, as the reference does.</summary>
    public long? FramesAfterEos { get; init; } = -1;

    /// <summary>Maximum generated audio frames; zero estimates the cap from text length. Captured during session preparation.</summary>
    public long? MaxFrames { get; init; } = 0;

    /// <summary>Tokenizer tokens per text chunk; must be positive, default 50. Set during preparation and overridable per request.</summary>
    public long? TextChunkTokens { get; init; } = 50;

    /// <summary>Positive flow noise temperature. Captured during session preparation.</summary>
    public double? Temperature { get; init; } = 0.3;

    /// <summary>Noise seed; omitted chooses a random seed. Captured during session preparation.</summary>
    public string? Seed { get; init; }

    /// <summary>Noise clamp; a negative value disables clamping. Captured during session preparation.</summary>
    public double? NoiseClamp { get; init; } = -1;

    /// <summary>EOS detection threshold on the EOS logit. Captured during session preparation.</summary>
    public double? EosThreshold { get; init; } = 4;

    /// <summary>Precomputed noise schedule, raw float32 [frames, 32], used as given. Captured during session preparation.</summary>
    public string? NoiseFile { get; init; }

    /// <summary>Voice file: a speaker_wavs latent file such as those of gradium/phonon-voices, or an exported voice state. Captured during session preparation.</summary>
    public string? VoiceEmbeddingFile { get; init; }

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
            throw new ArgumentException("gradium_phonon tts frames_after_eos must be at least -1", "FramesAfterEos");
        }

        if (MaxFrames != null && (MaxFrames.Value < 0))
        {
            throw new ArgumentException("gradium_phonon tts max_frames must be at least 0", "MaxFrames");
        }

        if (TextChunkTokens != null && (TextChunkTokens.Value < 1))
        {
            throw new ArgumentException("gradium_phonon tts text_chunk_tokens must be at least 1", "TextChunkTokens");
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

        if (Temperature != null && Temperature != 0.3)
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

        if (EosThreshold != null && EosThreshold != 4)
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