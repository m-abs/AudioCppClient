using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Mabs.AudioCpp.Http;
using Mabs.AudioCpp.Models;

namespace Mabs.AudioCpp;

/// <summary>Forced alignment against <c>POST /v1/audio/alignments</c> (multipart only).</summary>
public sealed class AlignmentClient
{
    private readonly AudioCppClient _client;

    /// <summary>Creates the alignment endpoint group bound to <paramref name="client"/>.</summary>
    /// <param name="client">The owning client that carries the transport and base URL.</param>
    internal AlignmentClient(AudioCppClient client)
    {
        _client = client;
    }

    /// <summary>Align a known transcript against uploaded WAV bytes.</summary>
    /// <param name="request">The alignment request (validated before sending).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The alignment result with word timestamps.</returns>
    /// <exception cref="AudioCppException">The request failed.</exception>
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

    /// <summary>Creates the generic task endpoint group bound to <paramref name="client"/>.</summary>
    /// <param name="client">The owning client that carries the transport and base URL.</param>
    internal TaskClient(AudioCppClient client)
    {
        _client = client;
    }

    /// <summary>Run one generic request.</summary>
    /// <param name="request">The run request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The task result.</returns>
    /// <exception cref="AudioCppException">The request failed.</exception>
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
    /// <param name="model">The model id.</param>
    /// <param name="audioBytes">The audio bytes, Base64-encoded into the request body.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The task result.</returns>
    /// <exception cref="AudioCppException">The request failed.</exception>
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
    /// <param name="request">The stream request.</param>
    /// <param name="cancellationToken">Cancellation token observed while enumerating.</param>
    /// <returns>The event sequence; iteration ends at <c>[DONE]</c> or connection close.</returns>
    /// <exception cref="AudioCppException">The response failed or an SSE error event arrives.</exception>
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
    /// <param name="request">The stream request.</param>
    /// <param name="onEvent">Callback invoked for each event; iteration stops if it throws.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Completes when the stream terminates or the callback stops consuming events.</returns>
    /// <exception cref="AudioCppException">The response failed or an SSE error event arrives.</exception>
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
    /// <param name="request">The batch request.</param>
    /// <param name="cancellationToken">Cancellation token observed while enumerating.</param>
    /// <returns>The event sequence; iteration ends at <c>[DONE]</c> or connection close.</returns>
    /// <exception cref="AudioCppException">The response failed or an SSE error event arrives.</exception>
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
    /// <param name="request">The batch request.</param>
    /// <param name="onEvent">Callback invoked for each event; iteration stops if it throws.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Completes when the stream terminates or the callback stops consuming events.</returns>
    /// <exception cref="AudioCppException">The response failed or an SSE error event arrives.</exception>
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

    /// <summary>Creates the recipe endpoint group bound to <paramref name="client"/>.</summary>
    /// <param name="client">The owning client that carries the transport and base URL.</param>
    internal RecipeClient(AudioCppClient client)
    {
        _client = client;
    }

    /// <summary>Run a recipe and return the final result.</summary>
    /// <param name="request">The recipe request; <see cref="RecipeRunRequest.Stream"/> must be false.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The final task result.</returns>
    /// <exception cref="AudioCppException">The request failed.</exception>
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
    /// <param name="request">The recipe request; <see cref="RecipeRunRequest.Stream"/> is forced to true.</param>
    /// <param name="cancellationToken">Cancellation token observed while enumerating.</param>
    /// <returns>The event sequence; iteration ends at <c>[DONE]</c> or connection close.</returns>
    /// <exception cref="AudioCppException">The response failed or an SSE error event arrives.</exception>
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
    /// <param name="request">The recipe request.</param>
    /// <param name="onEvent">Callback invoked for each event; iteration stops if it throws.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Completes when the stream terminates or the callback stops consuming events.</returns>
    /// <exception cref="AudioCppException">The response failed or an SSE error event arrives.</exception>
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