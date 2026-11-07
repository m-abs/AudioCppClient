# Introduction

`Mabs.AudioCppClient` is a NuGet-packable .NET client for the
[audio.cpp](https://github.com/0xShug0/audio.cpp) server protocol. It speaks the wire
protocol directly (HTTP + SSE); there is no OpenAI dependency.

- Package: `Mabs.AudioCppClient`
- Target frameworks: `netstandard2.0` and `net10.0`
- Dependencies: `System.Text.Json` (plus `Microsoft.Bcl.AsyncInterfaces` on netstandard2.0)

One `AudioCppClient` instance maps to one server base URL, is thread-safe, and should be
shared. Endpoint groups sit on sub-clients: `Speech`, `Transcriptions`, `Alignments`,
`Tasks`, `Recipes`; readiness, model, voice and unload routes sit directly on the client.

See [Getting Started](getting-started.md) and [Common Tasks](common-tasks.md).
