# STJ cutover plan — declarative System.Text.Json

## Locked decisions
- Result types → `sealed record` with `[JsonPropertyName]` + `{ get; init; }` (value equality accepted).
- Strict STJ: all lenient `ReadString/ReadDouble/ReadInt/ReadBool/ReadTiming/ReadWords/...` helpers deleted; malformed fields throw `JsonException`.
- Shared `internal static class Json` (Models): `Options` with `DefaultIgnoreCondition = WhenWritingNull` (required — STJ writes nulls by default) **and `TypeInfoResolver = new ShouldSerializeJsonTypeInfoResolver()`**, `Serialize(object)`, `Deserialize<T>(string)`. `RequestJson` keeps only `ThrowOnOptionCollision`.
- `JsonConverter<T>` only where attributes can't express (user-endorsed).
- Public `Parse(string? json)` wrappers kept on result records (null → `JsonException`; non-object root → `JsonException` via STJ) — docs snippet `AlignmentResult.Parse(...)` stays valid.

## Resolver cleanup (`src/AudioCpp/ShouldSerializeJsonTypeInfoResolver.cs`)
Wire it via `Json.Options.TypeInfoResolver` (see Locked decisions). Fix before use:
- `public class` → `internal sealed class` (library plumbing convention; `SseReader`/`RequestJson` are internal).
- `MakeShouldSerializeFn` shadows its `type` parameter inside the lambda (`Type type = obj.GetType();`) → rename local to `valueType`.
- Drop redundant `isShouldSerializeMethodInvalid` (the `is null` check already covers it).
- **Bug fix, load-bearing:** `propName = property.Name` is the JSON name after `[JsonPropertyName]`, so attribute and method lookup fail for every snake_case property (`include_artifacts`, `busy_timeout_ms`, `model_ids`, `ttft_ms`). Resolve the CLR `PropertyInfo` per `JsonPropertyInfo` by scanning `type.GetProperties()` for a `[JsonPropertyName]` matching `property.Name` (fallback: plain name match), then look up `JsonIgnoreAttribute` and `ShouldSerialize{ClrName}()` on that `PropertyInfo`. Method names therefore follow the CLR property name, e.g. `ShouldSerializeIncludeArtifacts()`, `ShouldSerializeOptions()`.

## Conditional omission via `ShouldSerialize$PROPNAME()`
- Add `public bool ShouldSerializeOptions() => Options.Count > 0;` to `SpeechRequest`, `TranscriptionRequest`, `TaskRunRequest`, `TaskStreamRequest`, `TaskBatchRequest`, `RecipeRunRequest` — preserves the old wire behavior (empty options map omitted); goldens with options content are unaffected.
- Never-serialized properties keep `[JsonIgnore]` (Always; the resolver honors it): `SpeechJsonResult.Audio` (computed), `AudioCppTiming.TtftPresent` (computed → `TtftMs.HasValue`), `TranscriptionInputBase.AudioFile`/`AudioFileName`.
- No other `ShouldSerialize` methods: null omission comes from `WhenWritingNull`; `false` bools are written (accepted change).

## Converters (`src/AudioCpp/Models/Converters.cs`)
1. `ResponseFormat`, `StreamFormat` — write/read `Value` as string.
2. `SpeechStreamRequest` — write-only flattening: validate, `SerializeToElement(inner)`, copy properties, then `stream_format`, `return_text`; `Read` throws `NotSupportedException`.
3. Event structs stay public structs, get `[JsonConverter(typeof(...))]`, dispatch on `type`, unknown type → `JsonException`; internal `Parse(string json)` kept as thin wrapper (tests unchanged): `SpeechStreamEvent`, `TranscriptEvent`, `TaskStreamEvent`, `TaskBatchEvent`, `BatchTranscriptionEvent`.
4. `TaskStreamEventBody` — `text` vs nested `partial_text{text,language}`, `partial_text_snapshot{text}`.

## Per-file changes
- **Models/Common.cs** — records + attributes: `AudioCppTiming` (wire: wall_ms, audio_duration_ms, rtf, ttft_ms, first_audio_before_input_end, overlap_ms, request_start_to_first_audio_ms, input_end_ms; `TtftPresent` becomes computed `TtftMs.HasValue`, `[JsonIgnore]`), `AudioCppSegment`, `AudioCppWord`, `AudioCppSpeakerTurn`, `AudioArtifact` (meta strict: non-string values throw — accepted), `NamedAudioOutput`, `TaskResult` (words ← `words`), `AudioCppHealth`, `AudioCppModelParam`, `AudioCppModel` (JsonElement? props fine), `UnloadModelsResult`, `ModelLoadRequest` (attributes + Validate, WriteTo deleted), `ModelLoadResult`, `RecipeEntry`, `RecipeRole`. `Json` class added here.
- **Models/Speech.cs** — `SpeechRequest` record + attributes (`ResponseFormat?` already nullable → null omitted), `VoiceRefObject` declarative (Type/Path/Data attributes; WhenWritingNull reproduces path-only/base64-only shapes), `SpeechStreamRequest` + flattening converter + `Validate()` (return_text rule moves out of WriteTo), `SpeechJsonResult` (`Audio` getter `[JsonIgnore]`), results as records; `LiveSpeechRequest`/`QueryBuilder` unchanged (query-only).
- **Models/Transcription.cs** — `TranscriptionInputBase` attributes (`AudioPath`→audio, `Prompt`→text, `Stream`→stream, `BusyTimeoutMs`→busy_timeout_ms), `[JsonIgnore]` on `AudioFile`/`AudioFileName`; results records; `TranscriptEvent`/`BatchTranscriptionEvent` converters; `LiveTranscriptionRequest`/`BatchTranscriptionRequest`/`AudioUpload` unchanged (multipart/query).
- **Models/Tasks.cs** — `AudioArtifactInput` validation moves to `Validate()`; `TaskRequestBody`, `TaskRunRequest`, `TaskStreamRequest`, `TaskBatchRequest`, `RecipeRunRequest` records + attributes, `WriteTo` deleted; `TaskStreamEventBody` converter; event structs + converters.
- **Http/SseParsers.cs** — deleted.
- **AudioCppClient.cs** — `CreateJsonContent(object body)`; `SendJsonAsync<T>(method, path, object? body, CancellationToken)` → direct `Deserialize<T>`; internal envelope records for `{"object":"list","data":[...]}` (models/recipes), `{"voices":[...]}`, unload body DTO `{"model_ids":[...]}`; client methods unwrap envelopes (`?? Array.Empty` kept at unwrap to preserve return contract); `EnsureSuccess` error parsing unchanged.
- **Clients** — `Validate()` then `Json.Serialize(request)`; explicit `Validate()` added where WriteTo used to validate implicitly (TaskClients run/stream/batch/recipe, SpeechClient stream); `TranscribeStreamAsync` sets `request.Stream = true` before serialize; event parse call sites → `Json.Deserialize<T>(evt.Data)`.
- **Multipart + query builders** — unchanged.

## Tests to update
- `SpeechRequestSerializationTests` — `Serialize()` helper → `JsonSerializer.Serialize(request, Json.Options)`; `NullWriter` deleted; `ReturnTextWithoutSseThrows` and `ArtifactRejectsUnknownKindAndBothPayloadPath` call `Validate()`.
- `SseReaderTests` — `DiarizationDeltaAndDone` `ReadTiming` assertion → `Json.Deserialize<AudioCppTiming>`; `TtftPresent` false on explicit null still passes via computed property.
- `TransportTests` — 2 `SendJsonAsync` call sites (lines 24, 51) to new signature.
- All literal goldens verified to pass under declaration order + `WhenWritingNull` (SpeechRequest field order, options-after-typed, artifacts, recipe, package_id, stream flattening, `DoesNotContain stream_format`).

## Accepted behavior changes (no test pins them)
- Empty strings now serialized; empty option maps preserved as omitted via `ShouldSerializeOptions()` (see Conditional omission).
- `RecipeRunRequest.Stream`/`IncludeArtifacts` `false` now written instead of omitted.
- Leniency dropped: malformed server fields throw `JsonException`; `AudioArtifact.Meta` non-string values throw.
- `TtftPresent` doc sentence about "present true with null TtftMs" becomes `TtftPresent == TtftMs.HasValue`.

## Verification
1. Probe assumptions (throwaway test, then delete): `WhenWritingNull` skips null nullable-struct converter property; record constructor binding with attributes; `JsonProperty.WriteTo(writer)` exists; resolver wiring — `SpeechRequest` with empty `Options` serializes without an `"options"` key via `ShouldSerializeOptions()`, and `ShouldSerializeIncludeArtifacts()`-style CLR-name lookup works on a snake_case property (`RecipeRunRequest`) after the bug fix.
2. `dotnet build AudioCpp.slnx` both TFMs, 0 warnings (IDE0063/IDE0130 rules apply: no var, no fully qualified names, file-scoped ns).
3. Full suite on net10.0.
4. `dotnet pack -c Release`.
