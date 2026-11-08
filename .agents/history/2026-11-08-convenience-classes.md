# Task-based convenience classes (TTS, Clone, Design)

## Context

The client library is low-level (`TaskClient` with a stringly-typed `options` map). The ask: specialized convenience client classes per model family with typed properties, supported value sets, and the web-ui default values. First pass covers every family in the v0.9.4 `model_specs` whose `tasks` include `tts`, `clone`, or `design` (54 families, 106 family-task pairs). Classes are generated from vendored spec data; task interfaces exist for the three first-pass tags. Task tags outside tts/clone/design are not generated this pass — the generator filters by task.

Decisions (fixed, not open): generated from JSON data vendored into the repo; interfaces in `Mabs.AudioCpp.Interfaces`; client classes in per-task namespaces `Mabs.AudioCpp.Tts` / `Mabs.AudioCpp.Clone` / `Mabs.AudioCpp.Design` (folder must match namespace per repo rule); clients bound to an existing `AudioCppClient`; defaults pre-initialized but only serialized when they differ from the default; C# enums in `src/AudioCpp/Enums/` for closed option sets; interfaces for TTS, Clone, Design; one class per family-task pair (e.g. `PlapreTtsClient`, `PlapreCloneClient`), one request record per pair.

## Approach

### 1. Vendor the spec data

Copy `0xShug0/audio.cpp/model_specs/*.json` (140 files) and `0xShug0/audio.cpp/webui/configs/model_params.json` into `tools/model-specs/` (flat, `model_params.json` alongside the family JSONs). This is the generator's input of record; the external checkout is research-only.

### 2. Hand-written shared layer (written once, generator emits against it)

- `src/AudioCpp/Interfaces/ITtsClient.cs`, `src/AudioCpp/Interfaces/ICloneClient.cs`, `src/AudioCpp/Interfaces/IDesignClient.cs` (namespace `Mabs.AudioCpp.Interfaces`), exact signatures:
  ```csharp
  public interface ITtsClient
  {
      Task<TaskResult> SpeakAsync(TtsRequest request, CancellationToken cancellationToken = default);
  }
  public interface ICloneClient
  {
      Task<TaskResult> CloneAsync(CloneRequest request, CancellationToken cancellationToken = default);
  }
  public interface IDesignClient
  {
      Task<TaskResult> DesignAsync(DesignRequest request, CancellationToken cancellationToken = default);
  }
  ```
- Base records (non-sealed, one per task): `src/AudioCpp/Models/Tts/TtsRequest.cs` (namespace `Mabs.AudioCpp.Models.Tts`), `src/AudioCpp/Models/Clone/CloneRequest.cs` (`…Models.Clone`), `src/AudioCpp/Models/Design/DesignRequest.cs` (`…Models.Design`). Common fields mirror `TaskRequestBody` top-level fields (`src/AudioCpp/Models/Tasks/TaskRequestBody.cs`): `Text`, `Language`, `VoiceId`, `VoiceRef` (`object?` — string path or `VoiceRefObject`), `AudioPath`, `AudioBase64`, `Artifacts`; `DesignRequest` adds `Instruction`. Virtual members on the base:
  - `Validate()` — reuses the existing rules verbatim (audio one-of, VoiceRef type, artifact validation); generated overrides call `base.Validate()` then family constraints.
  - `BuildOptions()` — returns `Dictionary<string, string>`; base returns empty; generated overrides add set options.
  - `ToTaskRequestBody()` — calls `Validate()`, maps top-level fields, sets `Options` when `BuildOptions()` is non-empty.
  - `DesignRequest` adds `virtual string? ComposedText()` returning `Text`; families without an `instruction` option (e.g. `voxcpm2`) override it to `"(instruction) " + Text` when `Instruction` is set.
- Comment on the base `BuildOptions`: `shortcut: an explicit set equal to the default is indistinguishable from unset; values equal to the default are omitted — the server applies its own default, so the wire is equivalent. Upgrade when a server stops applying spec defaults.`

### 3. Generator: `tools/generate-convenience.py`

Python 3, stdlib only, deterministic (sorted by family name), idempotent. Reads `tools/model-specs/`, filters families whose `tasks` contain `tts`/`clone`/`design`, emits one output per family-task pair (106 pairs):

- **Client class** `src/AudioCpp/<Task>/<Family><Task>Client.cs` (e.g. `src/AudioCpp/Tts/PlapreTtsClient.cs`), namespace `Mabs.AudioCpp.<Task>` (`Mabs.AudioCpp.Tts`, `Mabs.AudioCpp.Clone`, `Mabs.AudioCpp.Design`), sealed, implements the matching interface from `Mabs.AudioCpp.Interfaces`. Naming rule: family name PascalCased per underscore segment, with the trailing segment stripped when it equals the task name case-insensitively (`kokoro_tts` → `KokoroTtsClient`, `plapre` → `PlapreTtsClient`, `moss_ttsd` → `MossTtsdTtsClient`). Shape:
  ```csharp
  public sealed class PlapreTtsClient : ITtsClient
  {
      private readonly AudioCppClient _client;

      public PlapreTtsClient(AudioCppClient client) { _client = client; }

      public string Model { get; set; } = "plapre_3_bf16";   // default package id from spec packages[].default

      public Task<TaskResult> SpeakAsync(TtsRequest request, CancellationToken cancellationToken = default)
      {
          return _client.Tasks.RunAsync(new TaskRunRequest { Model = Model, Request = request.ToTaskRequestBody() }, cancellationToken);
      }

      public IAsyncEnumerable<TaskStreamEvent> StreamAsync(TtsRequest request, CancellationToken cancellationToken = default)
      {
          return _client.Tasks.StreamAsync(new TaskStreamRequest { Model = Model, Request = request.ToTaskRequestBody() }, cancellationToken);
      }
  }
  ```
  `StreamAsync` is concrete-class-only (not on the interfaces). XML docs state that the configured model id must serve the class's task — `/v1/tasks/run` carries no task field; the server resolves it from the model entry.
- **Request record** `src/AudioCpp/Models/<Task>/<Family><Task>Request.cs`, sealed, inherits the base record. One property per `options.request` entry, typed per the mapping table below, initialized to the resolved default. Overrides `BuildOptions()` (emits entries in spec option order) and `Validate()`.
- **Enums** `src/AudioCpp/Enums/<Family><OptionName>.cs`, namespace `Mabs.AudioCpp.Enums`, only for enum-typed options with `values`. Members PascalCased per segment (`tag_aware` → `TagAware`); wire string mapping via a switch inside the generated record's `BuildOptions`.
- **Tests** `tests/AudioCpp.Tests/Convenience/<Task>GoldenTests.cs` (one test class per task): per pair, one golden serialization test (`Text="hello"` plus one option set to a value different from its default, asserted as an exact literal string through `Json.Serialize`) and one `Validate()` test for the first bounded option (out-of-range value throws `ArgumentException`).

**Type mapping** (spec type → C# property): `string`→`string?`, `int`→`long?`, `float`→`double?`, `bool`→`bool?`, `audio_path`→`string?`, `enum`→generated enum (nullable), `seed`→`string?` (uint64 precision, existing convention in `AGENTS.md`; exception to `int`→`long?`).

**Option value stringification** (matches `TaskRequestBody.Options` convention): bool `true`/`false`; long invariant without decimals; double invariant culture (`0.8`); string as-is; enum via switch to its wire value.

**Default resolution**: `model_params.json` entry for the family wins when it has a default (8 families disagree with the spec — user instruction names web-ui config as the default source); otherwise the spec `options.request` default. Families without a `model_params.json` entry (44 across all specs) use spec defaults.

**Validate rules** (exact pattern, one message per rule): required option missing → `throw new ArgumentException("plapre tts requires voice_id", nameof(VoiceId))` style; value outside `min`/`max` → `"plapre tts top_k must be between -1 and 50"` style; enum value not in `values` → `"plapre tts text_chunk_mode must be one of: default, tag_aware, japanese, endline"` style; capability gates: `VoiceId` allowed only when the task's `capabilities` include `built_in_voices`, `VoiceRef` only when they include `speaker_reference` (`"plapre clone does not accept voice_id"` style).

**Formatting for generated code** (`.editorconfig` is authority; StyleCop runs in build with `EnforceCodeStyleInBuild`): no `var`; file-scoped namespaces matching folders; one public type per file, filename matches type; XML doc comments on every public member (summary/param/returns — SA1600/SA1611/SA1616/SA1618); no trailing newline on `*.cs`; usings outside namespace, `System.*` first, blank-line groups; braces always, no expression-bodied methods/ctors. Generated client files carry `using Mabs.AudioCpp.Interfaces;`, `using Mabs.AudioCpp.Models.<Task>;`, `using Mabs.AudioCpp.Models.Tasks;` and `using Mabs.AudioCpp.Utils;` as one group, alphabetically sorted within it (matches existing test-file convention).

### 4. Demo CLI

Add a `convenience` command to `examples/AudioCpp.Demo/Program.cs`: against the live server, `KokoroTtsClient` (family `kokoro_tts`, default package `kokoro_82m_q8_0`) with `Model = "kokoro"`, `SpeakAsync(new PlapreTtsRequest…)` equivalent using `TtsRequest { Text = "Hello world" }`; exits non-zero if `Audio` is empty or `Timing` is null; prints audio byte count and `Timing`. Run the built dll directly.

### 5. README + docs + skill

- README: one "Convenience clients" section (pattern snippet runnable as written, using declarations, no `var`) + `docs/convenience.md` + `toc.yml` entry.
- Extend `.agents/skills/audiocpp-api-update/SKILL.md` with a convenience-layer phase: on a version bump, re-fetch `model_specs/*.json` + `webui/configs/model_params.json` for the new tag into `tools/model-specs/`, re-run `tools/generate-convenience.py`, diff generated files, rebuild, run tests, report drift; note that the formatting rules apply to generated code.

## Critical files & anchors

- `src/AudioCpp/Models/Tasks/TaskRequestBody.cs` — field order (`text, language, audio, audio_base64, voice_id, voice_ref, artifacts, options`) and options stringification that base records and golden tests must mirror.
- `src/AudioCpp/Models/Tasks/TaskRunRequest.cs` — golden body order `model, busy_timeout_ms, request`; reused unchanged by generated clients.
- `tests/AudioCpp.Tests/SpeechRequestSerializationTests.cs` — golden-literal test pattern to copy.
- `tests/AudioCpp.Tests/FakeHandler.cs` — transport assertion pattern (`OnSendAsync`, records method/URI/body).
- `tools/model-specs/model_params.json` — default source of truth (web-ui wins on conflict).

## Verification

1. Run generator → `dotnet build AudioCpp.slnx -v q --nologo` must be 0 warnings / 0 errors; re-run generator → `git diff` empty (idempotent).
2. Golden example (concrete input → exact output): `PlapreTtsRequest { Text = "Hej", VoiceId = PlapreVoiceId.Ida }` through `Json.Serialize(ToTaskRequestBody() wrapped in TaskRunRequest)` equals `{"model":"plapre_3_bf16","request":{"text":"Hej","options":{"voice_id":"ida"}}}` — defaults omitted, options in spec order.
3. `dotnet test tests/AudioCpp.Tests -v q --nologo`: existing 79 tests green plus generated goldens/Validate tests; report the actual total.
4. Polymorphism check: `ITtsClient tts = new PlapreTtsClient(client);` with a plain `TtsRequest { Text = "hi" }` → golden `{"model":"plapre_3_bf16","request":{"text":"hi"}}` via `FakeHandler`; transport test asserts `POST /v1/tasks/run` and the recorded body.
5. Design prefix check: `Voxcpm2DesignClient` with `Instruction = "whisper"` and `Text = "hello"` → request text is `"(whisper) hello"` and no `instruct` option appears (voxcpm2 has no `instruction` option; all other first-pass design families declare `instruct` and must route it through options).
6. Live E2E: start the qemu server per `AGENTS.md` (port 8092, `/tmp/models.json` kokoro+parakeet), run `dotnet examples/AudioCpp.Demo/bin/Debug/net10.0/AudioCpp.Demo.dll convenience http://127.0.0.1:8092` — non-zero exit on empty audio or missing timing headers; paste observed values.

## Assumptions & contingencies

- The user note "plapre clone cannot take a voice_ref or reference text" contradicts v0.9.4 docs (`docs/models/plapre.md` shows `--task clon` with `--voice-ref` + `--reference-text`) and the spec (clone capabilities include `speaker_reference`; `built_in_voices` only on tts). Follow the spec capabilities; if the user overrides, add a per-family Validate exception.
- If `0xShug0/audio.cpp` is absent at implementation time, fetch tag v0.9.4; the vendored `tools/model-specs/` copy is what the generator reads.
- Web-ui defaults beat spec defaults on the 8 known conflicts (user instruction). If a family has neither, the property has no initializer (server applies its default).
- Multi-task families: one class per family-task pair, one record per pair; per-task constraints come from `capabilities` per task. Task selection is server-side (config model entry `task`), so each class's XML doc names the task its model id must serve.
- Enum generation is limited to options with declared `values`; open sets (e.g. unbounded voice ids) stay `string?`.

## Decisions (ADR-style, recorded here per repo convention)

1. **Web-ui config beats spec defaults.** On the 8 families where `webui/configs/model_params.json` disagrees with `model_specs` defaults, the web-ui value wins (user instruction). Golden tests pin these values; a regeneration that drops `model_params.json` would silently change serialized output.
2. **Defaults equal to the spec default are omitted from the wire.** An explicit set is indistinguishable from unset server-side, and the server applies its own default, so the wire is equivalent; golden tests assert omission. Upgrade when a server stops applying spec defaults.
3. **Generated files are committed artifacts, not build-time output.** `src/AudioCpp/Enums/`, the per-family records/clients, and the convenience tests are the output of `tools/generate-convenience.py`; never hand-edit them — vendor new specs and re-run the generator (skill Phase 6). Idempotency (sorted, deterministic) is what makes the diff reviewable.
4. **Interfaces stay single-method (run-only); `StreamAsync` is concrete-class-only.** Polymorphic use is the interface contract; streaming requires the concrete client. No interface method was added for it.
5. **Capability gates follow the spec, not verbal notes.** The user note "plapre clone cannot take a voice_ref or reference text" contradicts the spec (`speaker_reference` capability on clone); the generator emits spec capabilities. If the user overrides, add a per-family Validate exception.
6. **Implementation deviations:** generated files carry only used usings (IDE0005 in build beats the plan's fixed using list — clients don't reference `Utils`); the live E2E ran on a native x64 build (GCC -O2 after a -O3 ICE on `registry.cpp`, clang unusable without libomp) instead of the qemu binary, and the server ran without `--ui-management` — `tasks/run` needs neither.
