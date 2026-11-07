# Speech Synthesis

Examples assume a shared `AudioCppClient client = new("http://127.0.0.1:8080");`. Model ids
are server-configured; substitute yours (`GET /v1/models`, `GET /v1/voices`).

## Synthesize to WAV

```csharp
using Mabs.AudioCpp;
using Mabs.AudioCpp.Models;

SpeechResult result = await client.Speech.SynthesizeAsync(new SpeechRequest
{
    Model = "kokoro",
    Input = "Hello from audio.cpp.",
    Voice = "af_bella",
    Language = "en",
});

await File.WriteAllBytesAsync("out.wav", result.Audio);
Console.WriteLine($"wall {result.Timing?.WallMs} ms, rtf {result.Timing?.Rtf}");
```

Timing comes from the `X-AudioCPP-Wall-Ms` / `X-AudioCPP-Audio-Duration-Ms` /
`X-AudioCPP-RTF` response headers.

## JSON envelope

`response_format=json` returns `{"audio":<b64>,"format":…,"timing":{…}}` — no
`sample_rate`/`channels`. The client parses it:

```csharp
SpeechJsonResult envelope = await client.Speech.SynthesizeJsonAsync(new SpeechRequest
{
    Model = "kokoro",
    Input = "Hello from audio.cpp.",
    Voice = "af_bella",
});

await File.WriteAllBytesAsync("out.wav", envelope.Audio); // base64-decoded WAV
Console.WriteLine($"format {envelope.Format}, audio duration {envelope.Timing?.AudioDurationMs} ms");
```

## Voice cloning

`VoiceRef` takes a server-local path string or a `VoiceRefObject` (5 MiB payload limit);
give the reference audio's transcript in `ReferenceText`. `Seed` is a `string` so full
uint64 seeds round-trip without JSON number precision loss.

```csharp
// Server-local reference
SpeechResult byPath = await client.Speech.SynthesizeAsync(new SpeechRequest
{
    Model = "kokoro",
    Input = "Reading with a cloned voice.",
    VoiceRef = "/voices/ref.wav",
    ReferenceText = "Transcript of the reference audio.",
    Seed = "18446744073709551615",
});

// Inline reference bytes
SpeechResult byBytes = await client.Speech.SynthesizeAsync(new SpeechRequest
{
    Model = "kokoro",
    Input = "Reading with a cloned voice.",
    VoiceRef = VoiceRefObject.FromBytes(File.ReadAllBytes("ref.wav")),
    ReferenceText = "Transcript of the reference audio.",
});
```

## Style, speed, temperature

`Instruct` (model option) and `Instructions` (OpenAI-style top level, mapped to
`instruction`) are mutually exclusive — the client throws locally before sending. Same for
`Speed` vs `SpeakingRate`.

```csharp
SpeechRequest styled = new()
{
    Model = "kokoro",
    Input = "Whispered, slowly.",
    Voice = "zf_fawn",
    Instruct = "whisper; slow",
    Temperature = 0.7,
    TopP = 0.9,
    MaxTokens = 256,
    BusyTimeoutMs = 30000,
};

SpeechResult result = await client.Speech.SynthesizeAsync(styled);
```

## Streaming speech

Requires a model configured with `mode: "streaming"` and only supports
`response_format=pcm`. SSE events (the default `stream_format`):

```csharp
SpeechStreamRequest stream = new()
{
    Request = new SpeechRequest { Model = "kokoro-stream", Input = "Hello.", ResponseFormat = ResponseFormat.Pcm },
    StreamFormat = StreamFormat.Sse,
    ReturnText = true, // speech-to-speech models can stream their text too
};

await foreach (SpeechStreamEvent evt in client.Speech.StreamAsync(stream))
{
    switch (evt.Kind)
    {
        case SpeechStreamEventKind.AudioDelta:
            await output.WriteAsync(evt.Pcm); // PCM16 chunk
            break;
        case SpeechStreamEventKind.TextDelta:
            Console.Write(evt.Text);
            break;
        case SpeechStreamEventKind.AudioDone:
            Console.WriteLine($"ttft {evt.Timing?.TtftMs} ms");
            break;
    }
}
```

Raw chunked PCM instead of events — `StreamPcmAsync` forces `stream_format=audio` and
`response_format=pcm`; you own the returned stream:

```csharp
Stream pcm = await client.Speech.StreamPcmAsync(stream);
byte[] buffer = new byte[4096];
int read;
while ((read = await pcm.ReadAsync(buffer)) > 0)
{
    await output.WriteAsync(buffer.AsMemory(0, read));
}
```

Callback variant instead of `foreach`:

```csharp
await client.Speech.StreamAsync(stream, async evt =>
{
    if (evt.Kind == SpeechStreamEventKind.AudioDelta) await output.WriteAsync(evt.Pcm);
});
```

## Live duplex speech-to-speech

`/v1/audio/speech/live` uploads raw PCM as a chunked request body and streams events on the
same connection; all parameters travel as query string. EOF on the input stream is the
model's end-of-speech signal. Requires full-duplex HTTP/1.1 — browser/WASM handlers throw
`PlatformNotSupportedException`.

```csharp
LiveSpeechRequest live = new()
{
    Model = "kokoro-stream",
    Voice = "af_bella",
    SampleRate = 16000,
    Channels = 1,
    SampleFormat = "s16le",
};

await foreach (SpeechStreamEvent evt in client.Speech.SpeakLiveAsync(live, micPcmStream))
{
    if (evt.Kind == SpeechStreamEventKind.AudioDelta) await output.WriteAsync(evt.Pcm);
}
```

See [Transcription](transcription.md) for the live capture variant, and
[Error handling](error-handling.md) for what happens when a live route fails.
