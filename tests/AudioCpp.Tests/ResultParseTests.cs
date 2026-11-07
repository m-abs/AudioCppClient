using System.Text.Json;

using Mabs.AudioCpp.Models.Alignment;
using Mabs.AudioCpp.Models.Speech;
using Mabs.AudioCpp.Models.Tasks;
using Mabs.AudioCpp.Models.Transcription;

using Xunit;

namespace Mabs.AudioCpp.Tests;

/// <summary>Public parse entry points that rebuild a cached response JSON (issue #2).</summary>
public class ResultParseTests
{
    /// <summary>Alignment result rebuilds from stored JSON with words and timing.</summary>
    [Fact]
    public void AlignmentRebuildsFromCachedJson()
    {
        AlignmentResult result = AlignmentResult.Parse(
            "{\"text\":\"hello world\",\"language\":\"en\",\"words\":[{\"word\":\"hello\",\"start\":0.0,\"end\":0.5,\"start_sample\":0,\"end_sample\":12000,\"confidence\":0.9}],\"timing\":{\"wall_ms\":120.0,\"audio_duration_ms\":1000.0,\"rtf\":0.12}}");
        Assert.Equal("hello world", result.Text);
        Assert.Equal("hello", result.Words[0].Word);
        Assert.Equal(12000L, result.Words[0].EndSample);
        Assert.Equal(0.12, result.Timing!.Rtf);
    }

    /// <summary>Transcription result and details result rebuild, keeping the detail arrays.</summary>
    [Fact]
    public void TranscriptionAndDetailsRebuildFromCachedJson()
    {
        TranscriptionResult plain = TranscriptionResult.Parse("{\"text\":\"hi\",\"timing\":{\"wall_ms\":42.0}}");
        Assert.Equal("hi", plain.Text);
        Assert.Equal(42.0, plain.Timing!.WallMs);

        TranscriptionDetailsResult details = TranscriptionDetailsResult.Parse(
            "{\"text\":\"hi\",\"language\":\"da\",\"segments\":[{\"start_sample\":0,\"end_sample\":8000,\"text\":\"hi\"}],\"words\":[{\"word\":\"hi\",\"start_sample\":0,\"end_sample\":8000}],\"sample_rate\":16000}");
        Assert.Equal("da", details.Language);
        Assert.Equal(8000L, details.Segments![0].EndSample);
        Assert.Equal(16000, details.SampleRate);
        Assert.Null(details.Timing);
    }

    /// <summary>Task result rebuilds, decoding Base64 audio, named outputs and artifacts.</summary>
    [Fact]
    public void TaskResultRebuildsFromCachedJson()
    {
        TaskResult result = TaskResult.Parse(
            "{\"text\":\"Sure!\",\"audio\":\"AQID\",\"sample_rate\":24000,\"channels\":1,\"named_audio_outputs\":[{\"id\":\"vocal\",\"audio\":\"BAU=\",\"sample_rate\":24000}],\"artifacts\":[{\"id\":\"a1\",\"kind\":\"speaker_embedding\",\"payload\":\"Bgc=\",\"meta\":{\"dim\":\"192\"}}],\"timing\":{\"ttft_ms\":5.0}}");
        Assert.Equal("Sure!", result.Text);
        Assert.Equal(new byte[] { 1, 2, 3 }, result.Audio);
        Assert.Equal(new byte[] { 4, 5 }, result.NamedAudioOutputs![0].Audio);
        Assert.Equal("speaker_embedding", result.Artifacts![0].Kind);
        Assert.Equal(new byte[] { 6, 7 }, result.Artifacts![0].Payload);
        Assert.Equal("192", result.Artifacts![0].Meta!["dim"]);
        Assert.True(result.Timing!.TtftPresent);
    }

    /// <summary>Speech JSON result rebuilds and still decodes its Base64 audio.</summary>
    [Fact]
    public void SpeechJsonRebuildsFromCachedJson()
    {
        SpeechJsonResult result = SpeechJsonResult.Parse("{\"audio\":\"AQID\",\"format\":\"wav\",\"timing\":{\"wall_ms\":90.0,\"rtf\":0.9}}");
        Assert.Equal("AQID", result.AudioBase64);
        Assert.Equal(new byte[] { 1, 2, 3 }, result.Audio);
        Assert.Equal("wav", result.Format);
    }

    /// <summary>A cached payload that is not JSON throws a JsonException rather than returning an empty result.</summary>
    [Fact]
    public void InvalidCachedJsonThrows()
    {
        Assert.ThrowsAny<JsonException>(() => AlignmentResult.Parse("not json"));
    }

    /// <summary>A null body or valid-but-not-object JSON throws a JsonException instead of returning an empty result.</summary>
    [Fact]
    public void NullAndNonObjectRootThrow()
    {
        Assert.Throws<JsonException>(() => TaskResult.Parse("[1,2]"));
        Assert.Throws<JsonException>(() => AlignmentResult.Parse(null));
    }
}