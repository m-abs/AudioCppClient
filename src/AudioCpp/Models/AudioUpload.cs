using System;

namespace Mabs.AudioCpp.Models;

/// <summary>One uploaded file (multipart part).</summary>
public sealed class AudioUpload
{
    /// <summary>Creates an upload part from a filename and its bytes.</summary>
    /// <param name="fileName">Filename reported in the multipart form.</param>
    /// <param name="content">File bytes.</param>
    public AudioUpload(string fileName, byte[] content)
    {
        FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        Content = content ?? throw new ArgumentNullException(nameof(content));
    }

    /// <summary>Reported filename.</summary>
    public string FileName { get; }

    /// <summary>File bytes.</summary>
    public byte[] Content { get; }
}