using System;

using Mabs.AudioCpp.Enums;
using Mabs.AudioCpp.Models.Clone;
using Mabs.AudioCpp.Models.Tasks;
using Mabs.AudioCpp.Utils;

using Xunit;

namespace Mabs.AudioCpp.Tests.Convenience;

/// <summary>Golden serialization, validation and composition tests for the generated clone convenience records.</summary>
public class CloneGoldenTests
{
    /// <summary>Golden serialization for the generated <c>Audio8TtsCloneRequest</c> (set options emitted in spec order).</summary>
    [Fact]
    public void Audio8TtsGolden()
    {
        Audio8TtsCloneRequest request = new() { Text = "hello", ReferenceText = "x" };
        string json = Json.Serialize(new TaskRunRequest { Model = "audio8_tts_preview_0_6b_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"audio8_tts_preview_0_6b_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"reference_text\":\"x\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>AukCloneRequest</c> (set options emitted in spec order).</summary>
    [Fact]
    public void AukGolden()
    {
        AukCloneRequest request = new() { Text = "hello", Instruct = "x" };
        string json = Json.Serialize(new TaskRunRequest { Model = "auk_base_f32", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"auk_base_f32\",\"request\":{\"text\":\"hello\",\"options\":{\"instruct\":\"x\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>BreezeTtsCloneRequest</c> (set options emitted in spec order).</summary>
    [Fact]
    public void BreezeTtsGolden()
    {
        BreezeTtsCloneRequest request = new() { Text = "hello", Instruct = "x" };
        string json = Json.Serialize(new TaskRunRequest { Model = "breeze_tts_2_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"breeze_tts_2_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"instruct\":\"x\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>ChatterboxCloneRequest</c> (set options emitted in spec order).</summary>
    [Fact]
    public void ChatterboxGolden()
    {
        ChatterboxCloneRequest request = new() { Text = "hello", Exaggeration = 1.5 };
        string json = Json.Serialize(new TaskRunRequest { Model = "chatterbox_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"chatterbox_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"exaggeration\":\"1.5\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>Confucius4TtsCloneRequest</c> (set options emitted in spec order).</summary>
    [Fact]
    public void Confucius4TtsGolden()
    {
        Confucius4TtsCloneRequest request = new() { Text = "hello", Language = "x" };
        string json = Json.Serialize(new TaskRunRequest { Model = "confucius4_tts_orig", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"confucius4_tts_orig\",\"request\":{\"text\":\"hello\",\"options\":{\"language\":\"x\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>Cosyvoice3CloneRequest</c> (set options emitted in spec order).</summary>
    [Fact]
    public void Cosyvoice3Golden()
    {
        Cosyvoice3CloneRequest request = new() { Text = "hello", TemplateName = Cosyvoice3TemplateName.CrossLingual };
        string json = Json.Serialize(new TaskRunRequest { Model = "cosyvoice3_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"cosyvoice3_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"template_name\":\"cross_lingual\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>DotsTtsCloneRequest</c> (set options emitted in spec order).</summary>
    [Fact]
    public void DotsTtsGolden()
    {
        DotsTtsCloneRequest request = new() { Text = "hello", ReferenceText = "x" };
        string json = Json.Serialize(new TaskRunRequest { Model = "dots_tts_soar_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"dots_tts_soar_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"reference_text\":\"x\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>DramaboxCloneRequest</c> (set options emitted in spec order).</summary>
    [Fact]
    public void DramaboxGolden()
    {
        DramaboxCloneRequest request = new() { Text = "hello", TargetVoice = "x" };
        string json = Json.Serialize(new TaskRunRequest { Model = "dramabox_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"dramabox_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"target_voice\":\"x\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>EchoTtsCloneRequest</c> (set options emitted in spec order).</summary>
    [Fact]
    public void EchoTtsGolden()
    {
        EchoTtsCloneRequest request = new() { Text = "hello", TextChunkSize = 301 };
        string json = Json.Serialize(new TaskRunRequest { Model = "echo_tts_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"echo_tts_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"text_chunk_size\":\"301\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>F5TtsCloneRequest</c> (set options emitted in spec order).</summary>
    [Fact]
    public void F5TtsGolden()
    {
        F5TtsCloneRequest request = new() { Text = "hello", ReferenceText = "x", Dialect = "x" };
        string json = Json.Serialize(new TaskRunRequest { Model = "habibi_unified", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"habibi_unified\",\"request\":{\"text\":\"hello\",\"options\":{\"reference_text\":\"x\",\"dialect\":\"x\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>FireredAudioCloneRequest</c> (set options emitted in spec order).</summary>
    [Fact]
    public void FireredAudioGolden()
    {
        FireredAudioCloneRequest request = new() { Text = "hello", TemplateName = FireredAudioTemplateName.TtsClone };
        string json = Json.Serialize(new TaskRunRequest { Model = "firered_audio_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"firered_audio_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"template_name\":\"tts_clone\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>Fireredtts3CloneRequest</c> (set options emitted in spec order).</summary>
    [Fact]
    public void Fireredtts3Golden()
    {
        Fireredtts3CloneRequest request = new() { Text = "hello", TemplateName = Fireredtts3TemplateName.InstructTts };
        string json = Json.Serialize(new TaskRunRequest { Model = "fireredtts3_instruct_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"fireredtts3_instruct_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"template_name\":\"instruct_tts\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>FishAudioCloneRequest</c> (set options emitted in spec order).</summary>
    [Fact]
    public void FishAudioGolden()
    {
        FishAudioCloneRequest request = new() { Text = "hello", ReferenceText = "x" };
        string json = Json.Serialize(new TaskRunRequest { Model = "fish_audio_s2_pro_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"fish_audio_s2_pro_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"reference_text\":\"x\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>FishDrama3CloneRequest</c> (set options emitted in spec order).</summary>
    [Fact]
    public void FishDrama3Golden()
    {
        FishDrama3CloneRequest request = new() { Text = "hello", EncodeReferenceOnly = true };
        string json = Json.Serialize(new TaskRunRequest { Model = "fish_drama3_q4_experts_q8_backbone", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"fish_drama3_q4_experts_q8_backbone\",\"request\":{\"text\":\"hello\",\"options\":{\"encode_reference_only\":\"true\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>GlmTtsCloneRequest</c> (set options emitted in spec order).</summary>
    [Fact]
    public void GlmTtsGolden()
    {
        GlmTtsCloneRequest request = new() { Text = "hello", ReferenceText = "x", MaxTokens = 1 };
        string json = Json.Serialize(new TaskRunRequest { Model = "glm_tts_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"glm_tts_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"reference_text\":\"x\",\"max_tokens\":\"1\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>HiggsAudioTtsCloneRequest</c> (set options emitted in spec order).</summary>
    [Fact]
    public void HiggsAudioTtsGolden()
    {
        HiggsAudioTtsCloneRequest request = new() { Text = "hello", StreamFramesPerEvent = 33 };
        string json = Json.Serialize(new TaskRunRequest { Model = "higgs_audio_tts_4b_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"higgs_audio_tts_4b_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"stream_frames_per_event\":\"33\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>IndexTts2CloneRequest</c> (set options emitted in spec order).</summary>
    [Fact]
    public void IndexTts2Golden()
    {
        IndexTts2CloneRequest request = new() { Text = "x" };
        string json = Json.Serialize(new TaskRunRequest { Model = "index_tts2_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"index_tts2_q8_0\",\"request\":{\"options\":{\"text\":\"x\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>IrodoriTtsCloneRequest</c> (set options emitted in spec order).</summary>
    [Fact]
    public void IrodoriTtsGolden()
    {
        IrodoriTtsCloneRequest request = new() { Text = "hello", StreamFramesPerEvent = 13 };
        string json = Json.Serialize(new TaskRunRequest { Model = "irodori_tts_v4_small_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"irodori_tts_v4_small_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"stream_frames_per_event\":\"13\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>KittenTts2CloneRequest</c> (set options emitted in spec order).</summary>
    [Fact]
    public void KittenTts2Golden()
    {
        KittenTts2CloneRequest request = new() { Text = "hello", Preset = KittenTts2Preset.Expressive };
        string json = Json.Serialize(new TaskRunRequest { Model = "kitten_tts2_20261021_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"kitten_tts2_20261021_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"preset\":\"expressive\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>Ming3AudioCloneRequest</c> (set options emitted in spec order).</summary>
    [Fact]
    public void Ming3AudioGolden()
    {
        Ming3AudioCloneRequest request = new() { Text = "hello", SystemPrompt = "x" };
        string json = Json.Serialize(new TaskRunRequest { Model = "ming3_audio_tiny_bf16", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"ming3_audio_tiny_bf16\",\"request\":{\"text\":\"hello\",\"options\":{\"system_prompt\":\"x\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>Ming3TtsCloneRequest</c> (set options emitted in spec order).</summary>
    [Fact]
    public void Ming3TtsGolden()
    {
        Ming3TtsCloneRequest request = new() { Text = "hello", TemplateName = Ming3TtsTemplateName.Tts };
        string json = Json.Serialize(new TaskRunRequest { Model = "ming_3_0_tts_0_5b_bf16", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"ming_3_0_tts_0_5b_bf16\",\"request\":{\"text\":\"hello\",\"options\":{\"template_name\":\"tts\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>MiottsCloneRequest</c> (set options emitted in spec order).</summary>
    [Fact]
    public void MiottsGolden()
    {
        MiottsCloneRequest request = new() { Text = "hello", MaxTokens = 1 };
        string json = Json.Serialize(new TaskRunRequest { Model = "miotts_1_7b_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"miotts_1_7b_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"max_tokens\":\"1\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>MiraTtsCloneRequest</c> (set options emitted in spec order).</summary>
    [Fact]
    public void MiraTtsGolden()
    {
        MiraTtsCloneRequest request = new() { Text = "hello", MaxTokens = 1025 };
        string json = Json.Serialize(new TaskRunRequest { Model = "mira_tts", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"mira_tts\",\"request\":{\"text\":\"hello\",\"options\":{\"max_tokens\":\"1025\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>MossTtsLocalCloneRequest</c> (set options emitted in spec order).</summary>
    [Fact]
    public void MossTtsLocalGolden()
    {
        MossTtsLocalCloneRequest request = new() { Text = "hello", MaxTokens = 4097 };
        string json = Json.Serialize(new TaskRunRequest { Model = "moss_tts_local_v1_5_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"moss_tts_local_v1_5_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"max_tokens\":\"4097\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>MossTtsNanoCloneRequest</c> (set options emitted in spec order).</summary>
    [Fact]
    public void MossTtsNanoGolden()
    {
        MossTtsNanoCloneRequest request = new() { Text = "hello", ReferenceText = "x" };
        string json = Json.Serialize(new TaskRunRequest { Model = "moss_tts_nano_100m_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"moss_tts_nano_100m_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"reference_text\":\"x\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>MossTtsV15CloneRequest</c> (set options emitted in spec order).</summary>
    [Fact]
    public void MossTtsV15Golden()
    {
        MossTtsV15CloneRequest request = new() { Text = "hello", MinFrames = 1 };
        string json = Json.Serialize(new TaskRunRequest { Model = "moss_tts_v15_q8_0_codec_f16", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"moss_tts_v15_q8_0_codec_f16\",\"request\":{\"text\":\"hello\",\"options\":{\"min_frames\":\"1\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>MossTtsdCloneRequest</c> (set options emitted in spec order).</summary>
    [Fact]
    public void MossTtsdGolden()
    {
        MossTtsdCloneRequest request = new() { Text = "hello", VoiceSamples = "x" };
        string json = Json.Serialize(new TaskRunRequest { Model = "moss_ttsd_q8_0_codec_f16", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"moss_ttsd_q8_0_codec_f16\",\"request\":{\"text\":\"hello\",\"options\":{\"voice_samples\":\"x\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>OmnivoiceCloneRequest</c> (set options emitted in spec order).</summary>
    [Fact]
    public void OmnivoiceGolden()
    {
        OmnivoiceCloneRequest request = new() { Text = "hello", Language = "x" };
        string json = Json.Serialize(new TaskRunRequest { Model = "omnivoice_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"omnivoice_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"language\":\"x\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>OutettsCloneRequest</c> (set options emitted in spec order).</summary>
    [Fact]
    public void OutettsGolden()
    {
        OutettsCloneRequest request = new() { Text = "hello", MaxTokens = 1 };
        string json = Json.Serialize(new TaskRunRequest { Model = "outetts_1_0_1b_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"outetts_1_0_1b_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"max_tokens\":\"1\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>PlapreCloneRequest</c> (set options emitted in spec order).</summary>
    [Fact]
    public void PlapreGolden()
    {
        PlapreCloneRequest request = new() { Text = "hello", ReferenceText = "x" };
        string json = Json.Serialize(new TaskRunRequest { Model = "plapre_3_bf16", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"plapre_3_bf16\",\"request\":{\"text\":\"hello\",\"options\":{\"reference_text\":\"x\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>PocketTtsCloneRequest</c> (set options emitted in spec order).</summary>
    [Fact]
    public void PocketTtsGolden()
    {
        PocketTtsCloneRequest request = new() { Text = "hello", FramesAfterEos = 0 };
        string json = Json.Serialize(new TaskRunRequest { Model = "pocket_tts_english_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"pocket_tts_english_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"frames_after_eos\":\"0\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>PocketTtsSmallCloneRequest</c> (set options emitted in spec order).</summary>
    [Fact]
    public void PocketTtsSmallGolden()
    {
        PocketTtsSmallCloneRequest request = new() { Text = "hello", FramesAfterEos = 0 };
        string json = Json.Serialize(new TaskRunRequest { Model = "pocket_tts_small_english_18m_refl_f32", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"pocket_tts_small_english_18m_refl_f32\",\"request\":{\"text\":\"hello\",\"options\":{\"frames_after_eos\":\"0\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>Qwen3TtsCloneRequest</c> (set options emitted in spec order).</summary>
    [Fact]
    public void Qwen3TtsGolden()
    {
        Qwen3TtsCloneRequest request = new() { Text = "hello", StreamChunkPolicy = Qwen3TtsStreamChunkPolicy.Fixed };
        string json = Json.Serialize(new TaskRunRequest { Model = "qwen3_tts_1_7b_base_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"qwen3_tts_1_7b_base_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"stream_chunk_policy\":\"fixed\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>SoproTtsCloneRequest</c> (set options emitted in spec order).</summary>
    [Fact]
    public void SoproTtsGolden()
    {
        SoproTtsCloneRequest request = new() { Text = "hello", Language = "x" };
        string json = Json.Serialize(new TaskRunRequest { Model = "sopro_v2_turbo_f16", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"sopro_v2_turbo_f16\",\"request\":{\"text\":\"hello\",\"options\":{\"language\":\"x\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>VieneuV3TurboCloneRequest</c> (set options emitted in spec order).</summary>
    [Fact]
    public void VieneuV3TurboGolden()
    {
        VieneuV3TurboCloneRequest request = new() { Text = "hello", AutoFrameCap = false };
        string json = Json.Serialize(new TaskRunRequest { Model = "vieneu_v3_turbo_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"vieneu_v3_turbo_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"auto_frame_cap\":\"false\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>Voxcpm1CloneRequest</c> (set options emitted in spec order).</summary>
    [Fact]
    public void Voxcpm1Golden()
    {
        Voxcpm1CloneRequest request = new() { Text = "hello", TextChunkMode = "x" };
        string json = Json.Serialize(new TaskRunRequest { Model = "voxcpm1_0_5b_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"voxcpm1_0_5b_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"text_chunk_mode\":\"x\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>Voxcpm2CloneRequest</c> (set options emitted in spec order).</summary>
    [Fact]
    public void Voxcpm2Golden()
    {
        Voxcpm2CloneRequest request = new() { Text = "hello", MinTokens = 3 };
        string json = Json.Serialize(new TaskRunRequest { Model = "voxcpm2_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"voxcpm2_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"min_tokens\":\"3\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>ZipvoiceCloneRequest</c> (set options emitted in spec order).</summary>
    [Fact]
    public void ZipvoiceGolden()
    {
        ZipvoiceCloneRequest request = new() { Text = "hello", ReferenceText = "x", TextChunkSize = 129 };
        string json = Json.Serialize(new TaskRunRequest { Model = "zipvoice_distill_gguf", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"zipvoice_distill_gguf\",\"request\":{\"text\":\"hello\",\"options\":{\"reference_text\":\"x\",\"text_chunk_size\":\"129\"}}}", json);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>Audio8TtsCloneRequest</c>.</summary>
    [Fact]
    public void Audio8TtsValidate()
    {
        Audio8TtsCloneRequest request = new() { MaxTokens = -1 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("audio8_tts clone max_tokens must be at least 0", ex.Message);
        Assert.Equal("MaxTokens", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>AukCloneRequest</c>.</summary>
    [Fact]
    public void AukValidate()
    {
        AukCloneRequest request = new() { NumInferenceSteps = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("auk clone num_inference_steps must be at least 1", ex.Message);
        Assert.Equal("NumInferenceSteps", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>BreezeTtsCloneRequest</c>.</summary>
    [Fact]
    public void BreezeTtsValidate()
    {
        BreezeTtsCloneRequest request = new() { TextChunkSize = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("breeze_tts clone text_chunk_size must be at least 1", ex.Message);
        Assert.Equal("TextChunkSize", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>ChatterboxCloneRequest</c>.</summary>
    [Fact]
    public void ChatterboxValidate()
    {
        ChatterboxCloneRequest request = new() { MaxTokens = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("chatterbox clone max_tokens must be at least 1", ex.Message);
        Assert.Equal("MaxTokens", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>Confucius4TtsCloneRequest</c>.</summary>
    [Fact]
    public void Confucius4TtsValidate()
    {
        Confucius4TtsCloneRequest request = new() { Temperature = -1 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("confucius4_tts clone temperature must be at least 0", ex.Message);
        Assert.Equal("Temperature", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>Cosyvoice3CloneRequest</c>.</summary>
    [Fact]
    public void Cosyvoice3Validate()
    {
        Cosyvoice3CloneRequest request = new() { TextChunkSize = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("cosyvoice3 clone text_chunk_size must be at least 1", ex.Message);
        Assert.Equal("TextChunkSize", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>DotsTtsCloneRequest</c>.</summary>
    [Fact]
    public void DotsTtsValidate()
    {
        DotsTtsCloneRequest request = new() { ReferenceDurationSec = -1 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("dots_tts clone reference_duration_sec must be at least 0", ex.Message);
        Assert.Equal("ReferenceDurationSec", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>DramaboxCloneRequest</c>.</summary>
    [Fact]
    public void DramaboxValidate()
    {
        DramaboxCloneRequest request = new() { DurationSec = -1 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("dramabox clone duration_sec must be at least 0", ex.Message);
        Assert.Equal("DurationSec", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>EchoTtsCloneRequest</c>.</summary>
    [Fact]
    public void EchoTtsValidate()
    {
        EchoTtsCloneRequest request = new() { TextChunkSize = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("echo_tts clone text_chunk_size must be at least 1", ex.Message);
        Assert.Equal("TextChunkSize", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>F5TtsCloneRequest</c>.</summary>
    [Fact]
    public void F5TtsValidate()
    {
        F5TtsCloneRequest request = new() { ReferenceText = "x", SwaySamplingCoef = -6 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("f5_tts clone sway_sampling_coef must be between -5 and 5", ex.Message);
        Assert.Equal("SwaySamplingCoef", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>FireredAudioCloneRequest</c>.</summary>
    [Fact]
    public void FireredAudioValidate()
    {
        FireredAudioCloneRequest request = new() { NumInferenceSteps = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("firered_audio clone num_inference_steps must be at least 1", ex.Message);
        Assert.Equal("NumInferenceSteps", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>Fireredtts3CloneRequest</c>.</summary>
    [Fact]
    public void Fireredtts3Validate()
    {
        Fireredtts3CloneRequest request = new() { TextChunkSize = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("fireredtts3 clone text_chunk_size must be at least 1", ex.Message);
        Assert.Equal("TextChunkSize", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>FishAudioCloneRequest</c>.</summary>
    [Fact]
    public void FishAudioValidate()
    {
        FishAudioCloneRequest request = new() { MaxTokens = -1 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("fish_audio clone max_tokens must be at least 0", ex.Message);
        Assert.Equal("MaxTokens", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>FishDrama3CloneRequest</c>.</summary>
    [Fact]
    public void FishDrama3Validate()
    {
        FishDrama3CloneRequest request = new() { MaxTokens = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("fish_drama3 clone max_tokens must be at least 1", ex.Message);
        Assert.Equal("MaxTokens", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>GlmTtsCloneRequest</c>.</summary>
    [Fact]
    public void GlmTtsValidate()
    {
        GlmTtsCloneRequest request = new() { ReferenceText = "x", NumInferenceSteps = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("glm_tts clone num_inference_steps must be at least 1", ex.Message);
        Assert.Equal("NumInferenceSteps", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>HiggsAudioTtsCloneRequest</c>.</summary>
    [Fact]
    public void HiggsAudioTtsValidate()
    {
        HiggsAudioTtsCloneRequest request = new() { StreamFramesPerEvent = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("higgs_audio_tts clone stream_frames_per_event must be at least 1", ex.Message);
        Assert.Equal("StreamFramesPerEvent", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>IndexTts2CloneRequest</c>.</summary>
    [Fact]
    public void IndexTts2Validate()
    {
        IndexTts2CloneRequest request = new() { EmotionAlpha = -1 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("index_tts2 clone emotion_alpha must be between 0 and 1", ex.Message);
        Assert.Equal("EmotionAlpha", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>IrodoriTtsCloneRequest</c>.</summary>
    [Fact]
    public void IrodoriTtsValidate()
    {
        IrodoriTtsCloneRequest request = new() { StreamFramesPerEvent = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("irodori_tts clone stream_frames_per_event must be at least 1", ex.Message);
        Assert.Equal("StreamFramesPerEvent", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>KittenTts2CloneRequest</c>.</summary>
    [Fact]
    public void KittenTts2Validate()
    {
        KittenTts2CloneRequest request = new() { Temperature = -1 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("kitten_tts2 clone temperature must be at least 0", ex.Message);
        Assert.Equal("Temperature", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>Ming3AudioCloneRequest</c>.</summary>
    [Fact]
    public void Ming3AudioValidate()
    {
        Ming3AudioCloneRequest request = new() { MaxTokens = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("ming3_audio clone max_tokens must be at least 1", ex.Message);
        Assert.Equal("MaxTokens", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>Ming3TtsCloneRequest</c>.</summary>
    [Fact]
    public void Ming3TtsValidate()
    {
        Ming3TtsCloneRequest request = new() { MaxTokens = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("ming3_tts clone max_tokens must be at least 1", ex.Message);
        Assert.Equal("MaxTokens", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>MiottsCloneRequest</c>.</summary>
    [Fact]
    public void MiottsValidate()
    {
        MiottsCloneRequest request = new() { MaxTokens = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("miotts clone max_tokens must be at least 1", ex.Message);
        Assert.Equal("MaxTokens", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>MiraTtsCloneRequest</c>.</summary>
    [Fact]
    public void MiraTtsValidate()
    {
        MiraTtsCloneRequest request = new() { MaxTokens = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("mira_tts clone max_tokens must be at least 1", ex.Message);
        Assert.Equal("MaxTokens", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>MossTtsLocalCloneRequest</c>.</summary>
    [Fact]
    public void MossTtsLocalValidate()
    {
        MossTtsLocalCloneRequest request = new() { MaxTokens = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("moss_tts_local clone max_tokens must be at least 1", ex.Message);
        Assert.Equal("MaxTokens", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>MossTtsNanoCloneRequest</c>.</summary>
    [Fact]
    public void MossTtsNanoValidate()
    {
        MossTtsNanoCloneRequest request = new() { MaxTokens = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("moss_tts_nano clone max_tokens must be at least 1", ex.Message);
        Assert.Equal("MaxTokens", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>MossTtsV15CloneRequest</c>.</summary>
    [Fact]
    public void MossTtsV15Validate()
    {
        MossTtsV15CloneRequest request = new() { TextChunkSize = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("moss_tts_v15 clone text_chunk_size must be at least 1", ex.Message);
        Assert.Equal("TextChunkSize", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>MossTtsdCloneRequest</c>.</summary>
    [Fact]
    public void MossTtsdValidate()
    {
        MossTtsdCloneRequest request = new() { Temperature = -1 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("moss_ttsd clone temperature must be at least 0", ex.Message);
        Assert.Equal("Temperature", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>OmnivoiceCloneRequest</c>.</summary>
    [Fact]
    public void OmnivoiceValidate()
    {
        OmnivoiceCloneRequest request = new() { NumInferenceSteps = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("omnivoice clone num_inference_steps must be at least 1", ex.Message);
        Assert.Equal("NumInferenceSteps", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>OutettsCloneRequest</c>.</summary>
    [Fact]
    public void OutettsValidate()
    {
        OutettsCloneRequest request = new() { MaxTokens = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("outetts clone max_tokens must be at least 1", ex.Message);
        Assert.Equal("MaxTokens", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>PlapreCloneRequest</c>.</summary>
    [Fact]
    public void PlapreValidate()
    {
        PlapreCloneRequest request = new() { EditStartFrame = -1 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("plapre clone edit_start_frame must be at least 0", ex.Message);
        Assert.Equal("EditStartFrame", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>PocketTtsCloneRequest</c>.</summary>
    [Fact]
    public void PocketTtsValidate()
    {
        PocketTtsCloneRequest request = new() { FramesAfterEos = -2 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("pocket_tts clone frames_after_eos must be at least -1", ex.Message);
        Assert.Equal("FramesAfterEos", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>PocketTtsSmallCloneRequest</c>.</summary>
    [Fact]
    public void PocketTtsSmallValidate()
    {
        PocketTtsSmallCloneRequest request = new() { FramesAfterEos = -2 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("pocket_tts_small clone frames_after_eos must be at least -1", ex.Message);
        Assert.Equal("FramesAfterEos", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>Qwen3TtsCloneRequest</c>.</summary>
    [Fact]
    public void Qwen3TtsValidate()
    {
        Qwen3TtsCloneRequest request = new() { StreamFramesPerEvent = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("qwen3_tts clone stream_frames_per_event must be at least 1", ex.Message);
        Assert.Equal("StreamFramesPerEvent", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>SoproTtsCloneRequest</c>.</summary>
    [Fact]
    public void SoproTtsValidate()
    {
        SoproTtsCloneRequest request = new() { NumInferenceSteps = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("sopro_tts clone num_inference_steps must be at least 1", ex.Message);
        Assert.Equal("NumInferenceSteps", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>VieneuV3TurboCloneRequest</c>.</summary>
    [Fact]
    public void VieneuV3TurboValidate()
    {
        VieneuV3TurboCloneRequest request = new() { MaxTokens = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("vieneu_v3_turbo clone max_tokens must be at least 1", ex.Message);
        Assert.Equal("MaxTokens", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>Voxcpm1CloneRequest</c>.</summary>
    [Fact]
    public void Voxcpm1Validate()
    {
        Voxcpm1CloneRequest request = new() { TextChunkSize = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("voxcpm1 clone text_chunk_size must be at least 1", ex.Message);
        Assert.Equal("TextChunkSize", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>Voxcpm2CloneRequest</c>.</summary>
    [Fact]
    public void Voxcpm2Validate()
    {
        Voxcpm2CloneRequest request = new() { MinTokens = -1 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("voxcpm2 clone min_tokens must be at least 0", ex.Message);
        Assert.Equal("MinTokens", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>ZipvoiceCloneRequest</c>.</summary>
    [Fact]
    public void ZipvoiceValidate()
    {
        ZipvoiceCloneRequest request = new() { ReferenceText = "x", TextChunkSize = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("zipvoice clone text_chunk_size must be at least 1", ex.Message);
        Assert.Equal("TextChunkSize", ex.ParamName);
    }
}