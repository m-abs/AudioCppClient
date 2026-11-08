using System;
using System.Collections.Generic;
using System.Globalization;

using Mabs.AudioCpp.Enums;

namespace Mabs.AudioCpp.Models.Tts;

/// <summary>Convenience request for the <c>magpie_tts</c> spec, tts task (model default <c>magpie_tts_q8_0</c>).</summary>
public sealed record MagpieTtsRequest : TtsRequest
{
    /// <summary>Target synthesis language code used by the Magpie text frontend; default en.</summary>
    public new string? Language { get; init; } = "en";

    /// <summary>Packaged MagpieTTS voice name; default Aria. Numeric speaker index strings are also accepted within the checkpoint speaker count.</summary>
    public new MagpieTtsVoiceId? VoiceId { get; init; }

    /// <summary>Autoregressive audio-token sampling temperature; default 0.6 from the v2607 model config.</summary>
    public double? Temperature { get; init; } = 0.6;

    /// <summary>Autoregressive audio-token top-k sampling limit; default 80.</summary>
    public long? TopK { get; init; } = 80;

    /// <summary>Classifier-free guidance scale for Magpie decoder logits; default 2.5.</summary>
    public double? GuidanceScale { get; init; } = 2.5;

    /// <summary>Maximum Magpie decoder frames per text segment; default 500 from the v2607 model config.</summary>
    public long? MaxTokens { get; init; } = 500;

    /// <summary>Maximum Unicode codepoints per long-form text chunk; default 100 for zh and ko, otherwise 300.</summary>
    public long? TextChunkSize { get; init; } = 300;

    /// <summary>Framework text chunking mode.</summary>
    public MagpieTtsTextChunkMode? TextChunkMode { get; init; } = MagpieTtsTextChunkMode.Default;

    /// <summary>Seed for audio-token sampling; default 0.</summary>
    public string? Seed { get; init; } = "0";

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (VoiceId != null)
        {
            throw new ArgumentException("magpie_tts tts does not accept voice_id (capability built_in_voices is false)");
        }

        if (Temperature != null && (Temperature.Value < 0))
        {
            throw new ArgumentException("magpie_tts tts temperature must be at least 0", "Temperature");
        }

        if (TopK != null && (TopK.Value < 1))
        {
            throw new ArgumentException("magpie_tts tts top_k must be at least 1", "TopK");
        }

        if (GuidanceScale != null && (GuidanceScale.Value < 0))
        {
            throw new ArgumentException("magpie_tts tts guidance_scale must be at least 0", "GuidanceScale");
        }

        if (MaxTokens != null && (MaxTokens.Value < 1))
        {
            throw new ArgumentException("magpie_tts tts max_tokens must be at least 1", "MaxTokens");
        }

        if (TextChunkSize != null && (TextChunkSize.Value < 1))
        {
            throw new ArgumentException("magpie_tts tts text_chunk_size must be at least 1", "TextChunkSize");
        }
    }

    /// <inheritdoc />
    public override Dictionary<string, string> BuildOptions()
    {
        Dictionary<string, string> options = [];

        if (Language != null && Language != "en")
        {
            options["language"] = Language;
        }

        if (VoiceId != null && VoiceId != MagpieTtsVoiceId.Aria)
        {
            options["voice_id"] = VoiceId switch
            {
            MagpieTtsVoiceId.Aria => "Aria",
            MagpieTtsVoiceId.Jason => "Jason",
            MagpieTtsVoiceId.John => "John",
            MagpieTtsVoiceId.Leo => "Leo",
            MagpieTtsVoiceId.Sofia => "Sofia",
            _ => throw new ArgumentException("magpie_tts tts voice_id must be one of: Aria, Jason, John, Leo, Sofia"),
            };
        }

        if (Temperature != null && Temperature != 0.6)
        {
            options["temperature"] = Temperature.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TopK != null && TopK != 80)
        {
            options["top_k"] = TopK.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (GuidanceScale != null && GuidanceScale != 2.5)
        {
            options["guidance_scale"] = GuidanceScale.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (MaxTokens != null && MaxTokens != 500)
        {
            options["max_tokens"] = MaxTokens.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TextChunkSize != null)
        {
            options["text_chunk_size"] = TextChunkSize.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TextChunkMode != null && TextChunkMode != MagpieTtsTextChunkMode.Default)
        {
            options["text_chunk_mode"] = TextChunkMode switch
            {
            MagpieTtsTextChunkMode.Default => "default",
            MagpieTtsTextChunkMode.TagAware => "tag_aware",
            MagpieTtsTextChunkMode.Japanese => "japanese",
            MagpieTtsTextChunkMode.Endline => "endline",
            _ => throw new ArgumentException("magpie_tts tts text_chunk_mode must be one of: default, tag_aware, japanese, endline"),
            };
        }

        if (Seed != null && Seed != "0")
        {
            options["seed"] = Seed;
        }

        return options;
    }
}