using System.Net.Http;
using System.Net.Http.Headers;

namespace Mabs.AudioCpp.Http;

/// <summary>Raw audio response bytes with their HTTP headers and media type.</summary>
internal readonly struct AudioRawResponse
{
    internal AudioRawResponse(byte[] bytes, HttpResponseHeaders headers, string? mediaType)
    {
        Bytes = bytes;
        Headers = headers;
        MediaType = mediaType;
    }

    internal byte[] Bytes { get; }

    internal HttpResponseHeaders Headers { get; }

    internal string? MediaType { get; }
}