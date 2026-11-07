# Recipes

Multi-model recipes against `POST /v1/recipes/run`. Role-specific option prefixes
(`tts.`, `avatar.`, `conversation.`, …) are the caller's responsibility: put them directly
in `TaskRequestBody.Options` keys.

## Run a recipe

```csharp
using Mabs.AudioCpp;
using Mabs.AudioCpp.Models;

RecipeRunRequest recipe = new()
{
    Recipe = "audiobook",
    Request = new TaskRequestBody
    {
        Text = "Once upon a time.",
        Options = new Dictionary<string, string>
        {
            ["tts.voice"] = "af_bella",
            ["tts.language"] = "en",
        },
    },
    BusyTimeoutMs = 120000,
};

// One entry per role the recipe requires.
recipe.Models["tts"] = "kokoro";

TaskResult result = await client.Recipes.RunAsync(recipe);
Console.WriteLine(result.Text);
```

## Stream a recipe

`RunStreamAsync` forces `stream: true` and uses the same
`task.stream.event` / `task.stream.done` envelope as generic task streaming; with
`IncludeArtifacts` artifact-only events count toward `ttft_ms` when timing is enabled.

```csharp
RecipeRunRequest recipe = new()
{
    Recipe = "audiobook",
    IncludeArtifacts = true,
};
recipe.Models["tts"] = "kokoro";

await foreach (TaskStreamEvent evt in client.Recipes.RunStreamAsync(recipe))
{
    if (evt.Kind == TaskStreamEventKind.Done)
    {
        foreach (AudioArtifact artifact in evt.Result?.Artifacts ?? Array.Empty<AudioArtifact>())
            Console.WriteLine($"{artifact.Kind} {artifact.Id} {artifact.Payload.Length} bytes");
    }
}
```

`RunAsync` rejects `Stream = true` — use `RunStreamAsync` for that.

## Discover recipes

```csharp
foreach (RecipeEntry entry in await client.ListRecipesAsync())
{
    Console.WriteLine($"{entry.Id}: {entry.Description}");
    foreach (RecipeRole role in entry.Roles)
        Console.WriteLine($"  role {role.Name} task={role.Task} mode={role.Mode}");
}
```

See [Error handling](error-handling.md) for failure shapes.
