---
name: audiocpp-api-update
description: >
  Update the AudioCpp .NET client when the audio.cpp server gains or changes an API,
  endpoint, request/response field, SSE event, or error shape. Use when the user says to
  "support a new audio.cpp feature/endpoint", "the server added X", "bump AudioCpp for a new
  audio.cpp release/version", mentions a new wire field or event type, or asks to reconcile
  the client against a newer audio.cpp server. Grounds every wire fact on the running server
  or its source before touching code, then threads the change through Models → Http (if a new
  SSE family) → client method → tests → demo → README and re-verifies.
---

# audio.cpp API update

You are extending `AudioCpp`, a .NET client for the **audio.cpp** server protocol (see
`AGENTS.md` for layout, build/test commands, formatting rules, and the verified v0.9.3 wire
facts). The one rule that overrides every other instinct here:

> **The live server (or its source) is the source of truth — never the plan doc, never a
> memory of how the protocol "should" look, never the old OpenAI-compatible shape.**

Every wire fact below the API surface — status codes, error `type` strings, field names, JSON
key order, which params are query vs body, event type strings — MUST be observed, not assumed.
This project already caught the plan doc being wrong (unknown model is **500** `server_error`,
not 404; `response_format=json` has no `sample_rate`/`channels`; live routes take params as
query string). Assume the doc is stale until you see it on the wire.

## Phase 0 — get ground truth (do not skip)

Before writing any C#, establish the exact shape of the new/changed thing:

1. **Running server.** Start it the way `AGENTS.md` "Live server for E2E" describes (qemu +
   eSpeak env if a TTS model is involved). Then probe the endpoint directly with `curl` and
   read the raw body. For SSE, capture frames with `curl -N`. For multipart, hand-build a
   request body and post it. Confirm: method, path, request fields (names + types + which are
   query vs body vs form parts), success response shape, and the **error shapes** (bad input,
   unknown model, missing keys, wrong content-type) with their real HTTP status and `error.type`.
2. **Server source** when the running build won't show it (an error branch you can't trigger,
   a field only set in a rare mode). The v0.9.3 sources used during the original build may be
   at `/tmp/*.cpp` (`runtime.cpp`, `server_protocol.cpp`, `request.cpp`) and `/tmp/api.md`; for a
   newer release, fetch the matching version from the audio.cpp repo. Grep for the route string,
   the JSON key, or the event `type` literal to find the writer/parser and read what it emits.
3. Record the observed facts in a scratch list before coding. Every code decision in the
   phases below cites one of these.

If you cannot reach a server **and** cannot find the source, STOP: name the exact fact you
need (e.g. "the field name and type for X on the `/v1/.../live` route") and where it comes
from. Do not guess a shape into a public type.

## Phase 1 — pick the smallest faithful surface

Map the observed API onto the existing client. Read the sibling it most resembles and copy its
conventions rather than inventing a second style:

- New endpoint under speech/transcription/tasks/recipes → a method on the matching sub-client
  (`SpeechClient.cs`, `TranscriptionClient.cs`, `TaskClients.cs`), or a GET endpoint on
  `AudioCppClient` (reuse `SendJsonAsync`/`GetHealthAsync` patterns).
- New request body → a `*Request` class in the right `Models/*.cs` file, following the
  `Validate()` + `WriteTo(Utf8JsonWriter)` shape already used (see `SpeechRequest`,
  `TaskRequestBody`). Match the server's **field order** — tests assert literal goldens.
- New response/event → a result type + an internal parser in `Http/SseParsers.cs` (or the
  model's own `Parse(JsonElement)`), using the `SseReader.ReadXxx` helpers. For a new SSE event
  family, add a `*Event` type with a `Kind` enum + `Parse`, and cover it in `SseReader`.
- Live/duplex route → model params as **query string** with a raw PCM body, mirroring
  `LiveSpeechRequest.BuildQueryString` / `SpeakLiveAsync`.
- New error behavior → confirm it flows through `AudioCppException{Message,ErrorType,StatusCode}`;
  the transport already parses `{"error":{message,type}}` and falls back to raw text.

Keep to the repo's protocol conventions already encoded: `Seed` is `string?` (uint64 precision);
`Options` is `Dictionary<string,string>` and typed-property/options collisions throw locally;
`SseEvent.Data` is `string`. Do not add an abstraction, option, or overload the API doesn't need.

## Phase 2 — implement

- Edit the relevant files. **Prefer a full-file `write` over a range `edit`** — range edits
  have repeatedly corrupted files in this repo; `dotnet build` is the only reliable check.
- No `var`; explicit types everywhere; 4-space `.cs` indent, 2-space xml/json; file-scoped
  namespaces; namespace must match folder; no trailing newline on `*.cs`; `_camelCase` privates;
  usings outside the namespace, `System.*` first, blank-line groups. (`.editorconfig` wins on
  any doubt.)
- Build both library TFMs (`netstandard2.0` + `net10.0`) after each substantive change; fix
  corruption or warnings immediately. The bar is 0 warnings / 0 errors, with
  `EnforceCodeStyleInBuild` on so the `.editorconfig` IDE rules (no `var`, IDE0063 using
  declarations) actually run.

## Phase 3 — test (the change isn't done until these exist)

`tests/AudioCpp.Tests/`, 68 tests today, all FakeItEasy/`FakeHandler`-driven, zero network:

- **Serialization golden** for any new/changed request field: assert the exact JSON string in
  `SpeechRequestSerializationTests.cs` (literal expected text, correct field order).
- **Transport** assertions in `TransportTests.cs`: correct method+path, auth only when key set,
  multipart part names in **unquoted** form (`name=file`, `filename=…` — .NET serializes
  unquoted; the server accepts both), timing-header surfacing.
- **Error envelope**: queue the real status/type observed in Phase 0; assert
  `AudioCppException.Message/ErrorType/StatusCode`.
- **SSE**: if a new event family, add fixtures to `SseReaderTests.cs` (real event JSON from
  Phase 0) and drive it through the client with `FakeHandler.QueueSse`.

Run the test TFM: `net10.0`.

## Phase 4 — demo + README

- Add or extend a command in `examples/AudioCpp.Demo/Program.cs` that proves the new behavior
  against the live server and exits non-zero if the expectation breaks (this is the real-world
  check the fakes can't give — cold model loads, real multipart parse, actual error codes).
  Run the **built dll** directly, not `dotnet run … -- ` with option flags (they mangle args).
- **README ships with the change** — update it in the same pass, and re-derive its claims from
  ground truth rather than from the previous README text:
  - **Endpoint table:** one row per route actually hit. Add the new route; split a row when two
    client methods hit different routes (`/v1/tasks/unload_models` vs
    `/v1/tasks/unload_all_models`). The "Client call" column must match the real public method
    name on the sub-client.
  - **Notes and caveats:** record every deviation the client now encodes — status codes, error
    `type` strings, field names, JSON key order, query-vs-body params — the same facts reported
    in Phase 5. If the new behavior contradicts an existing caveat, rewrite that caveat.
  - **Quick start / Demo CLI:** snippets must be runnable as written. Base URL is the server
    default `8080`; the examples use the `8092` development instance so they don't clash with a
    service already running. New demo commands get listed in the Demo CLI section, and snippets
    follow repo formatting (using declarations, no `var`).
  - **Header facts:** NuGet `PackageId` (`Mabs.AudioCppClient`), target frameworks,
    dependencies, the test count from an actual `dotnet test` run, and the pack output filename
    (`Mabs.AudioCppClient.<version>.nupkg`) — these drift, so check them against the csproj and a
    real run.

## Phase 5 — verify + pack + report

- `dotnet build AudioCpp.slnx` (0W/0E on netstandard2.0+net10.0).
- `dotnet test`, all green.
- Run the demo command live; paste the observed values.
- `dotnet pack src/AudioCpp -c Release`; if the API is a release-level change, bump
  `<Version>` in `src/AudioCpp/AudioCpp.csproj` and confirm the new `.nupkg` still carries
  both TFMs under `lib/`.
- **Report every deviation** between the plan doc and the observed server, and any behavior the
  client now encodes that differs from older docs (status codes, field names, key order). The
  live server wins; say what you found rather than silently reconciling to the doc.

## Phase 6 — convenience layer (generated code)

The generated convenience layer (`tools/generate-convenience.py` reading `tools/model-specs/`)
refreshes on every version bump, in the same pass as the manual phases:

1. Re-fetch `model_specs/*.json` and `webui/configs/model_params.json` for the new tag into
   `tools/model-specs/` — the vendored copy is what the generator reads; never hand-edit
   generated `.cs` files.
2. Re-run `python3 tools/generate-convenience.py` and diff the generated files. The generator
   is deterministic and idempotent, so any diff is real spec drift (new families, new options,
   changed defaults or capabilities) — report it.
3. Rebuild and run the tests. The golden serialization and `Validate()` tests are generated per
   family-task pair, so they re-encode the new wire shape automatically. The `.editorconfig`
   formatting rules apply to generated code: the bar is 0 warnings for generated files too.

## Shortcuts with known limits

- `shortcut:` v0.9.3 source snapshots at `/tmp` are for convenience; on a version bump, re-fetch
  sources from the matching audio.cpp tag before trusting them.
- `shortcut:` the eSpeak stub + qemu server startup in `AGENTS.md` is this box's test rig, not
  part of the library; if the server or models change, re-derive the startup, don't assume it.
