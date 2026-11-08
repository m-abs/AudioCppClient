using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Mabs.AudioCpp.Interfaces;
using Mabs.AudioCpp.Models.Tasks;
using Mabs.AudioCpp.Models.Tts;

namespace Mabs.AudioCpp.Tts;

/// <summary>Convenience tts client for the <c>higgs_audio_tts</c> family (model default <c>higgs_audio_tts_4b_q8_0</c>).
/// The configured model id must serve the <c>tts</c> task: <c>/v1/tasks/run</c> carries no task
/// field, the server resolves it from the model entry.</summary>
public sealed class HiggsAudioTtsClient : ITtsClient
{
    private readonly AudioCppClient _client;

    /// <summary>Initializes the client bound to an existing <see cref="AudioCppClient"/>.</summary>
    /// <param name="client">The underlying client.</param>
    public HiggsAudioTtsClient(AudioCppClient client)
    {
        _client = client;
    }

    /// <summary>Model id to run against; must serve the <c>tts</c> task. Defaults to the spec
    /// default package <c>higgs_audio_tts_4b_q8_0</c>.</summary>
    public string Model { get; set; } = "higgs_audio_tts_4b_q8_0";

    /// <summary>Runs one tts request against <see cref="Model"/>.</summary>
    /// <param name="request">The request (validated before sending).</param>
    /// <param name="cancellationToken">Cancellation token observed while enumerating.</param>
    /// <returns>The task result.</returns>
    public Task<TaskResult> SpeakAsync(TtsRequest request, CancellationToken cancellationToken = default)
    {
        return _client.Tasks.RunAsync(new TaskRunRequest { Model = Model, Request = request.ToTaskRequestBody() }, cancellationToken);
    }

    /// <summary>Streams one tts request against <see cref="Model"/>.</summary>
    /// <param name="request">The request (validated before sending).</param>
    /// <param name="cancellationToken">Cancellation token observed while enumerating.</param>
    /// <returns>The stream events.</returns>
    public IAsyncEnumerable<TaskStreamEvent> StreamAsync(TtsRequest request, CancellationToken cancellationToken = default)
    {
        return _client.Tasks.StreamAsync(new TaskStreamRequest { Model = Model, Request = request.ToTaskRequestBody() }, cancellationToken);
    }
}