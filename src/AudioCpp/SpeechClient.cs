using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Mabs.AudioCpp.Http;
using Mabs.AudioCpp.Models;
using Mabs.AudioCpp.Models.Speech;
using Mabs.AudioCpp.Models.Speech.Live;
using Mabs.AudioCpp.Utils;

namespace Mabs.AudioCpp;

/// <summary>
/// Speech synthesis against <c>POST /v1/audio/speech</c> and <c>/v1/audio/speech/live</c>.
/// </summary>
public sealed class SpeechClient
{
    private readonly AudioCppClient _client;

    /// <summary>Creates the speech endpoint group bound to <paramref name="client"/>.</summary>
    /// <param name="client">The owning client that carries the transport and base URL.</param>
    internal SpeechClient(AudioCppClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Synthesize speech. Returns the raw audio bytes (WAV by default) with the
    /// <c>X-AudioCPP-*</c> timing headers parsed into <see cref="SpeechResult.Timing"/>.
    /// </summary>
    /// <param name="request">The speech request (validated before sending).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The audio bytes, content type, and timing.</returns>
    /// <exception cref="AudioCppException">The request failed.</exception>
    public async Task<SpeechResult> SynthesizeAsync(SpeechRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        request.Validate();
        using HttpRequestMessage httpRequest = _client.NewRequest(HttpMethod.Post, "v1/audio/speech");
        httpRequest.Content = AudioCppClient.CreateJsonContent(request);
        AudioRawResponse raw = await _client.SendForBytesAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        return new SpeechResult
        {
            Audio = raw.Bytes,
            ContentType = raw.MediaType ?? "audio/wav",
            Timing = AudioCppClient.ReadTimingHeaders(raw.Headers),
        };
    }

    /// <summary>
    /// Synthesize with <c>response_format=json</c> and parse the JSON envelope
    /// (<c>audio</c> Base64 WAV, <c>format</c>, <c>timing</c>).
    /// </summary>
    /// <param name="request">The speech request; <see cref="SpeechRequest.ResponseFormat"/> is forced to <c>json</c>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The parsed JSON envelope.</returns>
    /// <exception cref="AudioCppException">The request failed.</exception>
    public async Task<SpeechJsonResult> SynthesizeJsonAsync(SpeechRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        request.ResponseFormat = ResponseFormat.Json;
        request.Validate();
        using HttpRequestMessage httpRequest = _client.NewRequest(HttpMethod.Post, "v1/audio/speech");
        httpRequest.Content = AudioCppClient.CreateJsonContent(request);
        return await _client.ReadJsonAsync<SpeechJsonResult>(httpRequest, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Stream speech as SSE events (requires a model configured with <c>mode: "streaming"</c>).
    /// Ends after <c>speech.audio.done</c> and <c>[DONE]</c>; throws
    /// <see cref="AudioCppException"/> on an SSE error event.
    /// </summary>
    /// <param name="request">The stream request; <c>stream_format</c> must be <c>sse</c>.</param>
    /// <param name="cancellationToken">Cancellation token observed while enumerating.</param>
    /// <returns>The event sequence; iteration ends at <c>[DONE]</c> or connection close.</returns>
    /// <exception cref="AudioCppException">The response failed or an SSE error event arrives.</exception>
    public async IAsyncEnumerable<SpeechStreamEvent> StreamAsync(SpeechStreamRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        SseReader reader = await OpenSpeechStreamAsync(request, cancellationToken).ConfigureAwait(false);
        await foreach (SseEvent evt in reader.EnumerateAsync(cancellationToken).ConfigureAwait(false))
        {
            SseReader.ThrowIfErrorEvent(evt.Data);
            yield return Json.Deserialize<SpeechStreamEvent>(evt.Data);
        }
    }

    /// <summary>Callback streaming variant.</summary>
    /// <param name="request">The stream request; <c>stream_format</c> must be <c>sse</c>.</param>
    /// <param name="onEvent">Callback invoked for each event; iteration stops if it throws.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Completes when the stream terminates or the callback stops consuming events.</returns>
    /// <exception cref="AudioCppException">The response failed or an SSE error event arrives.</exception>
    public async Task StreamAsync(SpeechStreamRequest request, Func<SpeechStreamEvent, Task> onEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(onEvent);

        await foreach (SpeechStreamEvent evt in StreamAsync(request, cancellationToken).ConfigureAwait(false))
        {
            await onEvent(evt).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Raw PCM streaming: <c>stream_format=audio</c> + <c>response_format=pcm</c> returns
    /// chunked octet-stream bytes. The caller owns the returned stream; reading it yields
    /// PCM16 bytes as the server produces them.
    /// </summary>
    /// <param name="request">The stream request; <c>stream_format</c> is forced to <c>audio</c>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The open response content stream.</returns>
    /// <exception cref="AudioCppException">The response failed.</exception>
    public async Task<Stream> StreamPcmAsync(SpeechStreamRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        request.StreamFormat = StreamFormat.Audio;
        request.Request.ResponseFormat = ResponseFormat.Pcm;
        request.Validate();
        using HttpRequestMessage httpRequest = _client.NewRequest(HttpMethod.Post, "v1/audio/speech");
        httpRequest.Content = AudioCppClient.CreateJsonContent(request);
        HttpResponseMessage response = await _client.OpenStreamedResponseAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Live speech-to-speech: the request body is raw PCM pulled from
    /// <paramref name="pcmInput"/> as it produces data (chunked transfer), and the response
    /// streams events on the same connection. Consuming events before
    /// <paramref name="pcmInput"/> ends requires full-duplex HTTP; on platforms without it
    /// (browser/WASM) a <see cref="PlatformNotSupportedException"/> is thrown. The request
    /// ends — and the model treats it as end-of-speech — when <paramref name="pcmInput"/>
    /// reaches EOF; closing the connection without that is a server error, by contract.
    /// </summary>
    /// <param name="request">The live request; all fields travel as query parameters.</param>
    /// <param name="pcmInput">The PCM stream to upload; EOF marks end-of-speech.</param>
    /// <param name="cancellationToken">Cancellation token observed while enumerating.</param>
    /// <returns>The event sequence; iteration ends at <c>[DONE]</c> or connection close.</returns>
    /// <exception cref="AudioCppException">The response failed or an SSE error event arrives.</exception>
    public async IAsyncEnumerable<SpeechStreamEvent> SpeakLiveAsync(LiveSpeechRequest request, Stream pcmInput, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        ArgumentNullException.ThrowIfNull(pcmInput);

        string query = request.BuildQueryString();
        using HttpRequestMessage httpRequest = new(HttpMethod.Post, _client.BuildUri("v1/audio/speech/live", query));
        httpRequest.Content = new AudioCppClient.ChunkedStreamContent(pcmInput, cancellationToken);
        httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        using HttpResponseMessage response = await _client.OpenStreamedResponseAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        SseReader reader = new(stream);
        await foreach (SseEvent evt in reader.EnumerateAsync(cancellationToken).ConfigureAwait(false))
        {
            SseReader.ThrowIfErrorEvent(evt.Data);
            yield return Json.Deserialize<SpeechStreamEvent>(evt.Data);
        }
    }

    /// <summary>Callback variant of <see cref="SpeakLiveAsync(LiveSpeechRequest, Stream, CancellationToken)"/>.</summary>
    /// <param name="request">The live request.</param>
    /// <param name="pcmInput">The PCM stream to upload; EOF marks end-of-speech.</param>
    /// <param name="onEvent">Callback invoked for each event; iteration stops if it throws.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Completes when the stream terminates or the callback stops consuming events.</returns>
    /// <exception cref="AudioCppException">The response failed or an SSE error event arrives.</exception>
    public async Task SpeakLiveAsync(LiveSpeechRequest request, Stream pcmInput, Func<SpeechStreamEvent, Task> onEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(onEvent);

        await foreach (SpeechStreamEvent evt in SpeakLiveAsync(request, pcmInput, cancellationToken).ConfigureAwait(false))
        {
            await onEvent(evt).ConfigureAwait(false);
        }
    }

    /// <summary>Opens the SSE stream response and wraps its content in a reader.</summary>
    /// <param name="request">The stream request; <c>stream_format</c> must be <c>sse</c>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A reader over the open event stream.</returns>
    /// <exception cref="ArgumentException">The request is not SSE-shaped or not PCM output.</exception>
    /// <exception cref="AudioCppException">The response failed.</exception>
    private async Task<SseReader> OpenSpeechStreamAsync(SpeechStreamRequest request, CancellationToken cancellationToken)
    {
        if (request.StreamFormat != StreamFormat.Sse)
        {
            throw new ArgumentException("StreamAsync emits SSE events; use StreamPcmAsync for stream_format=audio");
        }

        if (request.Request.ResponseFormat == null)
        {
            // Streaming speech only supports PCM output; set it so the server does not 400.
            request.Request.ResponseFormat = ResponseFormat.Pcm;
        }
        else if (request.Request.ResponseFormat.Value != ResponseFormat.Pcm)
        {
            throw new ArgumentException("streaming speech only supports response_format=pcm");
        }

        request.Validate();
        HttpRequestMessage httpRequest = _client.NewRequest(HttpMethod.Post, "v1/audio/speech");
        try
        {
            httpRequest.Content = AudioCppClient.CreateJsonContent(request);
            HttpResponseMessage response = await _client.OpenStreamedResponseAsync(httpRequest, cancellationToken).ConfigureAwait(false);
            Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            return new SseReader(stream);
        }
        catch
        {
            httpRequest.Dispose();
            throw;
        }
    }
}