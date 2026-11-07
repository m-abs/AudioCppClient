using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Mabs.AudioCpp.Http;
using Mabs.AudioCpp.Models;
using Mabs.AudioCpp.Utils;

namespace Mabs.AudioCpp;

/// <summary>
/// Transcription against <c>POST /v1/audio/transcriptions</c>, its details/batch/live
/// siblings, and SSE streaming.
/// </summary>
public sealed class TranscriptionClient
{
    private readonly AudioCppClient _client;

    /// <summary>Creates the transcription endpoint group bound to <paramref name="client"/>.</summary>
    /// <param name="client">The owning client that carries the transport and base URL.</param>
    internal TranscriptionClient(AudioCppClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Transcribe audio. The JSON body form uses <see cref="TranscriptionInputBase.AudioPath"/>
    /// (server-local path); the multipart upload form uses
    /// <see cref="TranscriptionInputBase.AudioFile"/>. Exactly one must be set.
    /// </summary>
    /// <param name="request">The transcription request (validated before sending).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The transcription result.</returns>
    /// <exception cref="AudioCppException">The request failed.</exception>
    public async Task<TranscriptionResult> TranscribeAsync(TranscriptionRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        request.Validate();
        if (request.IsMultipart)
        {
            using HttpRequestMessage httpRequest = Multipart.Post(_client.BuildUri("v1/audio/transcriptions"), request);
            return await _client.ReadJsonAsync<TranscriptionResult>(httpRequest, cancellationToken).ConfigureAwait(false);
        }

        using HttpRequestMessage jsonRequest = _client.NewRequest(HttpMethod.Post, "v1/audio/transcriptions");
        jsonRequest.Content = AudioCppClient.CreateJsonContent(request);
        return await _client.ReadJsonAsync<TranscriptionResult>(jsonRequest, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Transcribe with the detail arrays (segments, speaker turns, words) the model produced.
    /// Same inputs as <see cref="TranscribeAsync"/>; the server rejects <c>stream</c> on
    /// this route.
    /// </summary>
    /// <param name="request">The details request (validated before sending).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The transcription result with detail arrays.</returns>
    /// <exception cref="AudioCppException">The request failed.</exception>
    public async Task<TranscriptionDetailsResult> TranscribeDetailsAsync(TranscriptionDetailsRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        request.Validate();
        if (request.IsMultipart)
        {
            using HttpRequestMessage httpRequest = Multipart.Post(_client.BuildUri("v1/audio/transcriptions/details"), request);
            return await _client.ReadJsonAsync<TranscriptionDetailsResult>(httpRequest, cancellationToken).ConfigureAwait(false);
        }

        using HttpRequestMessage jsonRequest = _client.NewRequest(HttpMethod.Post, "v1/audio/transcriptions/details");
        jsonRequest.Content = AudioCppClient.CreateJsonContent(request);
        return await _client.ReadJsonAsync<TranscriptionDetailsResult>(jsonRequest, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Stream a transcript as SSE events (requires a model configured with
    /// <c>mode: "streaming"</c>). Uploads the file with the <c>stream=true</c> field when
    /// <see cref="TranscriptionInputBase.AudioFile"/> is set, or sends the JSON body with
    /// <c>stream=true</c> when <see cref="TranscriptionInputBase.AudioPath"/> is set.
    /// </summary>
    /// <param name="request">The transcription request (validated before sending).</param>
    /// <param name="cancellationToken">Cancellation token observed while enumerating.</param>
    /// <returns>The event sequence; iteration ends at <c>[DONE]</c> or connection close.</returns>
    /// <exception cref="AudioCppException">The response failed or an SSE error event arrives.</exception>
    public async IAsyncEnumerable<TranscriptEvent> TranscribeStreamAsync(TranscriptionRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        request.Validate();
        request.Stream = true;
        HttpRequestMessage httpRequest;
        if (request.IsMultipart)
        {
            httpRequest = Multipart.Post(_client.BuildUri("v1/audio/transcriptions"), request, stream: true);
        }
        else
        {
            httpRequest = _client.NewRequest(HttpMethod.Post, "v1/audio/transcriptions");
            httpRequest.Content = AudioCppClient.CreateJsonContent(request);
        }

        await foreach (TranscriptEvent evt in EnumerateTranscriptEventsAsync(httpRequest, cancellationToken).ConfigureAwait(false))
        {
            yield return evt;
        }
    }

    /// <summary>Callback variant of <see cref="TranscribeStreamAsync(TranscriptionRequest, CancellationToken)"/>.</summary>
    /// <param name="request">The transcription request.</param>
    /// <param name="onEvent">Callback invoked for each event; iteration stops if it throws.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Completes when the stream terminates or the callback stops consuming events.</returns>
    /// <exception cref="AudioCppException">The response failed or an SSE error event arrives.</exception>
    public async Task TranscribeStreamAsync(TranscriptionRequest request, Func<TranscriptEvent, Task> onEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(onEvent);

        await foreach (TranscriptEvent evt in TranscribeStreamAsync(request, cancellationToken).ConfigureAwait(false))
        {
            await onEvent(evt).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Live capture: raw PCM pulled from <paramref name="pcmInput"/> as it produces data
    /// (chunked request body), transcript events on the same connection. Requires
    /// full-duplex HTTP; throws <see cref="PlatformNotSupportedException"/> on platforms
    /// that cannot stream a request body while reading the response (browser/WASM). The
    /// request ends — the model's end-of-speech signal — when <paramref name="pcmInput"/>
    /// reaches EOF.
    /// </summary>
    /// <param name="request">The live request; all fields travel as query parameters.</param>
    /// <param name="pcmInput">The PCM stream to upload; EOF marks end-of-speech.</param>
    /// <param name="cancellationToken">Cancellation token observed while enumerating.</param>
    /// <returns>The event sequence; iteration ends at <c>[DONE]</c> or connection close.</returns>
    /// <exception cref="AudioCppException">The response failed or an SSE error event arrives.</exception>
    public async IAsyncEnumerable<TranscriptEvent> TranscribeLiveAsync(LiveTranscriptionRequest request, Stream pcmInput, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        ArgumentNullException.ThrowIfNull(pcmInput);

        string query = request.BuildQueryString();
        using HttpRequestMessage httpRequest = new(HttpMethod.Post, _client.BuildUri("v1/audio/transcriptions/live", query));
        httpRequest.Content = new AudioCppClient.ChunkedStreamContent(pcmInput, cancellationToken);
        httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        await foreach (TranscriptEvent evt in EnumerateTranscriptEventsAsync(httpRequest, cancellationToken).ConfigureAwait(false))
        {
            yield return evt;
        }
    }

    /// <summary>Callback variant of <see cref="TranscribeLiveAsync(LiveTranscriptionRequest, Stream, CancellationToken)"/>.</summary>
    /// <param name="request">The live request.</param>
    /// <param name="pcmInput">The PCM stream to upload; EOF marks end-of-speech.</param>
    /// <param name="onEvent">Callback invoked for each event; iteration stops if it throws.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Completes when the stream terminates or the callback stops consuming events.</returns>
    /// <exception cref="AudioCppException">The response failed or an SSE error event arrives.</exception>
    public async Task TranscribeLiveAsync(LiveTranscriptionRequest request, Stream pcmInput, Func<TranscriptEvent, Task> onEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(onEvent);

        await foreach (TranscriptEvent evt in TranscribeLiveAsync(request, pcmInput, cancellationToken).ConfigureAwait(false))
        {
            await onEvent(evt).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Native transcription batch: several WAV uploads through one batched run of a
    /// native-batching model. Results stream per file as the model finishes it;
    /// <see cref="BatchTranscriptionEvent.Index"/> maps a result back to its upload position.
    /// </summary>
    /// <param name="request">The batch request (validated before sending).</param>
    /// <param name="cancellationToken">Cancellation token observed while enumerating.</param>
    /// <returns>The event sequence; iteration ends at <c>[DONE]</c> or connection close.</returns>
    /// <exception cref="AudioCppException">The response failed or an SSE error event arrives.</exception>
    public async IAsyncEnumerable<BatchTranscriptionEvent> TranscribeBatchAsync(BatchTranscriptionRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        request.Validate();
        using HttpRequestMessage httpRequest = new(HttpMethod.Post, _client.BuildUri("v1/batches/transcriptions"));
        httpRequest.Content = Multipart.BuildBatch(request);
        using HttpResponseMessage response = await _client.OpenStreamedResponseAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        SseReader reader = new(stream);
        await foreach (SseEvent evt in reader.EnumerateAsync(cancellationToken).ConfigureAwait(false))
        {
            SseReader.ThrowIfErrorEvent(evt.Data);
            yield return Json.Deserialize<BatchTranscriptionEvent>(evt.Data);
        }
    }

    /// <summary>Callback variant of <see cref="TranscribeBatchAsync(BatchTranscriptionRequest, CancellationToken)"/>.</summary>
    /// <param name="request">The batch request.</param>
    /// <param name="onEvent">Callback invoked for each event; iteration stops if it throws.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Completes when the stream terminates or the callback stops consuming events.</returns>
    /// <exception cref="AudioCppException">The response failed or an SSE error event arrives.</exception>
    public async Task TranscribeBatchAsync(BatchTranscriptionRequest request, Func<BatchTranscriptionEvent, Task> onEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(onEvent);

        await foreach (BatchTranscriptionEvent evt in TranscribeBatchAsync(request, cancellationToken).ConfigureAwait(false))
        {
            await onEvent(evt).ConfigureAwait(false);
        }
    }

    /// <summary>Opens the response, enumerates its SSE events, and parses each as a transcript event.</summary>
    /// <param name="httpRequest">The request to send; disposed here.</param>
    /// <param name="cancellationToken">Cancellation token observed while enumerating.</param>
    /// <returns>The event sequence; iteration ends at <c>[DONE]</c> or connection close.</returns>
    /// <exception cref="AudioCppException">The response failed or an SSE error event arrives.</exception>
    private async IAsyncEnumerable<TranscriptEvent> EnumerateTranscriptEventsAsync(HttpRequestMessage httpRequest, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using (httpRequest)
        {
            using HttpResponseMessage response = await _client.OpenStreamedResponseAsync(httpRequest, cancellationToken).ConfigureAwait(false);
            Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            SseReader reader = new(stream);
            await foreach (SseEvent evt in reader.EnumerateAsync(cancellationToken).ConfigureAwait(false))
            {
                SseReader.ThrowIfErrorEvent(evt.Data);
                yield return Json.Deserialize<TranscriptEvent>(evt.Data);
            }
        }
    }
}