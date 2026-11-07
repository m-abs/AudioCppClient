using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Mabs.AudioCpp;
using Mabs.AudioCpp.Http;
using Mabs.AudioCpp.Models;
using Mabs.AudioCpp.Models.Recipes;
using Mabs.AudioCpp.Models.Speech;
using Mabs.AudioCpp.Models.Tasks;
using Mabs.AudioCpp.Models.Transcription;

// Demo CLI proving the client library against a real audio.cpp server.
// Every command exits non-zero when its expected observable does not hold.
if (args.Length < 2)
{
    Console.Error.WriteLine("usage: demo <command> <baseUrl> [args…]");
    Console.Error.WriteLine("commands: health | voices | models | unload | load | load-package-error | recipes |");
    Console.Error.WriteLine("          speech | transcribe | transcribe-details |");
    Console.Error.WriteLine("          speech-error | speech-stream-error | tasks-run-error | recipe-run-error");
    return 2;
}

string command = args[0];
string baseUrl = args[1];

// Cold model loads under emulation can take minutes; give the transport room.
HttpClient http = new() { Timeout = TimeSpan.FromMinutes(30) };
AudioCppClient client = new(baseUrl, http);

try
{
    return command switch
    {
        "health" => await HealthAsync(),
        "voices" => await VoicesAsync(),
        "models" => await ModelsAsync(),
        "unload" => await UnloadAsync(),
        "load" => await LoadAsync(),
        "load-package-error" => await LoadPackageErrorAsync(),
        "recipes" => await RecipesAsync(),
        "speech" => await SpeechAsync(),
        "speech-stream" => await SpeechStreamAsync(),
        "transcribe" => await TranscribeAsync(),
        "transcribe-details" => await TranscribeDetailsAsync(),
        "speech-error" => await SpeechErrorAsync(),
        "speech-stream-error" => await SpeechStreamErrorAsync(),
        "tasks-run-error" => await TasksRunErrorAsync(),
        "recipe-run-error" => await RecipeRunErrorAsync(),
        _ => Fail("unknown command: " + command),
    };
}
catch (AudioCppException ex)
{
    Console.Error.WriteLine($"AudioCppException: status={ex.StatusCode} type={ex.ErrorType} message={ex.Message}");
    return 1;
}

async Task<int> HealthAsync()
{
    AudioCppHealth health = await client.GetHealthAsync();
    Console.WriteLine($"status={health.Status} backend={health.Backend} version={health.Version} git={health.Git} models={health.Models} ui={health.Ui} ui_management={health.UiManagement}");
    return health.Status == "ok" ? 0 : Fail("status not ok");
}

async Task<int> VoicesAsync()
{
    IReadOnlyList<string> voices = await client.ListVoicesAsync();
    Console.WriteLine("voices=[" + string.Join(", ", voices) + "]");
    return voices.Count > 0 ? 0 : Fail("no voices returned");
}

async Task<int> ModelsAsync()
{
    bool includeParams = Array.IndexOf(args, "--include-params") >= 0;
    IReadOnlyList<AudioCppModel> models = await client.ListModelsAsync(includeParams: includeParams, includeSessionOptions: includeParams);
    Console.WriteLine($"models={models.Count}");
    foreach (AudioCppModel model in models)
    {
        Console.WriteLine($"  id={model.Id} configured={model.Configured} loaded={model.Loaded} family={model.Family} task={model.Task} path={model.Path} device={model.Device} params={model.Params?.Count.ToString() ?? "-"}");
    }

    return 0;
}

async Task<int> UnloadAsync()
{
    string id = args.Length > 2 ? args[2] : "unknown-id";
    UnloadModelsResult result = await client.UnloadModelsAsync(new[] { id });
    Console.WriteLine($"unloaded=[{string.Join(", ", result.Unloaded)}] not_found=[{string.Join(", ", result.NotFound)}]");
    return result.NotFound.Count == 1 && result.NotFound[0] == id ? 0 : Fail("expected not_found to report the unknown id");
}

async Task<int> LoadAsync()
{
    // load <baseUrl> <id> <path> <family> <task> <mode>
    if (args.Length < 7)
    {
        return Fail("load requires: id path family task mode");
    }

    ModelLoadResult load = await client.LoadModelAsync(new ModelLoadRequest
    {
        Id = args[2],
        Path = args[3],
        Family = args[4],
        Task = args[5],
        Mode = args[6],
    });
    Console.WriteLine($"loaded id={load.Id} loaded={load.Loaded} reconfigured={load.Reconfigured}");
    return 0;
}

async Task<int> RecipesAsync()
{
    IReadOnlyList<RecipeEntry> recipes = await client.ListRecipesAsync();
    Console.WriteLine($"recipes={recipes.Count}");
    foreach (RecipeEntry recipe in recipes)
    {
        Console.WriteLine($"  id={recipe.Id} roles=[{string.Join(",", Enumerable.Select(recipe.Roles, r => r.Name))}]");
    }

    return 0;
}

async Task<int> SpeechAsync()
{
    // speech <baseUrl> <model> <text> [--out file] [--voice name] [--option name=value …]
    if (args.Length < 4)
    {
        return Fail("speech requires: model text");
    }

    SpeechRequest request = new() { Model = args[2], Input = args[3] };
    string outPath = "/tmp/speech.wav";
    for (int i = 4; i < args.Length; i++)
    {
        if (args[i] == "--out" && i + 1 < args.Length)
        {
            outPath = args[++i];
        }
        else if (args[i] == "--voice" && i + 1 < args.Length)
        {
            request.Voice = args[++i];
        }
        else if (args[i] == "--option" && i + 1 < args.Length)
        {
            string[] pair = args[++i].Split(['='], 2);
            if (pair.Length == 2)
            {
                if (request.Options == null)
                {
                    request.Options = [];
                }

                request.Options[pair[0]] = pair[1];
            }
        }
    }

    SpeechResult result = await client.Speech.SynthesizeAsync(request);
    File.WriteAllBytes(outPath, result.Audio);
    string magic = result.Audio.Length >= 4 ? Encoding.ASCII.GetString(result.Audio, 0, 4) : string.Empty;
    Console.WriteLine($"wrote {outPath} bytes={result.Audio.Length} content_type={result.ContentType} magic={magic} wall_ms={result.Timing?.WallMs} rtf={result.Timing?.Rtf}");
    return magic == "RIFF" ? 0 : Fail("WAV magic missing");
}

async Task<int> SpeechStreamAsync()
{
    // speech-stream <baseUrl> <model> <text> [--out pcm]
    if (args.Length < 4)
    {
        return Fail("speech-stream requires: model text");
    }

    string outPath = "/tmp/speech.pcm";
    for (int i = 4; i + 1 < args.Length; i++)
    {
        if (args[i] == "--out")
        {
            outPath = args[i + 1];
        }
    }

    long bytes = 0;
    int deltas = 0;
    bool done = false;
    using (FileStream pcm = File.Create(outPath))
    {
        await foreach (SpeechStreamEvent evt in client.Speech.StreamAsync(new SpeechStreamRequest
        {
            Request = new SpeechRequest { Model = args[2], Input = args[3] },
        }))
        {
            if (evt.Kind == SpeechStreamEventKind.AudioDelta && evt.Pcm != null)
            {
                await pcm.WriteAsync(evt.Pcm, 0, evt.Pcm.Length);
                bytes += evt.Pcm.Length;
                deltas++;
            }
            else if (evt.Kind == SpeechStreamEventKind.AudioDone)
            {
                done = true;
                Console.WriteLine($"audio.done ttft_ms={evt.Timing?.TtftMs}");
            }
        }
    }

    Console.WriteLine($"wrote {outPath} pcm_bytes={bytes} deltas={deltas} done_event={done}");
    return done && bytes > 0 ? 0 : Fail("stream produced no audio");
}

async Task<int> TranscribeAsync()
{
    // transcribe <baseUrl> <model> <wavpath> [--multipart]
    if (args.Length < 4)
    {
        return Fail("transcribe requires: model wav-path");
    }

    TranscriptionResult result;
    if (Array.IndexOf(args, "--multipart") >= 0)
    {
        result = await client.Transcriptions.TranscribeAsync(
            new TranscriptionRequest { Model = args[2], AudioFile = File.OpenRead(args[3]), AudioFileName = Path.GetFileName(args[3]) });
    }
    else
    {
        result = await client.Transcriptions.TranscribeAsync(
            new TranscriptionRequest { Model = args[2], AudioPath = args[3] });
    }

    Console.WriteLine($"text={result.Text}");
    Console.WriteLine($"timing wall_ms={result.Timing?.WallMs} audio_duration_ms={result.Timing?.AudioDurationMs} rtf={result.Timing?.Rtf}");
    return !string.IsNullOrWhiteSpace(result.Text) ? 0 : Fail("empty transcript");
}

async Task<int> TranscribeDetailsAsync()
{
    if (args.Length < 4)
    {
        return Fail("transcribe-details requires: model wav-path");
    }

    TranscriptionDetailsRequest details = new() { Model = args[2] };
    if (Array.IndexOf(args, "--multipart") >= 0)
    {
        details.AudioFile = File.OpenRead(args[3]);
        details.AudioFileName = Path.GetFileName(args[3]);
    }
    else
    {
        details.AudioPath = args[3];
    }

    TranscriptionDetailsResult result = await client.Transcriptions.TranscribeDetailsAsync(details);
    Console.WriteLine($"text={result.Text}");
    Console.WriteLine($"language={result.Language} words={result.Words?.Count ?? 0} segments={result.Segments?.Count ?? 0} speaker_turns={result.SpeakerTurns?.Count ?? 0} sample_rate={result.SampleRate}");
    if (result.Words is { Count: > 0 })
    {
        AudioCppWord word = result.Words[0];
        Console.WriteLine($"first word: {word.Word} [{word.Start}-{word.End}]");
    }

    return result.Words is { Count: > 0 } ? 0 : Fail("expected a non-empty words array");
}

async Task<int> SpeechErrorAsync()
{
    try
    {
        await client.Speech.SynthesizeAsync(new SpeechRequest { Model = "definitely-not-configured", Input = "x" });
        return Fail("expected AudioCppException");
    }
    catch (AudioCppException ex)
    {
        Console.WriteLine($"ok: status={ex.StatusCode} type={ex.ErrorType} message={ex.Message}");
        return ex.StatusCode == 500 && ex.Message.Contains("unknown model id") ? 0 : Fail("unexpected error shape");
    }
}

async Task<int> SpeechStreamErrorAsync()
{
    // Streaming an unknown model: the server reports before the stream starts.
    try
    {
        await foreach (SpeechStreamEvent evt in client.Speech.StreamAsync(new SpeechStreamRequest
        {
            Request = new SpeechRequest { Model = "definitely-not-configured", Input = "x" },
        }))
        {
            Console.WriteLine("event: " + evt.Kind);
        }

        return Fail("expected an error");
    }
    catch (AudioCppException ex)
    {
        Console.WriteLine($"ok: error surfaced for streamed unknown model: status={ex.StatusCode} message={ex.Message}");
        return 0;
    }
}

async Task<int> TasksRunErrorAsync()
{
    try
    {
        await client.Tasks.RunAsync(new TaskRunRequest { Model = "definitely-not-configured", Request = new TaskRequestBody { Text = "x" } });
        return Fail("expected AudioCppException");
    }
    catch (AudioCppException ex)
    {
        Console.WriteLine($"ok: status={ex.StatusCode} message={ex.Message}");
        return 0;
    }
}

async Task<int> LoadPackageErrorAsync()
{
    try
    {
        await client.LoadModelAsync(new ModelLoadRequest { Id = "pkgload", PackageId = "definitely-not-installed" });
        return Fail("expected AudioCppException");
    }
    catch (AudioCppException ex)
    {
        Console.WriteLine($"ok: status={ex.StatusCode} type={ex.ErrorType} message={ex.Message}");
        return ex.StatusCode == 404 && ex.ErrorType == "not_found" ? 0 : Fail("expected 404 not_found");
    }
}

async Task<int> RecipeRunErrorAsync()
{
    try
    {
        RecipeRunRequest request = new() { Recipe = "definitely-not-a-recipe" };
        request.Models["tts"] = "definitely-not-configured";
        request.Models["converter"] = "definitely-not-configured";
        request.Request.Text = "x";
        await client.Recipes.RunAsync(request);
        return Fail("expected AudioCppException");
    }
    catch (AudioCppException ex)
    {
        Console.WriteLine($"ok: status={ex.StatusCode} message={ex.Message}");
        return ex.Message.Contains("unknown recipe") ? 0 : Fail("unexpected message");
    }
}

int Fail(string message)
{
    Console.Error.WriteLine("FAIL: " + message);
    return 1;
}