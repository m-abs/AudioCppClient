# Tasks

Generic framework runs against `POST /v1/tasks/run`, `/v1/tasks/stream` and
`/v1/tasks/batch`, for any configured framework model. `TaskRequestBody.Options` is the
stringly-typed escape hatch mirroring the server's option map; a typed property and an
options key that collide throw locally before sending.

## Run one request

```csharp
using Mabs.AudioCpp;
using Mabs.AudioCpp.Models.Transcription;
using Mabs.AudioCpp.Models.Tasks;

TaskResult result = await client.Tasks.RunAsync(new TaskRunRequest
{
    Model = "pyannote",
    Request = new TaskRequestBody
    {
        AudioPath = "/data/meeting.wav",
        Options = new Dictionary<string, string> { ["spk_detect"] = "auto" },
    },
    BusyTimeoutMs = 60000,
});

foreach (AudioCppSpeakerTurn turn in result.SpeakerTurns ?? Array.Empty<AudioCppSpeakerTurn>())
{
    Console.WriteLine($"{turn.SpeakerId}: {turn.Text}");
}
```

Inline audio bytes without a server-local path (Base64 form):

```csharp
TaskResult result = await client.Tasks.RunAsync("mossformer2", File.ReadAllBytes("meeting.wav"));
```

## Stream events

`StreamFormat = "sse"` (default) streams events as the model produces them; `null` returns
one buffered JSON body which the client replays as the same event sequence. Any other
string is a server 400. The stream route ignores `audio_base64` — use `AudioPath`.

```csharp
TaskStreamRequest stream = new()
{
    Model = "kokoro",
    Request = new TaskRequestBody { Text = "Hello.", VoiceId = "af_bella" },
    IncludeArtifacts = true, // events gain an artifacts array when the build supports it
};

await foreach (TaskStreamEvent evt in client.Tasks.StreamAsync(stream))
{
    if (evt.Kind == TaskStreamEventKind.Event)
    {
        Console.Write(evt.Event?.PartialText);
    }
    else if (evt.Kind == TaskStreamEventKind.Done)
    {
        Console.WriteLine($"done, ttft {evt.Result?.Timing?.TtftMs} ms");
    }
}
```

## Batch

Native offline batch; results stream per entry with the original request index preserved,
per-result timing null by design.

```csharp
TaskBatchRequest batch = new() { Model = "parakeet-tdt-0.6b-v3" };
batch.Requests.Add(new TaskRequestBody { AudioPath = "/data/a.wav" });
batch.Requests.Add(new TaskRequestBody { AudioPath = "/data/b.wav", Options = new Dictionary<string, string> { ["language"] = "da" } });

await foreach (TaskBatchEvent evt in client.Tasks.BatchAsync(batch))
{
    if (evt.Kind == TaskBatchEventKind.Result)
        Console.WriteLine($"[{evt.Index}] {evt.Result?.Text}");
}
```

## Input artifacts

Artifacts are delivered to the model in array order; payload bytes or a server-local path,
one of the two:

```csharp
TaskRunRequest run = new()
{
    Model = "kokoro",
    Request = new TaskRequestBody
    {
        Text = "Hello.",
        Artifacts = new[]
        {
            new AudioArtifactInput { Id = "spk", Kind = ArtifactKind.SpeakerEmbedding, Path = "/data/spk.bin" },
            new AudioArtifactInput { Id = "style", Kind = ArtifactKind.StyleEmbedding, Payload = File.ReadAllBytes("style.bin") },
        },
    },
};

TaskResult result = await client.Tasks.RunAsync(run);
```

See [Recipes](recipes.md) for multi-model runs and [Error handling](error-handling.md) for
failure shapes.
