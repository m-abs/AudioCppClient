using System;
using System.Net.Http;
using System.Threading.Tasks;

using Mabs.AudioCpp.Interfaces;
using Mabs.AudioCpp.Models.Tasks;
using Mabs.AudioCpp.Models.Tts;
using Mabs.AudioCpp.Tts;

using Xunit;

namespace Mabs.AudioCpp.Tests.Convenience;

/// <summary>Transport check for the generated convenience clients through the fake handler (no network).</summary>
public class ConvenienceTransportTests
{
    /// <summary>A plain base request dispatched through <see cref="ITtsClient"/> posts the golden body to <c>/v1/tasks/run</c>.</summary>
    /// <returns>Completes after the assertions pass.</returns>
    [Fact]
    public async Task PolymorphicTtsClientPostsGoldenBodyToTasksRun()
    {
        using FakeHandler handler = new();
        handler.QueueJson("{\"text\":\"hi\"}");
        AudioCppClient client = new(new Uri("http://127.0.0.1:8092/"), handler.Client);
        ITtsClient tts = new PlapreTtsClient(client);
        TaskResult result = await tts.SpeakAsync(new TtsRequest { Text = "hi" });
        Assert.Equal("hi", result.Text);
        Assert.Equal(HttpMethod.Post, handler.Last.Method);
        Assert.Equal("http://127.0.0.1:8092/v1/tasks/run", handler.Last.Uri.AbsoluteUri);
        Assert.Equal("{\"model\":\"plapre_3_bf16\",\"request\":{\"text\":\"hi\"}}", handler.Last.ContentText());
    }
}