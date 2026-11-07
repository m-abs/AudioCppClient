using Mabs.AudioCpp.Models;

namespace Mabs.AudioCpp.Enums;

/// <summary>Kind of a <see cref="SpeechStreamEvent"/>.</summary>
public enum SpeechStreamEventKind
{
    /// <summary>A chunk of PCM16 audio.</summary>
    AudioDelta,

    /// <summary>Final audio event carrying timing; a healthy stream sends <c>[DONE]</c> after it.</summary>
    AudioDone,

    /// <summary>Text written since the previous event (live streams with <c>return_text</c>).</summary>
    TextDelta,

    /// <summary>The whole text, before <see cref="AudioDone"/>.</summary>
    TextDone,
}