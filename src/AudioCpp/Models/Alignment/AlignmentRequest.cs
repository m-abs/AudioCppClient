using System;

namespace Mabs.AudioCpp.Models.Alignment;

/// <summary>Request of <c>POST /v1/audio/alignments</c>: forced alignment of a known transcript against uploaded audio.</summary>
public sealed class AlignmentRequest
{
    /// <summary>Alignment model id (configured with <c>task: "align"</c>, <c>mode: "offline"</c>). Required.</summary>
    public string? Model { get; set; }

    /// <summary>Known transcript. Required.</summary>
    public string? Text { get; set; }

    /// <summary>Language; model-dependent (Qwen3 Forced Aligner requires it).</summary>
    public string? Language { get; set; }

    /// <summary>Audio bytes to upload (multipart). Required.</summary>
    public byte[]? AudioFile { get; set; }

    /// <summary>Reported filename of <see cref="AudioFile"/>; must name a WAV.</summary>
    public string? AudioFileName { get; set; }

    /// <summary>Validates required fields.</summary>
    public void Validate()
    {
        if (string.IsNullOrEmpty(Model))
        {
            throw new ArgumentException("alignment requires a model id", nameof(Model));
        }

        if (string.IsNullOrEmpty(Text))
        {
            throw new ArgumentException("alignment requires the transcript text", nameof(Text));
        }

        if (AudioFile == null || string.IsNullOrEmpty(AudioFileName))
        {
            throw new ArgumentException("alignment requires an uploaded audio file (bytes + filename)");
        }
    }
}