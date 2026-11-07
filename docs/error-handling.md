# Error Handling

Every failure throws `AudioCppException` with the wire-reported message, error type and
HTTP status.

```csharp
using Mabs.AudioCpp;
using Mabs.AudioCpp.Http;

try
{
    SpeechResult result = await client.Speech.SynthesizeAsync(new SpeechRequest { Model = "nope", Input = "hi" });
}
catch (AudioCppException ex)
{
    Console.Error.WriteLine($"HTTP {ex.StatusCode} ({ex.ErrorType}): {ex.Message}");
}
```

## Status codes (verified against v0.9.4)

| Situation | Status | `type` |
|---|---|---|
| Unknown model id (speech, tasks routes) | 500 | `server_error` |
| Invalid request (live routes, option collisions) | 400 | `invalid_request_error` |
| `models/load` without `--ui-management` | 403 | `forbidden` |
| Unknown or not-installed `package_id` (`models/load`) | 404 | `not_found` |
| Busy model | — | `server_busy` |
| Memory pressure | — | `insufficient_memory` |

Older docs say unknown model → 404 on speech; the live server supersedes that with 500.

## SSE error events

Error events arrive as `{"type":"error","error":{"message":…}}` and end the stream with
**no** `[DONE]`. `SseReader` throws `AudioCppException` mid-iteration on those; for SSE
error events `StatusCode` and `ErrorType` are null:

```csharp
try
{
    await foreach (TranscriptEvent evt in client.Transcriptions.TranscribeStreamAsync(request))
    {
        Console.Write(evt.Text);
    }
}
catch (AudioCppException ex)
{
    // ex.StatusCode == null for mid-stream SSE errors
    Console.Error.WriteLine($"stream error: {ex.Message}");
}
```

## Non-JSON bodies

When the body is not an error envelope, `Message` carries the raw body text.

See the task pages for where each route lives: [Speech Synthesis](speech-synthesis.md),
[Transcription](transcription.md), [Alignment](alignment.md), [Tasks](tasks.md),
[Recipes](recipes.md).
