using System.Threading;
using System.Threading.Tasks;

using Mabs.AudioCpp.Models.Design;
using Mabs.AudioCpp.Models.Tasks;

namespace Mabs.AudioCpp.Interfaces;

/// <summary>Voice-design convenience client contract for design-capable model families.</summary>
public interface IDesignClient
{
    /// <summary>Runs one design request against the configured model.</summary>
    /// <param name="request">The request (validated before sending).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The task result.</returns>
    Task<TaskResult> DesignAsync(DesignRequest request, CancellationToken cancellationToken = default);
}