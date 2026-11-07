namespace Mabs.AudioCpp.Models.Tasks;

/// <summary>Kind of a task batch stream event.</summary>
public enum TaskBatchEventKind
{
    /// <summary>One entry's result, with its original request index.</summary>
    Result,

    /// <summary>Aggregate batch timing; the terminal event.</summary>
    Done,
}