using System;
using System.Collections.Generic;
using System.Globalization;

using Mabs.AudioCpp.Enums;

namespace Mabs.AudioCpp.Models.Tts;

/// <summary>Convenience request for the <c>ming3_audio</c> spec, tts task (model default <c>ming3_audio_tiny_bf16</c>).</summary>
public sealed record Ming3AudioTtsRequest : TtsRequest
{
    /// <summary>Thinker system instruction. When supplied, the user prompt precedes audio as in the official chat examples.</summary>
    public string? SystemPrompt { get; init; }

    /// <summary>Enable the Thinker chat template's detailed thinking mode.</summary>
    public bool? EnableThinking { get; init; }

    /// <summary>Thinker instruction accompanying the input audio; omitted ASR requests use the official transcription prompt.</summary>
    public string? Prompt { get; init; }

    /// <summary>Maximum generated Thinker text tokens.</summary>
    public long? MaxTokens { get; init; }

    /// <summary>Maximum generated Talker acoustic patches per text fragment.</summary>
    public long? MaxAudioTokens { get; init; }

    /// <summary>Talker built-in voice name; ignored when reference audio is supplied.</summary>
    public new Ming3AudioVoiceId? VoiceId { get; init; }

    /// <summary>Reference transcript enabling acoustic-prefix continuation; without it reference audio supplies speaker conditioning only.</summary>
    public string? ReferenceText { get; init; }

    /// <summary>Talker output language; inferred from its input text when omitted.</summary>
    public new string? Language { get; init; }

    /// <summary>Talker flow and reference-posterior sampling seed. Thinker generation is greedy.</summary>
    public string? Seed { get; init; }

    /// <summary>Talker conditioned prediction plus guidance_scale times its difference from the null prediction.</summary>
    public double? GuidanceScale { get; init; }

    /// <summary>Talker flow SDE noise scale.</summary>
    public double? Sigma { get; init; }

    /// <summary>Talker flow sampling temperature; zero still uses seeded initial noise.</summary>
    public double? Temperature { get; init; }

    /// <summary>Synthesize text as one fragment without framework text chunking.</summary>
    public bool? RawText { get; init; }

    /// <summary>Trim trailing low-energy 100-ms frames per Talker fragment, retaining a 300-ms tail.</summary>
    public bool? TrimSilence { get; init; }

    /// <summary>Talker text chunk budget in Unicode codepoints.</summary>
    public long? TextChunkSize { get; init; }

    /// <summary>Framework Talker text chunking policy.</summary>
    public Ming3AudioTextChunkMode? TextChunkMode { get; init; }

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (MaxTokens != null && (MaxTokens.Value < 1))
        {
            throw new ArgumentException("ming3_audio tts max_tokens must be at least 1", "MaxTokens");
        }

        if (MaxAudioTokens != null && (MaxAudioTokens.Value < 1))
        {
            throw new ArgumentException("ming3_audio tts max_audio_tokens must be at least 1", "MaxAudioTokens");
        }

        if (Sigma != null && (Sigma.Value < 0))
        {
            throw new ArgumentException("ming3_audio tts sigma must be at least 0", "Sigma");
        }

        if (Temperature != null && (Temperature.Value < 0))
        {
            throw new ArgumentException("ming3_audio tts temperature must be at least 0", "Temperature");
        }

        if (TextChunkSize != null && (TextChunkSize.Value < 1))
        {
            throw new ArgumentException("ming3_audio tts text_chunk_size must be at least 1", "TextChunkSize");
        }
    }

    /// <inheritdoc />
    public override Dictionary<string, string> BuildOptions()
    {
        Dictionary<string, string> options = [];

        if (SystemPrompt != null)
        {
            options["system_prompt"] = SystemPrompt;
        }

        if (EnableThinking != null && EnableThinking != false)
        {
            options["enable_thinking"] = EnableThinking.Value ? "true" : "false";
        }

        if (Prompt != null)
        {
            options["prompt"] = Prompt;
        }

        if (MaxTokens != null && MaxTokens != 256)
        {
            options["max_tokens"] = MaxTokens.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (MaxAudioTokens != null && MaxAudioTokens != 1000)
        {
            options["max_audio_tokens"] = MaxAudioTokens.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (VoiceId != null)
        {
            options["voice_id"] = VoiceId switch
            {
            Ming3AudioVoiceId.USJames => "US_James",
            Ming3AudioVoiceId.USStella => "US_Stella",
            Ming3AudioVoiceId.灵小柔 => "灵小柔",
            Ming3AudioVoiceId.灵楷 => "灵楷",
            Ming3AudioVoiceId.USEmma => "US_Emma",
            Ming3AudioVoiceId.UKGeorge => "UK_George",
            Ming3AudioVoiceId.UKCharlotte => "UK_Charlotte",
            Ming3AudioVoiceId.灵小安 => "灵小安",
            Ming3AudioVoiceId.灵语汐 => "灵语汐",
            Ming3AudioVoiceId.灵小萌 => "灵小萌",
            Ming3AudioVoiceId.灵小雅 => "灵小雅",
            Ming3AudioVoiceId.花小呗 => "花小呗",
            Ming3AudioVoiceId.灵韵 => "灵韵",
            Ming3AudioVoiceId.妩媚妲己 => "妩媚妲己",
            Ming3AudioVoiceId.灵婉 => "灵婉",
            Ming3AudioVoiceId.灵小月 => "灵小月",
            Ming3AudioVoiceId.灵朵儿 => "灵朵儿",
            Ming3AudioVoiceId.灵岩 => "灵岩",
            Ming3AudioVoiceId.灵小锋 => "灵小锋",
            Ming3AudioVoiceId.灵小川 => "灵小川",
            Ming3AudioVoiceId.灵小甄 => "灵小甄",
            Ming3AudioVoiceId.灵小知 => "灵小知",
            Ming3AudioVoiceId.灵小惠 => "灵小惠",
            Ming3AudioVoiceId.灵小晴 => "灵小晴",
            Ming3AudioVoiceId.灵小琪 => "灵小琪",
            Ming3AudioVoiceId.灵梦 => "灵梦",
            Ming3AudioVoiceId.灵恬 => "灵恬",
            Ming3AudioVoiceId.灵小汐 => "灵小汐",
            Ming3AudioVoiceId.灵小棠 => "灵小棠",
            Ming3AudioVoiceId.灵小梨 => "灵小梨",
            Ming3AudioVoiceId.灵舒 => "灵舒",
            Ming3AudioVoiceId.灵娇 => "灵娇",
            Ming3AudioVoiceId.灵小芸 => "灵小芸",
            Ming3AudioVoiceId.灵锐 => "灵锐",
            Ming3AudioVoiceId.灵空 => "灵空",
            Ming3AudioVoiceId.灵宇 => "灵宇",
            Ming3AudioVoiceId.灵朗 => "灵朗",
            Ming3AudioVoiceId.灵旭 => "灵旭",
            Ming3AudioVoiceId.灵骁 => "灵骁",
            Ming3AudioVoiceId.灵江 => "灵江",
            Ming3AudioVoiceId.悲伤女声 => "悲伤女声",
            Ming3AudioVoiceId.皮六 => "皮六",
            Ming3AudioVoiceId.说唱小哥 => "说唱小哥",
            Ming3AudioVoiceId.蜡笔小新 => "蜡笔小新",
            Ming3AudioVoiceId.悟空 => "悟空",
            Ming3AudioVoiceId.灵信远生气 => "灵信远_生气",
            Ming3AudioVoiceId.灵信远厌恶 => "灵信远_厌恶",
            Ming3AudioVoiceId.灵信远惊讶 => "灵信远_惊讶",
            Ming3AudioVoiceId.灵信远高兴 => "灵信远_高兴",
            Ming3AudioVoiceId.灵信远恐惧 => "灵信远_恐惧",
            Ming3AudioVoiceId.灵信远中性 => "灵信远_中性",
            Ming3AudioVoiceId.灵信远悲伤 => "灵信远_悲伤",
            Ming3AudioVoiceId.灵思齐高兴 => "灵思齐_高兴",
            Ming3AudioVoiceId.灵思齐恐惧 => "灵思齐_恐惧",
            Ming3AudioVoiceId.灵思齐生气 => "灵思齐_生气",
            Ming3AudioVoiceId.灵思齐厌恶 => "灵思齐_厌恶",
            Ming3AudioVoiceId.灵思齐中性 => "灵思齐_中性",
            Ming3AudioVoiceId.灵思齐惊讶 => "灵思齐_惊讶",
            Ming3AudioVoiceId.灵思齐悲伤 => "灵思齐_悲伤",
            Ming3AudioVoiceId.灵叙柔高兴 => "灵叙柔_高兴",
            Ming3AudioVoiceId.灵叙柔恐惧 => "灵叙柔_恐惧",
            Ming3AudioVoiceId.灵叙柔惊讶 => "灵叙柔_惊讶",
            Ming3AudioVoiceId.灵叙柔悲伤 => "灵叙柔_悲伤",
            Ming3AudioVoiceId.灵叙柔生气 => "灵叙柔_生气",
            Ming3AudioVoiceId.灵叙柔中性 => "灵叙柔_中性",
            Ming3AudioVoiceId.灵叙柔厌恶 => "灵叙柔_厌恶",
            Ming3AudioVoiceId.灵绮木悲伤 => "灵绮木_悲伤",
            Ming3AudioVoiceId.灵绮木厌恶 => "灵绮木_厌恶",
            Ming3AudioVoiceId.灵绮木中性 => "灵绮木_中性",
            Ming3AudioVoiceId.灵绮木恐惧 => "灵绮木_恐惧",
            Ming3AudioVoiceId.灵绮木惊讶 => "灵绮木_惊讶",
            Ming3AudioVoiceId.灵绮木高兴 => "灵绮木_高兴",
            Ming3AudioVoiceId.灵绮木生气 => "灵绮木_生气",
            Ming3AudioVoiceId.灵书悦恐惧 => "灵书悦_恐惧",
            Ming3AudioVoiceId.灵书悦厌恶 => "灵书悦_厌恶",
            Ming3AudioVoiceId.灵书悦高兴 => "灵书悦_高兴",
            Ming3AudioVoiceId.灵书悦惊讶 => "灵书悦_惊讶",
            Ming3AudioVoiceId.灵书悦悲伤 => "灵书悦_悲伤",
            Ming3AudioVoiceId.灵书悦生气 => "灵书悦_生气",
            Ming3AudioVoiceId.灵书悦中性 => "灵书悦_中性",
            Ming3AudioVoiceId.灵渊霆厌恶 => "灵渊霆_厌恶",
            Ming3AudioVoiceId.灵渊霆中性 => "灵渊霆_中性",
            Ming3AudioVoiceId.灵渊霆恐惧 => "灵渊霆_恐惧",
            Ming3AudioVoiceId.灵渊霆惊讶 => "灵渊霆_惊讶",
            Ming3AudioVoiceId.灵渊霆悲伤 => "灵渊霆_悲伤",
            Ming3AudioVoiceId.灵渊霆生气 => "灵渊霆_生气",
            Ming3AudioVoiceId.灵渊霆高兴 => "灵渊霆_高兴",
            Ming3AudioVoiceId.灵素心高兴 => "灵素心_高兴",
            Ming3AudioVoiceId.灵素心惊讶 => "灵素心_惊讶",
            Ming3AudioVoiceId.灵素心恐惧 => "灵素心_恐惧",
            Ming3AudioVoiceId.灵素心中性 => "灵素心_中性",
            Ming3AudioVoiceId.灵素心生气 => "灵素心_生气",
            Ming3AudioVoiceId.灵素心悲伤 => "灵素心_悲伤",
            Ming3AudioVoiceId.灵素心厌恶 => "灵素心_厌恶",
            Ming3AudioVoiceId.灵晓声惊讶 => "灵晓声_惊讶",
            Ming3AudioVoiceId.灵晓声厌恶 => "灵晓声_厌恶",
            Ming3AudioVoiceId.灵晓声高兴 => "灵晓声_高兴",
            Ming3AudioVoiceId.灵晓声生气 => "灵晓声_生气",
            Ming3AudioVoiceId.灵晓声中性 => "灵晓声_中性",
            Ming3AudioVoiceId.灵晓声悲伤 => "灵晓声_悲伤",
            Ming3AudioVoiceId.灵风信生气 => "灵风信_生气",
            Ming3AudioVoiceId.灵风信中性 => "灵风信_中性",
            Ming3AudioVoiceId.灵风信厌恶 => "灵风信_厌恶",
            Ming3AudioVoiceId.灵风信高兴 => "灵风信_高兴",
            Ming3AudioVoiceId.灵风信惊讶 => "灵风信_惊讶",
            Ming3AudioVoiceId.灵风信悲伤 => "灵风信_悲伤",
            Ming3AudioVoiceId.灵风信恐惧 => "灵风信_恐惧",
            Ming3AudioVoiceId.灵采薇惊讶 => "灵采薇_惊讶",
            Ming3AudioVoiceId.灵采薇厌恶 => "灵采薇_厌恶",
            Ming3AudioVoiceId.灵采薇恐惧 => "灵采薇_恐惧",
            Ming3AudioVoiceId.灵采薇生气 => "灵采薇_生气",
            Ming3AudioVoiceId.灵采薇悲伤 => "灵采薇_悲伤",
            Ming3AudioVoiceId.灵采薇中性 => "灵采薇_中性",
            Ming3AudioVoiceId.灵采薇高兴 => "灵采薇_高兴",
            Ming3AudioVoiceId.灵卓然厌恶 => "灵卓然_厌恶",
            Ming3AudioVoiceId.灵卓然中性 => "灵卓然_中性",
            Ming3AudioVoiceId.灵卓然恐惧 => "灵卓然_恐惧",
            Ming3AudioVoiceId.灵卓然惊讶 => "灵卓然_惊讶",
            Ming3AudioVoiceId.灵卓然高兴 => "灵卓然_高兴",
            Ming3AudioVoiceId.灵卓然悲伤 => "灵卓然_悲伤",
            Ming3AudioVoiceId.灵卓然生气 => "灵卓然_生气",
            Ming3AudioVoiceId.灵惜梦中性 => "灵惜梦_中性",
            Ming3AudioVoiceId.灵惜梦厌恶 => "灵惜梦_厌恶",
            Ming3AudioVoiceId.灵惜梦高兴 => "灵惜梦_高兴",
            Ming3AudioVoiceId.灵惜梦悲伤 => "灵惜梦_悲伤",
            Ming3AudioVoiceId.灵惜梦恐惧 => "灵惜梦_恐惧",
            Ming3AudioVoiceId.灵惜梦惊讶 => "灵惜梦_惊讶",
            Ming3AudioVoiceId.灵惜梦生气 => "灵惜梦_生气",
            Ming3AudioVoiceId.灵浅忧高兴 => "灵浅忧_高兴",
            Ming3AudioVoiceId.灵浅忧生气 => "灵浅忧_生气",
            Ming3AudioVoiceId.灵浅忧厌恶 => "灵浅忧_厌恶",
            Ming3AudioVoiceId.灵浅忧悲伤 => "灵浅忧_悲伤",
            Ming3AudioVoiceId.灵浅忧惊讶 => "灵浅忧_惊讶",
            Ming3AudioVoiceId.灵浅忧恐惧 => "灵浅忧_恐惧",
            Ming3AudioVoiceId.灵浅忧中性 => "灵浅忧_中性",
            Ming3AudioVoiceId.灵昭野生气 => "灵昭野_生气",
            Ming3AudioVoiceId.灵昭野高兴 => "灵昭野_高兴",
            Ming3AudioVoiceId.灵昭野悲伤 => "灵昭野_悲伤",
            Ming3AudioVoiceId.灵昭野中性 => "灵昭野_中性",
            Ming3AudioVoiceId.灵昭野厌恶 => "灵昭野_厌恶",
            Ming3AudioVoiceId.灵昭野恐惧 => "灵昭野_恐惧",
            Ming3AudioVoiceId.灵昭野惊讶 => "灵昭野_惊讶",
            Ming3AudioVoiceId.灵心澜厌恶 => "灵心澜_厌恶",
            Ming3AudioVoiceId.灵心澜惊讶 => "灵心澜_惊讶",
            Ming3AudioVoiceId.灵心澜生气 => "灵心澜_生气",
            Ming3AudioVoiceId.灵心澜中性 => "灵心澜_中性",
            Ming3AudioVoiceId.灵心澜高兴 => "灵心澜_高兴",
            Ming3AudioVoiceId.灵心澜悲伤 => "灵心澜_悲伤",
            Ming3AudioVoiceId.灵心澜恐惧 => "灵心澜_恐惧",
            Ming3AudioVoiceId.灵犀言悲伤 => "灵犀言_悲伤",
            Ming3AudioVoiceId.灵犀言厌恶 => "灵犀言_厌恶",
            Ming3AudioVoiceId.灵犀言恐惧 => "灵犀言_恐惧",
            Ming3AudioVoiceId.灵犀言中性 => "灵犀言_中性",
            Ming3AudioVoiceId.灵犀言惊讶 => "灵犀言_惊讶",
            Ming3AudioVoiceId.灵犀言高兴 => "灵犀言_高兴",
            Ming3AudioVoiceId.灵犀言生气 => "灵犀言_生气",
            Ming3AudioVoiceId.灵语冰悲伤 => "灵语冰_悲伤",
            Ming3AudioVoiceId.灵语冰厌恶 => "灵语冰_厌恶",
            Ming3AudioVoiceId.灵语冰高兴 => "灵语冰_高兴",
            Ming3AudioVoiceId.灵语冰恐惧 => "灵语冰_恐惧",
            Ming3AudioVoiceId.灵语冰惊讶 => "灵语冰_惊讶",
            Ming3AudioVoiceId.灵语冰中性 => "灵语冰_中性",
            Ming3AudioVoiceId.灵语冰生气 => "灵语冰_生气",
            Ming3AudioVoiceId.灵逸风生气 => "灵逸风_生气",
            Ming3AudioVoiceId.灵逸风悲伤 => "灵逸风_悲伤",
            Ming3AudioVoiceId.灵逸风惊讶 => "灵逸风_惊讶",
            Ming3AudioVoiceId.灵逸风中性 => "灵逸风_中性",
            Ming3AudioVoiceId.灵逸风恐惧 => "灵逸风_恐惧",
            Ming3AudioVoiceId.灵逸风高兴 => "灵逸风_高兴",
            Ming3AudioVoiceId.灵逸风厌恶 => "灵逸风_厌恶",
            Ming3AudioVoiceId.灵悦澜中性 => "灵悦澜_中性",
            Ming3AudioVoiceId.灵悦澜高兴 => "灵悦澜_高兴",
            Ming3AudioVoiceId.灵悦澜厌恶 => "灵悦澜_厌恶",
            Ming3AudioVoiceId.灵悦澜恐惧 => "灵悦澜_恐惧",
            Ming3AudioVoiceId.灵悦澜生气 => "灵悦澜_生气",
            Ming3AudioVoiceId.灵悦澜悲伤 => "灵悦澜_悲伤",
            Ming3AudioVoiceId.灵悦澜惊讶 => "灵悦澜_惊讶",
            Ming3AudioVoiceId.灵惊鸿厌恶 => "灵惊鸿_厌恶",
            Ming3AudioVoiceId.灵惊鸿高兴 => "灵惊鸿_高兴",
            Ming3AudioVoiceId.灵惊鸿中性 => "灵惊鸿_中性",
            Ming3AudioVoiceId.灵惊鸿惊讶 => "灵惊鸿_惊讶",
            Ming3AudioVoiceId.灵惊鸿恐惧 => "灵惊鸿_恐惧",
            Ming3AudioVoiceId.灵惊鸿生气 => "灵惊鸿_生气",
            Ming3AudioVoiceId.灵惊鸿悲伤 => "灵惊鸿_悲伤",
            Ming3AudioVoiceId.灵御心高兴 => "灵御心_高兴",
            Ming3AudioVoiceId.灵御心中性 => "灵御心_中性",
            Ming3AudioVoiceId.灵御心生气 => "灵御心_生气",
            Ming3AudioVoiceId.灵御心恐惧 => "灵御心_恐惧",
            Ming3AudioVoiceId.灵御心厌恶 => "灵御心_厌恶",
            Ming3AudioVoiceId.灵御心惊讶 => "灵御心_惊讶",
            Ming3AudioVoiceId.灵御心悲伤 => "灵御心_悲伤",
            Ming3AudioVoiceId.灵语恬厌恶 => "灵语恬_厌恶",
            Ming3AudioVoiceId.灵语恬高兴 => "灵语恬_高兴",
            Ming3AudioVoiceId.灵语恬恐惧 => "灵语恬_恐惧",
            Ming3AudioVoiceId.灵语恬悲伤 => "灵语恬_悲伤",
            Ming3AudioVoiceId.灵语恬惊讶 => "灵语恬_惊讶",
            Ming3AudioVoiceId.灵语恬生气 => "灵语恬_生气",
            Ming3AudioVoiceId.灵语恬中性 => "灵语恬_中性",
            Ming3AudioVoiceId.灵心恬厌恶 => "灵心恬_厌恶",
            Ming3AudioVoiceId.灵心恬惊讶 => "灵心恬_惊讶",
            Ming3AudioVoiceId.灵心恬悲伤 => "灵心恬_悲伤",
            Ming3AudioVoiceId.灵心恬中性 => "灵心恬_中性",
            Ming3AudioVoiceId.灵心恬恐惧 => "灵心恬_恐惧",
            Ming3AudioVoiceId.灵心恬高兴 => "灵心恬_高兴",
            Ming3AudioVoiceId.灵心恬生气 => "灵心恬_生气",
            Ming3AudioVoiceId.灵惜蕊惊讶 => "灵惜蕊_惊讶",
            Ming3AudioVoiceId.灵惜蕊厌恶 => "灵惜蕊_厌恶",
            Ming3AudioVoiceId.灵惜蕊恐惧 => "灵惜蕊_恐惧",
            Ming3AudioVoiceId.灵惜蕊高兴 => "灵惜蕊_高兴",
            Ming3AudioVoiceId.灵惜蕊中性 => "灵惜蕊_中性",
            Ming3AudioVoiceId.灵惜蕊悲伤 => "灵惜蕊_悲伤",
            Ming3AudioVoiceId.灵惜蕊生气 => "灵惜蕊_生气",
            Ming3AudioVoiceId.灵语萱厌恶 => "灵语萱_厌恶",
            Ming3AudioVoiceId.灵语萱高兴 => "灵语萱_高兴",
            Ming3AudioVoiceId.灵语萱惊讶 => "灵语萱_惊讶",
            Ming3AudioVoiceId.灵语萱恐惧 => "灵语萱_恐惧",
            Ming3AudioVoiceId.灵语萱生气 => "灵语萱_生气",
            Ming3AudioVoiceId.灵语萱悲伤 => "灵语萱_悲伤",
            Ming3AudioVoiceId.灵语萱中性 => "灵语萱_中性",
            Ming3AudioVoiceId.灵语烟生气 => "灵语烟_生气",
            Ming3AudioVoiceId.灵语烟恐惧 => "灵语烟_恐惧",
            Ming3AudioVoiceId.灵语烟中性 => "灵语烟_中性",
            Ming3AudioVoiceId.灵语烟厌恶 => "灵语烟_厌恶",
            Ming3AudioVoiceId.灵语烟惊讶 => "灵语烟_惊讶",
            Ming3AudioVoiceId.灵语烟高兴 => "灵语烟_高兴",
            Ming3AudioVoiceId.灵语烟悲伤 => "灵语烟_悲伤",
            Ming3AudioVoiceId.灵律雅恐惧 => "灵律雅_恐惧",
            Ming3AudioVoiceId.灵律雅生气 => "灵律雅_生气",
            Ming3AudioVoiceId.灵律雅惊讶 => "灵律雅_惊讶",
            Ming3AudioVoiceId.灵律雅高兴 => "灵律雅_高兴",
            Ming3AudioVoiceId.灵律雅厌恶 => "灵律雅_厌恶",
            Ming3AudioVoiceId.灵律雅中性 => "灵律雅_中性",
            Ming3AudioVoiceId.灵律雅悲伤 => "灵律雅_悲伤",
            Ming3AudioVoiceId.灵惊涛厌恶 => "灵惊涛_厌恶",
            Ming3AudioVoiceId.灵惊涛恐惧 => "灵惊涛_恐惧",
            Ming3AudioVoiceId.灵惊涛高兴 => "灵惊涛_高兴",
            Ming3AudioVoiceId.灵惊涛悲伤 => "灵惊涛_悲伤",
            Ming3AudioVoiceId.灵惊涛中性 => "灵惊涛_中性",
            Ming3AudioVoiceId.灵惊涛惊讶 => "灵惊涛_惊讶",
            Ming3AudioVoiceId.灵惊涛生气 => "灵惊涛_生气",
            Ming3AudioVoiceId.灵寒陌生气 => "灵寒陌_生气",
            Ming3AudioVoiceId.灵寒陌厌恶 => "灵寒陌_厌恶",
            Ming3AudioVoiceId.灵寒陌恐惧 => "灵寒陌_恐惧",
            Ming3AudioVoiceId.灵寒陌高兴 => "灵寒陌_高兴",
            Ming3AudioVoiceId.灵寒陌中性 => "灵寒陌_中性",
            Ming3AudioVoiceId.灵寒陌惊讶 => "灵寒陌_惊讶",
            Ming3AudioVoiceId.灵寒陌悲伤 => "灵寒陌_悲伤",
            Ming3AudioVoiceId.灵决冥悲伤 => "灵决冥_悲伤",
            Ming3AudioVoiceId.灵决冥厌恶 => "灵决冥_厌恶",
            Ming3AudioVoiceId.灵决冥生气 => "灵决冥_生气",
            Ming3AudioVoiceId.灵决冥中性 => "灵决冥_中性",
            Ming3AudioVoiceId.灵决冥惊讶 => "灵决冥_惊讶",
            Ming3AudioVoiceId.灵决冥恐惧 => "灵决冥_恐惧",
            Ming3AudioVoiceId.灵决冥高兴 => "灵决冥_高兴",
            Ming3AudioVoiceId.灵亦尘中性 => "灵亦尘_中性",
            Ming3AudioVoiceId.灵亦尘厌恶 => "灵亦尘_厌恶",
            Ming3AudioVoiceId.灵亦尘高兴 => "灵亦尘_高兴",
            Ming3AudioVoiceId.灵亦尘恐惧 => "灵亦尘_恐惧",
            Ming3AudioVoiceId.灵亦尘生气 => "灵亦尘_生气",
            Ming3AudioVoiceId.灵亦尘悲伤 => "灵亦尘_悲伤",
            Ming3AudioVoiceId.灵亦尘惊讶 => "灵亦尘_惊讶",
            Ming3AudioVoiceId.灵语尘生气 => "灵语尘_生气",
            Ming3AudioVoiceId.灵语尘高兴 => "灵语尘_高兴",
            Ming3AudioVoiceId.灵语尘厌恶 => "灵语尘_厌恶",
            Ming3AudioVoiceId.灵语尘惊讶 => "灵语尘_惊讶",
            Ming3AudioVoiceId.灵语尘中性 => "灵语尘_中性",
            Ming3AudioVoiceId.灵语尘悲伤 => "灵语尘_悲伤",
            Ming3AudioVoiceId.灵语尘恐惧 => "灵语尘_恐惧",
            Ming3AudioVoiceId.灵慈萱高兴 => "灵慈萱_高兴",
            Ming3AudioVoiceId.灵慈萱厌恶 => "灵慈萱_厌恶",
            Ming3AudioVoiceId.灵慈萱惊讶 => "灵慈萱_惊讶",
            Ming3AudioVoiceId.灵慈萱生气 => "灵慈萱_生气",
            Ming3AudioVoiceId.灵慈萱中性 => "灵慈萱_中性",
            Ming3AudioVoiceId.灵慈萱悲伤 => "灵慈萱_悲伤",
            Ming3AudioVoiceId.灵慈萱恐惧 => "灵慈萱_恐惧",
            Ming3AudioVoiceId.灵妙音中性 => "灵妙音_中性",
            Ming3AudioVoiceId.灵妙音悲伤 => "灵妙音_悲伤",
            Ming3AudioVoiceId.灵妙音惊讶 => "灵妙音_惊讶",
            Ming3AudioVoiceId.灵妙音厌恶 => "灵妙音_厌恶",
            Ming3AudioVoiceId.灵妙音生气 => "灵妙音_生气",
            Ming3AudioVoiceId.灵妙音高兴 => "灵妙音_高兴",
            Ming3AudioVoiceId.灵妙音恐惧 => "灵妙音_恐惧",
            Ming3AudioVoiceId.灵断尘悲伤 => "灵断尘_悲伤",
            Ming3AudioVoiceId.灵断尘高兴 => "灵断尘_高兴",
            Ming3AudioVoiceId.灵断尘惊讶 => "灵断尘_惊讶",
            Ming3AudioVoiceId.灵断尘恐惧 => "灵断尘_恐惧",
            Ming3AudioVoiceId.灵断尘中性 => "灵断尘_中性",
            Ming3AudioVoiceId.灵断尘生气 => "灵断尘_生气",
            Ming3AudioVoiceId.灵断尘厌恶 => "灵断尘_厌恶",
            Ming3AudioVoiceId.灵悦兮高兴 => "灵悦兮_高兴",
            Ming3AudioVoiceId.灵悦兮中性 => "灵悦兮_中性",
            Ming3AudioVoiceId.灵悦兮生气 => "灵悦兮_生气",
            Ming3AudioVoiceId.灵悦兮惊讶 => "灵悦兮_惊讶",
            Ming3AudioVoiceId.灵悦兮厌恶 => "灵悦兮_厌恶",
            Ming3AudioVoiceId.灵悦兮悲伤 => "灵悦兮_悲伤",
            Ming3AudioVoiceId.灵悦兮恐惧 => "灵悦兮_恐惧",
            Ming3AudioVoiceId.灵若虚恐惧 => "灵若虚_恐惧",
            Ming3AudioVoiceId.灵若虚惊讶 => "灵若虚_惊讶",
            Ming3AudioVoiceId.灵若虚中性 => "灵若虚_中性",
            Ming3AudioVoiceId.灵若虚悲伤 => "灵若虚_悲伤",
            Ming3AudioVoiceId.灵若虚高兴 => "灵若虚_高兴",
            Ming3AudioVoiceId.灵若虚厌恶 => "灵若虚_厌恶",
            Ming3AudioVoiceId.灵若虚生气 => "灵若虚_生气",
            Ming3AudioVoiceId.灵熙婉厌恶 => "灵熙婉_厌恶",
            Ming3AudioVoiceId.灵熙婉生气 => "灵熙婉_生气",
            Ming3AudioVoiceId.灵熙婉惊讶 => "灵熙婉_惊讶",
            Ming3AudioVoiceId.灵熙婉高兴 => "灵熙婉_高兴",
            Ming3AudioVoiceId.灵熙婉中性 => "灵熙婉_中性",
            Ming3AudioVoiceId.灵熙婉恐惧 => "灵熙婉_恐惧",
            Ming3AudioVoiceId.灵熙婉悲伤 => "灵熙婉_悲伤",
            Ming3AudioVoiceId.灵旭扬中性 => "灵旭扬_中性",
            Ming3AudioVoiceId.灵旭扬生气 => "灵旭扬_生气",
            Ming3AudioVoiceId.灵旭扬恐惧 => "灵旭扬_恐惧",
            Ming3AudioVoiceId.灵旭扬高兴 => "灵旭扬_高兴",
            Ming3AudioVoiceId.灵旭扬惊讶 => "灵旭扬_惊讶",
            Ming3AudioVoiceId.灵旭扬悲伤 => "灵旭扬_悲伤",
            Ming3AudioVoiceId.灵旭扬厌恶 => "灵旭扬_厌恶",
            Ming3AudioVoiceId.灵阅川生气 => "灵阅川_生气",
            Ming3AudioVoiceId.灵阅川悲伤 => "灵阅川_悲伤",
            Ming3AudioVoiceId.灵阅川高兴 => "灵阅川_高兴",
            Ming3AudioVoiceId.灵阅川厌恶 => "灵阅川_厌恶",
            Ming3AudioVoiceId.灵阅川惊讶 => "灵阅川_惊讶",
            Ming3AudioVoiceId.灵阅川中性 => "灵阅川_中性",
            Ming3AudioVoiceId.灵阅川恐惧 => "灵阅川_恐惧",
            Ming3AudioVoiceId.灵未央高兴 => "灵未央_高兴",
            Ming3AudioVoiceId.灵未央中性 => "灵未央_中性",
            Ming3AudioVoiceId.灵未央惊讶 => "灵未央_惊讶",
            Ming3AudioVoiceId.灵未央悲伤 => "灵未央_悲伤",
            Ming3AudioVoiceId.灵未央厌恶 => "灵未央_厌恶",
            Ming3AudioVoiceId.灵未央恐惧 => "灵未央_恐惧",
            Ming3AudioVoiceId.灵未央生气 => "灵未央_生气",
            Ming3AudioVoiceId.灵妩月厌恶 => "灵妩月_厌恶",
            Ming3AudioVoiceId.灵妩月生气 => "灵妩月_生气",
            Ming3AudioVoiceId.灵妩月中性 => "灵妩月_中性",
            Ming3AudioVoiceId.灵妩月惊讶 => "灵妩月_惊讶",
            Ming3AudioVoiceId.灵妩月恐惧 => "灵妩月_恐惧",
            Ming3AudioVoiceId.灵妩月高兴 => "灵妩月_高兴",
            Ming3AudioVoiceId.灵妩月悲伤 => "灵妩月_悲伤",
            Ming3AudioVoiceId.灵梦瑶中性 => "灵梦瑶_中性",
            Ming3AudioVoiceId.灵梦瑶恐惧 => "灵梦瑶_恐惧",
            Ming3AudioVoiceId.灵梦瑶高兴 => "灵梦瑶_高兴",
            Ming3AudioVoiceId.灵梦瑶悲伤 => "灵梦瑶_悲伤",
            Ming3AudioVoiceId.灵梦瑶惊讶 => "灵梦瑶_惊讶",
            Ming3AudioVoiceId.灵梦瑶生气 => "灵梦瑶_生气",
            Ming3AudioVoiceId.灵妙声中性 => "灵妙声_中性",
            Ming3AudioVoiceId.灵妙声恐惧 => "灵妙声_恐惧",
            Ming3AudioVoiceId.灵妙声高兴 => "灵妙声_高兴",
            Ming3AudioVoiceId.灵妙声悲伤 => "灵妙声_悲伤",
            Ming3AudioVoiceId.灵妙声惊讶 => "灵妙声_惊讶",
            Ming3AudioVoiceId.灵妙声生气 => "灵妙声_生气",
            Ming3AudioVoiceId.灵烈瑶中性 => "灵烈瑶_中性",
            Ming3AudioVoiceId.灵烈瑶厌恶 => "灵烈瑶_厌恶",
            Ming3AudioVoiceId.灵烈瑶恐惧 => "灵烈瑶_恐惧",
            Ming3AudioVoiceId.灵烈瑶悲伤 => "灵烈瑶_悲伤",
            Ming3AudioVoiceId.灵烈瑶惊讶 => "灵烈瑶_惊讶",
            Ming3AudioVoiceId.灵烈瑶生气 => "灵烈瑶_生气",
            Ming3AudioVoiceId.灵烈瑶高兴 => "灵烈瑶_高兴",
            Ming3AudioVoiceId.灵幽月中性 => "灵幽月_中性",
            Ming3AudioVoiceId.灵幽月厌恶 => "灵幽月_厌恶",
            Ming3AudioVoiceId.灵幽月恐惧 => "灵幽月_恐惧",
            Ming3AudioVoiceId.灵幽月悲伤 => "灵幽月_悲伤",
            Ming3AudioVoiceId.灵幽月惊讶 => "灵幽月_惊讶",
            Ming3AudioVoiceId.灵幽月生气 => "灵幽月_生气",
            Ming3AudioVoiceId.灵幽月高兴 => "灵幽月_高兴",
            Ming3AudioVoiceId.灵悦心中性 => "灵悦心_中性",
            Ming3AudioVoiceId.灵悦心厌恶 => "灵悦心_厌恶",
            Ming3AudioVoiceId.灵悦心恐惧 => "灵悦心_恐惧",
            Ming3AudioVoiceId.灵悦心悲伤 => "灵悦心_悲伤",
            Ming3AudioVoiceId.灵悦心惊讶 => "灵悦心_惊讶",
            Ming3AudioVoiceId.灵悦心生气 => "灵悦心_生气",
            Ming3AudioVoiceId.灵悦心高兴 => "灵悦心_高兴",
            Ming3AudioVoiceId.灵汐月中性 => "灵汐月_中性",
            Ming3AudioVoiceId.灵汐月厌恶 => "灵汐月_厌恶",
            Ming3AudioVoiceId.灵汐月恐惧 => "灵汐月_恐惧",
            Ming3AudioVoiceId.灵汐月悲伤 => "灵汐月_悲伤",
            Ming3AudioVoiceId.灵汐月惊讶 => "灵汐月_惊讶",
            Ming3AudioVoiceId.灵汐月生气 => "灵汐月_生气",
            Ming3AudioVoiceId.灵汐月高兴 => "灵汐月_高兴",
            Ming3AudioVoiceId.灵书意中性 => "灵书意_中性",
            Ming3AudioVoiceId.灵书意厌恶 => "灵书意_厌恶",
            Ming3AudioVoiceId.灵书意恐惧 => "灵书意_恐惧",
            Ming3AudioVoiceId.灵书意悲伤 => "灵书意_悲伤",
            Ming3AudioVoiceId.灵书意惊讶 => "灵书意_惊讶",
            Ming3AudioVoiceId.灵书意生气 => "灵书意_生气",
            Ming3AudioVoiceId.灵书意高兴 => "灵书意_高兴",
            Ming3AudioVoiceId.灵知远中性 => "灵知远_中性",
            Ming3AudioVoiceId.灵知远厌恶 => "灵知远_厌恶",
            Ming3AudioVoiceId.灵知远恐惧 => "灵知远_恐惧",
            Ming3AudioVoiceId.灵知远悲伤 => "灵知远_悲伤",
            Ming3AudioVoiceId.灵知远惊讶 => "灵知远_惊讶",
            Ming3AudioVoiceId.灵知远生气 => "灵知远_生气",
            Ming3AudioVoiceId.灵知远高兴 => "灵知远_高兴",
            Ming3AudioVoiceId.灵温言中性 => "灵温言_中性",
            Ming3AudioVoiceId.灵温言厌恶 => "灵温言_厌恶",
            Ming3AudioVoiceId.灵温言恐惧 => "灵温言_恐惧",
            Ming3AudioVoiceId.灵温言悲伤 => "灵温言_悲伤",
            Ming3AudioVoiceId.灵温言惊讶 => "灵温言_惊讶",
            Ming3AudioVoiceId.灵温言生气 => "灵温言_生气",
            Ming3AudioVoiceId.灵温言高兴 => "灵温言_高兴",
            Ming3AudioVoiceId.灵傲尘中性 => "灵傲尘_中性",
            Ming3AudioVoiceId.灵傲尘厌恶 => "灵傲尘_厌恶",
            Ming3AudioVoiceId.灵傲尘恐惧 => "灵傲尘_恐惧",
            Ming3AudioVoiceId.灵傲尘悲伤 => "灵傲尘_悲伤",
            Ming3AudioVoiceId.灵傲尘生气 => "灵傲尘_生气",
            Ming3AudioVoiceId.灵傲尘高兴 => "灵傲尘_高兴",
            Ming3AudioVoiceId.灵佑安中性 => "灵佑安_中性",
            Ming3AudioVoiceId.灵佑安厌恶 => "灵佑安_厌恶",
            Ming3AudioVoiceId.灵佑安恐惧 => "灵佑安_恐惧",
            Ming3AudioVoiceId.灵佑安悲伤 => "灵佑安_悲伤",
            Ming3AudioVoiceId.灵佑安惊讶 => "灵佑安_惊讶",
            Ming3AudioVoiceId.灵佑安生气 => "灵佑安_生气",
            Ming3AudioVoiceId.灵佑安高兴 => "灵佑安_高兴",
            Ming3AudioVoiceId.灵惜晨中性 => "灵惜晨_中性",
            Ming3AudioVoiceId.灵惜晨厌恶 => "灵惜晨_厌恶",
            Ming3AudioVoiceId.灵惜晨恐惧 => "灵惜晨_恐惧",
            Ming3AudioVoiceId.灵惜晨悲伤 => "灵惜晨_悲伤",
            Ming3AudioVoiceId.灵惜晨惊讶 => "灵惜晨_惊讶",
            Ming3AudioVoiceId.灵惜晨生气 => "灵惜晨_生气",
            Ming3AudioVoiceId.灵惜晨高兴 => "灵惜晨_高兴",
            Ming3AudioVoiceId.灵宛溪中性 => "灵宛溪_中性",
            Ming3AudioVoiceId.灵宛溪厌恶 => "灵宛溪_厌恶",
            Ming3AudioVoiceId.灵宛溪恐惧 => "灵宛溪_恐惧",
            Ming3AudioVoiceId.灵宛溪悲伤 => "灵宛溪_悲伤",
            Ming3AudioVoiceId.灵宛溪惊讶 => "灵宛溪_惊讶",
            Ming3AudioVoiceId.灵宛溪生气 => "灵宛溪_生气",
            Ming3AudioVoiceId.灵宛溪高兴 => "灵宛溪_高兴",
            Ming3AudioVoiceId.灵言澈中性 => "灵言澈_中性",
            Ming3AudioVoiceId.灵言澈厌恶 => "灵言澈_厌恶",
            Ming3AudioVoiceId.灵言澈恐惧 => "灵言澈_恐惧",
            Ming3AudioVoiceId.灵言澈悲伤 => "灵言澈_悲伤",
            Ming3AudioVoiceId.灵言澈惊讶 => "灵言澈_惊讶",
            Ming3AudioVoiceId.灵言澈生气 => "灵言澈_生气",
            Ming3AudioVoiceId.灵言澈高兴 => "灵言澈_高兴",
            Ming3AudioVoiceId.灵溯光中性 => "灵溯光_中性",
            Ming3AudioVoiceId.灵溯光厌恶 => "灵溯光_厌恶",
            Ming3AudioVoiceId.灵溯光恐惧 => "灵溯光_恐惧",
            Ming3AudioVoiceId.灵溯光悲伤 => "灵溯光_悲伤",
            Ming3AudioVoiceId.灵溯光惊讶 => "灵溯光_惊讶",
            Ming3AudioVoiceId.灵溯光生气 => "灵溯光_生气",
            Ming3AudioVoiceId.灵溯光高兴 => "灵溯光_高兴",
            Ming3AudioVoiceId.灵惊弦中性 => "灵惊弦_中性",
            Ming3AudioVoiceId.灵惊弦厌恶 => "灵惊弦_厌恶",
            Ming3AudioVoiceId.灵惊弦恐惧 => "灵惊弦_恐惧",
            Ming3AudioVoiceId.灵惊弦悲伤 => "灵惊弦_悲伤",
            Ming3AudioVoiceId.灵惊弦惊讶 => "灵惊弦_惊讶",
            Ming3AudioVoiceId.灵惊弦生气 => "灵惊弦_生气",
            Ming3AudioVoiceId.灵惊弦高兴 => "灵惊弦_高兴",
            Ming3AudioVoiceId.灵嘉肴中性 => "灵嘉肴_中性",
            Ming3AudioVoiceId.灵嘉肴厌恶 => "灵嘉肴_厌恶",
            Ming3AudioVoiceId.灵嘉肴恐惧 => "灵嘉肴_恐惧",
            Ming3AudioVoiceId.灵嘉肴悲伤 => "灵嘉肴_悲伤",
            Ming3AudioVoiceId.灵嘉肴惊讶 => "灵嘉肴_惊讶",
            Ming3AudioVoiceId.灵嘉肴生气 => "灵嘉肴_生气",
            Ming3AudioVoiceId.灵嘉肴高兴 => "灵嘉肴_高兴",
            Ming3AudioVoiceId.灵黛嗔中性 => "灵黛嗔_中性",
            Ming3AudioVoiceId.灵黛嗔厌恶 => "灵黛嗔_厌恶",
            Ming3AudioVoiceId.灵黛嗔恐惧 => "灵黛嗔_恐惧",
            Ming3AudioVoiceId.灵黛嗔悲伤 => "灵黛嗔_悲伤",
            Ming3AudioVoiceId.灵黛嗔惊讶 => "灵黛嗔_惊讶",
            Ming3AudioVoiceId.灵黛嗔生气 => "灵黛嗔_生气",
            Ming3AudioVoiceId.灵黛嗔高兴 => "灵黛嗔_高兴",
            Ming3AudioVoiceId.灵孤影中性 => "灵孤影_中性",
            Ming3AudioVoiceId.灵孤影厌恶 => "灵孤影_厌恶",
            Ming3AudioVoiceId.灵孤影恐惧 => "灵孤影_恐惧",
            Ming3AudioVoiceId.灵孤影悲伤 => "灵孤影_悲伤",
            Ming3AudioVoiceId.灵孤影惊讶 => "灵孤影_惊讶",
            Ming3AudioVoiceId.灵孤影生气 => "灵孤影_生气",
            Ming3AudioVoiceId.灵孤影高兴 => "灵孤影_高兴",
            Ming3AudioVoiceId.灵雨汐中性 => "灵雨汐_中性",
            Ming3AudioVoiceId.灵雨汐恐惧 => "灵雨汐_恐惧",
            Ming3AudioVoiceId.灵雨汐高兴 => "灵雨汐_高兴",
            Ming3AudioVoiceId.灵雨汐悲伤 => "灵雨汐_悲伤",
            Ming3AudioVoiceId.灵雨汐惊讶 => "灵雨汐_惊讶",
            Ming3AudioVoiceId.灵雨汐生气 => "灵雨汐_生气",
            Ming3AudioVoiceId.蜡笔小新野原新之助小新 => "蜡笔小新_野原新之助 (小新)",
            Ming3AudioVoiceId.蜡笔小新野原美伢美伢 => "蜡笔小新_野原美伢 (美伢)",
            Ming3AudioVoiceId.小猪佩奇佩奇 => "小猪佩奇_佩奇",
            Ming3AudioVoiceId.康熙王朝康熙 => "康熙王朝_康熙",
            Ming3AudioVoiceId.西游记唐僧 => "西游记_唐僧",
            Ming3AudioVoiceId.甄嬛传四郎 => "甄嬛传_四郎",
            Ming3AudioVoiceId.潜伏余则成 => "潜伏_余则成",
            Ming3AudioVoiceId.水浒传武松 => "水浒传_武松",
            Ming3AudioVoiceId.水浒传潘金莲 => "水浒传_潘金莲",
            Ming3AudioVoiceId.雍正王朝雍正 => "雍正王朝_雍正",
            _ => throw new ArgumentException("ming3_audio tts voice_id must be one of: US_James, US_Stella, 灵小柔, 灵楷, US_Emma, UK_George, UK_Charlotte, 灵小安, 灵语汐, 灵小萌, 灵小雅, 花小呗, 灵韵, 妩媚妲己, 灵婉, 灵小月, 灵朵儿, 灵岩, 灵小锋, 灵小川, 灵小甄, 灵小知, 灵小惠, 灵小晴, 灵小琪, 灵梦, 灵恬, 灵小汐, 灵小棠, 灵小梨, 灵舒, 灵娇, 灵小芸, 灵锐, 灵空, 灵宇, 灵朗, 灵旭, 灵骁, 灵江, 悲伤女声, 皮六, 说唱小哥, 蜡笔小新, 悟空, 灵信远_生气, 灵信远_厌恶, 灵信远_惊讶, 灵信远_高兴, 灵信远_恐惧, 灵信远_中性, 灵信远_悲伤, 灵思齐_高兴, 灵思齐_恐惧, 灵思齐_生气, 灵思齐_厌恶, 灵思齐_中性, 灵思齐_惊讶, 灵思齐_悲伤, 灵叙柔_高兴, 灵叙柔_恐惧, 灵叙柔_惊讶, 灵叙柔_悲伤, 灵叙柔_生气, 灵叙柔_中性, 灵叙柔_厌恶, 灵绮木_悲伤, 灵绮木_厌恶, 灵绮木_中性, 灵绮木_恐惧, 灵绮木_惊讶, 灵绮木_高兴, 灵绮木_生气, 灵书悦_恐惧, 灵书悦_厌恶, 灵书悦_高兴, 灵书悦_惊讶, 灵书悦_悲伤, 灵书悦_生气, 灵书悦_中性, 灵渊霆_厌恶, 灵渊霆_中性, 灵渊霆_恐惧, 灵渊霆_惊讶, 灵渊霆_悲伤, 灵渊霆_生气, 灵渊霆_高兴, 灵素心_高兴, 灵素心_惊讶, 灵素心_恐惧, 灵素心_中性, 灵素心_生气, 灵素心_悲伤, 灵素心_厌恶, 灵晓声_惊讶, 灵晓声_厌恶, 灵晓声_高兴, 灵晓声_生气, 灵晓声_中性, 灵晓声_悲伤, 灵风信_生气, 灵风信_中性, 灵风信_厌恶, 灵风信_高兴, 灵风信_惊讶, 灵风信_悲伤, 灵风信_恐惧, 灵采薇_惊讶, 灵采薇_厌恶, 灵采薇_恐惧, 灵采薇_生气, 灵采薇_悲伤, 灵采薇_中性, 灵采薇_高兴, 灵卓然_厌恶, 灵卓然_中性, 灵卓然_恐惧, 灵卓然_惊讶, 灵卓然_高兴, 灵卓然_悲伤, 灵卓然_生气, 灵惜梦_中性, 灵惜梦_厌恶, 灵惜梦_高兴, 灵惜梦_悲伤, 灵惜梦_恐惧, 灵惜梦_惊讶, 灵惜梦_生气, 灵浅忧_高兴, 灵浅忧_生气, 灵浅忧_厌恶, 灵浅忧_悲伤, 灵浅忧_惊讶, 灵浅忧_恐惧, 灵浅忧_中性, 灵昭野_生气, 灵昭野_高兴, 灵昭野_悲伤, 灵昭野_中性, 灵昭野_厌恶, 灵昭野_恐惧, 灵昭野_惊讶, 灵心澜_厌恶, 灵心澜_惊讶, 灵心澜_生气, 灵心澜_中性, 灵心澜_高兴, 灵心澜_悲伤, 灵心澜_恐惧, 灵犀言_悲伤, 灵犀言_厌恶, 灵犀言_恐惧, 灵犀言_中性, 灵犀言_惊讶, 灵犀言_高兴, 灵犀言_生气, 灵语冰_悲伤, 灵语冰_厌恶, 灵语冰_高兴, 灵语冰_恐惧, 灵语冰_惊讶, 灵语冰_中性, 灵语冰_生气, 灵逸风_生气, 灵逸风_悲伤, 灵逸风_惊讶, 灵逸风_中性, 灵逸风_恐惧, 灵逸风_高兴, 灵逸风_厌恶, 灵悦澜_中性, 灵悦澜_高兴, 灵悦澜_厌恶, 灵悦澜_恐惧, 灵悦澜_生气, 灵悦澜_悲伤, 灵悦澜_惊讶, 灵惊鸿_厌恶, 灵惊鸿_高兴, 灵惊鸿_中性, 灵惊鸿_惊讶, 灵惊鸿_恐惧, 灵惊鸿_生气, 灵惊鸿_悲伤, 灵御心_高兴, 灵御心_中性, 灵御心_生气, 灵御心_恐惧, 灵御心_厌恶, 灵御心_惊讶, 灵御心_悲伤, 灵语恬_厌恶, 灵语恬_高兴, 灵语恬_恐惧, 灵语恬_悲伤, 灵语恬_惊讶, 灵语恬_生气, 灵语恬_中性, 灵心恬_厌恶, 灵心恬_惊讶, 灵心恬_悲伤, 灵心恬_中性, 灵心恬_恐惧, 灵心恬_高兴, 灵心恬_生气, 灵惜蕊_惊讶, 灵惜蕊_厌恶, 灵惜蕊_恐惧, 灵惜蕊_高兴, 灵惜蕊_中性, 灵惜蕊_悲伤, 灵惜蕊_生气, 灵语萱_厌恶, 灵语萱_高兴, 灵语萱_惊讶, 灵语萱_恐惧, 灵语萱_生气, 灵语萱_悲伤, 灵语萱_中性, 灵语烟_生气, 灵语烟_恐惧, 灵语烟_中性, 灵语烟_厌恶, 灵语烟_惊讶, 灵语烟_高兴, 灵语烟_悲伤, 灵律雅_恐惧, 灵律雅_生气, 灵律雅_惊讶, 灵律雅_高兴, 灵律雅_厌恶, 灵律雅_中性, 灵律雅_悲伤, 灵惊涛_厌恶, 灵惊涛_恐惧, 灵惊涛_高兴, 灵惊涛_悲伤, 灵惊涛_中性, 灵惊涛_惊讶, 灵惊涛_生气, 灵寒陌_生气, 灵寒陌_厌恶, 灵寒陌_恐惧, 灵寒陌_高兴, 灵寒陌_中性, 灵寒陌_惊讶, 灵寒陌_悲伤, 灵决冥_悲伤, 灵决冥_厌恶, 灵决冥_生气, 灵决冥_中性, 灵决冥_惊讶, 灵决冥_恐惧, 灵决冥_高兴, 灵亦尘_中性, 灵亦尘_厌恶, 灵亦尘_高兴, 灵亦尘_恐惧, 灵亦尘_生气, 灵亦尘_悲伤, 灵亦尘_惊讶, 灵语尘_生气, 灵语尘_高兴, 灵语尘_厌恶, 灵语尘_惊讶, 灵语尘_中性, 灵语尘_悲伤, 灵语尘_恐惧, 灵慈萱_高兴, 灵慈萱_厌恶, 灵慈萱_惊讶, 灵慈萱_生气, 灵慈萱_中性, 灵慈萱_悲伤, 灵慈萱_恐惧, 灵妙音_中性, 灵妙音_悲伤, 灵妙音_惊讶, 灵妙音_厌恶, 灵妙音_生气, 灵妙音_高兴, 灵妙音_恐惧, 灵断尘_悲伤, 灵断尘_高兴, 灵断尘_惊讶, 灵断尘_恐惧, 灵断尘_中性, 灵断尘_生气, 灵断尘_厌恶, 灵悦兮_高兴, 灵悦兮_中性, 灵悦兮_生气, 灵悦兮_惊讶, 灵悦兮_厌恶, 灵悦兮_悲伤, 灵悦兮_恐惧, 灵若虚_恐惧, 灵若虚_惊讶, 灵若虚_中性, 灵若虚_悲伤, 灵若虚_高兴, 灵若虚_厌恶, 灵若虚_生气, 灵熙婉_厌恶, 灵熙婉_生气, 灵熙婉_惊讶, 灵熙婉_高兴, 灵熙婉_中性, 灵熙婉_恐惧, 灵熙婉_悲伤, 灵旭扬_中性, 灵旭扬_生气, 灵旭扬_恐惧, 灵旭扬_高兴, 灵旭扬_惊讶, 灵旭扬_悲伤, 灵旭扬_厌恶, 灵阅川_生气, 灵阅川_悲伤, 灵阅川_高兴, 灵阅川_厌恶, 灵阅川_惊讶, 灵阅川_中性, 灵阅川_恐惧, 灵未央_高兴, 灵未央_中性, 灵未央_惊讶, 灵未央_悲伤, 灵未央_厌恶, 灵未央_恐惧, 灵未央_生气, 灵妩月_厌恶, 灵妩月_生气, 灵妩月_中性, 灵妩月_惊讶, 灵妩月_恐惧, 灵妩月_高兴, 灵妩月_悲伤, 灵梦瑶_中性, 灵梦瑶_恐惧, 灵梦瑶_高兴, 灵梦瑶_悲伤, 灵梦瑶_惊讶, 灵梦瑶_生气, 灵妙声_中性, 灵妙声_恐惧, 灵妙声_高兴, 灵妙声_悲伤, 灵妙声_惊讶, 灵妙声_生气, 灵烈瑶_中性, 灵烈瑶_厌恶, 灵烈瑶_恐惧, 灵烈瑶_悲伤, 灵烈瑶_惊讶, 灵烈瑶_生气, 灵烈瑶_高兴, 灵幽月_中性, 灵幽月_厌恶, 灵幽月_恐惧, 灵幽月_悲伤, 灵幽月_惊讶, 灵幽月_生气, 灵幽月_高兴, 灵悦心_中性, 灵悦心_厌恶, 灵悦心_恐惧, 灵悦心_悲伤, 灵悦心_惊讶, 灵悦心_生气, 灵悦心_高兴, 灵汐月_中性, 灵汐月_厌恶, 灵汐月_恐惧, 灵汐月_悲伤, 灵汐月_惊讶, 灵汐月_生气, 灵汐月_高兴, 灵书意_中性, 灵书意_厌恶, 灵书意_恐惧, 灵书意_悲伤, 灵书意_惊讶, 灵书意_生气, 灵书意_高兴, 灵知远_中性, 灵知远_厌恶, 灵知远_恐惧, 灵知远_悲伤, 灵知远_惊讶, 灵知远_生气, 灵知远_高兴, 灵温言_中性, 灵温言_厌恶, 灵温言_恐惧, 灵温言_悲伤, 灵温言_惊讶, 灵温言_生气, 灵温言_高兴, 灵傲尘_中性, 灵傲尘_厌恶, 灵傲尘_恐惧, 灵傲尘_悲伤, 灵傲尘_生气, 灵傲尘_高兴, 灵佑安_中性, 灵佑安_厌恶, 灵佑安_恐惧, 灵佑安_悲伤, 灵佑安_惊讶, 灵佑安_生气, 灵佑安_高兴, 灵惜晨_中性, 灵惜晨_厌恶, 灵惜晨_恐惧, 灵惜晨_悲伤, 灵惜晨_惊讶, 灵惜晨_生气, 灵惜晨_高兴, 灵宛溪_中性, 灵宛溪_厌恶, 灵宛溪_恐惧, 灵宛溪_悲伤, 灵宛溪_惊讶, 灵宛溪_生气, 灵宛溪_高兴, 灵言澈_中性, 灵言澈_厌恶, 灵言澈_恐惧, 灵言澈_悲伤, 灵言澈_惊讶, 灵言澈_生气, 灵言澈_高兴, 灵溯光_中性, 灵溯光_厌恶, 灵溯光_恐惧, 灵溯光_悲伤, 灵溯光_惊讶, 灵溯光_生气, 灵溯光_高兴, 灵惊弦_中性, 灵惊弦_厌恶, 灵惊弦_恐惧, 灵惊弦_悲伤, 灵惊弦_惊讶, 灵惊弦_生气, 灵惊弦_高兴, 灵嘉肴_中性, 灵嘉肴_厌恶, 灵嘉肴_恐惧, 灵嘉肴_悲伤, 灵嘉肴_惊讶, 灵嘉肴_生气, 灵嘉肴_高兴, 灵黛嗔_中性, 灵黛嗔_厌恶, 灵黛嗔_恐惧, 灵黛嗔_悲伤, 灵黛嗔_惊讶, 灵黛嗔_生气, 灵黛嗔_高兴, 灵孤影_中性, 灵孤影_厌恶, 灵孤影_恐惧, 灵孤影_悲伤, 灵孤影_惊讶, 灵孤影_生气, 灵孤影_高兴, 灵雨汐_中性, 灵雨汐_恐惧, 灵雨汐_高兴, 灵雨汐_悲伤, 灵雨汐_惊讶, 灵雨汐_生气, 蜡笔小新_野原新之助 (小新), 蜡笔小新_野原美伢 (美伢), 小猪佩奇_佩奇, 康熙王朝_康熙, 西游记_唐僧, 甄嬛传_四郎, 潜伏_余则成, 水浒传_武松, 水浒传_潘金莲, 雍正王朝_雍正"),
            };
        }

        if (ReferenceText != null)
        {
            options["reference_text"] = ReferenceText;
        }

        if (Language != null)
        {
            options["language"] = Language;
        }

        if (Seed != null && Seed != "42")
        {
            options["seed"] = Seed;
        }

        if (GuidanceScale != null && GuidanceScale != 2)
        {
            options["guidance_scale"] = GuidanceScale.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Sigma != null && Sigma != 0.25)
        {
            options["sigma"] = Sigma.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (Temperature != null && Temperature != 0)
        {
            options["temperature"] = Temperature.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (RawText != null && RawText != false)
        {
            options["raw_text"] = RawText.Value ? "true" : "false";
        }

        if (TrimSilence != null && TrimSilence != true)
        {
            options["trim_silence"] = TrimSilence.Value ? "true" : "false";
        }

        if (TextChunkSize != null && TextChunkSize != 100)
        {
            options["text_chunk_size"] = TextChunkSize.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (TextChunkMode != null && TextChunkMode != Ming3AudioTextChunkMode.TagAware)
        {
            options["text_chunk_mode"] = TextChunkMode switch
            {
            Ming3AudioTextChunkMode.Default => "default",
            Ming3AudioTextChunkMode.TagAware => "tag_aware",
            Ming3AudioTextChunkMode.Japanese => "japanese",
            Ming3AudioTextChunkMode.Endline => "endline",
            _ => throw new ArgumentException("ming3_audio tts text_chunk_mode must be one of: default, tag_aware, japanese, endline"),
            };
        }

        return options;
    }
}