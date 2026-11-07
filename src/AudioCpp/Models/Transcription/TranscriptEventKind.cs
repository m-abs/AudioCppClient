namespace Mabs.AudioCpp.Models.Transcription;

/// <summary>Kind of a transcription stream event.</summary>
public enum TranscriptEventKind
{
    /// <summary>Finalized text appended to the transcript.</summary>
    TextDelta,

    /// <summary>Whole provisional transcript to replace, not append (revisable models, e.g. Phonon-2).</summary>
    TextPartial,

    /// <summary>Full transcript; the final text event.</summary>
    TextDone,

    /// <summary>Newly emitted speaker turns (diarization models).</summary>
    DiarizationDelta,

    /// <summary>Complete speaker turns; do not append to the deltas.</summary>
    DiarizationDone,
}