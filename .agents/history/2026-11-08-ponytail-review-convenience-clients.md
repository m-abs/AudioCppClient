# Ponytail review — branch `convenience-clients` (1 commit, 490 files, +68,524)

**What this change does:** A Python generator (`tools/generate-convenience.py`) reads vendored
audio.cpp model specs and emits 106 sealed client classes + 106 request records + 116 enums for
the tts/clone/design tasks, plus 164 golden/validation tests. Hand-written: three interfaces,
three base request records, and the shared `ToTaskRequestBody` that rides the existing
`Tasks.RunAsync`. Checked: build is 0 warnings/0 errors, all 296 tests pass, generator re-run
produces an empty diff (idempotent). Load assumed: one caller against one server — no scale
findings; dictionary insertion order is deterministic and the generator is sorted.

## Must fix

1. **The default-omission guard drops the web-ui default, so the server runs its spec default
   instead** (`tools/generate-convenience.py:117-121` + `build_options_body`, visible in
   `src/AudioCpp/Models/Tts/Audio8TtsRequest.cs:111`)
   - **What this is:** `BuildOptions()` emits an option only when its value differs from the
     *resolved* default, and the resolved default is the web-ui `model_params.json` value when it
     exists. The comment and ADR 2 justify this by saying the server applies its own default, so
     the wire is equivalent.
   - **Problem:** The server applies the **spec** default. Where the two disagree, equivalence
     breaks. Concrete case: `Audio8TtsRequest { Text = "hello", Temperature = 0.7 }` — 0.7 is the
     documented web-ui default, spec default is 0.8. The guard `Temperature != 0.7` omits it, the
     wire is `{"model":"audio8_asr_...","request":{"text":"hello"}}`, and the server generates at
     0.8. The user asked for 0.7 and got 0.8. This hits ~24 options across 12 generated families
     (audio8_tts ×3, minimax_h3 ×2, cosyvoice3, miotts ×5, sopro_tts ×6, vevo2 ×6, vibevoice ×2,
     magpie_tts, lfm2_audio, irodori_tts, moss_ttsd, voxcpm1). No golden test catches it because
     `test_value()` never picks a value equal to the default — the equality branch is exactly the
     broken one.
   - **Fix:** Make the guard compare against the **spec** default (`opt["default"]`), not the
     resolved default. Omission stays provably equivalent when they agree; when they conflict, the
     web-ui default is serialized — which is what ADR 1 promised. One line in `option_info` plus
     the golden literal, then regenerate.
   - **If we skip it:** ADR 1 ("web-ui wins on conflicts") is silently voided for those families;
     outputs differ from the documented defaults and nobody notices until audio sounds wrong.

## Should fix

2. **Docs claim properties are pre-initialized to the default; the generator emits none**
   (`docs/convenience.md:10`, `README.md:100`, `record_file` in `tools/generate-convenience.py:410`)
   - **What this is:** The plan decision says defaults are pre-initialized; `record_file` emits
     `public double? Temperature { get; init; }` with no initializer — the default only exists as
     the omission guard.
   - **Problem:** `new Audio8TtsRequest().Temperature` is `null`, not 0.7. The README/docs
     sentence is false as written, and combined with finding 1 the default request gets the spec
     default, not the documented one.
   - **Fix:** Emit `= {init}` in `record_file` when `init` is not None. One line; regenerate.
     With finding 1's fix, the wire then actually carries the documented default.
   - **If we skip it:** Docs lie; users reading `request.Temperature` see null and get server
     behavior they can't predict.

3. **Ten capability-gate rules have no test** (`validate_entry` in
   `tools/generate-convenience.py:316` skips gated options)
   - **What this is:** Generated `Validate()` throws for `voice_id`/`voice_ref` when the task's
     capabilities lack `built_in_voices`/`speaker_reference` (e.g.
     `PlapreCloneRequest { VoiceId = PlapreVoiceId.Ida }` → "plapre clone does not accept
     voice_id").
   - **Problem:** Zero tests assert any gate. Flip `GATE_CAP` or read `capabilities` at spec level
     instead of per-task, and all 296 tests still pass. Gates are real branch logic with no
     failing-before test.
   - **Fix:** In `validate_entry`, also accept the first gated option: set it, assert
     `ArgumentException` with the gate message. Roughly 10 extra tests, generated.
   - **If we skip it:** A generator edit can silently open or close all gates with a green suite.

4. **README snippet does not compile as written** (`README.md:104-113`)
   - **What this is:** The convenience snippet uses `TaskResult` but omits
     `using Mabs.AudioCpp.Models.Tasks;`.
   - **Problem:** Repo rule is that README snippets run as written; this one fails to compile.
   - **Fix:** Add the one missing `using` line.
   - **If we skip it:** Copy-paste fails for the first reader.

## Nice to have

5. **`fmt_num` formats floats with Python `repr`, which differs from C# for exponent-form
   doubles** (`tools/generate-convenience.py:68-71`)
   - **What this is:** Golden tests compare Python-formatted strings against C#
     `ToString(InvariantCulture)`.
   - **Problem:** `1e-05` in Python vs `1E-05` in C#. Current data has no exponent-form defaults
     (only `min` values inside messages, which are embedded as literals on both sides, so they
     agree), so nothing fails today; a future spec default like `1e-05` breaks the golden test.
   - **Fix:** One line: format golden floats the way C# does. Not worth chasing now.

6. ~**Stray `convenience classes.md` at the repo root** — the original user note, untracked. Repo
   rule puts agent work under `.agents/`; the completed plan is already in `.agents/history/`.
   Delete it.~ - Already deleted.

## Verdict

Fix 1 and 2 first (one generator edit + one line, then regenerate; golden tests change only for
the conflict families). Then 3 and 4.

Lean: nothing material to delete — the generator is stdlib-only, no speculative abstraction, and
the 46-line client files are the deliverable. Only the `q` alias (`def q(text): return
csharp_string(text)`) is redundant, -1 line.

Not checked: live E2E demo run (recorded in the plan as run on a native build, not re-run here);
server-side merge of `options.text` vs top-level `text` for `index_tts2` (assumed per plan, no
test).
