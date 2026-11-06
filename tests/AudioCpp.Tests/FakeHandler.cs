using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using FakeItEasy;

namespace mabs.AudioCpp.Tests;

/// <summary>
/// Base handler exposing the protected <c>SendAsync</c> as a public virtual method so
/// FakeItEasy can configure it directly.
/// </summary>
public class PipelineHandler : HttpMessageHandler
{
    protected override sealed Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return OnSendAsync(request, cancellationToken);
    }

    /// <summary>Interception point for fakes.</summary>
    public virtual Task<HttpResponseMessage> OnSendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
    }
}

/// <summary>
/// FakeItEasy-faked <see cref="PipelineHandler"/> that queues canned responses per send
/// and records every outgoing request — method, URI, Authorization header, and the body
/// buffered to bytes — for assertions. Zero network access.
/// </summary>
public sealed class FakeHandler : IDisposable
{
    private readonly Queue<Func<HttpRequestMessage, HttpResponseMessage>> _responders = new();

    public FakeHandler()
    {
        Recorded = new List<RecordedRequest>();
        Fake = A.Fake<PipelineHandler>();
        A.CallTo(() => Fake.OnSendAsync(A<HttpRequestMessage>.Ignored, A<CancellationToken>.Ignored))
            .ReturnsLazily(call => RespondAsync(call.GetArgument<HttpRequestMessage>(0)!));
        Client = new HttpClient(Fake);
    }

    /// <summary>The underlying FakeItEasy fake, for advanced assertions.</summary>
    public PipelineHandler Fake { get; }

    /// <summary>HttpClient wired to the fake handler; pass it to <c>AudioCppClient</c>.</summary>
    public HttpClient Client { get; }

    /// <summary>Recorded requests, in send order.</summary>
    public List<RecordedRequest> Recorded { get; }

    /// <summary>The most recent recorded request.</summary>
    public RecordedRequest Last => Recorded[Recorded.Count - 1];

    /// <summary>Queue a canned JSON response.</summary>
    public void QueueJson(string json, HttpStatusCode status = HttpStatusCode.OK)
    {
        _responders.Enqueue(_ => new HttpResponseMessage(status)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        });
    }

    /// <summary>Queue a canned binary response.</summary>
    public void QueueBytes(byte[] bytes, string mediaType, HttpStatusCode status = HttpStatusCode.OK, Action<HttpResponseMessage>? configure = null)
    {
        _responders.Enqueue(_ =>
        {
            HttpResponseMessage response = new(status)
            {
                Content = new ByteArrayContent(bytes),
            };
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mediaType);
            configure?.Invoke(response);
            return response;
        });
    }

    /// <summary>Queue a canned SSE response body.</summary>
    public void QueueSse(string sseText, HttpStatusCode status = HttpStatusCode.OK)
    {
        _responders.Enqueue(_ => new HttpResponseMessage(status)
        {
            Content = new StringContent(sseText, Encoding.UTF8, "text/event-stream"),
        });
    }

    /// <summary>Queue an arbitrary responder.</summary>
    public void Queue(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        _responders.Enqueue(responder);
    }

    private async Task<HttpResponseMessage> RespondAsync(HttpRequestMessage request)
    {
        byte[]? contentBytes = null;
        string? contentType = request.Content?.Headers.ContentType?.ToString();
        if (request.Content != null)
        {
            contentBytes = await request.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
        }

        Recorded.Add(new RecordedRequest(
            request.Method,
            request.RequestUri!,
            request.Headers.Authorization?.ToString(),
            contentBytes,
            contentType));

        if (_responders.Count == 0)
        {
            throw new InvalidOperationException("FakeHandler has no queued response for request " + request.Method + " " + request.RequestUri);
        }

        return _responders.Dequeue()(request);
    }

    public void Dispose()
    {
        Client.Dispose();
    }
}

/// <summary>An outgoing request captured by <see cref="FakeHandler"/>.</summary>
public sealed class RecordedRequest
{
    internal RecordedRequest(HttpMethod method, Uri uri, string? authorization, byte[]? content, string? contentType)
    {
        Method = method;
        Uri = uri;
        Authorization = authorization;
        Content = content;
        ContentType = contentType;
    }

    public HttpMethod Method { get; }

    public Uri Uri { get; }

    /// <summary>Raw Authorization header value, when set.</summary>
    public string? Authorization { get; }

    /// <summary>Buffered request body, when the request had content.</summary>
    public byte[]? Content { get; }

    /// <summary>Content-Type header of the request body, when set.</summary>
    public string? ContentType { get; }

    /// <summary>The body decoded as UTF-8 text (for JSON assertions).</summary>
    public string ContentText() => Content == null ? string.Empty : Encoding.UTF8.GetString(Content!);
}