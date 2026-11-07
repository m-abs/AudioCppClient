using System.Net.Http;
using System.Net.Http.Headers;

namespace Mabs.AudioCpp.Http;

/// <summary>Raw audio response bytes with their HTTP headers and media type.</summary>
internal readonly struct AudioRawResponse
{
    /// <summary>Stores the response body bytes, the response headers (for the timing
    /// headers), and the content media type.</summary>
    /// <param name="bytes">The response body bytes.</param>
    /// <param name="headers">The response headers, read for the <c>X-AudioCPP-*</c> timing values.</param>
    /// <param name="mediaType">The content media type, or <see langword="null"/> when the server sent none.</param>
    internal AudioRawResponse(byte[] bytes, HttpResponseHeaders headers, string? mediaType)
    {
        Bytes = bytes;
        Headers = headers;
        MediaType = mediaType;
    }

    /// <summary>The response body bytes.</summary>
    internal byte[] Bytes { get; }

    /// <summary>Response headers, read for the <c>X-AudioCPP-*</c> timing values.</summary>
    internal HttpResponseHeaders Headers { get; }

    /// <summary>Content media type, for example <c>audio/wav</c>; <see langword="null"/> when the server sent no content type.</summary>
    internal string? MediaType { get; }
}