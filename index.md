---
_layout: landing
---

# Mabs.AudioCppClient

A NuGet-packable .NET client for the [audio.cpp](https://github.com/0xShug0/audio.cpp)
server protocol — speech synthesis, transcription, forced alignment, generic tasks,
recipes, and the live duplex routes. It speaks the wire protocol directly (HTTP + SSE),
has no OpenAI dependency, and targets `netstandard2.0` and `net10.0`.

- [Getting Started](docs/getting-started.md) — install (GitHub Packages feed), create a client
- [Common Tasks](docs/common-tasks.md) — one page per task area with runnable examples:
  [Speech Synthesis](docs/speech-synthesis.md), [Transcription](docs/transcription.md),
  [Alignment](docs/alignment.md), [Tasks](docs/tasks.md), [Recipes](docs/recipes.md),
  [Error Handling](docs/error-handling.md), [Model Management](docs/model-management.md)
- [API Reference](api/Mabs.AudioCpp.yml) — per-type docs generated from the source

The package is published to GitHub Packages as `Mabs.AudioCppClient`
(`https://nuget.pkg.github.com/m-abs/index.json`).
