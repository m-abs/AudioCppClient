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

    /// <summary>Initializes a new instance of the <see cref="AudioCppClient"/> class.Creates a client for the server at <paramref name="baseUrl"/>.</summary>
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
        if (baseUrl == null)
        {
            throw new ArgumentNullException(nameof(baseUrl));
        }

        _baseUri = baseUrl;
        _http = httpClient ?? new HttpClient();
        ApiKey = apiKey;
        Speech = new SpeechClient(this);
        Transcriptions = new TranscriptionClient(this);
        Alignments = new AlignmentClient(this);
        Tasks = new TaskClient(this);
        Recipes = new RecipeClient(this);
    }

    /// <summary>Initializes a new instance of the <see cref="AudioCppClient"/> class.Creates a client from a base URL string.</summary>
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
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<AudioCppHealth> GetHealthAsync(CancellationToken cancellationToken = default)
    {
        return await SendJsonAsync(HttpMethod.Get, "health", writeBody: null, root =>
        {
            return new AudioCppHealth
            {
                Status = SseReader.ReadString(root, "status"),
                Backend = SseReader.ReadString(root, "backend"),
                Models = SseReader.ReadInt(root, "models") ?? 0,
                Ui = SseReader.ReadBool(root, "ui") ?? false,
                UiManagement = SseReader.ReadBool(root, "ui_management") ?? false,
                Version = SseReader.ReadString(root, "version"),
                Git = SseReader.ReadString(root, "git"),
                GitDate = SseReader.ReadString(root, "git_date"),
            };
        }, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Configured models from <c>GET /v1/models</c>.
    /// </summary>
    /// <param name="includeParams">Add each model's declared request options (<c>include_params=true</c>).</param>
    /// <param name="includeSessionOptions">Echo each model's configured session options (<c>include_session_options=true</c>).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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

        return await SendJsonAsync(HttpMethod.Get, "v1/models" + query, writeBody: null, root =>
        {
            List<AudioCppModel> models = new();
            if (root.TryGetProperty("data", out JsonElement data) &&
                data.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                foreach (JsonElement element in data.EnumerateArray())
                {
                    models.Add(ParseModel(element));
                }
            }

            return (IReadOnlyList<AudioCppModel>)models;
        }, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Discoverable voice names from <c>GET /v1/audio/voices</c>: cached voice ids,
    /// configured preset names, and shared voice-dir WAV basenames. Pass
    /// <paramref name="model"/> when several models are configured.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<IReadOnlyList<string>> ListVoicesAsync(string? model = null, CancellationToken cancellationToken = default)
    {
        QueryBuilder query = new();
        query.Add("model", model);
        return await SendJsonAsync(HttpMethod.Get, "v1/audio/voices" + query, writeBody: null, root =>
        {
            List<string> voices = new();
            if (root.TryGetProperty("voices", out JsonElement array) &&
                array.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                foreach (JsonElement element in array.EnumerateArray())
                {
                    if (element.ValueKind == System.Text.Json.JsonValueKind.String)
                    {
                        voices.Add(element.GetString()!);
                    }
                }
            }

            return (IReadOnlyList<string>)voices;
        }, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Unload specific models from memory; unknown ids are reported, not an error.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<UnloadModelsResult> UnloadModelsAsync(IEnumerable<string> modelIds, CancellationToken cancellationToken = default)
    {
        if (modelIds == null)
        {
            throw new ArgumentNullException(nameof(modelIds));
        }

        return await SendJsonAsync(
            HttpMethod.Post,
            "v1/tasks/unload_models",
            writer =>
            {
                writer.WriteStartObject();
                writer.WriteStartArray("model_ids");
                foreach (string id in modelIds)
                {
                    writer.WriteStringValue(id);
                }

                writer.WriteEndArray();
                writer.WriteEndObject();
            },
            SseParsers.UnloadResult,
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Unload every currently loaded model.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<UnloadModelsResult> UnloadAllModelsAsync(CancellationToken cancellationToken = default)
    {
        return await SendJsonAsync(HttpMethod.Post, "v1/tasks/unload_all_models", writeBody: null, SseParsers.UnloadResult, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Dynamically load a model through <c>POST /v1/models/load</c>. Only servers built with
    /// native model management and started with <c>--ui-management</c> serve this route
    /// (otherwise HTTP 403, error type <c>forbidden</c>).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<ModelLoadResult> LoadModelAsync(ModelLoadRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        request.Validate();
        return await SendJsonAsync(HttpMethod.Post, "v1/models/load", request.WriteTo, SseParsers.ModelLoadResult, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>List available recipes from <c>GET /v1/recipes</c>.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<IReadOnlyList<RecipeEntry>> ListRecipesAsync(CancellationToken cancellationToken = default)
    {
        return await SendJsonAsync(HttpMethod.Get, "v1/recipes", writeBody: null, root =>
        {
            List<RecipeEntry> recipes = new();
            if (root.TryGetProperty("data", out JsonElement data) &&
                data.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                foreach (JsonElement element in data.EnumerateArray())
                {
                    List<RecipeRole> roles = new();
                    if (element.TryGetProperty("roles", out JsonElement roleArray) &&
                        roleArray.ValueKind == System.Text.Json.JsonValueKind.Array)
                    {
                        foreach (JsonElement role in roleArray.EnumerateArray())
                        {
                            roles.Add(new RecipeRole
                            {
                                Name = SseReader.ReadString(role, "name"),
                                Task = SseReader.ReadString(role, "task"),
                                Mode = SseReader.ReadString(role, "mode"),
                            });
                        }
                    }

                    recipes.Add(new RecipeEntry
                    {
                        Id = SseReader.ReadString(element, "id"),
                        Description = SseReader.ReadString(element, "description"),
                        Roles = roles,
                    });
                }
            }

            return (IReadOnlyList<RecipeEntry>)recipes;
        }, cancellationToken).ConfigureAwait(false);
    }

    internal static AudioCppModel ParseModel(JsonElement element)
    {
        Dictionary<string, string>? sessionOptions = null;
        if (element.TryGetProperty("session_options", out JsonElement options) &&
            options.ValueKind == System.Text.Json.JsonValueKind.Object)
        {
            sessionOptions = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (JsonProperty property in options.EnumerateObject())
            {
                sessionOptions[property.Name] = property.Value.ValueKind == System.Text.Json.JsonValueKind.String
                    ? property.Value.GetString()!
                    : property.Value.GetRawText();
            }
        }

        List<AudioCppModelParam>? @params = null;
        if (element.TryGetProperty("params", out JsonElement paramsElement) &&
            paramsElement.ValueKind == System.Text.Json.JsonValueKind.Array)
        {
            @params = new List<AudioCppModelParam>();
            foreach (JsonElement row in paramsElement.EnumerateArray())
            {
                List<string>? values = null;
                if (row.TryGetProperty("values", out JsonElement valuesElement) &&
                    valuesElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    values = new List<string>();
                    foreach (JsonElement value in valuesElement.EnumerateArray())
                    {
                        values.Add(value.ValueKind == System.Text.Json.JsonValueKind.String
                            ? value.GetString()!
                            : value.GetRawText());
                    }
                }

                @params.Add(new AudioCppModelParam
                {
                    Name = SseReader.ReadString(row, "name"),
                    Type = SseReader.ReadString(row, "type"),
                    Required = SseReader.ReadBool(row, "required") ?? false,
                    Description = SseReader.ReadString(row, "description"),
                    Default = ElementOrNull(row, "default"),
                    Min = ElementOrNull(row, "min"),
                    Max = ElementOrNull(row, "max"),
                    Values = values,
                });
            }
        }

        return new AudioCppModel
        {
            Id = SseReader.ReadString(element, "id"),
            Object = SseReader.ReadString(element, "object"),
            Configured = SseReader.ReadBool(element, "configured") ?? false,
            Device = SseReader.ReadInt(element, "device"),
            SessionOptions = sessionOptions,
            Params = @params,
        };
    }

    /// <summary>Builds a JSON content body from a writer callback.</summary>
    /// <returns></returns>
    internal static StringContent CreateJsonContent(Action<Utf8JsonWriter> writeBody)
    {
        using (MemoryStream stream = new())
        {
            using (Utf8JsonWriter writer = new(stream))
            {
                writeBody(writer);
            }

            return new StringContent(Encoding.UTF8.GetString(stream.ToArray()), Encoding.UTF8, "application/json");
        }
    }

    /// <summary>
    /// Throws <see cref="AudioCppException"/> when the response failed. The server reports
    /// errors as <c>{"error":{"message":"…","type":"server_error"}}</c>; anything else is
    /// surfaced as the raw body text.
    /// </summary>
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
                using JsonDocument doc = System.Text.Json.JsonDocument.Parse(body);
                if (doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Object &&
                    doc.RootElement.TryGetProperty("error", out JsonElement error) &&
                    error.ValueKind == System.Text.Json.JsonValueKind.Object &&
                    error.TryGetProperty("message", out JsonElement messageElement) &&
                    messageElement.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    message = messageElement.GetString()!;
                    if (error.TryGetProperty("type", out JsonElement type) &&
                        type.ValueKind == System.Text.Json.JsonValueKind.String)
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

    private static JsonElement? ElementOrNull(JsonElement obj, string property)
    {
        if (!obj.TryGetProperty(property, out JsonElement value) ||
            value.ValueKind == System.Text.Json.JsonValueKind.Null)
        {
            return null;
        }

        return value.Clone();
    }

    internal Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption completion, CancellationToken cancellationToken)
    {
        Prepare(request);
        return _http.SendAsync(request, completion, cancellationToken);
    }

    /// <summary>Adds the bearer header when an API key is configured.</summary>
    private void Prepare(HttpRequestMessage request)
    {
        if (!string.IsNullOrEmpty(ApiKey))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);
        }
    }

    /// <summary>Sends a request and returns the raw response bytes with their headers (audio bodies).</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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

    internal Uri BuildUri(string relativePath, string? query = null)
    {
        string baseUrl = _baseUri.AbsoluteUri;
        if (!baseUrl.EndsWith("/", StringComparison.Ordinal))
        {
            baseUrl += "/";
        }

        return new Uri(new Uri(baseUrl), relativePath + query);
    }

    internal HttpRequestMessage NewRequest(HttpMethod method, string path)
    {
        return new HttpRequestMessage(method, BuildUri(path));
    }

    /// <summary>Sends a JSON request and parses the JSON response (validates the body writer callback once).</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    internal async Task<T> SendJsonAsync<T>(HttpMethod method, string path, Action<Utf8JsonWriter>? writeBody, Func<JsonElement, T> parse, CancellationToken cancellationToken)
    {
        using HttpRequestMessage request = NewRequest(method, path);
        if (writeBody != null)
        {
            request.Content = CreateJsonContent(writeBody);
        }

        using HttpResponseMessage response = await SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken).ConfigureAwait(false);
        string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        EnsureSuccess(response, body);
        using (JsonDocument doc = System.Text.Json.JsonDocument.Parse(body))
        {
            return parse(doc.RootElement);
        }
    }

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

    internal async Task<T> ReadJsonAsync<T>(HttpRequestMessage request, Func<JsonElement, T> parse, CancellationToken cancellationToken)
    {
        string body = await ReadBodyAsync(request, cancellationToken).ConfigureAwait(false);
        using (JsonDocument doc = System.Text.Json.JsonDocument.Parse(body))
        {
            return parse(doc.RootElement);
        }
    }

    /// <summary>
    /// Opens a streamed response: throws for HTTP-level errors (JSON envelope), otherwise
    /// hands back the response so the caller can read the content stream incrementally.
    /// The caller owns the returned message.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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