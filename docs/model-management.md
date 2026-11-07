# Model Management

Readiness, model, voice and unload routes sit directly on `AudioCppClient`.

## Health

```csharp
using Mabs.AudioCpp;
using Mabs.AudioCpp.Models;

AudioCppHealth health = await client.GetHealthAsync();
Console.WriteLine($"{health.Status} on {health.Backend}, {health.Models} models, version {health.Version}");
```

## List models

```csharp
IReadOnlyList<AudioCppModel> models = await client.ListModelsAsync(includeParams: true, includeSessionOptions: true);

foreach (AudioCppModel model in models)
{
    Console.WriteLine($"{model.Id} configured={model.Configured} loaded={model.Loaded} family={model.Family} task={model.Task} path={model.Path} device={model.Device}");
    foreach (AudioCppModelParam param in model.Params ?? Array.Empty<AudioCppModelParam>())
        Console.WriteLine($"  {param.Name} ({param.Type}) required={param.Required} default={param.Default}");
}
```

## List voices

```csharp
IReadOnlyList<string> voices = await client.ListVoicesAsync("kokoro"); // null = all models
Console.WriteLine(string.Join(", ", voices));
```

## Load a model

Requires the server started with `--ui-management` (otherwise 403). `Path` and `PackageId`
are either-or; both set or a non-string/empty `package_id` is a client-side 400, an
unknown or not-installed package a server 404.

```csharp
ModelLoadResult loaded = await client.LoadModelAsync(new ModelLoadRequest
{
    Id = "kokoro",
    PackageId = "kokoro", // replaces Path on native model-manager builds
    Family = "kokoro",
    Task = "tts",
    Mode = "offline",
    Device = 0,
});

Console.WriteLine($"{loaded.Id} loaded={loaded.Loaded} reconfigured={loaded.Reconfigured}");
```

## Unload

```csharp
UnloadModelsResult unloaded = await client.UnloadModelsAsync(new[] { "kokoro", "unknown-id" });
Console.WriteLine($"unloaded: {string.Join(", ", unloaded.Unloaded)}");
Console.WriteLine($"not found: {string.Join(", ", unloaded.NotFound)}");

// Or everything at once:
UnloadModelsResult all = await client.UnloadAllModelsAsync();
```

See [Error handling](error-handling.md) for the status codes these routes return.
