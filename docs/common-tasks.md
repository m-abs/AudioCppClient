# Common Tasks

One page per task area, each with runnable examples against a shared
`AudioCppClient client = new("http://127.0.0.1:8080");`. Model ids and voices are
server-configured; substitute yours (`GET /v1/models`, `GET /v1/voices`).

- [Speech Synthesis](speech-synthesis.md) — synthesize, JSON envelope, voice cloning, streaming, live duplex
- [Transcription](transcription.md) — path vs upload, details, streaming, live capture, batch
- [Alignment](alignment.md) — forced alignment of known transcripts
- [Tasks](tasks.md) — generic run, stream, batch, artifacts
- [Recipes](recipes.md) — multi-model runs
- [Error Handling](error-handling.md) — status codes, error envelopes, SSE errors
- [Model Management](model-management.md) — health, models, voices, load, unload

## Rebuilding cached results

Each result type describing one analysis — `TranscriptionResult`,
`TranscriptionDetailsResult`, `AlignmentResult`, `TaskResult` and `SpeechJsonResult` —
has a public static `Parse(string json)` that rebuilds it from a stored response body,
so a sidecar cache keyed by the analyzed audio needs no private JSON parsing:

```csharp
AlignmentResult result = AlignmentResult.Parse(File.ReadAllText(sidecarPath));
```

Base64 audio, named outputs and artifacts decode as on the live path. Model
load/unload results have no `Parse` — they describe server state, which a cache
cannot answer.
