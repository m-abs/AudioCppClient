using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Mabs.AudioCpp.Http;
using Mabs.AudioCpp.Models;
using Mabs.AudioCpp.Utils;

namespace Mabs.AudioCpp;

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
        ArgumentNullException.ThrowIfNull(request);

        if (request.Stream)
        {
            throw new ArgumentException("use RunStreamAsync for stream:true requests");
        }

        request.Validate();
        using HttpRequestMessage httpRequest = _client.NewRequest(HttpMethod.Post, "v1/recipes/run");
        httpRequest.Content = AudioCppClient.CreateJsonContent(request);
        return await _client.ReadJsonAsync<TaskResult>(httpRequest, cancellationToken).ConfigureAwait(false);
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
        ArgumentNullException.ThrowIfNull(request);

        request.Stream = true;
        request.Validate();
        using HttpRequestMessage httpRequest = _client.NewRequest(HttpMethod.Post, "v1/recipes/run");
        httpRequest.Content = AudioCppClient.CreateJsonContent(request);
        using HttpResponseMessage response = await _client.OpenStreamedResponseAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        SseReader reader = new(stream);
        await foreach (SseEvent evt in reader.EnumerateAsync(cancellationToken).ConfigureAwait(false))
        {
            SseReader.ThrowIfErrorEvent(evt.Data);
            yield return Json.Deserialize<TaskStreamEvent>(evt.Data);
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
        ArgumentNullException.ThrowIfNull(onEvent);

        await foreach (TaskStreamEvent evt in RunStreamAsync(request, cancellationToken).ConfigureAwait(false))
        {
            await onEvent(evt).ConfigureAwait(false);
        }
    }
}