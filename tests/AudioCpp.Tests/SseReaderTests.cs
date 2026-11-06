using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using mabs.AudioCpp.Http;
using mabs.AudioCpp.Models;

using Xunit;

namespace mabs.AudioCpp.Tests;

/// <summary>SseReader fixtures for every event family the server writes.</summary>
public class SseReaderTests
{
    private static SseReader Reader(string sse) => new(new MemoryStream(Encoding.UTF8.GetBytes(sse)));

    private static async Task<List<SseEvent>> ReadAllAsync(string sse)
    {
        List<SseEvent> events = new();
        await Reader(sse).ReadEventsAsync(evt =>
        {
            events.Add(evt);
            return Task.CompletedTask;
        });
        return events;
    }

    [Fact]
    public async Task SpeechTextAndAudioInterleave()
    {
        string sse =
            "data: {\"type\":\"speech.text.delta\",\"delta\":\"Sure! How about\"}\n\n" +
            "data: {\"type\":\"speech.audio.delta\",\"audio\":\"AAAA\"}\n\n" +
            "data: {\"type\":\"speech.text.done\",\"text\":\"Sure! How about\"}\n\n" +
            "data: {\"type\":\"speech.audio.done\",\"timing\":{\"ttft_ms\":12.5}}\n\n" +
            "data: [DONE]\n\n";
        List<SseEvent> events = await ReadAllAsync(sse);
        Assert.Equal(4, events.Count);
        List<SpeechStreamEvent> parsed = events.Select(e => SpeechStreamEvent.Parse(e.Data)).ToList();
        Assert.Equal(SpeechStreamEventKind.TextDelta, parsed[0].Kind);
        Assert.Equal("Sure! How about", parsed[0].Text);
        Assert.Equal(SpeechStreamEventKind.AudioDelta, parsed[1].Kind);
        Assert.Equal(new byte[] { 0, 0, 0 }, parsed[1].Pcm);
        Assert.Equal(SpeechStreamEventKind.TextDone, parsed[2].Kind);
        Assert.Equal(SpeechStreamEventKind.AudioDone, parsed[3].Kind);
        Assert.Equal(12.5, parsed[3].Timing!.TtftMs);
    }

    [Fact]
    public async Task TranscriptionDeltaPartialDone()
    {
        string sse =
            "data: {\"type\":\"transcript.text.delta\",\"delta\":\"hello\"}\n\n" +
            "data: {\"type\":\"transcript.text.partial\",\"text\":\"hello world\"}\n\n" +
            "data: {\"type\":\"transcript.text.done\",\"text\":\"hello world\",\"timing\":{\"ttft_ms\":3606}}\n\n" +
            "data: [DONE]\n\n";
        List<SseEvent> events = await ReadAllAsync(sse);
        Assert.Equal(3, events.Count);
        Assert.Equal(TranscriptEventKind.TextDelta, TranscriptEvent.Parse(events[0].Data).Kind);
        TranscriptEvent partial = TranscriptEvent.Parse(events[1].Data);
        Assert.Equal(TranscriptEventKind.TextPartial, partial.Kind);
        Assert.Equal("hello world", partial.Text);
        TranscriptEvent done = TranscriptEvent.Parse(events[2].Data);
        Assert.Equal(TranscriptEventKind.TextDone, done.Kind);
        Assert.Equal(3606, done.Timing!.TtftMs);
    }

    [Fact]
    public async Task DiarizationDeltaAndDone()
    {
        string sse =
            "data: {\"type\":\"diarization.delta\",\"speaker_turns\":[{\"start_sample\":5760,\"end_sample\":52480,\"speaker_id\":\"speaker_0\",\"confidence\":0.99}],\"sample_rate\":16000}\n\n" +
            "data: {\"type\":\"diarization.done\",\"speaker_turns\":[],\"sample_rate\":16000,\"timing\":{\"ttft_ms\":null}}\n\n" +
            "data: [DONE]\n\n";
        List<SseEvent> events = await ReadAllAsync(sse);
        TranscriptEvent delta = TranscriptEvent.Parse(events[0].Data);
        Assert.Equal(TranscriptEventKind.DiarizationDelta, delta.Kind);
        Assert.Single(delta.SpeakerTurns!);
        Assert.Equal("speaker_0", delta.SpeakerTurns![0].SpeakerId);
        Assert.Equal(16000, delta.SampleRate);
        TranscriptEvent done = TranscriptEvent.Parse(events[1].Data);
        Assert.Equal(TranscriptEventKind.DiarizationDone, done.Kind);

        // ttft_ms explicit null: present-but-null is distinguishable.
        using System.Text.Json.JsonDocument doc = System.Text.Json.JsonDocument.Parse(events[1].Data);
        AudioCppTiming? timing = SseReader.ReadTiming(doc.RootElement, "timing");
        Assert.Null(timing!.TtftMs);
        Assert.False(timing.TtftPresent);
    }

    [Fact]
    public async Task BatchResultAndDone()
    {
        string sse =
            "data: {\"type\":\"batch.transcription.result\",\"index\":0,\"filename\":\"meeting-a.wav\",\"text\":\"hi\",\"sample_rate\":16000}\n\n" +
            "data: {\"type\":\"batch.transcription.result\",\"index\":1,\"filename\":\"meeting-b.wav\",\"text\":\"\"}\n\n" +
            "data: {\"type\":\"batch.transcription.done\",\"result_count\":2,\"timing\":{\"wall_ms\":145.5,\"audio_duration_ms\":70000.0,\"rtf\":0.0021}}\n\n" +
            "data: [DONE]\n\n";
        List<SseEvent> events = await ReadAllAsync(sse);
        Assert.Equal(3, events.Count);
        BatchTranscriptionEvent first = BatchTranscriptionEvent.Parse(events[0].Data);
        Assert.Equal(BatchTranscriptionEventKind.Result, first.Kind);
        Assert.Equal(0, first.Index);
        Assert.Equal("meeting-a.wav", first.Filename);
        BatchTranscriptionEvent last = BatchTranscriptionEvent.Parse(events[2].Data);
        Assert.Equal(BatchTranscriptionEventKind.Done, last.Kind);
        Assert.Equal(2, last.ResultCount);
        Assert.Equal(0.0021, last.Timing!.Rtf);
    }

    [Fact]
    public async Task TaskStreamEventAndDone()
    {
        string sse =
            "data: {\"type\":\"task.stream.event\",\"event\":{\"partial_text\":{\"text\":\"Sure\",\"language\":\"en\"},\"audio\":\"AAAA\",\"is_final\":false}}\n\n" +
            "data: {\"type\":\"task.stream.done\",\"result\":{\"text\":\"Sure!\",\"audio\":\"AAAA\",\"sample_rate\":24000,\"channels\":1,\"timing\":{\"ttft_ms\":42.0}}}\n\n" +
            "data: [DONE]\n\n";
        List<SseEvent> events = await ReadAllAsync(sse);
        TaskStreamEvent evt = TaskStreamEvent.ParseSse(events[0].Data);
        Assert.Equal(TaskStreamEventKind.Event, evt.Kind);
        Assert.Equal("Sure", evt.Event!.PartialText);
        Assert.Equal("en", evt.Event!.Language);
        Assert.False(evt.Event!.IsFinal);
        TaskStreamEvent done = TaskStreamEvent.ParseSse(events[1].Data);
        Assert.Equal(TaskStreamEventKind.Done, done.Kind);
        Assert.Equal("Sure!", done.Result!.Text);
        Assert.Equal(24000, done.Result!.SampleRate);
        Assert.Equal(42.0, done.Result!.Timing!.TtftMs);
    }

    [Fact]
    public async Task ErrorTerminatedStreamHasNoDone()
    {
        string sse =
            "data: {\"type\":\"speech.audio.delta\",\"audio\":\"AAAA\"}\n\n" +
            "data: {\"type\":\"error\",\"error\":{\"message\":\"model exploded\"}}\n";
        List<SseEvent> events = await ReadAllAsync(sse);
        Assert.Equal(2, events.Count);
        AudioCppException ex = Assert.Throws<AudioCppException>(() => SseReader.ThrowIfErrorEvent(events[1].Data));
        Assert.Equal("model exploded", ex.Message);
        Assert.Null(ex.StatusCode);
    }

    [Fact]
    public async Task TerminatesAtDoneSentinel()
    {
        string sse = "data: {\"type\":\"x\"}\n\ndata: [DONE]\n\ndata: {\"type\":\"after\"}\n\n";
        List<SseEvent> events = await ReadAllAsync(sse);
        Assert.Single(events);
    }

    [Fact]
    public async Task HandlesCommentsAndEventField()
    {
        string sse = ": keep-alive\n\nevent: ping\ndata: hello\n\n";
        List<SseEvent> events = await ReadAllAsync(sse);
        Assert.Single(events);
        Assert.Equal("ping", events[0].EventType);
        Assert.Equal("hello", events[0].Data);
    }

    [Fact]
    public async Task JoinsMultiLineDataPerSpec()
    {
        string sse = "data: {\"a\":\ndata: 1}\n\n";
        List<SseEvent> events = await ReadAllAsync(sse);
        Assert.Single(events);
        Assert.Equal("{\"a\":\n1}", events[0].Data);
    }

    [Fact]
    public async Task LargeLineUnderLimitIsRead()
    {
        string big = new('x', 1024 * 1024);
        string sse = "data: " + big + "\n\ndata: [DONE]\n\n";
        List<SseEvent> events = await ReadAllAsync(sse);
        Assert.Single(events);
        Assert.Equal(big, events[0].Data.Substring("data: ".Length - 6));
    }

    [Fact]
    public async Task OversizedLineThrows()
    {
        string big = new('x', 5000);
        string sse = "data: " + big + "\n\n";
        SseReader reader = new(new MemoryStream(Encoding.UTF8.GetBytes(sse)), maxLineBytes: 1000);
        await Assert.ThrowsAsync<InvalidOperationException>(() => reader.ReadEventsAsync(_ => Task.CompletedTask));
    }

    [Fact]
    public async Task WorksAcrossBufferBoundaries()
    {
        // 64KB of base64 audio: far more than the 81920 reader buffer would tolerate if
        // lines were not reassembled across reads.
        string b64 = Convert.ToBase64String(Enumerable.Range(0, 70000).Select(i => (byte)(i % 251)).ToArray());
        string sse = "data: {\"type\":\"speech.audio.delta\",\"audio\":\"" + b64 + "\"}\n\ndata: [DONE]\n\n";
        List<SseEvent> events = await ReadAllAsync(sse);
        SpeechStreamEvent evt = SpeechStreamEvent.Parse(events[0].Data);
        Assert.Equal(70000, evt.Pcm!.Length);
    }

    [Fact]
    public async Task TaskBatchResultAndDone()
    {
        string sse =
            "data: {\"type\":\"task.batch.result\",\"index\":0,\"result\":{\"text\":\"Sure!\",\"timing\":{\"ttft_ms\":5.0}}}\n\n" +
            "data: {\"type\":\"task.batch.done\",\"result_count\":1,\"timing\":{\"wall_ms\":12.0}}\n\n" +
            "data: [DONE]\n\n";
        List<SseEvent> events = await ReadAllAsync(sse);
        TaskBatchEvent first = TaskBatchEvent.Parse(events[0].Data);
        Assert.Equal(TaskBatchEventKind.Result, first.Kind);
        Assert.Equal(0, first.Index);
        Assert.Equal("Sure!", first.Result!.Text);
        TaskBatchEvent last = TaskBatchEvent.Parse(events[1].Data);
        Assert.Equal(TaskBatchEventKind.Done, last.Kind);
        Assert.Equal(1, last.ResultCount);
        Assert.Equal(12.0, last.Timing!.WallMs);
        Assert.Throws<System.Text.Json.JsonException>(() => TaskBatchEvent.Parse("{\"type\":\"task.batch.other\"}"));
    }

    [Fact]
    public async Task EmptyDataLineIsSkippedPerSpec()
    {
        List<SseEvent> events = await ReadAllAsync("data:\n\ndata: hello\n\n");
        Assert.Single(events);
        Assert.Equal("hello", events[0].Data);
    }

    [Fact]
    public async Task LargeLineUnderDefaultLimitIsRead()
    {
        // Pins the raised default: task.stream.done carries the whole run's audio as one
        // line (~640 KB base64 per 10 s), so the default must cover runs past 30 s.
        string b64 = new('A', 3 * 1024 * 1024);
        List<SseEvent> events = await ReadAllAsync("data: " + b64 + "\n\ndata: [DONE]\n\n");
        Assert.Single(events);
        Assert.Equal(b64, events[0].Data);
    }
}