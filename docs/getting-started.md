# Getting Started

## Install

The package is published to GitHub Packages, not nuget.org, so restore needs the GitHub
feed and a token with `read:packages` access. `gh auth token` prints the token from your
current GitHub CLI login; a fine-grained PAT works too if it can read packages for
`m-abs`. `--store-password-in-clear-text` is required — GitHub Packages doesn't support
encrypted credential storage.

```bash
dotnet nuget add source --username m-abs --password "$(gh auth token)" --store-password-in-clear-text --name github https://nuget.pkg.github.com/m-abs/index.json
dotnet add package Mabs.AudioCppClient
```

This feed token is only for NuGet restore; it is not the client's `apiKey` bearer token.

## Create a client

```csharp
using Mabs.AudioCpp;

AudioCppClient client = new("http://127.0.0.1:8080");
```

The audio.cpp server default listening port is **8080** (`--port`); the examples assume a
server on that port. Pass your own `HttpClient` to control proxies, timeouts or TLS; for the live routes it must
be backed by a handler with full-duplex HTTP/1.1 support (the default handler on .NET Core
qualifies; browser/WASM handlers do not). `apiKey` is sent as a bearer token on every
request — the server ignores it today, frontends in front of it may require it.

## Check the server

```csharp
using Mabs.AudioCpp;

AudioCppClient client = new("http://127.0.0.1:8080");

AudioCppHealth health = await client.GetHealthAsync();
Console.WriteLine($"{health.Status} on {health.Backend}");

foreach (AudioCppModel model in await client.ListModelsAsync(includeParams: true))
{
    Console.WriteLine($"{model.Id} configured={model.Configured}");
}
```

Every failure throws `AudioCppException` with the server-reported `Message`, `ErrorType`
and `StatusCode` — see [Error Handling](error-handling.md).
