using System.Threading;
using System.Threading.Tasks;

using Mabs.AudioCpp.Models.Clone;
using Mabs.AudioCpp.Models.Tasks;

namespace Mabs.AudioCpp.Interfaces;

/// <summary>Voice-clone convenience client contract for clone-capable model families.</summary>
public interface ICloneClient
{
    /// <summary>Runs one clone request against the configured model.</summary>
    /// <param name="request">The request (validated before sending).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The task result.</returns>
    Task<TaskResult> CloneAsync(CloneRequest request, CancellationToken cancellationToken = default);
}