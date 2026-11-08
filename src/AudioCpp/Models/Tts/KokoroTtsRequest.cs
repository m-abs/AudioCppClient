using System;
using System.Collections.Generic;
using System.Globalization;

using Mabs.AudioCpp.Enums;

namespace Mabs.AudioCpp.Models.Tts;

/// <summary>Convenience request for the <c>kokoro_tts</c> spec, tts task (model default <c>kokoro_82m_q8_0</c>).</summary>
public sealed record KokoroTtsRequest : TtsRequest
{
    /// <summary>Built-in voice ID; structured voice input (CLI --voice-id) takes precedence.</summary>
    public new KokoroTtsVoiceId? VoiceId { get; init; } = KokoroTtsVoiceId.AfHeart;

    /// <summary>Language selection precedence: voice-style language, transcript language (CLI --language), request-option language, then voice prefix. Must match the first voice in a blend.</summary>
    public new string? Language { get; init; }

    /// <summary>Decoder noise seed for the current request; omitted requests choose a fresh random seed.</summary>
    public string? Seed { get; init; }

    /// <summary>Speech speed multiplier; default 1.0. Must be finite and positive.</summary>
    public double? Speed { get; init; } = 1;

    /// <summary>Kokoro-vocabulary phonemes to synthesize directly, bypassing the built-in eSpeak-ng G2P. One entry per chunk: entries are rendered in order and merged into one result, so the caller chooses the split points that only its own G2P knows. Each entry must be at most 510 symbols and must not be empty -- an entry is a chunk to speak, so drop it rather than leaving it blank. Text is still required and its language must still match the voice.</summary>
    public string? Phonemes { get; init; }

    /// <summary>Maximum codepoints per text chunk before synthesis. Default 100 for Chinese and Japanese voices, otherwise 240.</summary>
    public long? TextChunkSize { get; init; }

    /// <summary>Report where each phoneme group lands in the output, from the durations the model predicts before vocoding. Entries are phoneme groups in Kokoro's own alphabet, not written words, and do not map one-to-one onto the input text: see docs/models/kokoro_tts.md. Default false.</summary>
    public bool? ReturnTimestamps { get; init; } = false;

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (TextChunkSize != null && (TextChunkSize.Value < 1))
        {
            throw new ArgumentException("kokoro_tts tts text_chunk_size must be at least 1", "TextChunkSize");
        }
    }

    /// <inheritdoc />
    public override Dictionary<string, string> BuildOptions()
    {
        Dictionary<string, string> options = [];

        if (VoiceId != null && VoiceId != KokoroTtsVoiceId.AfHeart)
        {
            options["voice_id"] = VoiceId switch
            {
            KokoroTtsVoiceId.AfAlloy => "af_alloy",
            KokoroTtsVoiceId.AfAoede => "af_aoede",
            KokoroTtsVoiceId.AfBella => "af_bella",
            KokoroTtsVoiceId.AfHeart => "af_heart",
            KokoroTtsVoiceId.AfJessica => "af_jessica",
            KokoroTtsVoiceId.AfKore => "af_kore",
            KokoroTtsVoiceId.AfNicole => "af_nicole",
            KokoroTtsVoiceId.AfNova => "af_nova",
            KokoroTtsVoiceId.AfRiver => "af_river",
            KokoroTtsVoiceId.AfSarah => "af_sarah",
            KokoroTtsVoiceId.AfSky => "af_sky",
            KokoroTtsVoiceId.AmAdam => "am_adam",
            KokoroTtsVoiceId.AmEcho => "am_echo",
            KokoroTtsVoiceId.AmEric => "am_eric",
            KokoroTtsVoiceId.AmFenrir => "am_fenrir",
            KokoroTtsVoiceId.AmLiam => "am_liam",
            KokoroTtsVoiceId.AmMichael => "am_michael",
            KokoroTtsVoiceId.AmOnyx => "am_onyx",
            KokoroTtsVoiceId.AmPuck => "am_puck",
            KokoroTtsVoiceId.AmSanta => "am_santa",
            KokoroTtsVoiceId.BfAlice => "bf_alice",
            KokoroTtsVoiceId.BfEmma => "bf_emma",
            KokoroTtsVoiceId.BfIsabella => "bf_isabella",
            KokoroTtsVoiceId.BfLily => "bf_lily",
            KokoroTtsVoiceId.BmDaniel => "bm_daniel",
            KokoroTtsVoiceId.BmFable => "bm_fable",
            KokoroTtsVoiceId.BmGeorge => "bm_george",
            KokoroTtsVoiceId.BmLewis => "bm_lewis",
            KokoroTtsVoiceId.EfDora => "ef_dora",
            KokoroTtsVoiceId.EmAlex => "em_alex",
            KokoroTtsVoiceId.EmSanta => "em_santa",
            KokoroTtsVoiceId.FfSiwis => "ff_siwis",
            KokoroTtsVoiceId.HfAlpha => "hf_alpha",
            KokoroTtsVoiceId.HfBeta => "hf_beta",
            KokoroTtsVoiceId.HmOmega => "hm_omega",
            KokoroTtsVoiceId.HmPsi => "hm_psi",
            KokoroTtsVoiceId.IfSara => "if_sara",
            KokoroTtsVoiceId.ImNicola => "im_nicola",
            KokoroTtsVoiceId.JfAlpha => "jf_alpha",
            KokoroTtsVoiceId.JfGongitsune => "jf_gongitsune",
            KokoroTtsVoiceId.JfNezumi => "jf_nezumi",
            KokoroTtsVoiceId.JfTebukuro => "jf_tebukuro",
            KokoroTtsVoiceId.JmKumo => "jm_kumo",
            KokoroTtsVoiceId.PfDora => "pf_dora",
            KokoroTtsVoiceId.PmAlex => "pm_alex",
            KokoroTtsVoiceId.PmSanta => "pm_santa",
            KokoroTtsVoiceId.ZfXiaobei => "zf_xiaobei",
            KokoroTtsVoiceId.ZfXiaoni => "zf_xiaoni",
            KokoroTtsVoiceId.ZfXiaoxiao => "zf_xiaoxiao",
            KokoroTtsVoiceId.ZfXiaoyi => "zf_xiaoyi",
            KokoroTtsVoiceId.ZmYunjian => "zm_yunjian",
            KokoroTtsVoiceId.ZmYunxi => "zm_yunxi",
            KokoroTtsVoiceId.ZmYunxia => "zm_yunxia",
            KokoroTtsVoiceId.ZmYunyang => "zm_yunyang",
            _ => throw new ArgumentException("kokoro_tts tts voice_id must be one of: af_alloy, af_aoede, af_bella, af_heart, af_jessica, af_kore, af_nicole, af_nova, af_river, af_sarah, af_sky, am_adam, am_echo, am_eric, am_fenrir, am_liam, am_michael, am_onyx, am_puck, am_santa, bf_alice, bf_emma, bf_isabella, bf_lily, bm_daniel, bm_fable, bm_george, bm_lewis, ef_dora, em_alex, em_santa, ff_siwis, hf_alpha, hf_beta, hm_omega, hm_psi, if_sara, im_nicola, jf_alpha, jf_gongitsune, jf_nezumi, jf_tebukuro, jm_kumo, pf_dora, pm_alex, pm_santa, zf_xiaobei, zf_xiaoni, zf_xiaoxiao, zf_xiaoyi, zm_yunjian, zm_yunxi, zm_yunxia, zm_yunyang"),
            };
        }

        if (Language != null)
        {
            options["language"] = Language;
        }

        if (Seed != null)
        {
            options["seed"] = Seed;
        }

        if (Speed != null && Speed != 1)
        {
            options["speed"] = Speed.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Phonemes != null)
        {
            options["phonemes"] = Phonemes;
        }

        if (TextChunkSize != null)
        {
            options["text_chunk_size"] = TextChunkSize.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (ReturnTimestamps != null && ReturnTimestamps != false)
        {
            options["return_timestamps"] = ReturnTimestamps.Value ? "true" : "false";
        }

        return options;
    }
}