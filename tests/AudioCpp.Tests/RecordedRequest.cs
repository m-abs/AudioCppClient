using System;
using System.Net.Http;
using System.Text;

namespace Mabs.AudioCpp.Tests;

/// <summary>An outgoing request captured by <see cref="FakeHandler"/>.</summary>
public sealed class RecordedRequest
{
    /// <summary>Captures the request as it was sent.</summary>
    /// <param name="method">HTTP method used.</param>
    /// <param name="uri">Full request URI.</param>
    /// <param name="authorization">Raw Authorization header value, when set.</param>
    /// <param name="content">Buffered request body, when the request had content.</param>
    /// <param name="contentType">Content-Type header of the request body, when set.</param>
    internal RecordedRequest(HttpMethod method, Uri uri, string? authorization, byte[]? content, string? contentType)
    {
        Method = method;
        Uri = uri;
        Authorization = authorization;
        Content = content;
        ContentType = contentType;
    }

    /// <summary>HTTP method used for the request.</summary>
    public HttpMethod Method { get; }

    /// <summary>Full request URI.</summary>
    public Uri Uri { get; }

    /// <summary>Raw Authorization header value, when set.</summary>
    public string? Authorization { get; }

    /// <summary>Buffered request body, when the request had content.</summary>
    public byte[]? Content { get; }

    /// <summary>Content-Type header of the request body, when set.</summary>
    public string? ContentType { get; }

    /// <summary>The body decoded as UTF-8 text (for JSON assertions).</summary>
    /// <returns>The decoded body, or an empty string when there was no content.</returns>
    public string ContentText() => Content == null ? string.Empty : Encoding.UTF8.GetString(Content!);
}