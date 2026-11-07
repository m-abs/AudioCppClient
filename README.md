# AudioCpp

.NET client library for the [audio.cpp](https://github.com/0xShug0/audio.cpp) server protocol:
text-to-speech, transcription, alignment, live (duplex) routes, generic task execution,
batches, recipes, voices, and model management.

- NuGet: `AudioCpp` (0.0.1)
- Target frameworks: `netstandard2.0`, `net10.0`
- Dependencies: `System.Text.Json` only (`Microsoft.Bcl.AsyncInterfaces` on netstandard2.0)
- No OpenAI dependency; speaks the audio.cpp wire protocol directly

## Quick start

```csharp
using Mabs.AudioCpp;
using Mabs.AudioCpp.Models;

AudioCppClient client = new AudioCppClient("http://127.0.0.1:8092");

// Health
AudioCppHealth health = await client.GetHealthAsync();

// Speech synthesis (returns WAV/PCM/MP3 bytes as the server produced them)
SpeechResult speech = await client.Speech.SynthesizeAsync(new SpeechRequest
{
    Model = "kokoro",
    Input = "Hello from audio.cpp",
    Voice = "af_bella",
});
File.WriteAllBytes("hello.wav", speech.Audio);
// speech.Timing carries X-AudioCPP-Wall-Ms / Audio-Duration-Ms / RTF.

// Transcription from a server-local path
TranscriptionResult text = await client.Transcriptions.TranscribeAsync(new TranscriptionRequest
{
    Model = "parakeet",
    AudioPath = "/data/hello.wav",
});
Console.WriteLine(text.Text);

// Transcription by upload (multipart/form-data, OpenAI-compatible)
using FileStream wav = File.OpenRead("hello.wav");
TranscriptionResult uploaded = await client.Transcriptions.TranscribeAsync(new TranscriptionRequest
{
    Model = "parakeet",
    AudioFile = wav,
    AudioFileName = "hello.wav",
});
```

Streaming (C# 8+ / .NET targets — `IAsyncEnumerable`):

```csharp
using (Stream out = File.OpenWrite("out.pcm"))
{
    await foreach (SpeechStreamEvent evt in client.Speech.StreamAsync(new SpeechStreamRequest
    {
        Request = new SpeechRequest { Model = "stream-tts", Input = "Tell me a story" },
        ReturnText = true,
    }))
    {
        if (evt.Kind == SpeechStreamEventKind.AudioDelta) out.Write(evt.Pcm!);
        if (evt.Kind == SpeechStreamEventKind.TextDelta) Console.Write(evt.Text);
    }
}
```

On `netstandard2.0`, `IAsyncEnumerable` streaming is available via `Microsoft.Bcl.AsyncInterfaces`,
and every streaming method also has a callback overload (`Func<TEvent, Task> onEvent`) for
callers without `await foreach` language support.

## Endpoints

| Area | Method | Route | Client call |
|---|---|---|---|
| Health | GET | `/health` | `GetHealthAsync()` |
| Models | GET | `/v1/models` | `ListModelsAsync(includeParams, includeSessionOptions)` |
| Model load | POST | `/v1/models/load` | `LoadModelAsync(ModelLoadRequest)` (needs `--ui-management`; `Path` or `PackageId`) |
| Model unload | POST | `/v1/tasks/unload_models` | `UnloadModelsAsync(ids)` / `UnloadAllModelsAsync()` |
| Voices | GET | `/v1/audio/voices` | `ListVoicesAsync(model?)` |
| Recipes | GET | `/v1/recipes` | `ListRecipesAsync()` |
| Recipe run | POST | `/v1/recipes/run` | `Recipes.RunAsync(...)` / `RunStreamAsync(...)` |
| Speech | POST | `/v1/audio/speech` | `Speech.SynthesizeAsync(...)` → `SpeechResult` / `SynthesizeJsonAsync` → base64 envelope |
| Speech stream | POST | `/v1/audio/speech` (SSE) | `Speech.StreamAsync(...)` / `StreamPcmAsync` |
| Live speech | POST | `/v1/audio/speech/live` (duplex SSE) | `Speech.SpeakLiveAsync(request, pcmStream)` |
| Transcription | POST | `/v1/audio/transcriptions` | `Transcriptions.TranscribeAsync(...)` (JSON path or multipart upload) |
| Transcription details | POST | `/v1/audio/transcriptions/details` | `TranscribeDetailsAsync(...)` |
| Transcription stream | POST (SSE) | same | `TranscribeStreamAsync(...)` |
| Live transcription | POST | `/v1/audio/transcriptions/live` (duplex) | `TranscribeLiveAsync(request, pcmStream)` |
| Batch transcription | POST (SSE) | `/v1/batches/transcriptions` | `TranscribeBatchAsync(...)` |
| Alignment | POST (multipart) | `/v1/audio/alignments` | `Alignments.RunAsync(...)` |
| Task run | POST | `/v1/tasks/run` | `Tasks.RunAsync(...)` |
| Task stream | POST | `/v1/tasks/stream` | `Tasks.StreamAsync(...)` (`StreamFormat = "sse"` streams; `null` uses the buffered JSON envelope) |
| Task batch | POST (SSE) | `/v1/tasks/batch` | `Tasks.BatchAsync(...)` |

## Notes and caveats

- **Live routes are duplex.** `SpeakLiveAsync`/`TranscribeLiveAsync` POST raw PCM
  (`Transfer-Encoding: chunked`) and read SSE on the same connection: feed the request
  stream as your microphone produces data. Parameters travel as query string; the body is
  audio bytes only.
- **Streaming speech requires PCM.** The client forces `response_format=pcm` for
  `StreamAsync` because the server rejects other formats on the SSE path.
- **`AudioCppException`** carries the server's `error.message`, `error.type`, and HTTP
  status. On error-terminated SSE streams the exception is thrown mid-iteration.
- **Unknown models** return HTTP 500 (`type=server_error`) on `v0.9.3` and `v0.9.4`, despite
  older docs saying 404. The client surfaces whatever the server sends.
- **Model load by package id** (`v0.9.4`, native-model-manager builds): `ModelLoadRequest.PackageId`
  replaces `Path` and the server resolves the load target from the package registry. Sending both,
  or a non-string/empty `package_id`, is 400 `invalid_request_error`; an unknown or not-installed
  package is 404 `not_found`. The client enforces the either-or rule locally in `Validate()`.
- **`include_artifacts`** (`v0.9.4`): top-level flag on `TaskStreamRequest` and
  `RecipeRunRequest` (default `false`). When set, stream events carry an `artifacts` array and
  artifact-only events count toward `ttft_ms`.
- **Options map** (`Dictionary<string, string>`) passes model-specific request options
  through verbatim; the client throws locally if a typed property and an option key
  collide, instead of silently picking one.
- **Seed** is a `string` so full uint64 values round-trip without JSON number precision loss.
- `SseEvent.Data` is exposed as `string` (not `byte[]`): the server always writes UTF-8 JSON
  data lines; decode at use if you need bytes.

## Demo CLI

`examples/AudioCpp.Demo` proves each behavior against a live server:

```
dotnet run --project examples/AudioCpp.Demo -- health http://127.0.0.1:8092
dotnet run --project examples/AudioCpp.Demo -- speech http://127.0.0.1:8092 kokoro "Hello from audio.cpp" --out /tmp/speech.wav
dotnet run --project examples/AudioCpp.Demo -- speech-stream http://127.0.0.1:8092 kokoro "Hello from audio.cpp" --out /tmp/speech.pcm   # SSE; model must be configured mode=streaming
dotnet run --project examples/AudioCpp.Demo -- transcribe http://127.0.0.1:8092 parakeet /tmp/speech.wav --multipart
```

## Build and test

```
dotnet build AudioCpp.slnx
dotnet test tests/AudioCpp.Tests        # 57 tests, fake handler, no network
dotnet pack src/AudioCpp -c Release     # produces AudioCpp.0.0.1.nupkg
```