using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Mabs.AudioCpp.Http;
using Mabs.AudioCpp.Models.Tasks;
using Mabs.AudioCpp.Utils;

namespace Mabs.AudioCpp;

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
    /// <param name="request">The run request (validated before sending).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The task result.</returns>
    /// <exception cref="AudioCppException">The request failed.</exception>
    public async Task<TaskResult> RunAsync(TaskRunRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        request.Validate();
        using HttpRequestMessage httpRequest = _client.NewRequest(HttpMethod.Post, "v1/tasks/run");
        httpRequest.Content = AudioCppClient.CreateJsonContent(request);
        return await _client.ReadJsonAsync<TaskResult>(httpRequest, cancellationToken).ConfigureAwait(false);
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
    /// <param name="request">The stream request (validated before sending).</param>
    /// <param name="cancellationToken">Cancellation token observed while enumerating.</param>
    /// <returns>The event sequence; iteration ends at <c>[DONE]</c> or connection close.</returns>
    /// <exception cref="AudioCppException">The response failed or an SSE error event arrives.</exception>
    public async IAsyncEnumerable<TaskStreamEvent> StreamAsync(TaskStreamRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        request.Validate();
        using HttpRequestMessage httpRequest = _client.NewRequest(HttpMethod.Post, "v1/tasks/stream");
        httpRequest.Content = AudioCppClient.CreateJsonContent(request);
        if (request.StreamFormat == null)
        {
            // Buffered JSON response: {"events":[…],"result":{…}}.
            TaskStreamJsonResponse jsonResponse = await _client.ReadJsonAsync<TaskStreamJsonResponse>(httpRequest, cancellationToken).ConfigureAwait(false);
            if (jsonResponse.Events != null)
            {
                foreach (TaskStreamEventBody evt in jsonResponse.Events)
                {
                    yield return TaskStreamEvent.FromEvent(evt);
                }
            }

            if (jsonResponse.Result != null)
            {
                yield return TaskStreamEvent.FromDone(jsonResponse.Result);
            }

            yield break;
        }

        using HttpResponseMessage response = await _client.OpenStreamedResponseAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        SseReader reader = new(stream);
        await foreach (SseEvent evt in reader.EnumerateAsync(cancellationToken).ConfigureAwait(false))
        {
            SseReader.ThrowIfErrorEvent(evt.Data);
            yield return Json.Deserialize<TaskStreamEvent>(evt.Data);
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
        ArgumentNullException.ThrowIfNull(request);

        ArgumentNullException.ThrowIfNull(onEvent);

        await foreach (TaskStreamEvent evt in StreamAsync(request, cancellationToken).ConfigureAwait(false))
        {
            await onEvent(evt).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Native offline batch of generic requests. Results stream per entry (original
    /// request index preserved); per-result timing is null by design.
    /// </summary>
    /// <param name="request">The batch request (validated before sending).</param>
    /// <param name="cancellationToken">Cancellation token observed while enumerating.</param>
    /// <returns>The event sequence; iteration ends at <c>[DONE]</c> or connection close.</returns>
    /// <exception cref="AudioCppException">The response failed or an SSE error event arrives.</exception>
    public async IAsyncEnumerable<TaskBatchEvent> BatchAsync(TaskBatchRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        request.Validate();
        using HttpRequestMessage httpRequest = _client.NewRequest(HttpMethod.Post, "v1/tasks/batch");
        httpRequest.Content = AudioCppClient.CreateJsonContent(request);
        using HttpResponseMessage response = await _client.OpenStreamedResponseAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        SseReader reader = new(stream);
        await foreach (SseEvent evt in reader.EnumerateAsync(cancellationToken).ConfigureAwait(false))
        {
            SseReader.ThrowIfErrorEvent(evt.Data);
            yield return Json.Deserialize<TaskBatchEvent>(evt.Data);
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
        ArgumentNullException.ThrowIfNull(onEvent);

        await foreach (TaskBatchEvent evt in BatchAsync(request, cancellationToken).ConfigureAwait(false))
        {
            await onEvent(evt).ConfigureAwait(false);
        }
    }
}