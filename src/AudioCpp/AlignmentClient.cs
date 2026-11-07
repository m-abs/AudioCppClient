using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Mabs.AudioCpp.Http;
using Mabs.AudioCpp.Models;

namespace Mabs.AudioCpp;

/// <summary>Forced alignment against <c>POST /v1/audio/alignments</c> (multipart only).</summary>
public sealed class AlignmentClient
{
    private readonly AudioCppClient _client;

    /// <summary>Creates the alignment endpoint group bound to <paramref name="client"/>.</summary>
    /// <param name="client">The owning client that carries the transport and base URL.</param>
    internal AlignmentClient(AudioCppClient client)
    {
        _client = client;
    }

    /// <summary>Align a known transcript against uploaded WAV bytes.</summary>
    /// <param name="request">The alignment request (validated before sending).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The alignment result with word timestamps.</returns>
    /// <exception cref="AudioCppException">The request failed.</exception>
    public async Task<AlignmentResult> RunAsync(AlignmentRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        request.Validate();
        using HttpRequestMessage httpRequest = new(HttpMethod.Post, _client.BuildUri("v1/audio/alignments"));
        httpRequest.Content = Multipart.BuildAlignment(request);
        return await _client.ReadJsonAsync<AlignmentResult>(httpRequest, cancellationToken).ConfigureAwait(false);
    }
}