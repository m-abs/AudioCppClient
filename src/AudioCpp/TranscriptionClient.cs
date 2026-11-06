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

using mabs.AudioCpp.Http;
using mabs.AudioCpp.Models;

namespace mabs.AudioCpp;

/// <summary>
/// Transcription against <c>POST /v1/audio/transcriptions</c>, its details/batch/live
/// siblings, and SSE streaming.
/// </summary>
public sealed class TranscriptionClient
{
    private readonly AudioCppClient _client;

    internal TranscriptionClient(AudioCppClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Transcribe audio. The JSON body form uses <see cref="TranscriptionInputBase.AudioPath"/>
    /// (server-local path); the multipart upload form uses
    /// <see cref="TranscriptionInputBase.AudioFile"/>. Exactly one must be set.
    /// </summary>
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
            return await _client.ReadJsonAsync(httpRequest, TranscriptionDetailsResult.Parse, cancellationToken).ConfigureAwait(false);
        }

        using HttpRequestMessage jsonRequest = _client.NewRequest(HttpMethod.Post, "v1/audio/transcriptions/details");
        jsonRequest.Content = AudioCppClient.CreateJsonContent(writer => WriteJsonDetails(request, writer));
        return await _client.ReadJsonAsync(jsonRequest, TranscriptionDetailsResult.Parse, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Stream a transcript as SSE events (requires a model configured with
    /// <c>mode: "streaming"</c>). Uploads the file with the <c>stream=true</c> field when
    /// <see cref="TranscriptionInputBase.AudioFile"/> is set, or sends the JSON body with
    /// <c>stream=true</c> when <see cref="TranscriptionInputBase.AudioPath"/> is set.
    /// </summary>
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
    public async IAsyncEnumerable<BatchTranscriptionEvent> TranscribeBatchAsync(BatchTranscriptionRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        request.Validate();
        using HttpRequestMessage httpRequest = new(HttpMethod.Post, _client.BuildUri("v1/batches/transcriptions"));
        httpRequest.Content = Multipart.BuildBatch(request);
        HttpResponseMessage response = await _client.OpenStreamedResponseAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        using (response)
        {
            Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            SseReader reader = new(stream);
            await foreach (SseEvent evt in reader.EnumerateAsync(cancellationToken).ConfigureAwait(false))
            {
                SseReader.ThrowIfErrorEvent(evt.Data);
                yield return BatchTranscriptionEvent.Parse(evt.Data);
            }
        }
    }

    /// <summary>Callback variant of <see cref="TranscribeBatchAsync(BatchTranscriptionRequest, CancellationToken)"/>.</summary>
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

    private async IAsyncEnumerable<TranscriptEvent> EnumerateTranscriptEventsAsync(HttpRequestMessage httpRequest, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
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
                    yield return TranscriptEvent.Parse(evt.Data);
                }
            }
        }
    }

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
}

/// <summary>Multipart request construction shared by the transcription/alignment/batch routes.</summary>
internal static class Multipart
{
    internal static HttpRequestMessage Post(Uri uri, TranscriptionInputBase request, bool stream = false)
    {
        HttpRequestMessage httpRequest = new(HttpMethod.Post, uri);
        httpRequest.Content = BuildSingle(request, stream);
        return httpRequest;
    }

    internal static MultipartFormDataContent BuildSingle(TranscriptionInputBase request, bool stream)
    {
        MultipartFormDataContent content = new();
        AddFile(content, "file", request.AudioFile!, request.AudioFileName!);
        content.Add(new StringContent(request.Model!), "model");
        if (!string.IsNullOrEmpty(request.Language))
        {
            content.Add(new StringContent(request.Language!), "language");
        }

        string? prompt = request is TranscriptionRequest transcription ? transcription.Prompt
            : request is TranscriptionDetailsRequest details ? details.Prompt
            : null;
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

    internal static MultipartFormDataContent BuildBatch(BatchTranscriptionRequest request)
    {
        MultipartFormDataContent content = new();
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

    internal static MultipartFormDataContent BuildAlignment(AlignmentRequest request)
    {
        MultipartFormDataContent content = new();
        AddFile(content, "file", request.AudioFile!, request.AudioFileName!);
        content.Add(new StringContent(request.Model!, Encoding.UTF8), "model");
        content.Add(new StringContent(request.Text!, Encoding.UTF8), "text");
        if (!string.IsNullOrEmpty(request.Language))
        {
            content.Add(new StringContent(request.Language!, Encoding.UTF8), "language");
        }

        return content;
    }

    private static void AddFile(MultipartFormDataContent content, string name, byte[] data, string fileName)
    {
        ByteArrayContent part = new(data);
        part.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Add(part, name, fileName);
    }

    private static void AddFile(MultipartFormDataContent content, string name, Stream data, string fileName)
    {
        StreamContent part = new(data);
        part.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Add(part, name, fileName);
    }

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