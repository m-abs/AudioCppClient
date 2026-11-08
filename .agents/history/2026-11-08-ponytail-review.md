# Ponytail review — changes since v0.0.3

**What this change does:** The cutover replaces the hand-written JSON writers/parsers with System.Text.Json: request records with `[JsonPropertyName]`, per-event `JsonConverter`s, and a custom resolver that honors `ShouldSerializeX()` methods. `netstandard2.0` is dropped, models are split into per-domain folders (one type per file), and two CI fixes land (coverage workflow missing `runs-on`, docs deploy gated to main pushes). Assumed load: one user, one process, one server — a library, not a service.

## Must fix

1. **Artifact validation no longer runs on the real request path** (`src/AudioCpp/Models/Tasks/TaskRequestBody.cs:66-76`, test `tests/AudioCpp.Tests/SpeechRequestSerializationTests.cs:323-327`)
   - **What this is:** Before the cutover, serializing a task request ran `AudioArtifactInput.Validate()` inside `WriteTo`, so an unknown artifact kind or a payload-plus-path artifact threw locally before sending.
   - **Problem:** `Json.Serialize` never calls `Validate()`, and `TaskRequestBody.Validate()` checks audio and voice-ref but never loops over `Artifacts`. A `TaskRunRequest` with `Kind = "tokens"` and `Path = "/x"` now passes client validation and goes to the server, which 400s — the client-side guarantee documented in docs/tasks.md is gone. The test still passes only because it calls `Validate()` directly, which no caller does.
   - **Fix:** Add the missing loop to `TaskRequestBody.Validate()` (three lines: `foreach (AudioArtifactInput a in Artifacts) a.Validate();`), and repoint the test to `TaskRunRequest.Validate()`.
   - **If we skip it:** The "throws locally before sending" promise silently degrades to a server error with a worse message, and the green test hides it.

## Should fix

2. **`Parse` returns null instead of throwing on a `null` JSON body** (`src/AudioCpp/Utils/Json.cs:41-48`)
   - **What this is:** `ParseObject` documents "throws on null, invalid JSON, or a non-object root", and the README now claims strict parsing.
   - **Problem:** A sidecar file containing the JSON literal `null` is a valid non-object root, but STJ deserializes it to `null` without throwing — verified by running it. `AlignmentResult.Parse("null")` returns null, and the caller gets a NullReferenceException later.
   - **Fix:** In `ParseObject`, `T? value = Deserialize<T>(json); return value ?? throw new JsonException(...)`. One line.
   - **If we skip it:** A corrupt cache file produces a null result instead of the loud `JsonException` the docs promise.

3. **Live routes can send the same query key twice** (`src/AudioCpp/Models/Speech/Live/LiveSpeechRequest.cs:84-109`)
   - **What this is:** The JSON speech path throws when a typed field and an options key collide (`RequestJson.ThrowOnOptionCollision`) because the server doesn't merge them.
   - **Problem:** On the live path both the typed fields and the options map become query parameters, but `BuildQueryString` does no collision check. `Seed = "1"` plus `Options["seed"] = "2"` sends `?seed=1&seed=2` — the server silently picks one. Same for `LiveTranscriptionRequest` (`prompt`, `language`).
   - **Fix:** Call the existing `RequestJson.ThrowOnOptionCollision` for the typed keys before adding options. Reuses existing code, a few lines.
   - **If we skip it:** The footgun the collision rule exists to prevent is still live on the live routes.

4. **The troubleshooting snippet fails as pasted** (`docs/getting-started.md:31-35`)
   - **What this is:** It tells users to add the missing `read:packages` scope with `gh auth login --scope read:packages`.
   - **Problem:** `--scope` is not a flag (verified against gh 2.46: "unknown flag: --scope"). And `gh auth login` mints a *new* token, which contradicts the next sentence — "the stored password is the old token". `gh auth refresh-token -s read:packages` re-scopes the existing token, which is what the paragraph actually describes.
   - **Fix:** Change the command to `gh auth refresh-token -s read:packages`. One line.
   - **If we skip it:** Users hit an error, then a new token that doesn't match the stored NuGet password.

5. **The riskiest new branch has no test** (`src/AudioCpp/Utils/Converters/TaskStreamEventConverter.cs:17-22`)
   - **What this is:** The task-stream converter has two shapes for `task.stream.done`: result object present, and absent.
   - **Problem:** A payload `{"type":"task.stream.done","result":null}` takes the fallback branch and yields an empty `TaskResult` instead of an error — untested, and the choice (empty result vs. throw) is undocumented. Every other event family has a golden or parse test; this branch is the only one where a protocol change lands silently.
   - **Fix:** One test in the existing FakeHandler style. While there, add the missing check for finding 1 through `TaskRunRequest.Validate()`.
   - **If we skip it:** The first server behavior change here passes tests and surprises users.

6. **The cutover opts into reflection-based JSON — AOT/WASM consumers lose what the old code had** (`src/AudioCpp/Utils/Json.cs:17`, `src/AudioCpp/Utils/ShouldSerializeJsonTypeInfoResolver.cs`)
   - **What this is:** The custom resolver subclasses `DefaultJsonTypeInfoResolver`; the old hand-written writers needed no reflection.
   - **Problem:** On .NET 10, reflection-based serialization is disabled by default in trimmed/AOT apps (verified: a plain run throws "Reflection-based serialization has been disabled"), and the docs still advertise browser/WASM. AOT builds emit IL2026 warnings and can break under trimming.
   - **Fix:** One README line saying AOT/trimming/WASM is not supported with this resolver, or drop the WASM sentence. No code change needed.
   - **If we skip it:** A WASM/AOT user follows the docs and gets a runtime exception.

## Nice to have

7. **Two 90-line structs are the same struct** (`src/AudioCpp/Models/StreamFormat.cs`, `src/AudioCpp/Models/Speech/ResponseFormat.cs`)
   - **What this is:** `StreamFormat` and `ResponseFormat` differ only in their constant names; equality, hash, operators, and implicit conversions are copy-paste, plus two near-identical converters.
   - **Problem:** Any fix to one must be made to the other. Their converter `Read` paths are never exercised — no response type carries either type.
   - **Fix:** One `WireString` type with both constant sets, or keep one and alias. About −45 lines.
   - **If we skip it:** Nothing breaks; the next change touches two files instead of one.

8. **Stale committed documents** (`AUDIT.md`, `.agents/plans/STJ_CUTOVER_PLAN_DECLARATIVE_PLAN.md`)
   - **What this is:** The plan file is the completed cutover plan; AUDIT.md is the previous audit whose findings this diff implemented.
   - **Problem:** AUDIT.md now contradicts the code it describes (says 2 MiB line limit — it's 16 MiB; references `SseParsers.cs`, `Polyfills.cs`, `TaskClients.cs`, netstandard2.0 — all deleted). A contributor reads it as current truth.
   - **Fix:** Delete both, or move to `docs/history/`. Deletion beats addition.
   - **If we skip it:** The next contributor trusts a document that is wrong.

9. **`IsIgnored` allocates per value-type property per serialize** (`src/AudioCpp/Utils/ShouldSerializeJsonTypeInfoResolver.cs:14-24`)
   - **Problem:** `Activator.CreateInstance(value.GetType())` runs on every value-type property, every request. Irrelevant at single-user load.
   - **Fix:** Cache default values per type in a static dictionary, if it ever matters. Skip otherwise.

## Checked and good

- CI fixes are correct for their triggers (coverage job now has `runs-on`; docs gate matches the recorded v0.0.3 tag-push failure). One pre-existing caveat: the publish job pushes the csproj `Version` (0.0.3), not the tag name, so tag/version drift publishes the wrong package.
- SseReader framing is right: `[DONE]`, error events without `[DONE]`, empty-data no-dispatch (the old audit's finding 7 is fixed), pending event at EOF, single leading-space trim.
- Golden tests still pin exact literals and field order; the resolver's CLR-name lookup is pinned by a test. Multipart, QueryBuilder encoding, error shapes, and timing headers all have tests.

**Verdict: Fix 1 first; 2, 3, 4 are one-liners worth doing before release.**

**Lean: −45 lines possible** (finding 7), plus two stale docs to delete (finding 8).

Not checked: live server E2E, AOT/WASM runtime behavior (inferred from .NET 10 defaults), full test suite not run.
