# Ponytail audit — AudioCppClient (2026-11-06)

## What this repo does

A NuGet client library for the audio.cpp server (TTS, ASR, alignment, live duplex routes, tasks, batches, recipes). One `AudioCppClient` per server URL; requests are JSON or multipart, streaming responses are SSE. Assumed load: one app (or a few threads) sharing one client against one server — a library, not a service.

Baseline verified: build clean on all three TFMs, 57 tests pass, no network.

## Must fix

1. **Long streamed runs crash on the SSE line limit the client cannot raise** (`src/AudioCpp/Http/SseReader.cs:57,495-503`; every client construction site, e.g. `src/AudioCpp/TaskClients.cs:110,156,222`)
   - **What this is:** `SseReader` caps one line at 2 MiB by default; `task.stream.done` carries the whole result audio as one Base64 line.
   - **Problem:** 640 KB of Base64 per 10 s of audio means a run longer than ~30 s exceeds the cap. The docs say callers "should size this via the constructor", but every client method hardcodes `new SseReader(stream)` with the default — there is no way to pass a bigger limit. A 60-second streamed task run throws `InvalidOperationException` mid-iteration.
   - **Fix:** raise `DefaultMaxLineBytes` to 16 MiB (one line), or add one optional constructor parameter on `AudioCppClient` and use it at the seven `new SseReader` sites.
   - **If we skip it:** any streamed task over ~30 seconds fails for every user, and the error message tells them to use an API that doesn't exist.

## Should fix

2. **Cancelling a live session mid-body does nothing on netstandard2.0** (`src/AudioCpp/AudioCppClient.cs:545-566`, `ChunkedStreamContent`)
   - **What this is:** live routes stream microphone PCM as a chunked request body while reading SSE on the same connection.
   - **Problem:** `SerializeToStreamAsync` copies with `CancellationToken.None`, so the token is never checked while the body is open. Cancellation only works if the handler aborts the socket (true on .NET Core, not guaranteed on netstandard2.0 handlers). Cancel mid-session with an open mic stream: nothing happens until the input reaches EOF.
   - **Fix:** take the token in `ChunkedStreamContent`'s constructor and check it per chunk — three lines.
   - **If we skip it:** live sessions can't be stopped on some platforms; users close the stream instead, which the server reads as end-of-speech.

3. **An empty `AudioPath` passes validation and sends `"audio":""`** (`src/AudioCpp/Models/Transcription.cs:38-56`)
   - **What this is:** `Validate()` requires "path or file" using `!= null`, not `IsNullOrEmpty`.
   - **Problem:** `AudioPath = ""` passes, `WriteJson` emits `"audio":""`, the server 400s — after the client claimed the request was valid.
   - **Fix:** use `string.IsNullOrEmpty` in the two checks. One line.
   - **If we skip it:** a caller with an empty string (config, env var) gets a confusing server error instead of the client's own clear message.

4. **The README endpoint table lists three wrong routes** (`README.md:57-75`)
   - **What this is:** the table says `/v1/voices`, `/v1/speech/live`, `/v1/transcriptions/live`.
   - **Problem:** the code calls `/v1/audio/voices`, `/v1/audio/speech/live`, `/v1/audio/transcriptions/live` (matching AGENTS.md). Someone copying the table for a proxy rule or curl test gets 404s.
   - **Fix:** correct three table cells.
   - **If we skip it:** docs disagree with the wire; the code is right, the README is the trap.

5. **The riskiest paths have no tests** (`tests/AudioCpp.Tests/`)
   - **What this is:** serialization and transport are well tested, but `TaskBatchEvent.Parse`, `StreamPcmAsync`, `SpeakLiveAsync`, `TranscribeStreamAsync`, and the max-line behavior of finding 1 have no test that fails when they break.
   - **Problem:** the parsers for batch events throw raw `JsonException` on unknown types; the duplex body-copy path (finding 2) is untested at all.
   - **Fix:** one test per family, same FakeHandler style, no network. Roughly 40 lines total.
   - **If we skip it:** these are the paths a protocol change breaks first, and nothing fails loudly.

6. **README streaming example truncates the output file on every delta** (`README.md:50-57`)
   - **What this is:** `File.OpenWrite("out.pcm").Write(evt.Pcm!)` inside the loop.
   - **Problem:** `OpenWrite` truncates and is never closed — after 10 deltas the file holds only the last chunk, and file handles leak until GC.
   - **Fix:** open once outside the loop, write inside. Two lines.
   - **If we skip it:** the copy-pasted quick start produces broken output for everyone who uses it.

## Nice to have

7. **An empty `data:` line dispatches an event the SSE spec says to skip** (`src/AudioCpp/Http/SseReader.cs:137-168`)
   - **What this is:** `data != null` is the dispatch condition, so `data:\n\n` yields an event with `Data = ""`.
   - **Problem:** downstream `Parse("")` throws raw `JsonException`. The server never sends this today, but a proxy keep-alive could.
   - **Fix:** dispatch only when data is non-empty (`!string.IsNullOrEmpty(data)`). One word changed.
   - **If we skip it:** a stray empty data line crashes parsers with an unhelpful exception.

8. **Three internal helpers have no callers** (`src/AudioCpp/AudioCppClient.cs:388-413`)
   - **What this is:** `PostJsonAsync`, `PostJsonWithQueryAsync`, `FinishAsync` — grep across src, tests, demo, and skills finds only the definitions (one skill doc mentions `PostJsonAsync` as a pattern to reuse).
   - **Problem:** dead code that must be read, kept correct, and contradicts the live convention (callers use `SendJsonAsync`/`ReadJsonAsync` directly).
   - **Fix:** delete all three; `PostJsonWithQueryAsync`'s query-splitting logic is unused anywhere else.
   - **If we skip it:** nothing breaks, but the next contributor picks the dead path over the live one.

9. **`Polyfills.cs` is dead** (`src/AudioCpp/Polyfills.cs`)
   - **What this is:** `IsExternalInit` enables records with init-only setters on netstandard2.0.
   - **Problem:** there are no records in src or tests; every model is a class with plain internal setters. The polyfill compiles for nothing.
   - **Fix:** delete the file.
   - **If we skip it:** harmless, but it advertises a feature the code doesn't use.

10. **Multipart upload disposes the caller's stream** (`src/AudioCpp/TranscriptionClient.cs:330-334`)
    - **What this is:** `StreamContent` takes ownership, so disposing the request disposes the caller's `AudioFile`.
    - **Problem:** a caller reusing a pooled stream gets `ObjectDisposedException` on the next call. The README example is safe (`using`), but the ownership rule is invisible.
    - **Fix:** one doc line on `AudioFile` saying the client closes it.
    - **If we skip it:** standard .NET behavior, but surprising enough to file as a bug report.

Smaller items left out: 4 (typo "hostand" in `postCreate.sh`, `Validate()` runs twice per speech call, `SpeechJsonResult.Audio` re-decodes Base64 on every access, self-created `HttpClient` is never disposable).

**Verdict:** Healthy overall — clean build, disciplined tests, faithful wire protocol. Fix 1 first: it is the only finding that breaks a normal user at normal load.

**Lean: −35 lines, −0 dependencies possible.**

Not checked: the live server under qemu (test dependency, not run here), netstandard2.0 handler cancellation behavior (inferred from the code, not exercised), devcontainer runtime.
