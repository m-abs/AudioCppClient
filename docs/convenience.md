# Convenience clients

The convenience layer is generated from the vendored audio.cpp model specs
(`tools/model-specs/`, emitted by `tools/generate-convenience.py`). For every family whose
spec declares a `tts`, `clone`, or `design` task (106 family-task pairs), the generator emits
one sealed client class and one sealed request record bound to the matching task interface
(`ITtsClient`, `ICloneClient`, `IDesignClient`).

Each record exposes one typed property per spec option — enums for closed value sets,
`long?`/`double?`/`bool?`/`string?` otherwise — pre-initialized to the web-ui default
(`model_params.json` wins over the spec default on conflicts; capability-gated options are
not pre-initialized, since any set value fails `Validate()`). Options equal to the spec
default are not serialized — the server applies its own default, so the wire is equivalent;
a conflicting web-ui default is serialized.
`Validate()` enforces required options, min/max bounds, enum membership, and capability gates
(`voice_id` only where the task declares `built_in_voices`, `voice_ref` only where it declares
`speaker_reference`).

## Run one convenience request

```csharp
using Mabs.AudioCpp;
using Mabs.AudioCpp.Enums;
using Mabs.AudioCpp.Models.Tasks;
using Mabs.AudioCpp.Tts;

AudioCppClient client = new AudioCppClient("http://127.0.0.1:8080");

PlapreTtsClient plapre = new PlapreTtsClient(client);

TaskResult result = await plapre.SpeakAsync(new PlapreTtsRequest
{
    Text = "Hej",
    VoiceId = PlapreVoiceId.Ida,
});
```

The wire body for that call is exactly

```json
{"model":"plapre_3_bf16","request":{"text":"Hej","options":{"voice_id":"ida"}}}
```

`Model` defaults to the spec default package and must serve the class's task —
`/v1/tasks/run` carries no task field; the server resolves it from the model entry.
`StreamAsync` exists on the concrete classes only, not on the interfaces.

## Naming

Family names are PascalCased per underscore segment, with the trailing segment stripped when
it equals the task name: `kokoro_tts` → `KokoroTtsClient`/`KokoroTtsRequest`, `plapre` →
`PlapreTtsClient`. Namespaces: `Mabs.AudioCpp.Tts` / `.Clone` / `.Design` for clients,
`Mabs.AudioCpp.Models.<Task>` for records, `Mabs.AudioCpp.Enums` for enum options.

Design families that declare no `instruct` option route `Instruction` by prefixing the text
(`"(whisper) hello"`); families with an `instruct` option send it through the options map.

## Regenerating

On an audio.cpp version bump, re-fetch `model_specs/*.json` and
`webui/configs/model_params.json` for the new tag into `tools/model-specs/`, re-run
`python3 tools/generate-convenience.py`, diff the generated files, rebuild and run the tests.
The generator is deterministic and idempotent, and the `.editorconfig` formatting rules apply
to generated code.