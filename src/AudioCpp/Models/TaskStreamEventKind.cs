namespace Mabs.AudioCpp.Models;

/// <summary>Kind of a task stream event surfaced by the client.</summary>
public enum TaskStreamEventKind
{
    /// <summary>An incremental event.</summary>
    Event,

    /// <summary>The final result; the run succeeded when this arrives.</summary>
    Done,
}