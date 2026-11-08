using System.Threading;
using System.Threading.Tasks;

using Mabs.AudioCpp.Models.Tasks;
using Mabs.AudioCpp.Models.Tts;

namespace Mabs.AudioCpp.Interfaces;

/// <summary>Text-to-speech convenience client contract for tts-capable model families.</summary>
public interface ITtsClient
{
    /// <summary>Runs one tts request against the configured model.</summary>
    /// <param name="request">The request (validated before sending).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The task result.</returns>
    Task<TaskResult> SpeakAsync(TtsRequest request, CancellationToken cancellationToken = default);
}