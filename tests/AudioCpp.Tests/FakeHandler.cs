using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using FakeItEasy;

namespace Mabs.AudioCpp.Tests;

/// <summary>
/// FakeItEasy-faked <see cref="PipelineHandler"/> that queues canned responses per send
/// and records every outgoing request — method, URI, Authorization header, and the body
/// buffered to bytes — for assertions. Zero network access.
/// </summary>
public sealed class FakeHandler : IDisposable
{
    /// <summary>Canned responses in queue order, one per send.</summary>
    private readonly Queue<Func<HttpRequestMessage, HttpResponseMessage>> _responders = new();

    /// <summary>Builds the fake and wires every send to dequeue a queued responder.</summary>
    public FakeHandler()
    {
        Recorded = [];
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
    /// <param name="json">Response body JSON.</param>
    /// <param name="status">Status code to return; defaults to OK.</param>
    public void QueueJson(string json, HttpStatusCode status = HttpStatusCode.OK)
    {
        _responders.Enqueue(_ => new HttpResponseMessage(status)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        });
    }

    /// <summary>Queue a canned binary response.</summary>
    /// <param name="bytes">Response body bytes.</param>
    /// <param name="mediaType">Content-Type to set on the response.</param>
    /// <param name="status">Status code to return; defaults to OK.</param>
    /// <param name="configure">Optional hook to add headers or other response state.</param>
    public void QueueBytes(byte[] bytes, string mediaType, HttpStatusCode status = HttpStatusCode.OK, Action<HttpResponseMessage>? configure = null)
    {
        _responders.Enqueue(_ =>
        {
            HttpResponseMessage response = new(status)
            {
                Content = new ByteArrayContent(bytes),
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
            configure?.Invoke(response);
            return response;
        });
    }

    /// <summary>Queue a canned SSE response body.</summary>
    /// <param name="sseText">Event-stream text to serve.</param>
    /// <param name="status">Status code to return; defaults to OK.</param>
    public void QueueSse(string sseText, HttpStatusCode status = HttpStatusCode.OK)
    {
        _responders.Enqueue(_ => new HttpResponseMessage(status)
        {
            Content = new StringContent(sseText, Encoding.UTF8, "text/event-stream"),
        });
    }

    /// <summary>Queue an arbitrary responder.</summary>
    /// <param name="responder">Builds the response for the next send.</param>
    public void Queue(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        _responders.Enqueue(responder);
    }

    /// <summary>Records the request and dequeues the next canned response.</summary>
    /// <param name="request">The outgoing request to record.</param>
    /// <returns>The response from the next queued responder.</returns>
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

    /// <inheritdoc />
    public void Dispose()
    {
        Client.Dispose();
    }
}