using System;

using Mabs.AudioCpp.Enums;
using Mabs.AudioCpp.Models.Design;
using Mabs.AudioCpp.Models.Tasks;
using Mabs.AudioCpp.Utils;

using Xunit;

namespace Mabs.AudioCpp.Tests.Convenience;

/// <summary>Golden serialization, validation and composition tests for the generated design convenience records.</summary>
public class DesignGoldenTests
{
    /// <summary>Golden serialization for the generated <c>BreezeTtsDesignRequest</c> (set options emitted in spec order).</summary>
    [Fact]
    public void BreezeTtsGolden()
    {
        BreezeTtsDesignRequest request = new() { Text = "hello", Instruction = "x" };
        string json = Json.Serialize(new TaskRunRequest { Model = "breeze_tts_2_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"breeze_tts_2_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"instruct\":\"x\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>FireredAudioDesignRequest</c> (set options emitted in spec order).</summary>
    [Fact]
    public void FireredAudioGolden()
    {
        FireredAudioDesignRequest request = new() { Text = "hello", TemplateName = FireredAudioTemplateName.TtsClone };
        string json = Json.Serialize(new TaskRunRequest { Model = "firered_audio_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"firered_audio_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"template_name\":\"tts_clone\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>Fireredtts3DesignRequest</c> (set options emitted in spec order).</summary>
    [Fact]
    public void Fireredtts3Golden()
    {
        Fireredtts3DesignRequest request = new() { Text = "hello", TemplateName = Fireredtts3TemplateName.InstructTts };
        string json = Json.Serialize(new TaskRunRequest { Model = "fireredtts3_instruct_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"fireredtts3_instruct_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"template_name\":\"instruct_tts\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>IrodoriTtsDesignRequest</c> (set options emitted in spec order).</summary>
    [Fact]
    public void IrodoriTtsGolden()
    {
        IrodoriTtsDesignRequest request = new() { Text = "hello", StreamFramesPerEvent = 13 };
        string json = Json.Serialize(new TaskRunRequest { Model = "irodori_tts_v4_small_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"irodori_tts_v4_small_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"stream_frames_per_event\":\"13\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>Maya1DesignRequest</c> (set options emitted in spec order).</summary>
    [Fact]
    public void Maya1Golden()
    {
        Maya1DesignRequest request = new() { Text = "hello", Instruction = "x", MaxTokens = 2049 };
        string json = Json.Serialize(new TaskRunRequest { Model = "maya1_orig", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"maya1_orig\",\"request\":{\"text\":\"hello\",\"options\":{\"instruct\":\"x\",\"max_tokens\":\"2049\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>Ming3TtsDesignRequest</c> (set options emitted in spec order).</summary>
    [Fact]
    public void Ming3TtsGolden()
    {
        Ming3TtsDesignRequest request = new() { Text = "hello", TemplateName = Ming3TtsTemplateName.Tts };
        string json = Json.Serialize(new TaskRunRequest { Model = "ming_3_0_tts_0_5b_bf16", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"ming_3_0_tts_0_5b_bf16\",\"request\":{\"text\":\"hello\",\"options\":{\"template_name\":\"tts\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>MossVoicegenDesignRequest</c> (set options emitted in spec order).</summary>
    [Fact]
    public void MossVoicegenGolden()
    {
        MossVoicegenDesignRequest request = new() { Text = "hello", MinFrames = 1 };
        string json = Json.Serialize(new TaskRunRequest { Model = "moss_voicegen_bf16_codec_f16_decode", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"moss_voicegen_bf16_codec_f16_decode\",\"request\":{\"text\":\"hello\",\"options\":{\"min_frames\":\"1\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>OmnivoiceDesignRequest</c> (set options emitted in spec order).</summary>
    [Fact]
    public void OmnivoiceGolden()
    {
        OmnivoiceDesignRequest request = new() { Text = "hello", Language = "x" };
        string json = Json.Serialize(new TaskRunRequest { Model = "omnivoice_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"omnivoice_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"language\":\"x\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>Qwen3TtsDesignRequest</c> (set options emitted in spec order).</summary>
    [Fact]
    public void Qwen3TtsGolden()
    {
        Qwen3TtsDesignRequest request = new() { Text = "hello", StreamChunkPolicy = Qwen3TtsStreamChunkPolicy.Fixed };
        string json = Json.Serialize(new TaskRunRequest { Model = "qwen3_tts_1_7b_base_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"qwen3_tts_1_7b_base_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"stream_chunk_policy\":\"fixed\"}}}", json);
    }

    /// <summary>Golden serialization for the generated <c>Voxcpm2DesignRequest</c> (set options emitted in spec order).</summary>
    [Fact]
    public void Voxcpm2Golden()
    {
        Voxcpm2DesignRequest request = new() { Text = "hello", MinTokens = 3 };
        string json = Json.Serialize(new TaskRunRequest { Model = "voxcpm2_q8_0", Request = request.ToTaskRequestBody() });
        Assert.Equal("{\"model\":\"voxcpm2_q8_0\",\"request\":{\"text\":\"hello\",\"options\":{\"min_tokens\":\"3\"}}}", json);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>BreezeTtsDesignRequest</c>.</summary>
    [Fact]
    public void BreezeTtsValidate()
    {
        BreezeTtsDesignRequest request = new() { TextChunkSize = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("breeze_tts design text_chunk_size must be at least 1", ex.Message);
        Assert.Equal("TextChunkSize", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>FireredAudioDesignRequest</c>.</summary>
    [Fact]
    public void FireredAudioValidate()
    {
        FireredAudioDesignRequest request = new() { NumInferenceSteps = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("firered_audio design num_inference_steps must be at least 1", ex.Message);
        Assert.Equal("NumInferenceSteps", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>Fireredtts3DesignRequest</c>.</summary>
    [Fact]
    public void Fireredtts3Validate()
    {
        Fireredtts3DesignRequest request = new() { TextChunkSize = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("fireredtts3 design text_chunk_size must be at least 1", ex.Message);
        Assert.Equal("TextChunkSize", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>IrodoriTtsDesignRequest</c>.</summary>
    [Fact]
    public void IrodoriTtsValidate()
    {
        IrodoriTtsDesignRequest request = new() { StreamFramesPerEvent = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("irodori_tts design stream_frames_per_event must be at least 1", ex.Message);
        Assert.Equal("StreamFramesPerEvent", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>Maya1DesignRequest</c>.</summary>
    [Fact]
    public void Maya1Validate()
    {
        Maya1DesignRequest request = new() { Instruction = "x", MaxTokens = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("maya1 design max_tokens must be at least 1", ex.Message);
        Assert.Equal("MaxTokens", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>Ming3TtsDesignRequest</c>.</summary>
    [Fact]
    public void Ming3TtsValidate()
    {
        Ming3TtsDesignRequest request = new() { MaxTokens = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("ming3_tts design max_tokens must be at least 1", ex.Message);
        Assert.Equal("MaxTokens", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>MossVoicegenDesignRequest</c>.</summary>
    [Fact]
    public void MossVoicegenValidate()
    {
        MossVoicegenDesignRequest request = new() { TextChunkSize = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("moss_voicegen design text_chunk_size must be at least 1", ex.Message);
        Assert.Equal("TextChunkSize", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>OmnivoiceDesignRequest</c>.</summary>
    [Fact]
    public void OmnivoiceValidate()
    {
        OmnivoiceDesignRequest request = new() { NumInferenceSteps = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("omnivoice design num_inference_steps must be at least 1", ex.Message);
        Assert.Equal("NumInferenceSteps", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>Qwen3TtsDesignRequest</c>.</summary>
    [Fact]
    public void Qwen3TtsValidate()
    {
        Qwen3TtsDesignRequest request = new() { StreamFramesPerEvent = 0 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("qwen3_tts design stream_frames_per_event must be at least 1", ex.Message);
        Assert.Equal("StreamFramesPerEvent", ex.ParamName);
    }

    /// <summary>An out-of-range value fails validation for the generated <c>Voxcpm2DesignRequest</c>.</summary>
    [Fact]
    public void Voxcpm2Validate()
    {
        Voxcpm2DesignRequest request = new() { MinTokens = -1 };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("voxcpm2 design min_tokens must be at least 0", ex.Message);
        Assert.Equal("MinTokens", ex.ParamName);
    }

    /// <summary>The generated <c>Voxcpm2DesignRequest</c> has no instruction option, so the instruction prefixes the text.</summary>
    [Fact]
    public void Voxcpm2Prefix()
    {
        Voxcpm2DesignRequest request = new() { Text = "hello", Instruction = "warm" };
        TaskRequestBody body = request.ToTaskRequestBody();
        Assert.Equal("(warm) hello", body.Text);
        Assert.Null(body.Options);
    }
}