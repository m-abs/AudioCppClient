using System;

using Mabs.AudioCpp.Enums;
using Mabs.AudioCpp.Models.Tasks;
using Mabs.AudioCpp.Models.Tts;
using Mabs.AudioCpp.Utils;

using Xunit;

namespace Mabs.AudioCpp.Tests.Convenience;

/// <summary>Golden serialization, validation and composition tests for the generated tts convenience records.</summary>
public class TtsGoldenTests
{
    /// <summary>Golden serialization for the generated <c>Audio8TtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void Audio8Golden()
    {
        Audio8TtsRequest request = new() { Text = "hello", ReferenceText = "x" };
        string json = Json.Serialize(new TaskRunRequest { Model = "audio8_tts_preview_0_6b_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"audio8_tts_preview_0_6b_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"reference_text\":\"x\",\"top_p\":\"0.9\",\"top_k\":\"50\",\"temperature\":\"0.7\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>AukTtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void AukGolden()
    {
        AukTtsRequest request = new() { Text = "hello", Instruct = "x" };
        string json = Json.Serialize(new TaskRunRequest { Model = "auk_base_f32", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"auk_base_f32\",\"request\":{\"text\":\"hello\",\"options\":{\"instruct\":\"x\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>BreezeTtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void BreezeGolden()
    {
        BreezeTtsRequest request = new() { Text = "hello", Instruct = "x" };
        string json = Json.Serialize(new TaskRunRequest { Model = "breeze_tts_2_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"breeze_tts_2_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"instruct\":\"x\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>ChatterboxTtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void ChatterboxGolden()
    {
        ChatterboxTtsRequest request = new() { Text = "hello", Exaggeration = 1.5 };
        string json = Json.Serialize(new TaskRunRequest { Model = "chatterbox_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"chatterbox_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"exaggeration\":\"1.5\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>ChatterboxTurboTtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void ChatterboxTurboGolden()
    {
        ChatterboxTurboTtsRequest request = new() { Text = "hello", MaxTokens = 1001 };
        string json = Json.Serialize(new TaskRunRequest { Model = "chatterbox_turbo_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"chatterbox_turbo_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"max_tokens\":\"1001\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>Confucius4TtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void Confucius4Golden()
    {
        Confucius4TtsRequest request = new() { Text = "hello", Language = "x" };
        string json = Json.Serialize(new TaskRunRequest { Model = "confucius4_tts_orig", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"confucius4_tts_orig\",\"request\":{\"text\":\"hello\",\"options\":{\"language\":\"x\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>Cosyvoice3TtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void Cosyvoice3Golden()
    {
        Cosyvoice3TtsRequest request = new() { Text = "hello", TemplateName = Cosyvoice3TemplateName.CrossLingual };
        string json = Json.Serialize(new TaskRunRequest { Model = "cosyvoice3_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"cosyvoice3_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"template_name\":\"cross_lingual\",\"min_tokens\":\"0\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>DotsTtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void DotsGolden()
    {
        DotsTtsRequest request = new() { Text = "hello", ReferenceText = "x" };
        string json = Json.Serialize(new TaskRunRequest { Model = "dots_tts_soar_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"dots_tts_soar_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"reference_text\":\"x\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>DramaboxTtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void DramaboxGolden()
    {
        DramaboxTtsRequest request = new() { Text = "hello", TargetVoice = "x" };
        string json = Json.Serialize(new TaskRunRequest { Model = "dramabox_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"dramabox_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"target_voice\":\"x\",\"negative_prompt\":\"\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>EchoTtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void EchoGolden()
    {
        EchoTtsRequest request = new() { Text = "hello", TextChunkSize = 301 };
        string json = Json.Serialize(new TaskRunRequest { Model = "echo_tts_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"echo_tts_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"text_chunk_size\":\"301\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>F5TtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void F5Golden()
    {
        F5TtsRequest request = new() { Text = "hello", ReferenceText = "x", Dialect = "x" };
        string json = Json.Serialize(new TaskRunRequest { Model = "habibi_unified", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"habibi_unified\",\"request\":{\"text\":\"hello\",\"options\":{\"reference_text\":\"x\",\"dialect\":\"x\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>FireredAudioTtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void FireredAudioGolden()
    {
        FireredAudioTtsRequest request = new() { Text = "hello", TemplateName = FireredAudioTemplateName.TtsClone };
        string json = Json.Serialize(new TaskRunRequest { Model = "firered_audio_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"firered_audio_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"template_name\":\"tts_clone\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>Fireredtts3TtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void Fireredtts3Golden()
    {
        Fireredtts3TtsRequest request = new() { Text = "hello", TemplateName = Fireredtts3TemplateName.InstructTts };
        string json = Json.Serialize(new TaskRunRequest { Model = "fireredtts3_instruct_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"fireredtts3_instruct_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"template_name\":\"instruct_tts\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>FishAudioTtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void FishAudioGolden()
    {
        FishAudioTtsRequest request = new() { Text = "hello", ReferenceText = "x" };
        string json = Json.Serialize(new TaskRunRequest { Model = "fish_audio_s2_pro_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"fish_audio_s2_pro_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"reference_text\":\"x\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>FishDrama3TtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void FishDrama3Golden()
    {
        FishDrama3TtsRequest request = new() { Text = "hello", EncodeReferenceOnly = true };
        string json = Json.Serialize(new TaskRunRequest { Model = "fish_drama3_q4_experts_q8_backbone", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"fish_drama3_q4_experts_q8_backbone\",\"request\":{\"text\":\"hello\",\"options\":{\"encode_reference_only\":\"true\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>GlmTtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void GlmGolden()
    {
        GlmTtsRequest request = new() { Text = "hello", ReferenceText = "x", MaxTokens = 1 };
        string json = Json.Serialize(new TaskRunRequest { Model = "glm_tts_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"glm_tts_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"reference_text\":\"x\",\"max_tokens\":\"1\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>GradiumPhononTtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void GradiumPhononGolden()
    {
        GradiumPhononTtsRequest request = new() { Text = "hello", FramesAfterEos = 0 };
        string json = Json.Serialize(new TaskRunRequest { Model = "gradium_phonon_31m_en_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"gradium_phonon_31m_en_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"frames_after_eos\":\"0\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>HiggsAudioTtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void HiggsAudioGolden()
    {
        HiggsAudioTtsRequest request = new() { Text = "hello", StreamFramesPerEvent = 33 };
        string json = Json.Serialize(new TaskRunRequest { Model = "higgs_audio_tts_4b_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"higgs_audio_tts_4b_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"stream_frames_per_event\":\"33\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>IndexTts2TtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void IndexTts2Golden()
    {
        IndexTts2TtsRequest request = new() { Text = "x" };
        string json = Json.Serialize(new TaskRunRequest { Model = "index_tts2_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"index_tts2_q8_0\",\"request\":{\"options\":{\"text\":\"x\",\"emotion_text\":\"\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>InflectV2TtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void InflectV2Golden()
    {
        InflectV2TtsRequest request = new() { Text = "hello", Speed = 2 };
        string json = Json.Serialize(new TaskRunRequest { Model = "inflect_micro_v2_orig", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"inflect_micro_v2_orig\",\"request\":{\"text\":\"hello\",\"options\":{\"speed\":\"2\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>IrodoriTtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void IrodoriGolden()
    {
        IrodoriTtsRequest request = new() { Text = "hello", StreamFramesPerEvent = 13 };
        string json = Json.Serialize(new TaskRunRequest { Model = "irodori_tts_v4_small_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"irodori_tts_v4_small_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"stream_frames_per_event\":\"13\",\"num_inference_steps\":\"40\",\"duration_sec\":\"0\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>KittenTtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void KittenGolden()
    {
        KittenTtsRequest request = new() { Text = "hello", VoiceId = KittenTtsVoiceId.Bella };
        string json = Json.Serialize(new TaskRunRequest { Model = "kitten_tts_mini_0_8_orig", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"kitten_tts_mini_0_8_orig\",\"request\":{\"text\":\"hello\",\"options\":{\"voice_id\":\"Bella\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>KittenTts2TtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void KittenTts2Golden()
    {
        KittenTts2TtsRequest request = new() { Text = "hello", VoiceId = KittenTts2VoiceId.Bella };
        string json = Json.Serialize(new TaskRunRequest { Model = "kitten_tts2_20261021_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"kitten_tts2_20261021_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"voice_id\":\"Bella\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>KokoroTtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void KokoroGolden()
    {
        KokoroTtsRequest request = new() { Text = "hello", VoiceId = KokoroTtsVoiceId.AfAlloy };
        string json = Json.Serialize(new TaskRunRequest { Model = "kokoro_82m_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"kokoro_82m_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"voice_id\":\"af_alloy\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>KugelaudioTtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void KugelaudioGolden()
    {
        KugelaudioTtsRequest request = new() { Text = "hello", NumInferenceSteps = 21 };
        string json = Json.Serialize(new TaskRunRequest { Model = "kugelaudio_0_open_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"kugelaudio_0_open_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"num_inference_steps\":\"21\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>Lfm2AudioTtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void Lfm2AudioGolden()
    {
        Lfm2AudioTtsRequest request = new() { Text = "hello", Language = "x" };
        string json = Json.Serialize(new TaskRunRequest { Model = "lfm2_audio_1_5b_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"lfm2_audio_1_5b_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"language\":\"x\",\"max_tokens\":\"512\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>Lfm3AudioTtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void Lfm3AudioGolden()
    {
        Lfm3AudioTtsRequest request = new() { Text = "hello", Language = "x" };
        string json = Json.Serialize(new TaskRunRequest { Model = "lfm3_audio_2b_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"lfm3_audio_2b_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"language\":\"x\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>MagpieTtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void MagpieGolden()
    {
        MagpieTtsRequest request = new() { Text = "hello", Language = "x" };
        string json = Json.Serialize(new TaskRunRequest { Model = "magpie_tts_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"magpie_tts_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"language\":\"x\",\"text_chunk_size\":\"300\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>Maya1TtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void Maya1Golden()
    {
        Maya1TtsRequest request = new() { Text = "hello", Instruct = "x", MaxTokens = 2049 };
        string json = Json.Serialize(new TaskRunRequest { Model = "maya1_orig", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"maya1_orig\",\"request\":{\"text\":\"hello\",\"options\":{\"instruct\":\"x\",\"max_tokens\":\"2049\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>Ming3AudioTtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void Ming3AudioGolden()
    {
        Ming3AudioTtsRequest request = new() { Text = "hello", SystemPrompt = "x" };
        string json = Json.Serialize(new TaskRunRequest { Model = "ming3_audio_tiny_bf16", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"ming3_audio_tiny_bf16\",\"request\":{\"text\":\"hello\",\"options\":{\"system_prompt\":\"x\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>Ming3TtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void Ming3Golden()
    {
        Ming3TtsRequest request = new() { Text = "hello", TemplateName = Ming3TtsTemplateName.Tts };
        string json = Json.Serialize(new TaskRunRequest { Model = "ming_3_0_tts_0_5b_bf16", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"ming_3_0_tts_0_5b_bf16\",\"request\":{\"text\":\"hello\",\"options\":{\"template_name\":\"tts\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>MinimaxH3TtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void MinimaxH3Golden()
    {
        MinimaxH3TtsRequest request = new() { Text = "hello", NumInferenceSteps = 13 };
        string json = Json.Serialize(new TaskRunRequest { Model = "minimax_h3_q4_k", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"minimax_h3_q4_k\",\"request\":{\"text\":\"hello\",\"options\":{\"num_inference_steps\":\"13\",\"num_frames\":\"241\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>MiottsTtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void MiottsGolden()
    {
        MiottsTtsRequest request = new() { Text = "hello", MaxTokens = 1 };
        string json = Json.Serialize(new TaskRunRequest { Model = "miotts_1_7b_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"miotts_1_7b_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"max_tokens\":\"1\",\"top_k\":\"50\",\"top_p\":\"1\",\"temperature\":\"0.8\",\"repetition_penalty\":\"1\",\"best_of_n\":\"1\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>MiraTtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void MiraGolden()
    {
        MiraTtsRequest request = new() { Text = "hello", MaxTokens = 1025 };
        string json = Json.Serialize(new TaskRunRequest { Model = "mira_tts", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"mira_tts\",\"request\":{\"text\":\"hello\",\"options\":{\"max_tokens\":\"1025\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>MossTtsLocalTtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void MossTtsLocalGolden()
    {
        MossTtsLocalTtsRequest request = new() { Text = "hello", MaxTokens = 4097 };
        string json = Json.Serialize(new TaskRunRequest { Model = "moss_tts_local_v1_5_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"moss_tts_local_v1_5_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"max_tokens\":\"4097\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>MossTtsNanoTtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void MossTtsNanoGolden()
    {
        MossTtsNanoTtsRequest request = new() { Text = "hello", ReferenceText = "x" };
        string json = Json.Serialize(new TaskRunRequest { Model = "moss_tts_nano_100m_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"moss_tts_nano_100m_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"reference_text\":\"x\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>MossTtsV15TtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void MossTtsV15Golden()
    {
        MossTtsV15TtsRequest request = new() { Text = "hello", MinFrames = 1 };
        string json = Json.Serialize(new TaskRunRequest { Model = "moss_tts_v15_q8_0_codec_f16", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"moss_tts_v15_q8_0_codec_f16\",\"request\":{\"text\":\"hello\",\"options\":{\"min_frames\":\"1\",\"instruct\":\"\",\"language\":\"\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>MossTtsdTtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void MossTtsdGolden()
    {
        MossTtsdTtsRequest request = new() { Text = "hello", VoiceSamples = "x" };
        string json = Json.Serialize(new TaskRunRequest { Model = "moss_ttsd_q8_0_codec_f16", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"moss_ttsd_q8_0_codec_f16\",\"request\":{\"text\":\"hello\",\"options\":{\"voice_samples\":\"x\",\"instruct\":\"\",\"max_frames\":\"0\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>NeuttsTtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void NeuttsGolden()
    {
        NeuttsTtsRequest request = new() { Text = "hello", VoiceId = NeuttsVoiceId.Dave };
        string json = Json.Serialize(new TaskRunRequest { Model = "neutts_2e_orig", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"neutts_2e_orig\",\"request\":{\"text\":\"hello\",\"options\":{\"voice_id\":\"dave\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>OmnivoiceTtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void OmnivoiceGolden()
    {
        OmnivoiceTtsRequest request = new() { Text = "hello", Language = "x" };
        string json = Json.Serialize(new TaskRunRequest { Model = "omnivoice_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"omnivoice_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"language\":\"x\",\"instruct\":\"\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>OutettsTtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void OutettsGolden()
    {
        OutettsTtsRequest request = new() { Text = "hello", MaxTokens = 1 };
        string json = Json.Serialize(new TaskRunRequest { Model = "outetts_1_0_1b_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"outetts_1_0_1b_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"max_tokens\":\"1\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>PiperTtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void PiperGolden()
    {
        PiperTtsRequest request = new() { Text = "hello", Speed = 2 };
        string json = Json.Serialize(new TaskRunRequest { Model = "piper_lessac_medium_orig", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"piper_lessac_medium_orig\",\"request\":{\"text\":\"hello\",\"options\":{\"speed\":\"2\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>PlapreTtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void PlapreGolden()
    {
        PlapreTtsRequest request = new() { Text = "hello", VoiceId = PlapreVoiceId.Ida };
        string json = Json.Serialize(new TaskRunRequest { Model = "plapre_3_bf16", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"plapre_3_bf16\",\"request\":{\"text\":\"hello\",\"options\":{\"voice_id\":\"ida\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>PocketTtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void PocketGolden()
    {
        PocketTtsRequest request = new() { Text = "hello", FramesAfterEos = 0 };
        string json = Json.Serialize(new TaskRunRequest { Model = "pocket_tts_english_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"pocket_tts_english_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"frames_after_eos\":\"0\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>PocketTtsSmallTtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void PocketTtsSmallGolden()
    {
        PocketTtsSmallTtsRequest request = new() { Text = "hello", FramesAfterEos = 0 };
        string json = Json.Serialize(new TaskRunRequest { Model = "pocket_tts_small_english_18m_refl_f32", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"pocket_tts_small_english_18m_refl_f32\",\"request\":{\"text\":\"hello\",\"options\":{\"frames_after_eos\":\"0\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>Qwen3TtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void Qwen3Golden()
    {
        Qwen3TtsRequest request = new() { Text = "hello", StreamChunkPolicy = Qwen3TtsStreamChunkPolicy.Fixed };
        string json = Json.Serialize(new TaskRunRequest { Model = "qwen3_tts_1_7b_base_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"qwen3_tts_1_7b_base_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"stream_chunk_policy\":\"fixed\",\"instruct\":\"\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>SanottsTtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void SanottsGolden()
    {
        SanottsTtsRequest request = new() { Text = "hello", Speed = 2 };
        string json = Json.Serialize(new TaskRunRequest { Model = "sanotts_heart_nano_orig", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"sanotts_heart_nano_orig\",\"request\":{\"text\":\"hello\",\"options\":{\"speed\":\"2\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>SopranoTtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void SopranoGolden()
    {
        SopranoTtsRequest request = new() { Text = "hello", MaxTokens = 513 };
        string json = Json.Serialize(new TaskRunRequest { Model = "soprano_1_1_80m_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"soprano_1_1_80m_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"max_tokens\":\"513\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>SoproTtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void SoproGolden()
    {
        SoproTtsRequest request = new() { Text = "hello", Language = "x" };
        string json = Json.Serialize(new TaskRunRequest { Model = "sopro_v2_turbo_f16", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"sopro_v2_turbo_f16\",\"request\":{\"text\":\"hello\",\"options\":{\"language\":\"x\",\"temperature\":\"0.8\",\"top_p\":\"0.9\",\"top_k\":\"25\",\"num_inference_steps\":\"2\",\"text_chunk_size\":\"300\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>SupertonicTtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void SupertonicGolden()
    {
        SupertonicTtsRequest request = new() { Text = "hello", VoiceId = SupertonicVoiceId.M2 };
        string json = Json.Serialize(new TaskRunRequest { Model = "supertonic_3_orig", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"supertonic_3_orig\",\"request\":{\"text\":\"hello\",\"options\":{\"voice_id\":\"M2\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>Vevo2TtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void Vevo2Golden()
    {
        Vevo2TtsRequest request = new() { Text = "hello", Route = Vevo2Route.ZeroShotTts };
        string json = Json.Serialize(new TaskRunRequest { Model = "vevo2_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"vevo2_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"route\":\"zero_shot_tts\",\"temperature\":\"0.7\",\"top_k\":\"20\",\"top_p\":\"0.8\",\"audio_chunk_duration_sec\":\"0\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>VibevoiceTtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void VibevoiceGolden()
    {
        VibevoiceTtsRequest request = new() { Text = "hello", VoiceSamples = "x" };
        string json = Json.Serialize(new TaskRunRequest { Model = "vibevoice_1_5b_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"vibevoice_1_5b_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"voice_samples\":\"x\",\"num_inference_steps\":\"10\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>VieneuV3TurboTtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void VieneuV3TurboGolden()
    {
        VieneuV3TurboTtsRequest request = new() { Text = "hello", AutoFrameCap = false };
        string json = Json.Serialize(new TaskRunRequest { Model = "vieneu_v3_turbo_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"vieneu_v3_turbo_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"auto_frame_cap\":\"false\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>Voxcpm1TtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void Voxcpm1Golden()
    {
        Voxcpm1TtsRequest request = new() { Text = "hello", TextChunkMode = "x" };
        string json = Json.Serialize(new TaskRunRequest { Model = "voxcpm1_0_5b_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"voxcpm1_0_5b_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"text_chunk_mode\":\"x\",\"retry_badcase\":\"true\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>Voxcpm2TtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void Voxcpm2Golden()
    {
        Voxcpm2TtsRequest request = new() { Text = "hello", MinTokens = 3 };
        string json = Json.Serialize(new TaskRunRequest { Model = "voxcpm2_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"voxcpm2_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"min_tokens\":\"3\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>VoxtralTtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void VoxtralGolden()
    {
        VoxtralTtsRequest request = new() { Text = "hello", VoiceId = VoxtralTtsVoiceId.CasualFemale };
        string json = Json.Serialize(new TaskRunRequest { Model = "voxtral_4b_tts_2603_bf16", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"voxtral_4b_tts_2603_bf16\",\"request\":{\"text\":\"hello\",\"options\":{\"voice_id\":\"casual_female\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>XttsV2TtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void XttsV2Golden()
    {
        XttsV2TtsRequest request = new() { Text = "hello", Language = "x" };
        string json = Json.Serialize(new TaskRunRequest { Model = "xtts_v2_gguf_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"xtts_v2_gguf_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"language\":\"x\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>ZipvoiceTtsRequest</c> (set and pre-initialized options emitted in spec order).</summary>
    [Fact]
    public void ZipvoiceGolden()
    {
        ZipvoiceTtsRequest request = new() { Text = "hello", ReferenceText = "x", TextChunkSize = 129 };
        string json = Json.Serialize(new TaskRunRequest { Model = "zipvoice_distill_gguf", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"zipvoice_distill_gguf\",\"request\":{\"text\":\"hello\",\"options\":{\"reference_text\":\"x\",\"text_chunk_size\":\"129\"}}}", json);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>Audio8TtsRequest</c>.</summary>
    [Fact]
    public void Audio8Validate()
    {
        Audio8TtsRequest request = new() { MaxTokens = -1 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("audio8_tts tts max_tokens must be at least 0", ex.Message);
        Assert.Equal("MaxTokens", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>AukTtsRequest</c>.</summary>
    [Fact]
    public void AukValidate()
    {
        AukTtsRequest request = new() { NumInferenceSteps = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("auk tts num_inference_steps must be at least 1", ex.Message);
        Assert.Equal("NumInferenceSteps", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>BreezeTtsRequest</c>.</summary>
    [Fact]
    public void BreezeValidate()
    {
        BreezeTtsRequest request = new() { TextChunkSize = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("breeze_tts tts text_chunk_size must be at least 1", ex.Message);
        Assert.Equal("TextChunkSize", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>ChatterboxTtsRequest</c>.</summary>
    [Fact]
    public void ChatterboxValidate()
    {
        ChatterboxTtsRequest request = new() { MaxTokens = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("chatterbox tts max_tokens must be at least 1", ex.Message);
        Assert.Equal("MaxTokens", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>ChatterboxTurboTtsRequest</c>.</summary>
    [Fact]
    public void ChatterboxTurboValidate()
    {
        ChatterboxTurboTtsRequest request = new() { MaxTokens = -1 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("chatterbox_turbo tts max_tokens must be at least 0", ex.Message);
        Assert.Equal("MaxTokens", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>Confucius4TtsRequest</c>.</summary>
    [Fact]
    public void Confucius4Validate()
    {
        Confucius4TtsRequest request = new() { Temperature = -1 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("confucius4_tts tts temperature must be at least 0", ex.Message);
        Assert.Equal("Temperature", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>Cosyvoice3TtsRequest</c>.</summary>
    [Fact]
    public void Cosyvoice3Validate()
    {
        Cosyvoice3TtsRequest request = new() { TextChunkSize = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("cosyvoice3 tts text_chunk_size must be at least 1", ex.Message);
        Assert.Equal("TextChunkSize", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>DotsTtsRequest</c>.</summary>
    [Fact]
    public void DotsValidate()
    {
        DotsTtsRequest request = new() { ReferenceDurationSec = -1 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("dots_tts tts reference_duration_sec must be at least 0", ex.Message);
        Assert.Equal("ReferenceDurationSec", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>DramaboxTtsRequest</c>.</summary>
    [Fact]
    public void DramaboxValidate()
    {
        DramaboxTtsRequest request = new() { DurationSec = -1 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("dramabox tts duration_sec must be at least 0", ex.Message);
        Assert.Equal("DurationSec", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>EchoTtsRequest</c>.</summary>
    [Fact]
    public void EchoValidate()
    {
        EchoTtsRequest request = new() { TextChunkSize = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("echo_tts tts text_chunk_size must be at least 1", ex.Message);
        Assert.Equal("TextChunkSize", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>F5TtsRequest</c>.</summary>
    [Fact]
    public void F5Validate()
    {
        F5TtsRequest request = new() { ReferenceText = "x", SwaySamplingCoef = -6 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("f5_tts tts sway_sampling_coef must be between -5 and 5", ex.Message);
        Assert.Equal("SwaySamplingCoef", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>FireredAudioTtsRequest</c>.</summary>
    [Fact]
    public void FireredAudioValidate()
    {
        FireredAudioTtsRequest request = new() { NumInferenceSteps = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("firered_audio tts num_inference_steps must be at least 1", ex.Message);
        Assert.Equal("NumInferenceSteps", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>Fireredtts3TtsRequest</c>.</summary>
    [Fact]
    public void Fireredtts3Validate()
    {
        Fireredtts3TtsRequest request = new() { TextChunkSize = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("fireredtts3 tts text_chunk_size must be at least 1", ex.Message);
        Assert.Equal("TextChunkSize", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>FishAudioTtsRequest</c>.</summary>
    [Fact]
    public void FishAudioValidate()
    {
        FishAudioTtsRequest request = new() { MaxTokens = -1 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("fish_audio tts max_tokens must be at least 0", ex.Message);
        Assert.Equal("MaxTokens", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>FishDrama3TtsRequest</c>.</summary>
    [Fact]
    public void FishDrama3Validate()
    {
        FishDrama3TtsRequest request = new() { MaxTokens = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("fish_drama3 tts max_tokens must be at least 1", ex.Message);
        Assert.Equal("MaxTokens", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>GlmTtsRequest</c>.</summary>
    [Fact]
    public void GlmValidate()
    {
        GlmTtsRequest request = new() { ReferenceText = "x", NumInferenceSteps = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("glm_tts tts num_inference_steps must be at least 1", ex.Message);
        Assert.Equal("NumInferenceSteps", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>GradiumPhononTtsRequest</c>.</summary>
    [Fact]
    public void GradiumPhononValidate()
    {
        GradiumPhononTtsRequest request = new() { FramesAfterEos = -2 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("gradium_phonon tts frames_after_eos must be at least -1", ex.Message);
        Assert.Equal("FramesAfterEos", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>HiggsAudioTtsRequest</c>.</summary>
    [Fact]
    public void HiggsAudioValidate()
    {
        HiggsAudioTtsRequest request = new() { StreamFramesPerEvent = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("higgs_audio_tts tts stream_frames_per_event must be at least 1", ex.Message);
        Assert.Equal("StreamFramesPerEvent", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>IndexTts2TtsRequest</c>.</summary>
    [Fact]
    public void IndexTts2Validate()
    {
        IndexTts2TtsRequest request = new() { EmotionAlpha = -1 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("index_tts2 tts emotion_alpha must be between 0 and 1", ex.Message);
        Assert.Equal("EmotionAlpha", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>InflectV2TtsRequest</c>.</summary>
    [Fact]
    public void InflectV2Validate()
    {
        InflectV2TtsRequest request = new() { Speed = -0.5 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("inflect_v2 tts speed must be between 0.5 and 2", ex.Message);
        Assert.Equal("Speed", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>IrodoriTtsRequest</c>.</summary>
    [Fact]
    public void IrodoriValidate()
    {
        IrodoriTtsRequest request = new() { StreamFramesPerEvent = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("irodori_tts tts stream_frames_per_event must be at least 1", ex.Message);
        Assert.Equal("StreamFramesPerEvent", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>KittenTtsRequest</c>.</summary>
    [Fact]
    public void KittenValidate()
    {
        KittenTtsRequest request = new() { TextChunkSize = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("kitten_tts tts text_chunk_size must be at least 1", ex.Message);
        Assert.Equal("TextChunkSize", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>KittenTts2TtsRequest</c>.</summary>
    [Fact]
    public void KittenTts2Validate()
    {
        KittenTts2TtsRequest request = new() { Temperature = -1 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("kitten_tts2 tts temperature must be at least 0", ex.Message);
        Assert.Equal("Temperature", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>KokoroTtsRequest</c>.</summary>
    [Fact]
    public void KokoroValidate()
    {
        KokoroTtsRequest request = new() { TextChunkSize = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("kokoro_tts tts text_chunk_size must be at least 1", ex.Message);
        Assert.Equal("TextChunkSize", ex.ParamName);
    }

    /// <summary>A set <c>voice_id</c> fails the capability gate for the generated <c>KugelaudioTtsRequest</c>.</summary>
    [Fact]
    public void KugelaudioValidateVoiceId()
    {
        KugelaudioTtsRequest request = new() { VoiceId = KugelaudioVoiceId.Default };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("kugelaudio tts does not accept voice_id (capability built_in_voices is false)", ex.Message);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>KugelaudioTtsRequest</c>.</summary>
    [Fact]
    public void KugelaudioValidate()
    {
        KugelaudioTtsRequest request = new() { NumInferenceSteps = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("kugelaudio tts num_inference_steps must be between 1 and 1000", ex.Message);
        Assert.Equal("NumInferenceSteps", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>Lfm2AudioTtsRequest</c>.</summary>
    [Fact]
    public void Lfm2AudioValidate()
    {
        Lfm2AudioTtsRequest request = new() { MaxTokens = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("lfm2_audio tts max_tokens must be at least 1", ex.Message);
        Assert.Equal("MaxTokens", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>Lfm3AudioTtsRequest</c>.</summary>
    [Fact]
    public void Lfm3AudioValidate()
    {
        Lfm3AudioTtsRequest request = new() { MaxTokens = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("lfm3_audio tts max_tokens must be at least 1", ex.Message);
        Assert.Equal("MaxTokens", ex.ParamName);
    }

    /// <summary>A set <c>voice_id</c> fails the capability gate for the generated <c>MagpieTtsRequest</c>.</summary>
    [Fact]
    public void MagpieValidateVoiceId()
    {
        MagpieTtsRequest request = new() { VoiceId = MagpieTtsVoiceId.Aria };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("magpie_tts tts does not accept voice_id (capability built_in_voices is false)", ex.Message);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>MagpieTtsRequest</c>.</summary>
    [Fact]
    public void MagpieValidate()
    {
        MagpieTtsRequest request = new() { Temperature = -1 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("magpie_tts tts temperature must be at least 0", ex.Message);
        Assert.Equal("Temperature", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>Maya1TtsRequest</c>.</summary>
    [Fact]
    public void Maya1Validate()
    {
        Maya1TtsRequest request = new() { Instruct = "x", MaxTokens = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("maya1 tts max_tokens must be at least 1", ex.Message);
        Assert.Equal("MaxTokens", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>Ming3AudioTtsRequest</c>.</summary>
    [Fact]
    public void Ming3AudioValidate()
    {
        Ming3AudioTtsRequest request = new() { MaxTokens = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("ming3_audio tts max_tokens must be at least 1", ex.Message);
        Assert.Equal("MaxTokens", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>Ming3TtsRequest</c>.</summary>
    [Fact]
    public void Ming3Validate()
    {
        Ming3TtsRequest request = new() { MaxTokens = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("ming3_tts tts max_tokens must be at least 1", ex.Message);
        Assert.Equal("MaxTokens", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>MinimaxH3TtsRequest</c>.</summary>
    [Fact]
    public void MinimaxH3Validate()
    {
        MinimaxH3TtsRequest request = new() { NumInferenceSteps = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("minimax_h3 tts num_inference_steps must be at least 1", ex.Message);
        Assert.Equal("NumInferenceSteps", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>MiottsTtsRequest</c>.</summary>
    [Fact]
    public void MiottsValidate()
    {
        MiottsTtsRequest request = new() { MaxTokens = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("miotts tts max_tokens must be at least 1", ex.Message);
        Assert.Equal("MaxTokens", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>MiraTtsRequest</c>.</summary>
    [Fact]
    public void MiraValidate()
    {
        MiraTtsRequest request = new() { MaxTokens = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("mira_tts tts max_tokens must be at least 1", ex.Message);
        Assert.Equal("MaxTokens", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>MossTtsLocalTtsRequest</c>.</summary>
    [Fact]
    public void MossTtsLocalValidate()
    {
        MossTtsLocalTtsRequest request = new() { MaxTokens = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("moss_tts_local tts max_tokens must be at least 1", ex.Message);
        Assert.Equal("MaxTokens", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>MossTtsNanoTtsRequest</c>.</summary>
    [Fact]
    public void MossTtsNanoValidate()
    {
        MossTtsNanoTtsRequest request = new() { MaxTokens = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("moss_tts_nano tts max_tokens must be at least 1", ex.Message);
        Assert.Equal("MaxTokens", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>MossTtsV15TtsRequest</c>.</summary>
    [Fact]
    public void MossTtsV15Validate()
    {
        MossTtsV15TtsRequest request = new() { TextChunkSize = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("moss_tts_v15 tts text_chunk_size must be at least 1", ex.Message);
        Assert.Equal("TextChunkSize", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>MossTtsdTtsRequest</c>.</summary>
    [Fact]
    public void MossTtsdValidate()
    {
        MossTtsdTtsRequest request = new() { Temperature = -1 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("moss_ttsd tts temperature must be at least 0", ex.Message);
        Assert.Equal("Temperature", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>NeuttsTtsRequest</c>.</summary>
    [Fact]
    public void NeuttsValidate()
    {
        NeuttsTtsRequest request = new() { MaxTokens = -1 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("neutts tts max_tokens must be at least 0", ex.Message);
        Assert.Equal("MaxTokens", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>OmnivoiceTtsRequest</c>.</summary>
    [Fact]
    public void OmnivoiceValidate()
    {
        OmnivoiceTtsRequest request = new() { NumInferenceSteps = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("omnivoice tts num_inference_steps must be at least 1", ex.Message);
        Assert.Equal("NumInferenceSteps", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>OutettsTtsRequest</c>.</summary>
    [Fact]
    public void OutettsValidate()
    {
        OutettsTtsRequest request = new() { MaxTokens = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("outetts tts max_tokens must be at least 1", ex.Message);
        Assert.Equal("MaxTokens", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>PiperTtsRequest</c>.</summary>
    [Fact]
    public void PiperValidate()
    {
        PiperTtsRequest request = new() { Speed = -0.5 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("piper_tts tts speed must be between 0.5 and 2", ex.Message);
        Assert.Equal("Speed", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>PlapreTtsRequest</c>.</summary>
    [Fact]
    public void PlapreValidate()
    {
        PlapreTtsRequest request = new() { EditStartFrame = -1 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("plapre tts edit_start_frame must be at least 0", ex.Message);
        Assert.Equal("EditStartFrame", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>PocketTtsRequest</c>.</summary>
    [Fact]
    public void PocketValidate()
    {
        PocketTtsRequest request = new() { FramesAfterEos = -2 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("pocket_tts tts frames_after_eos must be at least -1", ex.Message);
        Assert.Equal("FramesAfterEos", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>PocketTtsSmallTtsRequest</c>.</summary>
    [Fact]
    public void PocketTtsSmallValidate()
    {
        PocketTtsSmallTtsRequest request = new() { FramesAfterEos = -2 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("pocket_tts_small tts frames_after_eos must be at least -1", ex.Message);
        Assert.Equal("FramesAfterEos", ex.ParamName);
    }

    /// <summary>A set <c>voice_id</c> fails the capability gate for the generated <c>Qwen3TtsRequest</c>.</summary>
    [Fact]
    public void Qwen3ValidateVoiceId()
    {
        Qwen3TtsRequest request = new() { VoiceId = Qwen3TtsVoiceId.Aiden };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("qwen3_tts tts does not accept voice_id (capability built_in_voices is false)", ex.Message);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>Qwen3TtsRequest</c>.</summary>
    [Fact]
    public void Qwen3Validate()
    {
        Qwen3TtsRequest request = new() { StreamFramesPerEvent = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("qwen3_tts tts stream_frames_per_event must be at least 1", ex.Message);
        Assert.Equal("StreamFramesPerEvent", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>SanottsTtsRequest</c>.</summary>
    [Fact]
    public void SanottsValidate()
    {
        SanottsTtsRequest request = new() { Speed = -0.5 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("sanotts tts speed must be between 0.5 and 2", ex.Message);
        Assert.Equal("Speed", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>SopranoTtsRequest</c>.</summary>
    [Fact]
    public void SopranoValidate()
    {
        SopranoTtsRequest request = new() { MaxTokens = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("soprano_tts tts max_tokens must be at least 1", ex.Message);
        Assert.Equal("MaxTokens", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>SoproTtsRequest</c>.</summary>
    [Fact]
    public void SoproValidate()
    {
        SoproTtsRequest request = new() { NumInferenceSteps = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("sopro_tts tts num_inference_steps must be at least 1", ex.Message);
        Assert.Equal("NumInferenceSteps", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>SupertonicTtsRequest</c>.</summary>
    [Fact]
    public void SupertonicValidate()
    {
        SupertonicTtsRequest request = new() { NumInferenceSteps = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("supertonic tts num_inference_steps must be at least 1", ex.Message);
        Assert.Equal("NumInferenceSteps", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>Vevo2TtsRequest</c>.</summary>
    [Fact]
    public void Vevo2Validate()
    {
        Vevo2TtsRequest request = new() { ReferenceDurationSec = -1 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("vevo2 tts reference_duration_sec must be at least 0", ex.Message);
        Assert.Equal("ReferenceDurationSec", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>VibevoiceTtsRequest</c>.</summary>
    [Fact]
    public void VibevoiceValidate()
    {
        VibevoiceTtsRequest request = new() { NumInferenceSteps = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("vibevoice tts num_inference_steps must be at least 1", ex.Message);
        Assert.Equal("NumInferenceSteps", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>VieneuV3TurboTtsRequest</c>.</summary>
    [Fact]
    public void VieneuV3TurboValidate()
    {
        VieneuV3TurboTtsRequest request = new() { MaxTokens = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("vieneu_v3_turbo tts max_tokens must be at least 1", ex.Message);
        Assert.Equal("MaxTokens", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>Voxcpm1TtsRequest</c>.</summary>
    [Fact]
    public void Voxcpm1Validate()
    {
        Voxcpm1TtsRequest request = new() { TextChunkSize = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("voxcpm1 tts text_chunk_size must be at least 1", ex.Message);
        Assert.Equal("TextChunkSize", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>Voxcpm2TtsRequest</c>.</summary>
    [Fact]
    public void Voxcpm2Validate()
    {
        Voxcpm2TtsRequest request = new() { MinTokens = -1 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("voxcpm2 tts min_tokens must be at least 0", ex.Message);
        Assert.Equal("MinTokens", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>VoxtralTtsRequest</c>.</summary>
    [Fact]
    public void VoxtralValidate()
    {
        VoxtralTtsRequest request = new() { MaxTokens = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("voxtral_tts tts max_tokens must be at least 1", ex.Message);
        Assert.Equal("MaxTokens", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>XttsV2TtsRequest</c>.</summary>
    [Fact]
    public void XttsV2Validate()
    {
        XttsV2TtsRequest request = new() { Temperature = -1 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("xtts_v2 tts temperature must be at least 0", ex.Message);
        Assert.Equal("Temperature", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>ZipvoiceTtsRequest</c>.</summary>
    [Fact]
    public void ZipvoiceValidate()
    {
        ZipvoiceTtsRequest request = new() { ReferenceText = "x", TextChunkSize = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("zipvoice tts text_chunk_size must be at least 1", ex.Message);
        Assert.Equal("TextChunkSize", ex.ParamName);
    }
}