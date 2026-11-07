# Alignment

Forced alignment of a known transcript against uploaded WAV bytes
(`POST /v1/audio/alignments`, multipart only). The model must be configured with
`task: "align"`, `mode: "offline"`.

## Align a transcript

```csharp
using Mabs.AudioCpp;
using Mabs.AudioCpp.Models.Alignment;
using Mabs.AudioCpp.Models.Transcription;

AlignmentResult aligned = await client.Alignments.RunAsync(new AlignmentRequest
{
    Model = "qwen3-align",
    Text = "hello world",
    Language = "en", // required by the Qwen3 Forced Aligner
    AudioFile = File.ReadAllBytes("hello.wav"),
    AudioFileName = "hello.wav",
});

foreach (AudioCppWord word in aligned.Words)
{
    Console.WriteLine($"{word.Word} {word.Start}-{word.End} (samples {word.StartSample}-{word.EndSample}) conf {word.Confidence}");
}
```

`Words` carries seconds (`Start`/`End`) plus sample offsets (`StartSample`/`EndSample`);
`Timing` is the run timing.

## Align and reuse the timestamps

Word timestamps from an alignment feed a caption or karaoke pipeline:

```csharp
AlignmentResult aligned = await client.Alignments.RunAsync(new AlignmentRequest
{
    Model = "crisperwhisper",
    Text = transcript,
    AudioFile = wavBytes,
    AudioFileName = "take.wav",
});

foreach (AudioCppWord word in aligned.Words)
{
    if (word.StartSample is long start && word.EndSample is long end)
        Console.WriteLine($"{start}\t{end}\t{word.Word}");
}
```

There is no streaming variant for alignment — the route is multipart, single-shot. See
[Error handling](error-handling.md) for failure shapes.
