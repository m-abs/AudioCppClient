using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Mabs.AudioCpp.Tests;

/// <summary>
/// Base handler exposing the protected <c>SendAsync</c> as a public virtual method so
/// FakeItEasy can configure it directly.
/// </summary>
public class PipelineHandler : HttpMessageHandler
{
    /// <inheritdoc />
    protected override sealed Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return OnSendAsync(request, cancellationToken);
    }

    /// <summary>Interception point for fakes.</summary>
    /// <param name="request">The outgoing request being intercepted.</param>
    /// <param name="cancellationToken">Cancellation token supplied by the HTTP stack.</param>
    /// <returns>The response the fake is configured to return.</returns>
    public virtual Task<HttpResponseMessage> OnSendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
    }
}