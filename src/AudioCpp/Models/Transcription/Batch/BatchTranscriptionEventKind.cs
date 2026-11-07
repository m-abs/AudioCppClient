namespace Mabs.AudioCpp.Models.Transcription.Batch;

/// <summary>Kind of a batch transcription stream event.</summary>
public enum BatchTranscriptionEventKind
{
    /// <summary>One file's result, published as the model finishes it.</summary>
    Result,

    /// <summary>Aggregate batch timing; the terminal event.</summary>
    Done,
}