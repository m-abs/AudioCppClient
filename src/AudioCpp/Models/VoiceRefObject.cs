using System;
using System.Text.Json.Serialization;

namespace Mabs.AudioCpp.Models;

/// <summary>
/// A <c>voice_ref</c> object: <c>{"type":"path","path":…}</c> or
/// <c>{"type":"base64","data":…}</c> (decoded payload limited to 5 MiB by the server).
/// Plain server-local path strings are also accepted by the wire; see
/// <see cref="SpeechRequest.VoiceRef"/>.
/// </summary>
public sealed record VoiceRefObject
{
    /// <summary><c>path</c> or <c>base64</c>.</summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "path";

    /// <summary>Server-local path; set when <see cref="Type"/> is <c>path</c>.</summary>
    [JsonPropertyName("path")]
    public string? Path { get; set; }

    /// <summary>Base64 payload; set when <see cref="Type"/> is <c>base64</c>.</summary>
    [JsonPropertyName("data")]
    public string? Data { get; set; }

    /// <summary>Creates a server-local path reference.</summary>
    /// <param name="path">Server-local WAV path.</param>
    /// <returns>The reference.</returns>
    public static VoiceRefObject FromPath(string path)
    {
        return new VoiceRefObject { Type = "path", Path = path ?? throw new ArgumentNullException(nameof(path)) };
    }

    /// <summary>Creates an inline Base64 WAV reference (bare Base64 or a data-URI; a data-URI is also accepted by the server).</summary>
    /// <param name="data">Base64 text.</param>
    /// <returns>The reference.</returns>
    public static VoiceRefObject FromBase64(string data)
    {
        return new VoiceRefObject { Type = "base64", Data = data ?? throw new ArgumentNullException(nameof(data)) };
    }

    /// <summary>Creates an inline reference from WAV bytes.</summary>
    /// <param name="wavBytes">WAV bytes to Base64-encode.</param>
    /// <returns>The reference.</returns>
    public static VoiceRefObject FromBytes(byte[] wavBytes)
    {
        return new VoiceRefObject { Type = "base64", Data = Convert.ToBase64String(wavBytes ?? throw new ArgumentNullException(nameof(wavBytes))) };
    }
}