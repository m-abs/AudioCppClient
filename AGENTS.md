# AGENTS.md

Working notes for agents in this repository. Read before editing.

## What this is

`AudioCpp` — a NuGet-packable .NET client library for the **audio.cpp** server protocol
(https://github.com/0xShug0/audio.cpp). It speaks the audio.cpp wire protocol (HTTP + SSE)
directly; there is **no OpenAI dependency**. Namespaces: `MabsDk.AudioCpp`,
`MabsDk.AudioCpp.Http`, `MabsDk.AudioCpp.Models`.

Target frameworks: `netstandard2.0;net10.0`. Only library dependencies are
`System.Text.Json` and (netstandard2.0 only) `Microsoft.Bcl.AsyncInterfaces`, both pinned to
`9.0.8`. Current package version `0.0.1`.

## Layout

```
src/AudioCpp/                 library (13 .cs files)
  AudioCppClient.cs           entry point: ctor + transport + GET endpoints (health/models/voices/recipes/unload/load)
  SpeechClient.cs             /v1/audio/speech: synthesize, JSON, stream, duplex live
  TranscriptionClient.cs      /v1/audio/transcriptions[...], live, batch; internal Multipart builders
  TaskClients.cs              AlignmentClient, TaskClient (run/stream/batch), RecipeClient
  Http/                       SseReader (+SseEvent), SseParsers (internal), AudioCppException, AudioRawResponse (internal)
  Models/                     Common.cs, Speech.cs, Transcription.cs, Tasks.cs — request/response types + JSON writers/parsers
  Polyfills.cs                netstandard2.0 IsExternalInit (records without `required`)
tests/AudioCpp.Tests/         xunit + FakeItEasy; FakeHandler.cs is the fake transport
examples/AudioCpp.Demo/       Program.cs CLI proving each behavior against a live server
Directory.Build.props         LangVersion latest, Nullable enable, ImplicitUsings false, Deterministic
AudioCpp.slnx                 handwritten solution (SDK `dotnet new slnx` is broken here)
```

Public entry surface: `AudioCppClient` (sub-clients `Speech`, `Transcriptions`, `Alignments`,
`Tasks`, `Recipes`). All request types expose `Validate()` + a JSON writer; responses parse via
internal `SseReader`/`SseParsers` helpers. The test assembly sees `internal`s via
`InternalsVisibleTo("AudioCpp.Tests")`.

## Build / test / pack

```
dotnet build AudioCpp.slnx -v q --nologo          # library + tests + demo
dotnet test tests/AudioCpp.Tests -v q --nologo
bin/coverage.sh                                                              # tests + cobertura + HTML report (needs reportgenerator global tool) -> coverage/report/index.html
dotnet pack src/AudioCpp -c Release --nologo      # -> src/AudioCpp/bin/Release/AudioCpp.0.0.1.nupkg
```

`dotnet` is 10.0.401. Expect **0 warnings, 0 errors** — the library builds clean on both
TFMs and the suite is 68 tests, no network. Coverage (coverlet.msbuild, test project only):
~74% line / ~65% branch as of 2026-11-07; `.github/workflows/coverage.yml` runs it on every
push and on PRs only when a maintainer comments `/coverage` (posts the summary back on the PR,
uploads the cobertura report as an artifact).

## Formatting — mandatory (`.editorconfig` is the authority)

- **No `var`** (warning severity): always explicit types, including `using (T x = …)`.
- File-scoped namespaces. 4-space indent for `*.cs`; 2-space for `*.csproj`/`*.xml`/`*.json`/`*.props`.
- `insert_final_newline = false` for `*.cs` (no trailing blank line).
- Braces always; expression-bodied properties/lambdas OK, **not** methods/ctors/accessors.
- Private fields `_camelCase`. Usings outside the namespace, `System.*` first, blank line
  between groups (`dotnet_separate_import_directive_groups = true`).
- Namespace must match folder (`Http/` → `…Http`).

## Edit-tool caution (learned this repo)

Range-based `edit` has repeatedly corrupted files here (dropping/duplicating context lines).
For active files prefer a full-file `write`, or a single anchored `PUT`. **`dotnet build` is
ground truth after every change** — fix corruption immediately, do not trust the edit echo.

## Protocol facts the code encodes (grounded in v0.9.4, git f7ac446f)

The live server is source of truth over the plan doc. Non-obvious wire behavior:

- **Unknown model → HTTP 500** `{"error":{"message":"unknown model id: X","type":"server_error"}}`
  on speech/tasks (older docs say 404 — superseded). Live routes return **400** `invalid_request_error`.
  `models/load` without `--ui-management` → **403** `forbidden`.
- **SSE** framing: `data: <json>\n\n`, terminator `data: [DONE]\n\n`. Error events are
  `{"type":"error","error":{"message":…}}` and end the stream with **no `[DONE]`**; `SseReader`
  throws `AudioCppException` mid-iteration on those. `SseEvent.Data` is a **`string`** (server
  always writes UTF-8 JSON data lines).
- **Live routes** (`speech/live`, `transcriptions/live`): body is raw chunked PCM
  (`application/octet-stream`); **all** parameters travel as query string. Duplex: the request
  stream and the SSE response share one connection.
- **Timing headers** on non-streaming audio: `X-AudioCPP-Wall-Ms`, `X-AudioCPP-Audio-Duration-Ms`,
  `X-AudioCPP-RTF` (surfaced on `SpeechResult.Timing`). `response_format=json` speech returns
  `{"audio":<b64>,"format":…,"timing":{…}}` only — no `sample_rate`/`channels`.
- **Streaming speech** requires `response_format=pcm`; the client forces it.
- **Seed** is `string?` so full uint64 round-trips (JSON-number precision).
- **Options map** is `Dictionary<string,string>`; a typed property and an options key that
  collide throw locally (server aliases don't merge — silent overwrite is a footgun).
- `TaskStreamRequest.StreamFormat = null` means the buffered JSON envelope, replayed client-side
  as the same event sequence; `"sse"` streams; any other value is rejected client-side (server 400s).
- **models/load by `package_id`** (v0.9.4, native model manager builds): `package_id` replaces
  `path`; both set or a non-string/empty `package_id` → 400 `invalid_request_error`; unknown or
  not-installed package → 404 `not_found` (verified live). The client enforces either-or in
  `ModelLoadRequest.Validate()` before sending.
- **`include_artifacts`** (v0.9.4 top-level flag on task/recipe stream bodies, default false):
  events gain an `artifacts` array between `named_audio_outputs` and `word_timestamps`, and
  artifact-only events count toward `ttft_ms` when enabled.

## Testing conventions

- `FakeHandler` cannot fake `HttpMessageHandler.SendAsync` directly (it is `protected override`
  and FakeItEasy intercepts the override itself). It fakes a `PipelineHandler` whose
  `sealed override SendAsync` delegates to a **`public virtual OnSendAsync`** — the fake
  configures `OnSendAsync` and returns canned JSON/bytes/SSE, recording method/URI/auth/body.
- Multipart assertions use **unquoted** needles (`name=file`, `filename=a.wav`): .NET serializes
  form-data names unquoted with an RFC-2231 `filename*` twin. The live server parses both forms.
- Serialization tests assert **literal goldens** (exact field order), not "grew / not empty".
- Error-shape tests queue 400/403/500/503 + a non-JSON fallback and assert
  `AudioCppException{Message,ErrorType,StatusCode}`.

## Demo CLI

`examples/AudioCpp.Demo` — run the **built dll** directly, not `dotnet run … -v q --`
(option flags before `--` mangle app args):
`dotnet examples/AudioCpp.Demo/bin/Debug/net10.0/AudioCpp.Demo.dll <cmd> <baseUrl> …`.
Each command exits non-zero on a broken expectation and prints its observable.

## Live server for E2E (this box)

Cross-compiled x86_64 binary under qemu at `/tmp/audiocpp093/audiocpp_server`; run:

```
cd /tmp/audiocpp093 && AUDIOCPP_ESPEAK_LIBRARY=/tmp/espeak-root/usr/lib/x86_64-linux-gnu/libespeak-ng.so.1 \
  AUDIOCPP_ESPEAK_DATA=/tmp/espeak-root/usr/lib/x86_64-linux-gnu/espeak-ng-data \
  qemu-x86_64-static -L /usr/x86_64-linux-gnu ./audiocpp_server --ui --ui-management \
  --backend cpu --port 8092 --config /tmp/models.json > /tmp/server.log 2>&1 &
```

`/tmp/models.json` = kokoro (TTS) + parakeet (ASR), `lazy_load:true`, `max_loaded_models:1`.
Kokoro needs eSpeak-ng: the amd64 `libespeak-ng.so.1`+data are extracted to `/tmp/espeak-root`,
with a **stub `libpcaudio.so.0`** installed in the qemu sysroot (espeak only calls it for device
playback, which the phonemizer path never uses). Cold model loads under qemu take minutes — the
demo passes `HttpClient{Timeout=30min}`. First model hit is slow; `tail /tmp/server.log` for
progress. The server is a test dependency, not a repo deliverable.

## When audio.cpp ships a new API or feature

Use the `audiocpp-api-update` skill (`skill://audiocpp-api-update`): probe the running server /
source for the exact wire shape first, then thread it through Models → Http (if new SSE family)
→ client method → tests → demo → README, and re-verify (build 3 TFMs, both test TFMs, live E2E,
pack). Report every deviation from the plan doc — the live server wins.
