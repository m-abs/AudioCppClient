using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using mabs.AudioCpp.Http;

using mabs.AudioCpp.Models;

using Xunit;

namespace mabs.AudioCpp.Tests;

/// <summary>FakeItEasy-handler-driven transport tests: no real sockets.</summary>
public class TransportTests
{
    private static AudioCppClient NewClient(FakeHandler handler, string? apiKey = null)
    {
        return new AudioCppClient(new Uri("http://127.0.0.1:8092/"), handler.Client, apiKey);
    }

    [Fact]
    public async Task GetHealthUsesCorrectMethodAndPath()
    {
        using FakeHandler handler = new();
        handler.QueueJson("{\"status\":\"ok\",\"backend\":\"cpu\",\"models\":0,\"ui\":true,\"ui_management\":true,\"version\":\"0.9.3\",\"git\":\"41c1f8ff\",\"git_date\":\"2026-10-26\"}");
        AudioCppHealth health = await NewClient(handler).GetHealthAsync();
        Assert.Equal("ok", health.Status);
        Assert.Equal("cpu", health.Backend);
        Assert.Equal("0.9.3", health.Version);
        Assert.True(health.UiManagement);
        Assert.Equal(HttpMethod.Get, handler.Last.Method);
        Assert.Equal("http://127.0.0.1:8092/health", handler.Last.Uri.AbsoluteUri);
        Assert.Null(handler.Last.Authorization);
    }

    [Fact]
    public async Task AuthorizationHeaderOnlyWithApiKey()
    {
        using FakeHandler handler = new();
        handler.QueueJson("{\"voices\":[]}");
        await NewClient(handler, apiKey: "secret").ListVoicesAsync();
        Assert.Equal("Bearer secret", handler.Last.Authorization);

        using FakeHandler anonymous = new();
        anonymous.QueueJson("{\"voices\":[]}");
        await NewClient(anonymous).ListVoicesAsync();
        Assert.Null(anonymous.Last.Authorization);
    }

    [Fact]
    public async Task ListModelsQueryFlags()
    {
        using FakeHandler handler = new();
        handler.QueueJson("{\"object\":\"list\",\"data\":[]}");
        IReadOnlyList<AudioCppModel> models = await NewClient(handler).ListModelsAsync(includeParams: true, includeSessionOptions: true);
        Assert.Empty(models);
        string query = handler.Last.Uri.Query;
        Assert.Contains("include_params=true", query);
        Assert.Contains("include_session_options=true", query);
    }

    [Fact]
    public async Task ListModelsParsesParamsRows()
    {
        using FakeHandler handler = new();
        handler.QueueJson(
            "{\"object\":\"list\",\"data\":[{\"id\":\"tts\",\"object\":\"model\",\"configured\":true,\"device\":0," +
            "\"session_options\":{\"threads\":\"4\"}," +
            "\"params\":[{\"name\":\"voice_id\",\"type\":\"enum\",\"required\":false,\"values\":[\"af_bella\",\"am_adam\"],\"default\":\"af_bella\"}," +
            "{\"name\":\"max_tokens\",\"type\":\"int\",\"required\":false,\"min\":1,\"max\":2048}]}]}");
        IReadOnlyList<AudioCppModel> models = await NewClient(handler).ListModelsAsync(includeParams: true, includeSessionOptions: true);
        AudioCppModel model = Assert.Single(models);
        Assert.Equal("tts", model.Id);
        Assert.True(model.Configured);
        Assert.Equal(0, model.Device);
        Assert.Equal("4", model.SessionOptions!["threads"]);
        Assert.Equal(2, model.Params!.Count);
        Assert.Equal("voice_id", model.Params![0].Name);
        Assert.Equal(new[] { "af_bella", "am_adam" }, model.Params![0].Values);
        Assert.Equal("af_bella", model.Params![0].Default!.Value.GetString());
        Assert.Equal(1, model.Params![1].Min!.Value.GetInt32());
    }

    [Fact]
    public async Task UnloadReportsNotFound()
    {
        using FakeHandler handler = new();
        handler.QueueJson("{\"unloaded\":[],\"not_found\":[\"unknown-id\"]}");
        UnloadModelsResult result = await NewClient(handler).UnloadModelsAsync(new[] { "unknown-id" });
        Assert.Equal(HttpMethod.Post, handler.Last.Method);
        Assert.Equal("http://127.0.0.1:8092/v1/tasks/unload_models", handler.Last.Uri.AbsoluteUri);
        Assert.Contains("\"model_ids\":[\"unknown-id\"]", handler.Last.ContentText());
        Assert.Empty(result.Unloaded);
        Assert.Equal(new[] { "unknown-id" }, result.NotFound);
    }

    [Fact]
    public async Task UnloadAllSendsWithNoBody()
    {
        using FakeHandler handler = new();
        handler.QueueJson("{\"unloaded\":[\"a\"]}");
        UnloadModelsResult result = await NewClient(handler).UnloadAllModelsAsync();
        Assert.Null(handler.Last.Content);
        Assert.Equal(new[] { "a" }, result.Unloaded);
    }

    [Fact]
    public async Task ErrorEnvelopeThrowsAudioCppExceptionWithFields()
    {
        // v0.9.3 live behavior: unknown model on speech is HTTP 500 with server_error.
        using FakeHandler handler = new();
        handler.QueueJson("{\"error\":{\"message\":\"unknown model id: nope\",\"type\":\"server_error\"}}", HttpStatusCode.InternalServerError);
        AudioCppException ex = await Assert.ThrowsAsync<AudioCppException>(() =>
            NewClient(handler).Speech.SynthesizeAsync(new SpeechRequest { Model = "nope", Input = "hi" }));
        Assert.Equal("unknown model id: nope", ex.Message);
        Assert.Equal("server_error", ex.ErrorType);
        Assert.Equal(500, ex.StatusCode);
    }

    [Fact]
    public async Task BadRequest400InvalidRequestErrorType()
    {
        using FakeHandler handler = new();
        handler.QueueJson("{\"error\":{\"message\":\"audio alignment requests must use multipart/form-data\",\"type\":\"invalid_request_error\"}}", HttpStatusCode.BadRequest);
        AudioCppException ex = await Assert.ThrowsAsync<AudioCppException>(() =>
            NewClient(handler).SendJsonAsync(HttpMethod.Post, "v1/audio/alignments", w => { w.WriteStartObject(); w.WriteEndObject(); }, _ => new object(), CancellationToken.None));
        Assert.Equal(400, ex.StatusCode);
        Assert.Equal("invalid_request_error", ex.ErrorType);
    }

    [Fact]
    public async Task Busy503ServerErrorType()
    {
        using FakeHandler handler = new();
        handler.QueueJson("{\"error\":{\"message\":\"server_busy\",\"type\":\"server_busy\"}}", HttpStatusCode.ServiceUnavailable);
        AudioCppException ex = await Assert.ThrowsAsync<AudioCppException>(() =>
            NewClient(handler).GetHealthAsync());
        Assert.Equal(503, ex.StatusCode);
        Assert.Equal("server_busy", ex.ErrorType);
    }

    [Fact]
    public async Task NonJsonErrorBodyFallsBackToRawText()
    {
        using FakeHandler handler = new();
        handler.Queue(_ => new HttpResponseMessage(HttpStatusCode.BadGateway)
        {
            Content = new StringContent("gateway exploded"),
        });
        AudioCppException ex = await Assert.ThrowsAsync<AudioCppException>(() => NewClient(handler).GetHealthAsync());
        Assert.Equal("gateway exploded", ex.Message);
        Assert.Null(ex.ErrorType);
        Assert.Equal(502, ex.StatusCode);
    }

    [Fact]
    public async Task SynthesizeReturnsBytesContentTypeAndTimingHeaders()
    {
        byte[] wav = Encoding.ASCII.GetBytes("RIFF....WAVEfake");
        using FakeHandler handler = new();
        handler.QueueBytes(wav, "audio/wav", configure: response =>
        {
            response.Headers.TryAddWithoutValidation("X-AudioCPP-Wall-Ms", "412.700000");
            response.Headers.TryAddWithoutValidation("X-AudioCPP-Audio-Duration-Ms", "2400.000000");
            response.Headers.TryAddWithoutValidation("X-AudioCPP-RTF", "0.171958");
        });
        SpeechResult result = await NewClient(handler).Speech.SynthesizeAsync(
            new SpeechRequest { Model = "kokoro", Input = "hello" });
        Assert.Equal(wav, result.Audio);
        Assert.Equal("audio/wav", result.ContentType);
        Assert.Equal(412.7, result.Timing!.WallMs);
        Assert.Equal(2400.0, result.Timing!.AudioDurationMs);
        Assert.Equal(0.171958, result.Timing!.Rtf);
        Assert.Equal(HttpMethod.Post, handler.Last.Method);
        Assert.Equal("http://127.0.0.1:8092/v1/audio/speech", handler.Last.Uri.AbsoluteUri);
        Assert.Equal("application/json; charset=utf-8", handler.Last.ContentType);
        Assert.Contains("\"model\":\"kokoro\"", handler.Last.ContentText());
    }

    [Fact]
    public async Task SynthesizeJsonParsesEnvelope()
    {
        using FakeHandler handler = new();
        handler.QueueJson("{\"audio\":\"UklGRg==\",\"format\":\"wav\",\"timing\":{\"wall_ms\":10.0,\"audio_duration_ms\":20.0,\"rtf\":0.5}}");
        SpeechJsonResult result = await NewClient(handler).Speech.SynthesizeJsonAsync(
            new SpeechRequest { Model = "m", Input = "t" });
        Assert.Equal("UklGRg==", result.AudioBase64);
        Assert.Equal("wav", result.Format);
        Assert.Equal(0.5, result.Timing!.Rtf);
        Assert.Contains("\"response_format\":\"json\"", handler.Last.ContentText());
    }

    [Fact]
    public async Task TranscribeUsesJsonForPathAndMultipartForFile()
    {
        using FakeHandler handler = new();
        handler.QueueJson("{\"text\":\"hi\",\"timing\":{\"wall_ms\":1.0,\"audio_duration_ms\":2.0,\"rtf\":0.5}}");
        TranscriptionResult viaPath = await NewClient(handler).Transcriptions.TranscribeAsync(
            new TranscriptionRequest { Model = "asr", AudioPath = "/srv/a.wav" });
        Assert.Equal("hi", viaPath.Text);
        Assert.Contains("application/json", handler.Last.ContentType);
        Assert.Contains("\"audio\":\"/srv/a.wav\"", handler.Last.ContentText());

        handler.QueueJson("{\"text\":\"uploaded\",\"timing\":{\"wall_ms\":1.0}}");
        TranscriptionResult viaUpload = await NewClient(handler).Transcriptions.TranscribeAsync(
            new TranscriptionRequest { Model = "asr", AudioFile = new System.IO.MemoryStream(new byte[] { 1, 2, 3 }), AudioFileName = "a.wav" });
        Assert.Equal("uploaded", viaUpload.Text);
        Assert.Contains("multipart/form-data", handler.Last.ContentType);
        string body = handler.Last.ContentText();
        // .NET serializes form-data names unquoted (RFC 2231 filename* twin); the live
        // server parses both quoted and unquoted forms (verified against v0.9.3).
        Assert.Contains("name=file", body);
        Assert.Contains("filename=a.wav", body);
        Assert.Contains("name=model", body);
        Assert.Contains("asr", body);
        Assert.Equal("http://127.0.0.1:8092/v1/audio/transcriptions", handler.Last.Uri.AbsoluteUri);
    }

    [Fact]
    public async Task MultipartPartsCarryAllFields()
    {
        using FakeHandler handler = new();
        handler.QueueJson("{\"model\":\"al\",\"text\":\"\",\"words\":[],\"timing\":{\"wall_ms\":1}}");
        await NewClient(handler).Alignments.RunAsync(new AlignmentRequest
        {
            Model = "al",
            Text = "hello there",
            Language = "English",
            AudioFile = new byte[] { 9 },
            AudioFileName = "in.wav",
        });
        string body = handler.Last.ContentText();
        foreach (string needle in new[] { "name=file", "filename=in.wav", "name=model", "name=text", "name=language", "hello there", "English" })
        {
            Assert.Contains(needle, body);
        }
    }

    [Fact]
    public async Task StreamSseRequestsAcceptHeaderAndParsesEvents()
    {
        using FakeHandler handler = new();
        handler.QueueSse(
            "data: {\"type\":\"speech.audio.delta\",\"audio\":\"AAAA\"}\n\n" +
            "data: {\"type\":\"speech.audio.done\",\"timing\":{\"ttft_ms\":1}}\n\n" +
            "data: [DONE]\n\n");
        List<SpeechStreamEvent> events = await NewClient(handler).Speech.StreamAsync(
            new SpeechStreamRequest { Request = new SpeechRequest { Model = "stream-tts", Input = "hi" } })
            .ToListAsync();
        Assert.Equal(2, events.Count);
        Assert.Equal(SpeechStreamEventKind.AudioDelta, events[0].Kind);
        // Response format defaulted to pcm for streaming.
        Assert.Contains("\"response_format\":\"pcm\"", handler.Last.ContentText());
        Assert.Contains("\"stream_format\":\"sse\"", handler.Last.ContentText());
    }

    [Fact]
    public async Task TaskStreamJsonModeReplaysEventsAndDone()
    {
        using FakeHandler handler = new();
        handler.QueueJson("{\"events\":[{\"partial_text\":{\"text\":\"Sure\",\"language\":\"en\"},\"is_final\":false},{\"is_final\":true}],\"result\":{\"text\":\"Sure!\",\"timing\":{\"ttft_ms\":5.0}}}");
        List<TaskStreamEvent> events = await NewClient(handler).Tasks.StreamAsync(
            new TaskStreamRequest { Model = "m", Request = new TaskRequestBody { Text = "t" }, StreamFormat = null })
            .ToListAsync();
        Assert.Equal(3, events.Count);
        Assert.Equal("Sure", events[0].Event!.PartialText);
        Assert.True(events[1].Event!.IsFinal);
        Assert.Equal(TaskStreamEventKind.Done, events[2].Kind);
        Assert.Equal("Sure!", events[2].Result!.Text);
        Assert.DoesNotContain("\"stream_format\"", handler.Last.ContentText(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task BatchTranscriptionRepeatsFileParts()
    {
        using FakeHandler handler = new();
        handler.QueueSse(
            "data: {\"type\":\"batch.transcription.result\",\"index\":0,\"filename\":\"a.wav\",\"text\":\"A\"}\n\n" +
            "data: {\"type\":\"batch.transcription.result\",\"index\":1,\"filename\":\"b.wav\",\"text\":\"B\"}\n\n" +
            "data: {\"type\":\"batch.transcription.done\",\"result_count\":2,\"timing\":{\"wall_ms\":3}}\n\n" +
            "data: [DONE]\n\n");
        BatchTranscriptionRequest request = new() { Model = "batch", Language = "en" };
        request.Files.Add(new AudioUpload("a.wav", Encoding.ASCII.GetBytes("RIFFaaaa")));
        request.Files.Add(new AudioUpload("b.wav", Encoding.ASCII.GetBytes("RIFFbbbb")));
        List<BatchTranscriptionEvent> events = await NewClient(handler).Transcriptions.TranscribeBatchAsync(request).ToListAsync();
        Assert.Equal(3, events.Count);
        Assert.Equal("A", events[0].Text);
        string body = handler.Last.ContentText();
        Assert.Contains("filename=a.wav", body);
        Assert.Contains("filename=b.wav", body);
        Assert.Contains("name=language", body);
        Assert.Equal("http://127.0.0.1:8092/v1/batches/transcriptions", handler.Last.Uri.AbsoluteUri);
    }

    [Fact]
    public async Task LiveRoutesPostChunkedPcmWithQuery()
    {
        using FakeHandler handler = new();
        handler.QueueSse(
            "data: {\"type\":\"transcript.text.delta\",\"delta\":\"hi\"}\n\n" +
            "data: {\"type\":\"transcript.text.done\",\"text\":\"hi\",\"timing\":{\"ttft_ms\":2}}\n\n" +
            "data: [DONE]\n\n");
        using System.IO.MemoryStream pcm = new(new byte[3200]);
        List<TranscriptEvent> events = await NewClient(handler).Transcriptions.TranscribeLiveAsync(
            new LiveTranscriptionRequest { Model = "stream-asr", SampleRate = 16000, Channels = 1 }, pcm)
            .ToListAsync();
        Assert.Equal(2, events.Count);
        Assert.Equal("application/octet-stream", handler.Last.ContentType);
        string query = handler.Last.Uri.Query;
        Assert.Contains("model=stream-asr", query);
        Assert.Contains("sample_rate=16000", query);
        Assert.Equal(3200, handler.Last.Content!.Length);
    }

    [Fact]
    public async Task SseErrorEventThrowsDuringStreaming()
    {
        using FakeHandler handler = new();
        handler.QueueSse(
            "data: {\"type\":\"task.stream.event\",\"event\":{\"is_final\":false}}\n\n" +
            "data: {\"type\":\"error\",\"error\":{\"message\":\"model failed mid-run\"}}\n");
        await Assert.ThrowsAsync<AudioCppException>(async () =>
        {
            await foreach (TaskStreamEvent evt in NewClient(handler).Tasks.StreamAsync(
                new TaskStreamRequest { Model = "m", Request = new TaskRequestBody { Text = "x" } }))
            {
                _ = evt;
            }
        });
    }

    [Fact]
    public async Task VoicesQueryIncludesModelWhenGiven()
    {
        using FakeHandler handler = new();
        handler.QueueJson("{\"voices\":[\"demo_1_man\",\"demo_2_man\"]}");
        IReadOnlyList<string> voices = await NewClient(handler).ListVoicesAsync("tts-model");
        Assert.Equal(new[] { "demo_1_man", "demo_2_man" }, voices);
        Assert.Contains("model=tts-model", handler.Last.Uri.Query);
    }

    [Fact]
    public async Task RecipesListParsesRoles()
    {
        using FakeHandler handler = new();
        handler.QueueJson("{\"object\":\"list\",\"data\":[{\"id\":\"any-tts\",\"description\":\"d\",\"roles\":[{\"name\":\"tts\",\"task\":\"tts\",\"mode\":\"offline\"},{\"name\":\"converter\",\"task\":\"vc\",\"mode\":\"offline\"}]}]}");
        IReadOnlyList<RecipeEntry> recipes = await NewClient(handler).ListRecipesAsync();
        RecipeEntry recipe = Assert.Single(recipes);
        Assert.Equal("any-tts", recipe.Id);
        Assert.Equal(2, recipe.Roles.Count);
        Assert.Equal("converter", recipe.Roles[1].Name);
        Assert.Equal("vc", recipe.Roles[1].Task);
    }

    [Fact]
    public async Task LoadModel403ForbiddenSurfaces()
    {
        using FakeHandler handler = new();
        handler.QueueJson("{\"error\":{\"message\":\"dynamic model management is disabled\",\"type\":\"forbidden\"}}", HttpStatusCode.Forbidden);
        AudioCppException ex = await Assert.ThrowsAsync<AudioCppException>(() =>
            NewClient(handler).LoadModelAsync(new ModelLoadRequest { Id = "x", Path = "/a.gguf", Family = "kokoro_tts", Task = "tts" }));
        Assert.Equal(403, ex.StatusCode);
        Assert.Equal("forbidden", ex.ErrorType);
    }

    [Fact]
    public void BaseUrlWithoutTrailingSlashStillResolvesRelativePaths()
    {
        AudioCppClient client = new("http://host:1/path");
        Assert.Equal("http://host:1/path/v1/models", client.BuildUri("v1/models").AbsoluteUri);
    }

    [Fact]
    public async Task StreamPcmForcesAudioFormatAndReturnsRawBytes()
    {
        byte[] pcm = new byte[] { 1, 2, 3, 4 };
        using FakeHandler handler = new();
        handler.QueueBytes(pcm, "application/octet-stream");
        System.IO.Stream stream = await NewClient(handler).Speech.StreamPcmAsync(
            new SpeechStreamRequest { Request = new SpeechRequest { Model = "m", Input = "hi" } });
        using (stream)
        {
            using System.IO.MemoryStream sink = new();
            await stream.CopyToAsync(sink);
            Assert.Equal(pcm, sink.ToArray());
        }
        Assert.Contains("\"stream_format\":\"audio\"", handler.Last.ContentText());
        Assert.Contains("\"response_format\":\"pcm\"", handler.Last.ContentText());
    }

    [Fact]
    public async Task SpeakLivePostsChunkedPcmAndParsesEvents()
    {
        using FakeHandler handler = new();
        handler.QueueSse("data: {\"type\":\"speech.text.delta\",\"delta\":\"hi\"}\n\ndata: [DONE]\n\n");
        using System.IO.MemoryStream pcm = new(new byte[] { 5, 6, 7 });
        List<SpeechStreamEvent> events = await NewClient(handler).Speech.SpeakLiveAsync(
            new LiveSpeechRequest { Model = "live-tts" }, pcm).ToListAsync();
        Assert.Equal(SpeechStreamEventKind.TextDelta, Assert.Single(events).Kind);
        Assert.Contains("/v1/audio/speech/live", handler.Last.Uri.AbsoluteUri);
        Assert.Contains("model=live-tts", handler.Last.Uri.Query);
        Assert.Equal("application/octet-stream", handler.Last.ContentType);
        Assert.Equal(new byte[] { 5, 6, 7 }, handler.Last.Content);
    }

    [Fact]
    public async Task TranscribeStreamParsesEventsOnJsonPath()
    {
        using FakeHandler handler = new();
        handler.QueueSse("data: {\"type\":\"transcript.text.delta\",\"delta\":\"hi\"}\n\ndata: [DONE]\n\n");
        List<TranscriptEvent> events = await NewClient(handler).Transcriptions.TranscribeStreamAsync(
            new TranscriptionRequest { Model = "asr", AudioPath = "/srv/a.wav" }).ToListAsync();
        Assert.Equal("hi", Assert.Single(events).Text);
        Assert.Contains("\"audio\":\"/srv/a.wav\"", handler.Last.ContentText());
    }

    [Fact]
    public void EmptyAudioPathFailsValidation()
    {
        Assert.Throws<ArgumentException>(() => new TranscriptionRequest { Model = "asr", AudioPath = "" }.Validate());
    }

    [Fact]
    public async Task LiveBodyCopyStopsOnCancellationMidBody()
    {
        using CancellationTokenSource cts = new();
        using CancelAfterFirstRead source = new(cts);
        AudioCppClient.ChunkedStreamContent content = new(source, cts.Token);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => content.CopyToAsync(new System.IO.MemoryStream(), cts.Token));
    }

    private sealed class CancelAfterFirstRead : System.IO.MemoryStream
    {
        private readonly CancellationTokenSource _cts;
        private int _reads;

        public CancelAfterFirstRead(CancellationTokenSource cts) : base(new byte[] { 1, 2, 3 })
        {
            _cts = cts;
        }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken)
        {
            int read = await base.ReadAsync(buffer, cancellationToken);
            if (++_reads == 1)
            {
                _cts.Cancel();
            }

            return read;
        }
    }
}

internal static class AsyncEnumerableTestExtensions
{
    internal static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source)
    {
        List<T> items = new();
        await foreach (T item in source)
        {
            items.Add(item);
        }

        return items;
    }
}