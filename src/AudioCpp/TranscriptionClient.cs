using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Mabs.AudioCpp.Http;
using Mabs.AudioCpp.Models;

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
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        request.Validate();
        if (request.IsMultipart)
        {
            using HttpRequestMessage httpRequest = Multipart.Post(_client.BuildUri("v1/audio/transcriptions"), request);
            return await _client.ReadJsonAsync(httpRequest, SseParsers.Transcription, cancellationToken).ConfigureAwait(false);
        }

        using HttpRequestMessage jsonRequest = _client.NewRequest(HttpMethod.Post, "v1/audio/transcriptions");
        jsonRequest.Content = AudioCppClient.CreateJsonContent(writer => WriteJson(request, writer, stream: false));
        return await _client.ReadJsonAsync(jsonRequest, SseParsers.Transcription, cancellationToken).ConfigureAwait(false);
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
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        request.Validate();
        if (request.IsMultipart)
        {
            using HttpRequestMessage httpRequest = Multipart.Post(_client.BuildUri("v1/audio/transcriptions/details"), request);
            return await _client.ReadJsonAsync(httpRequest, SseParsers.Details, cancellationToken).ConfigureAwait(false);
        }

        using HttpRequestMessage jsonRequest = _client.NewRequest(HttpMethod.Post, "v1/audio/transcriptions/details");
        jsonRequest.Content = AudioCppClient.CreateJsonContent(writer => WriteJsonDetails(request, writer));
        return await _client.ReadJsonAsync(jsonRequest, SseParsers.Details, cancellationToken).ConfigureAwait(false);
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
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        request.Validate();
        HttpRequestMessage httpRequest;
        if (request.IsMultipart)
        {
            httpRequest = Multipart.Post(_client.BuildUri("v1/audio/transcriptions"), request, stream: true);
        }
        else
        {
            httpRequest = _client.NewRequest(HttpMethod.Post, "v1/audio/transcriptions");
            httpRequest.Content = AudioCppClient.CreateJsonContent(writer => WriteJson(request, writer, stream: true));
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
        if (onEvent == null)
        {
            throw new ArgumentNullException(nameof(onEvent));
        }

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
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (pcmInput == null)
        {
            throw new ArgumentNullException(nameof(pcmInput));
        }

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
        if (onEvent == null)
        {
            throw new ArgumentNullException(nameof(onEvent));
        }

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
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        request.Validate();
        using HttpRequestMessage httpRequest = new(HttpMethod.Post, _client.BuildUri("v1/batches/transcriptions"));
        httpRequest.Content = Multipart.BuildBatch(request);
        using HttpResponseMessage response = await _client.OpenStreamedResponseAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        SseReader reader = new(stream);
        await foreach (SseEvent evt in reader.EnumerateAsync(cancellationToken).ConfigureAwait(false))
        {
            SseReader.ThrowIfErrorEvent(evt.Data);
            yield return BatchTranscriptionEvent.Parse(evt.Data);
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
        if (onEvent == null)
        {
            throw new ArgumentNullException(nameof(onEvent));
        }

        await foreach (BatchTranscriptionEvent evt in TranscribeBatchAsync(request, cancellationToken).ConfigureAwait(false))
        {
            await onEvent(evt).ConfigureAwait(false);
        }
    }

    /// <summary>Writes the transcription JSON body (the <c>stream</c> field is added only for the streaming route).</summary>
    /// <param name="request">The request to serialize.</param>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="stream">Whether to emit <c>stream: true</c>.</param>
    private static void WriteJson(TranscriptionRequest request, Utf8JsonWriter writer, bool stream)
    {
        writer.WriteStartObject();
        writer.WriteString("model", request.Model);
        writer.WriteString("audio", request.AudioPath);
        RequestJson.WriteStringIfSet(writer, "language", request.Language);
        RequestJson.WriteStringIfSet(writer, "text", request.Prompt);
        if (stream)
        {
            writer.WriteBoolean("stream", true);
        }

        RequestJson.WriteNumberIfSet(writer, "busy_timeout_ms", request.BusyTimeoutMs);
        RequestJson.WriteOptions(writer, request.Options);
        writer.WriteEndObject();
    }

    /// <summary>Writes the transcription-details JSON body.</summary>
    /// <param name="request">The request to serialize.</param>
    /// <param name="writer">The JSON writer.</param>
    private static void WriteJsonDetails(TranscriptionDetailsRequest request, Utf8JsonWriter writer)
    {
        writer.WriteStartObject();
        writer.WriteString("model", request.Model);
        writer.WriteString("audio", request.AudioPath);
        RequestJson.WriteStringIfSet(writer, "language", request.Language);
        RequestJson.WriteStringIfSet(writer, "text", request.Prompt);
        RequestJson.WriteNumberIfSet(writer, "busy_timeout_ms", request.BusyTimeoutMs);
        RequestJson.WriteOptions(writer, request.Options);
        writer.WriteEndObject();
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
                yield return TranscriptEvent.Parse(evt.Data);
            }
        }
    }
}

/// <summary>Multipart request construction shared by the transcription/alignment/batch routes.</summary>
internal static class Multipart
{
    /// <summary>Builds a POST request with the single-file multipart body.</summary>
    /// <param name="uri">The target route.</param>
    /// <param name="request">The request whose <see cref="TranscriptionInputBase.AudioFile"/> is uploaded.</param>
    /// <param name="stream">Whether to add the <c>stream=true</c> form field.</param>
    /// <returns>The request, ready to send.</returns>
    internal static HttpRequestMessage Post(Uri uri, TranscriptionInputBase request, bool stream = false)
    {
        HttpRequestMessage httpRequest = new(HttpMethod.Post, uri);
        httpRequest.Content = BuildSingle(request, stream);
        return httpRequest;
    }

    /// <summary>Builds the single-file multipart body: file part plus model/language/prompt/busy_timeout fields.</summary>
    /// <param name="request">The request whose file and fields are serialized.</param>
    /// <param name="stream">Whether to add the <c>stream=true</c> form field.</param>
    /// <returns>The multipart content.</returns>
    internal static MultipartFormDataContent BuildSingle(TranscriptionInputBase request, bool stream)
    {
        MultipartFormDataContent content = [];
        AddFile(content, "file", request.AudioFile!, request.AudioFileName!);
        content.Add(new StringContent(request.Model!), "model");
        if (!string.IsNullOrEmpty(request.Language))
        {
            content.Add(new StringContent(request.Language!), "language");
        }

        string? prompt = request is TranscriptionRequest transcription ? transcription.Prompt : request is TranscriptionDetailsRequest details ? details.Prompt : null;
        if (!string.IsNullOrEmpty(prompt))
        {
            content.Add(new StringContent(prompt!), "prompt");
        }

        if (request.BusyTimeoutMs.HasValue)
        {
            content.Add(new StringContent(request.BusyTimeoutMs.Value.ToString(CultureInfo.InvariantCulture)), "busy_timeout_ms");
        }

        if (stream)
        {
            content.Add(new StringContent("true"), "stream");
        }

        return content;
    }

    /// <summary>Builds the batch multipart body: one file part per upload plus the batch fields.</summary>
    /// <param name="request">The batch request.</param>
    /// <returns>The multipart content.</returns>
    internal static MultipartFormDataContent BuildBatch(BatchTranscriptionRequest request)
    {
        MultipartFormDataContent content = [];
        foreach (AudioUpload file in request.Files)
        {
            AddFile(content, "file", file.Content, file.FileName);
        }

        content.Add(new StringContent(request.Model!, Encoding.UTF8), "model");
        if (!string.IsNullOrEmpty(request.Language))
        {
            content.Add(new StringContent(request.Language!, Encoding.UTF8), "language");
        }

        if (!string.IsNullOrEmpty(request.Prompt))
        {
            content.Add(new StringContent(request.Prompt!, Encoding.UTF8), "prompt");
        }

        if (request.BusyTimeoutMs.HasValue)
        {
            content.Add(new StringContent(request.BusyTimeoutMs.Value.ToString(CultureInfo.InvariantCulture), Encoding.UTF8), "busy_timeout_ms");
        }

        if (request.Options != null && request.Options.Count > 0)
        {
            content.Add(new StringContent(BuildOptionsJson(request.Options), Encoding.UTF8, "application/json"), "options");
        }

        return content;
    }

    /// <summary>Builds the alignment multipart body: file, model, text, and optional language.</summary>
    /// <param name="request">The alignment request.</param>
    /// <returns>The multipart content.</returns>
    internal static MultipartFormDataContent BuildAlignment(AlignmentRequest request)
    {
        MultipartFormDataContent content = [];
        AddFile(content, "file", request.AudioFile!, request.AudioFileName!);
        content.Add(new StringContent(request.Model!, Encoding.UTF8), "model");
        content.Add(new StringContent(request.Text!, Encoding.UTF8), "text");
        if (!string.IsNullOrEmpty(request.Language))
        {
            content.Add(new StringContent(request.Language!, Encoding.UTF8), "language");
        }

        return content;
    }

    /// <summary>Adds one file part as bytes with an octet-stream content type.</summary>
    /// <param name="content">The multipart body to append to.</param>
    /// <param name="name">The form-field name (<c>file</c>).</param>
    /// <param name="data">The file bytes.</param>
    /// <param name="fileName">The filename reported in the part headers.</param>
    private static void AddFile(MultipartFormDataContent content, string name, byte[] data, string fileName)
    {
        ByteArrayContent part = new(data);
        part.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Add(part, name, fileName);
    }

    /// <summary>Adds one file part as a stream with an octet-stream content type.</summary>
    /// <param name="content">The multipart body to append to.</param>
    /// <param name="name">The form-field name (<c>file</c>).</param>
    /// <param name="data">The file stream.</param>
    /// <param name="fileName">The filename reported in the part headers.</param>
    private static void AddFile(MultipartFormDataContent content, string name, Stream data, string fileName)
    {
        StreamContent part = new(data);
        part.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Add(part, name, fileName);
    }

    /// <summary>Serializes the options map to a JSON string for the batch form field.</summary>
    /// <param name="options">The options map.</param>
    /// <returns>The JSON object string.</returns>
    private static string BuildOptionsJson(Dictionary<string, string> options)
    {
        using MemoryStream stream = new();
        using (Utf8JsonWriter writer = new(stream))
        {
            RequestJson.WriteOptions(writer, options);
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }
}