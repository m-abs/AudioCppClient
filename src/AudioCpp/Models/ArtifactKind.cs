using System;
using System.Collections.Generic;
using System.Linq;

namespace Mabs.AudioCpp.Models;

/// <summary>
/// The <c>kind</c> of an artifact. Wire values accepted by the server.
/// </summary>
public static class ArtifactKind
{
    /// <summary>Speaker embedding bytes.</summary>
    public const string SpeakerEmbedding = "speaker_embedding";

    /// <summary>Style embedding bytes.</summary>
    public const string StyleEmbedding = "style_embedding";

    /// <summary>Prompt embedding bytes.</summary>
    public const string PromptEmbedding = "prompt_embedding";

    /// <summary>Acoustic tokens.</summary>
    public const string AcousticTokens = "acoustic_tokens";

    /// <summary>MIDI data.</summary>
    public const string Midi = "midi";

    /// <summary>Transcript alignment state.</summary>
    public const string TranscriptAlignment = "transcript_alignment";

    /// <summary>Diarization continuation state.</summary>
    public const string DiarizationState = "diarization_state";

    /// <summary>VAD continuation state.</summary>
    public const string VadState = "vad_state";

    /// <summary>Application-defined payload.</summary>
    public const string Custom = "custom";

    /// <summary>The kinds the server accepts on request artifacts.</summary>
    public static readonly IReadOnlyList<string> All = new[]
    {
        SpeakerEmbedding, StyleEmbedding, PromptEmbedding, AcousticTokens, Midi,
        TranscriptAlignment, DiarizationState, VadState, Custom,
    };

    /// <summary>Throws when <paramref name="kind"/> is not a server-accepted value.</summary>
    /// <param name="kind">The artifact kind to check.</param>
    /// <exception cref="ArgumentException">The kind is not in <see cref="All"/>.</exception>
    public static void Validate(string kind)
    {
        if (!All.Contains(kind))
        {
            throw new ArgumentException(
                "unknown artifact kind '" + kind + "'; expected one of " + string.Join(", ", All),
                nameof(kind));
        }
    }
}