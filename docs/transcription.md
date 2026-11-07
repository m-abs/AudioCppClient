# Transcription

Examples assume a shared `AudioCppClient client = new("http://127.0.0.1:8080");`. Exactly
one of `AudioPath` (server-local) or `AudioFile` (multipart upload) must be set.

## From a server-local path

```csharp
using Mabs.AudioCpp;
using Mabs.AudioCpp.Models;

TranscriptionResult result = await client.Transcriptions.TranscribeAsync(new TranscriptionRequest
{
    Model = "parakeet-tdt-0.6b-v3",
    AudioPath = "/data/hello.wav",
    Language = "en",
});

Console.WriteLine(result.Text);
```

## Upload (multipart)

```csharp
using FileStream upload = File.OpenRead("hello.wav");

TranscriptionResult result = await client.Transcriptions.TranscribeAsync(new TranscriptionRequest
{
    Model = "parakeet-tdt-0.6b-v3",
    AudioFile = upload,
    AudioFileName = "hello.wav", // must name a WAV for most builds
    Prompt = "Proper nouns: Mabs, audio.cpp", // recognition-context prompt (hotwords)
});
```

The client disposes `AudioFile` when the request completes.

## Details: segments, speaker turns, word timestamps

The details route returns the detail arrays the model produced; streaming is rejected on
this route by the server.

```csharp
TranscriptionDetailsResult details = await client.Transcriptions.TranscribeDetailsAsync(new TranscriptionDetailsRequest
{
    Model = "parakeet-tdt-0.6b-v3",
    AudioPath = "/data/meeting.wav",
});

foreach (AudioCppSpeakerTurn turn in details.SpeakerTurns ?? Array.Empty<AudioCppSpeakerTurn>())
{
    Console.WriteLine($"{turn.SpeakerId}: {turn.Text}");
}

foreach (AudioCppWord word in details.Words ?? Array.Empty<AudioCppWord>())
{
    Console.WriteLine($"{word.Word} [{word.Start}-{word.End}] conf {word.Confidence}");
}

Console.WriteLine($"sample rate {details.SampleRate}");
```

## Streaming transcript events

Requires a model configured with `mode: "streaming"`. Uploads the file with the
`stream=true` form field when `AudioFile` is set, or sends the JSON body with
`stream: true` when `AudioPath` is set.

```csharp
TranscriptionRequest streamRequest = new()
{
    Model = "parakeet-tdt-0.6b-v3",
    AudioPath = "/data/hello.wav",
};

await foreach (TranscriptEvent evt in client.Transcriptions.TranscribeStreamAsync(streamRequest))
{
    switch (evt.Kind)
    {
        case TranscriptEventKind.TextDelta:
            Console.Write(evt.Text); // finalized delta
            break;
        case TranscriptEventKind.TextPartial:
            Console.Write(evt.Text); // provisional, may be revised
            break;
        case TranscriptEventKind.TextDone:
            Console.WriteLine($"done, ttft {evt.Timing?.TtftMs} ms");
            break;
        case TranscriptEventKind.DiarizationDelta:
            foreach (AudioCppSpeakerTurn turn in evt.SpeakerTurns ?? Array.Empty<AudioCppSpeakerTurn>())
                Console.WriteLine($"{turn.SpeakerId}: {turn.Text}");
            break;
    }
}
```

## Live capture

Raw PCM as a chunked request body, transcript events on the same connection; all fields
travel as query parameters. EOF on the input stream is the model's end-of-speech signal.
Requires full-duplex HTTP/1.1.

```csharp
LiveTranscriptionRequest live = new()
{
    Model = "parakeet-tdt-0.6b-v3",
    SampleRate = 16000,
    Channels = 1,
    Language = "en",
    Prompt = "Proper nouns: Mabs, audio.cpp",
};

await foreach (TranscriptEvent evt in client.Transcriptions.TranscribeLiveAsync(live, micPcmStream))
{
    if (evt.Kind == TranscriptEventKind.TextDelta) Console.Write(evt.Text);
}
```

`micPcmStream` is any `Stream` producing PCM16 (`s16le` by default; `f32le` also accepted).

## Batch

Several uploads through one native-batching run (the model must implement native
batching); results arrive per file as the model finishes it, `Index` maps back to upload
order. Per-result timing is null by design.

```csharp
BatchTranscriptionRequest batch = new() { Model = "parakeet-tdt-0.6b-v3", Language = "en" };
batch.Files.Add(new AudioUpload("a.wav", File.ReadAllBytes("a.wav")));
batch.Files.Add(new AudioUpload("b.wav", File.ReadAllBytes("b.wav")));

await foreach (BatchTranscriptionEvent evt in client.Transcriptions.TranscribeBatchAsync(batch))
{
    if (evt.Kind == BatchTranscriptionEventKind.Result)
        Console.WriteLine($"[{evt.Index}] {evt.Filename}: {evt.Text}");
    else if (evt.Kind == BatchTranscriptionEventKind.Done)
        Console.WriteLine($"aggregate wall {evt.Timing?.WallMs} ms");
}
```

Callback variants (`Func<…, Task>`) exist for every streaming method above. See
[Error handling](error-handling.md) for SSE error events.
