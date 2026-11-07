using System;
using System.Collections.Generic;

namespace Mabs.AudioCpp.Models.Transcription.Batch;

/// <summary>
/// Request of <c>POST /v1/batches/transcriptions</c>: multiple uploaded WAV files through
/// one native offline batch. The model must implement native batching.
/// </summary>
public sealed class BatchTranscriptionRequest
{
    /// <summary>Native-batching model id. Required.</summary>
    public string? Model { get; set; }

    /// <summary>(Filename, bytes) per file. At least one entry is required; upload order defines event <c>index</c>.</summary>
    public IList<AudioUpload> Files { get; } = [];

    /// <summary>Language applied to every file.</summary>
    public string? Language { get; set; }

    /// <summary>Recognition-context prompt applied to every file.</summary>
    public string? Prompt { get; set; }

    /// <summary>Busy wait bound applied to every file.</summary>
    public long? BusyTimeoutMs { get; set; }

    /// <summary>Model options applied to every file.</summary>
    public Dictionary<string, string>? Options { get; set; }

    /// <summary>Validates the request.</summary>
    public void Validate()
    {
        if (string.IsNullOrEmpty(Model))
        {
            throw new ArgumentException("batch transcription requires a model id", nameof(Model));
        }

        if (Files.Count == 0)
        {
            throw new ArgumentException("batch transcription requires at least one file");
        }

        foreach (AudioUpload file in Files)
        {
            if (string.IsNullOrEmpty(file.FileName) || file.Content == null)
            {
                throw new ArgumentException("every batch file needs a filename and content");
            }
        }
    }
}