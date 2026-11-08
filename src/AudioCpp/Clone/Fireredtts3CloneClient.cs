using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Mabs.AudioCpp.Interfaces;
using Mabs.AudioCpp.Models.Clone;
using Mabs.AudioCpp.Models.Tasks;

namespace Mabs.AudioCpp.Clone;

/// <summary>Convenience clone client for the <c>fireredtts3</c> family (model default <c>fireredtts3_instruct_q8_0</c>).
/// The configured model id must serve the <c>clone</c> task: <c>/v1/tasks/run</c> carries no task
/// field, the server resolves it from the model entry.</summary>
public sealed class Fireredtts3CloneClient : ICloneClient
{
    private readonly AudioCppClient _client;

    /// <summary>Initializes the client bound to an existing <see cref="AudioCppClient"/>.</summary>
    /// <param name="client">The underlying client.</param>
    public Fireredtts3CloneClient(AudioCppClient client)
    {
        _client = client;
    }

    /// <summary>Model id to run against; must serve the <c>clone</c> task. Defaults to the spec
    /// default package <c>fireredtts3_instruct_q8_0</c>.</summary>
    public string Model { get; set; } = "fireredtts3_instruct_q8_0";

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