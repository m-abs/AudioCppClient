using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using mabs.AudioCpp.Http;
using mabs.AudioCpp.Models;

namespace mabs.AudioCpp;

/// <summary>Forced alignment against <c>POST /v1/audio/alignments</c> (multipart only).</summary>
public sealed class AlignmentClient
{
    private readonly AudioCppClient _client;

    internal AlignmentClient(AudioCppClient client)
    {
        _client = client;
    }

    /// <summary>Align a known transcript against uploaded WAV bytes.</summary>
    public async Task<AlignmentResult> RunAsync(AlignmentRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        request.Validate();
        using HttpRequestMessage httpRequest = new(HttpMethod.Post, _client.BuildUri("v1/audio/alignments"));
        httpRequest.Content = Multipart.BuildAlignment(request);
        return await _client.ReadJsonAsync(httpRequest, AlignmentResult.Parse, cancellationToken).ConfigureAwait(false);
    }
}

/// <summary>
/// Generic framework tasks against <c>POST /v1/tasks/run</c>, <c>/v1/tasks/stream</c> and
/// <c>/v1/tasks/batch</c>.
/// </summary>
public sealed class TaskClient
{
    private readonly AudioCppClient _client;

    internal TaskClient(AudioCppClient client)
    {
        _client = client;
    }

    /// <summary>Run one generic request.</summary>
    public async Task<TaskResult> RunAsync(TaskRunRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        using HttpRequestMessage httpRequest = _client.NewRequest(HttpMethod.Post, "v1/tasks/run");
        httpRequest.Content = AudioCppClient.CreateJsonContent(writer => request.WriteTo(writer));
        return await _client.ReadJsonAsync(httpRequest, SseParsers.TaskResult, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Run one generic request with inline audio bytes (Base64 form).</summary>
    public Task<TaskResult> RunAsync(string model, byte[] audioBytes, CancellationToken cancellationToken = default)
    {
        return RunAsync(
            new TaskRunRequest
            {
                Model = model,
                Request = new TaskRequestBody { AudioBase64 = audioBytes },
            },
            cancellationToken);
    }

    /// <summary>
    /// Stream one generic request. With <see cref="TaskStreamRequest.StreamFormat"/> =
    /// <c>sse</c> (default) events arrive as the model produces them; with
    /// <see langword="null"/> the server returns one buffered JSON body, which this method
    /// replays as the same event sequence. A run succeeded when
    /// <see cref="TaskStreamEventKind.Done"/> arrives; mid-stream failures throw
    /// <see cref="AudioCppException"/> from the SSE error event.
    /// </summary>
    public async IAsyncEnumerable<TaskStreamEvent> StreamAsync(TaskStreamRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        HttpRequestMessage httpRequest = _client.NewRequest(HttpMethod.Post, "v1/tasks/stream");
        httpRequest.Content = AudioCppClient.CreateJsonContent(writer => request.WriteTo(writer));
        if (request.StreamFormat == null)
        {
            // Buffered JSON response: {"events":[…],"result":{…}}.
            using (httpRequest)
            {
                string body = await _client.ReadBodyAsync(httpRequest, cancellationToken).ConfigureAwait(false);
                using JsonDocument doc = JsonDocument.Parse(body);
                JsonElement root = doc.RootElement;
                if (root.TryGetProperty("events", out JsonElement events) &&
                    events.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement evt in events.EnumerateArray())
                    {
                        yield return TaskStreamEvent.FromEvent(TaskStreamEventBody.ParseEnvelope(evt));
                    }
                }

                if (root.TryGetProperty("result", out JsonElement result) &&
                    result.ValueKind == JsonValueKind.Object)
                {
                    yield return TaskStreamEvent.FromDone(SseReader.ReadTaskResult(result));
                }
            }

            yield break;
        }

        using (httpRequest)
        {
            HttpResponseMessage response = await _client.OpenStreamedResponseAsync(httpRequest, cancellationToken).ConfigureAwait(false);
            using (response)
            {
                Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                SseReader reader = new(stream);
                await foreach (SseEvent evt in reader.EnumerateAsync(cancellationToken).ConfigureAwait(false))
                {
                    SseReader.ThrowIfErrorEvent(evt.Data);
                    yield return TaskStreamEvent.ParseSse(evt.Data);
                }
            }
        }
    }

    /// <summary>Callback variant of <see cref="StreamAsync(TaskStreamRequest, CancellationToken)"/>.</summary>
    public async Task StreamAsync(TaskStreamRequest request, Func<TaskStreamEvent, Task> onEvent, CancellationToken cancellationToken = default)
    {
        if (onEvent == null)
        {
            throw new ArgumentNullException(nameof(onEvent));
        }

        await foreach (TaskStreamEvent evt in StreamAsync(request, cancellationToken).ConfigureAwait(false))
        {
            await onEvent(evt).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Native offline batch of generic requests. Results stream per entry (original
    /// request index preserved); per-result timing is null by design.
    /// </summary>
    public async IAsyncEnumerable<TaskBatchEvent> BatchAsync(TaskBatchRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        HttpRequestMessage httpRequest = _client.NewRequest(HttpMethod.Post, "v1/tasks/batch");
        httpRequest.Content = AudioCppClient.CreateJsonContent(writer => request.WriteTo(writer));
        using (httpRequest)
        {
            HttpResponseMessage response = await _client.OpenStreamedResponseAsync(httpRequest, cancellationToken).ConfigureAwait(false);
            using (response)
            {
                Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                SseReader reader = new(stream);
                await foreach (SseEvent evt in reader.EnumerateAsync(cancellationToken).ConfigureAwait(false))
                {
                    SseReader.ThrowIfErrorEvent(evt.Data);
                    yield return TaskBatchEvent.Parse(evt.Data);
                }
            }
        }
    }

    /// <summary>Callback variant of <see cref="BatchAsync(TaskBatchRequest, CancellationToken)"/>.</summary>
    public async Task BatchAsync(TaskBatchRequest request, Func<TaskBatchEvent, Task> onEvent, CancellationToken cancellationToken = default)
    {
        if (onEvent == null)
        {
            throw new ArgumentNullException(nameof(onEvent));
        }

        await foreach (TaskBatchEvent evt in BatchAsync(request, cancellationToken).ConfigureAwait(false))
        {
            await onEvent(evt).ConfigureAwait(false);
        }
    }
}

/// <summary>
/// Multi-model recipes against <c>POST /v1/recipes/run</c>. Role-specific option prefixes
/// (<c>tts.</c>, <c>avatar.</c>, <c>conversation.</c>, …) are the caller's responsibility:
/// put them directly in <see cref="TaskRequestBody.Options"/> keys, e.g.
/// <c>options["avatar.reference_image_file"] = "/path/portrait.png"</c>.
/// </summary>
public sealed class RecipeClient
{
    private readonly AudioCppClient _client;

    internal RecipeClient(AudioCppClient client)
    {
        _client = client;
    }

    /// <summary>Run a recipe and return the final result.</summary>
    public async Task<TaskResult> RunAsync(RecipeRunRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (request.Stream)
        {
            throw new ArgumentException("use RunStreamAsync for stream:true requests");
        }

        using HttpRequestMessage httpRequest = _client.NewRequest(HttpMethod.Post, "v1/recipes/run");
        httpRequest.Content = AudioCppClient.CreateJsonContent(writer => request.WriteTo(writer));
        return await _client.ReadJsonAsync(httpRequest, SseParsers.TaskResult, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Run a recipe with <c>stream:true</c>; events use the same
    /// <c>task.stream.event</c>/<c>task.stream.done</c> envelope as generic task streaming.
    /// </summary>
    public async IAsyncEnumerable<TaskStreamEvent> RunStreamAsync(RecipeRunRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        request.Stream = true;
        HttpRequestMessage httpRequest = _client.NewRequest(HttpMethod.Post, "v1/recipes/run");
        httpRequest.Content = AudioCppClient.CreateJsonContent(writer => request.WriteTo(writer));
        using (httpRequest)
        {
            HttpResponseMessage response = await _client.OpenStreamedResponseAsync(httpRequest, cancellationToken).ConfigureAwait(false);
            using (response)
            {
                Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                SseReader reader = new(stream);
                await foreach (SseEvent evt in reader.EnumerateAsync(cancellationToken).ConfigureAwait(false))
                {
                    SseReader.ThrowIfErrorEvent(evt.Data);
                    yield return TaskStreamEvent.ParseSse(evt.Data);
                }
            }
        }
    }

    /// <summary>Callback variant of <see cref="RunStreamAsync(RecipeRunRequest, CancellationToken)"/>.</summary>
    public async Task RunStreamAsync(RecipeRunRequest request, Func<TaskStreamEvent, Task> onEvent, CancellationToken cancellationToken = default)
    {
        if (onEvent == null)
        {
            throw new ArgumentNullException(nameof(onEvent));
        }

        await foreach (TaskStreamEvent evt in RunStreamAsync(request, cancellationToken).ConfigureAwait(false))
        {
            await onEvent(evt).ConfigureAwait(false);
        }
    }
}