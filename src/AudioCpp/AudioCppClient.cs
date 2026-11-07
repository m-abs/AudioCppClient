using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Mabs.AudioCpp.Http;
using Mabs.AudioCpp.Models;
using Mabs.AudioCpp.Utils;

namespace Mabs.AudioCpp;

/// <summary>
/// Client for the audio.cpp server protocol. One instance maps to one server base URL and
/// is thread-safe; share it. The per-area sub-clients (<see cref="Speech"/>,
/// <see cref="Transcriptions"/>, <see cref="Alignments"/>, <see cref="Tasks"/>,
/// <see cref="Recipes"/>) carry the endpoint groups; readiness, model, voice and unload
/// routes sit directly on this type.
/// </summary>
public sealed class AudioCppClient
{
    private readonly HttpClient _http;
    private readonly Uri _baseUri;

    /// <summary>Creates a client for the server at <paramref name="baseUrl"/>.</summary>
    /// <param name="baseUrl">Server root, for example <c>http://127.0.0.1:8080</c>.</param>
    /// <param name="httpClient">
    /// Optional transport. When omitted, a default <see cref="HttpClient"/> is created; pass
    /// your own to control proxies, timeouts or TLS. For the live routes it must be backed by
    /// a handler with full-duplex HTTP/1.1 support (the default handler on .NET Core and
    /// .NET Framework's WinHttpClient qualify; browser/WASM handlers do not).
    /// </param>
    /// <param name="apiKey">
    /// Optional bearer token sent as <c>Authorization: Bearer &lt;key&gt;</c> on every request.
    /// The audio.cpp server ignores it today; frontends in front of it may require it.
    /// </param>
    public AudioCppClient(Uri baseUrl, HttpClient? httpClient = null, string? apiKey = null)
    {
        ArgumentNullException.ThrowIfNull(baseUrl);

        _baseUri = baseUrl;
        _http = httpClient ?? new HttpClient();
        ApiKey = apiKey;
        Speech = new SpeechClient(this);
        Transcriptions = new TranscriptionClient(this);
        Alignments = new AlignmentClient(this);
        Tasks = new TaskClient(this);
        Recipes = new RecipeClient(this);
    }

    /// <summary>Creates a client from a base URL string.</summary>
    /// <param name="baseUrl">Server root, for example <c>http://127.0.0.1:8080</c>.</param>
    /// <param name="httpClient">Optional transport; see the <see cref="Uri"/> constructor overload.</param>
    /// <param name="apiKey">Optional bearer token sent on every request.</param>
    public AudioCppClient(string baseUrl, HttpClient? httpClient = null, string? apiKey = null)
        : this(new Uri(baseUrl ?? throw new ArgumentNullException(nameof(baseUrl))), httpClient, apiKey)
    {
    }

    /// <summary>Bearer token sent on every request, when set.</summary>
    public string? ApiKey { get; }

    /// <summary>Speech synthesis, streaming speech, and live speech-to-speech.</summary>
    public SpeechClient Speech { get; }

    /// <summary>Transcription, details, streaming, live capture, and native batches.</summary>
    public TranscriptionClient Transcriptions { get; }

    /// <summary>Forced alignment of known transcripts.</summary>
    public AlignmentClient Alignments { get; }

    /// <summary>Generic framework tasks: run, stream, batch.</summary>
    public TaskClient Tasks { get; }

    /// <summary>Multi-model recipes.</summary>
    public RecipeClient Recipes { get; }

    /// <summary>Server readiness from <c>GET /health</c>.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The health report.</returns>
    /// <exception cref="AudioCppException">The request failed.</exception>
    public async Task<AudioCppHealth> GetHealthAsync(CancellationToken cancellationToken = default)
    {
        return await SendJsonAsync<AudioCppHealth>(HttpMethod.Get, "health", body: null, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Configured models from <c>GET /v1/models</c>.
    /// </summary>
    /// <param name="includeParams">Add each model's declared request options (<c>include_params=true</c>).</param>
    /// <param name="includeSessionOptions">Echo each model's configured session options (<c>include_session_options=true</c>).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The model list (<c>data</c> array).</returns>
    /// <exception cref="AudioCppException">The request failed.</exception>
    public async Task<IReadOnlyList<AudioCppModel>> ListModelsAsync(
        bool includeParams = false,
        bool includeSessionOptions = false,
        CancellationToken cancellationToken = default)
    {
        QueryBuilder query = new();
        if (includeParams)
        {
            query.Add("include_params", "true");
        }

        if (includeSessionOptions)
        {
            query.Add("include_session_options", "true");
        }

        ModelListEnvelope envelope = await SendJsonAsync<ModelListEnvelope>(HttpMethod.Get, "v1/models" + query, body: null, cancellationToken).ConfigureAwait(false);
        return envelope.Data ?? Array.Empty<AudioCppModel>();
    }

    /// <summary>
    /// Discoverable voice names from <c>GET /v1/audio/voices</c>: cached voice ids,
    /// configured preset names, and shared voice-dir WAV basenames. Pass
    /// <paramref name="model"/> when several models are configured.
    /// </summary>
    /// <param name="model">Model id to scope the lookup to.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The voice names the server reports.</returns>
    /// <exception cref="AudioCppException">The request failed.</exception>
    public async Task<IReadOnlyList<string>> ListVoicesAsync(string? model = null, CancellationToken cancellationToken = default)
    {
        QueryBuilder query = new();
        query.Add("model", model);
        VoicesEnvelope envelope = await SendJsonAsync<VoicesEnvelope>(HttpMethod.Get, "v1/audio/voices" + query, body: null, cancellationToken).ConfigureAwait(false);
        return envelope.Voices ?? Array.Empty<string>();
    }

    /// <summary>Unload specific models from memory; unknown ids are reported, not an error.</summary>
    /// <param name="modelIds">Model ids to unload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The unload report (unloaded and not-found ids).</returns>
    /// <exception cref="AudioCppException">The request failed.</exception>
    public async Task<UnloadModelsResult> UnloadModelsAsync(IEnumerable<string> modelIds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(modelIds);

        return await SendJsonAsync<UnloadModelsResult>(HttpMethod.Post, "v1/tasks/unload_models", new UnloadModelsBody { ModelIds = modelIds }, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Unload every currently loaded model.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The unload report.</returns>
    /// <exception cref="AudioCppException">The request failed.</exception>
    public async Task<UnloadModelsResult> UnloadAllModelsAsync(CancellationToken cancellationToken = default)
    {
        return await SendJsonAsync<UnloadModelsResult>(HttpMethod.Post, "v1/tasks/unload_all_models", body: null, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Dynamically load a model through <c>POST /v1/models/load</c>. Only servers built with
    /// native model management and started with <c>--ui-management</c> serve this route
    /// (otherwise HTTP 403, error type <c>forbidden</c>).
    /// </summary>
    /// <param name="request">The load request (validated before sending).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The load report.</returns>
    /// <exception cref="AudioCppException">The request failed.</exception>
    public async Task<ModelLoadResult> LoadModelAsync(ModelLoadRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        request.Validate();
        return await SendJsonAsync<ModelLoadResult>(HttpMethod.Post, "v1/models/load", request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>List available recipes from <c>GET /v1/recipes</c>.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The recipe list (<c>data</c> array).</returns>
    /// <exception cref="AudioCppException">The request failed.</exception>
    public async Task<IReadOnlyList<RecipeEntry>> ListRecipesAsync(CancellationToken cancellationToken = default)
    {
        RecipeListEnvelope envelope = await SendJsonAsync<RecipeListEnvelope>(HttpMethod.Get, "v1/recipes", body: null, cancellationToken).ConfigureAwait(false);
        return envelope.Data ?? Array.Empty<RecipeEntry>();
    }

    /// <summary>Builds a JSON content body from a request record.</summary>
    /// <param name="body">The request record to serialize.</param>
    /// <returns>The <c>application/json</c> content.</returns>
    internal static StringContent CreateJsonContent(object body)
    {
        return new StringContent(Json.Serialize(body), Encoding.UTF8, "application/json");
    }

    /// <summary>
    /// Throws <see cref="AudioCppException"/> when the response failed. The server reports
    /// errors as <c>{"error":{"message":"…","type":"server_error"}}</c>; anything else is
    /// surfaced as the raw body text.
    /// </summary>
    /// <param name="response">The response to check.</param>
    /// <param name="body">The body text already read from it.</param>
    /// <exception cref="AudioCppException">The response is not a success status.</exception>
    internal static void EnsureSuccess(HttpResponseMessage response, string body)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        int status = (int)response.StatusCode;
        string message = body;
        string? errorType = null;
        if (!string.IsNullOrWhiteSpace(body))
        {
            try
            {
                using JsonDocument doc = JsonDocument.Parse(body);
                if (doc.RootElement.ValueKind == JsonValueKind.Object &&
                    doc.RootElement.TryGetProperty("error", out JsonElement error) &&
                    error.ValueKind == JsonValueKind.Object &&
                    error.TryGetProperty("message", out JsonElement messageElement) &&
                    messageElement.ValueKind == JsonValueKind.String)
                {
                    message = messageElement.GetString()!;
                    if (error.TryGetProperty("type", out JsonElement type) &&
                        type.ValueKind == JsonValueKind.String)
                    {
                        errorType = type.GetString();
                    }
                }
            }
            catch (JsonException)
            {
                // Not the documented envelope: fall back to the raw body text.
            }
        }
        else
        {
            message = response.ReasonPhrase ?? ("HTTP " + status);
        }

        throw new AudioCppException(message, errorType, status);
    }

    /// <summary>Reads the <c>X-AudioCPP-Wall-Ms</c>, <c>X-AudioCPP-Audio-Duration-Ms</c> and <c>X-AudioCPP-RTF</c> headers.</summary>
    /// <param name="headers">The response headers.</param>
    /// <returns>The timing, or <see langword="null"/> when no timing header is present.</returns>
    internal static AudioCppTiming? ReadTimingHeaders(HttpResponseHeaders headers)
    {
        double? wall = TryGetDoubleHeader(headers, "X-AudioCPP-Wall-Ms");
        double? duration = TryGetDoubleHeader(headers, "X-AudioCPP-Audio-Duration-Ms");
        double? rtf = TryGetDoubleHeader(headers, "X-AudioCPP-RTF");
        if (wall == null && duration == null && rtf == null)
        {
            return null;
        }

        return new AudioCppTiming { WallMs = wall, AudioDurationMs = duration, Rtf = rtf };
    }

    /// <summary>Sends one request through the transport, adding the auth header first.</summary>
    /// <param name="request">The request to send.</param>
    /// <param name="completion">Completion option (buffered vs headers-read).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response.</returns>
    internal Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption completion, CancellationToken cancellationToken)
    {
        Prepare(request);
        return _http.SendAsync(request, completion, cancellationToken);
    }

    /// <summary>Adds the bearer header when an API key is configured.</summary>
    /// <param name="request">The request to decorate.</param>
    private void Prepare(HttpRequestMessage request)
    {
        if (!string.IsNullOrEmpty(ApiKey))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);
        }
    }

    /// <summary>Sends a request and returns the raw response bytes with their headers (audio bodies).</summary>
    /// <param name="request">The request to send; disposed by this method.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The raw response (bytes, headers, content type).</returns>
    /// <exception cref="AudioCppException">The response failed.</exception>
    internal async Task<AudioRawResponse> SendForBytesAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        using (request)
        {
            using HttpResponseMessage response = await SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken).ConfigureAwait(false);
            byte[] bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            EnsureSuccess(response, Encoding.UTF8.GetString(bytes, 0, Math.Min(bytes.Length, 8192)));
            return new AudioRawResponse(bytes, response.Headers, response.Content.Headers.ContentType?.MediaType);
        }
    }

    /// <summary>Builds the request URI from a relative path and optional query string.</summary>
    /// <param name="relativePath">Path below the base URL, without a leading slash.</param>
    /// <param name="query">Optional query string with a leading <c>?</c>.</param>
    /// <returns>The absolute URI.</returns>
    internal Uri BuildUri(string relativePath, string? query = null)
    {
        string baseUrl = _baseUri.AbsoluteUri;
        if (!baseUrl.EndsWith("/", StringComparison.Ordinal))
        {
            baseUrl += "/";
        }

        return new Uri(new Uri(baseUrl), relativePath + query);
    }

    /// <summary>Creates a request for a path below the base URL.</summary>
    /// <param name="method">The HTTP method.</param>
    /// <param name="path">Path below the base URL, without a leading slash.</param>
    /// <returns>The request.</returns>
    internal HttpRequestMessage NewRequest(HttpMethod method, string path)
    {
        return new HttpRequestMessage(method, BuildUri(path));
    }

    /// <summary>Sends a JSON request and deserializes the JSON response.</summary>
    /// <typeparam name="T">The parsed response type.</typeparam>
    /// <param name="method">The HTTP method.</param>
    /// <param name="path">Path below the base URL, with an optional query string.</param>
    /// <param name="body">The request record to serialize, or <see langword="null"/> for a bodyless request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deserialized response.</returns>
    /// <exception cref="AudioCppException">The response failed.</exception>
    internal async Task<T> SendJsonAsync<T>(HttpMethod method, string path, object? body, CancellationToken cancellationToken)
    {
        using HttpRequestMessage request = NewRequest(method, path);
        if (body != null)
        {
            request.Content = CreateJsonContent(body);
        }

        using HttpResponseMessage response = await SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken).ConfigureAwait(false);
        string responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        EnsureSuccess(response, responseBody);
        return Json.Deserialize<T>(responseBody);
    }

    /// <summary>Sends a request and returns the response body text (throws on failure).</summary>
    /// <param name="request">The request to send; disposed by this method.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response body text.</returns>
    /// <exception cref="AudioCppException">The response failed.</exception>
    internal async Task<string> ReadBodyAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        using (request)
        {
            using HttpResponseMessage response = await SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken).ConfigureAwait(false);
            string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            EnsureSuccess(response, body);
            return body;
        }
    }

    /// <summary>Sends a request, deserializes the JSON body, and returns the parsed value.</summary>
    /// <typeparam name="T">The parsed response type.</typeparam>
    /// <param name="request">The request to send; disposed by this method.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deserialized response.</returns>
    /// <exception cref="AudioCppException">The response failed.</exception>
    internal async Task<T> ReadJsonAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string body = await ReadBodyAsync(request, cancellationToken).ConfigureAwait(false);
        return Json.Deserialize<T>(body);
    }

    /// <summary>
    /// Opens a streamed response: throws for HTTP-level errors (JSON envelope), otherwise
    /// hands back the response so the caller can read the content stream incrementally.
    /// The caller owns the returned message.
    /// </summary>
    /// <param name="request">The request to send; not disposed — the caller owns the response.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The successful response, with the content stream still open.</returns>
    /// <exception cref="AudioCppException">The response failed.</exception>
    internal async Task<HttpResponseMessage> OpenStreamedResponseAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            using (response)
            {
                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                EnsureSuccess(response, body);
            }

            // EnsureSuccess always throws for non-2xx; keep the compiler happy.
            throw new AudioCppException("request failed", errorType: null, statusCode: null);
        }

        return response;
    }

    /// <summary>Parses the first invariant-culture double value of a response header.</summary>
    /// <param name="headers">The response headers.</param>
    /// <param name="name">The header name.</param>
    /// <returns>The parsed value, or <see langword="null"/> when absent or unparseable.</returns>
    private static double? TryGetDoubleHeader(HttpResponseHeaders headers, string name)
    {
        if (headers.TryGetValues(name, out IEnumerable<string>? values))
        {
            foreach (string value in values)
            {
                if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double parsed))
                {
                    return parsed;
                }
            }
        }

        return null;
    }

    /// <summary>HTTP content that streams a caller-owned PCM stream as a chunked request body.</summary>
    internal sealed class ChunkedStreamContent : HttpContent
    {
        private readonly Stream _source;
        private readonly CancellationToken _cancellationToken;

        /// <summary>Wraps the PCM stream to stream as a chunked body.</summary>
        /// <param name="source">The PCM stream to copy.</param>
        /// <param name="cancellationToken">Cancellation token observed while copying.</param>
        internal ChunkedStreamContent(Stream source, CancellationToken cancellationToken)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _cancellationToken = cancellationToken;
        }

        /// <inheritdoc/>
        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            try
            {
                await _source.CopyToAsync(stream, 81920, _cancellationToken).ConfigureAwait(false);
            }
            catch (PlatformNotSupportedException ex)
            {
                // Surfaced on platforms without full-duplex request streaming (browser/WASM).
                throw new PlatformNotSupportedException(
                    "The audio.cpp live routes stream the request body and response on one connection; this platform's HTTP stack cannot do that. Use a native client runtime.",
                    ex);
            }
        }

        /// <inheritdoc/>
        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }
    }
}