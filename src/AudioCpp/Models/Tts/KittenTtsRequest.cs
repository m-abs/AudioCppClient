using System;
using System.Collections.Generic;
using System.Globalization;

using Mabs.AudioCpp.Enums;

namespace Mabs.AudioCpp.Models.Tts;

/// <summary>Convenience request for the <c>kitten_tts</c> spec, tts task (model default <c>kitten_tts_mini_0_8_orig</c>).</summary>
public sealed record KittenTtsRequest : TtsRequest
{
    /// <summary>Built-in voice ID or package alias; structured voice input (CLI --voice-id) takes precedence.</summary>
    public new KittenTtsVoiceId? VoiceId { get; init; }

    /// <summary>Speech speed multiplier; larger values produce faster speech. Must be finite and positive.</summary>
    public double? Speed { get; init; }

    /// <summary>Decoder noise seed for the current request; omitted requests choose a fresh random seed.</summary>
    public string? Seed { get; init; }

    /// <summary>Long-form text chunking mode.</summary>
    public KittenTtsTextChunkMode? TextChunkMode { get; init; }

    /// <summary>Maximum Unicode codepoints per long-form chunk.</summary>
    public long? TextChunkSize { get; init; }

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (TextChunkSize != null && (TextChunkSize.Value < 1))
        {
            throw new ArgumentException("kitten_tts tts text_chunk_size must be at least 1", "TextChunkSize");
        }
    }

    /// <inheritdoc />
    public override Dictionary<string, string> BuildOptions()
    {
        Dictionary<string, string> options = [];

        if (VoiceId != null && VoiceId != KittenTtsVoiceId.ExprVoice5M)
        {
            options["voice_id"] = VoiceId switch
            {
            KittenTtsVoiceId.Bella => "Bella",
            KittenTtsVoiceId.Bruno => "Bruno",
            KittenTtsVoiceId.Hugo => "Hugo",
            KittenTtsVoiceId.Jasper => "Jasper",
            KittenTtsVoiceId.Kiki => "Kiki",
            KittenTtsVoiceId.Leo => "Leo",
            KittenTtsVoiceId.Luna => "Luna",
            KittenTtsVoiceId.Rosie => "Rosie",
            KittenTtsVoiceId.ExprVoice2F => "expr-voice-2-f",
            KittenTtsVoiceId.ExprVoice2M => "expr-voice-2-m",
            KittenTtsVoiceId.ExprVoice3F => "expr-voice-3-f",
            KittenTtsVoiceId.ExprVoice3M => "expr-voice-3-m",
            KittenTtsVoiceId.ExprVoice4F => "expr-voice-4-f",
            KittenTtsVoiceId.ExprVoice4M => "expr-voice-4-m",
            KittenTtsVoiceId.ExprVoice5F => "expr-voice-5-f",
            KittenTtsVoiceId.ExprVoice5M => "expr-voice-5-m",
            _ => throw new ArgumentException("kitten_tts tts voice_id must be one of: Bella, Bruno, Hugo, Jasper, Kiki, Leo, Luna, Rosie, expr-voice-2-f, expr-voice-2-m, expr-voice-3-f, expr-voice-3-m, expr-voice-4-f, expr-voice-4-m, expr-voice-5-f, expr-voice-5-m"),
            };
        }

        if (Speed != null && Speed != 1)
        {
            options["speed"] = Speed.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Seed != null)
        {
            options["seed"] = Seed;
        }

        if (TextChunkMode != null && TextChunkMode != KittenTtsTextChunkMode.Default)
        {
            options["text_chunk_mode"] = TextChunkMode switch
            {
            KittenTtsTextChunkMode.Default => "default",
            KittenTtsTextChunkMode.TagAware => "tag_aware",
            KittenTtsTextChunkMode.Japanese => "japanese",
            KittenTtsTextChunkMode.Endline => "endline",
            _ => throw new ArgumentException("kitten_tts tts text_chunk_mode must be one of: default, tag_aware, japanese, endline"),
            };
        }

        if (TextChunkSize != null && TextChunkSize != 400)
        {
            options["text_chunk_size"] = TextChunkSize.Value.ToString(CultureInfo.InvariantCulture);
        }

        return options;
    }
}