using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Mabs.AudioCpp.Interfaces;
using Mabs.AudioCpp.Models.Clone;
using Mabs.AudioCpp.Models.Tasks;

namespace Mabs.AudioCpp.Clone;

/// <summary>Convenience clone client for the <c>miotts</c> family (model default <c>miotts_1_7b_q8_0</c>).
/// The configured model id must serve the <c>clone</c> task: <c>/v1/tasks/run</c> carries no task
/// field, the server resolves it from the model entry.</summary>
public sealed class MiottsCloneClient : ICloneClient
{
    private readonly AudioCppClient _client;

    /// <summary>Initializes the client bound to an existing <see cref="AudioCppClient"/>.</summary>
    /// <param name="client">The underlying client.</param>
    public MiottsCloneClient(AudioCppClient client)
    {
        _client = client;
    }

    /// <summary>Model id to run against; must serve the <c>clone</c> task. Defaults to the spec
    /// default package <c>miotts_1_7b_q8_0</c>.</summary>
    public string Model { get; set; } = "miotts_1_7b_q8_0";

    /// <summary>Runs one clone request against <see cref="Model"/>.</summary>
    /// <param name="request">The request (validated before sending).</param>
    /// <param name="cancellationToken">Cancellation token observed while enumerating.</param>
    /// <returns>The task result.</returns>
    public Task<TaskResult> CloneAsync(CloneRequest request, CancellationToken cancellationToken = default)
    {
        return _client.Tasks.RunAsync(new TaskRunRequest { Model = Model, Request = request.ToTaskRequestBody() }, cancellationToken);
    }

    /// <summary>Streams one clone request against <see cref="Model"/>.</summary>
    /// <param name="request">The request (validated before sending).</param>
    /// <param name="cancellationToken">Cancellation token observed while enumerating.</param>
    /// <returns>The stream events.</returns>
    public IAsyncEnumerable<TaskStreamEvent> StreamAsync(CloneRequest request, CancellationToken cancellationToken = default)
    {
        return _client.Tasks.StreamAsync(new TaskStreamRequest { Model = Model, Request = request.ToTaskRequestBody() }, cancellationToken);
    }
}