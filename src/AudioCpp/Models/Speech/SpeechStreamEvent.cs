using System.Text.Json;
using System.Text.Json.Serialization;

using Mabs.AudioCpp.Utils;

namespace Mabs.AudioCpp.Models.Speech;

/// <summary>A speech stream event.</summary>
[JsonConverter(typeof(SpeechStreamEventConverter))]
public readonly struct SpeechStreamEvent
{
    /// <summary>Stores the parsed event fields.</summary>
    /// <param name="kind">The event kind.</param>
    /// <param name="pcm">PCM bytes for <see cref="SpeechStreamEventKind.AudioDelta"/>.</param>
    /// <param name="text">Text delta or final text.</param>
    /// <param name="timing">Timing for <see cref="SpeechStreamEventKind.AudioDone"/>.</param>
    internal SpeechStreamEvent(SpeechStreamEventKind kind, byte[]? pcm, string? text, AudioCppTiming? timing)
    {
        Kind = kind;
        Pcm = pcm;
        Text = text;
        Timing = timing;
    }

    /// <summary>Which event this is.</summary>
    public SpeechStreamEventKind Kind { get; }

    /// <summary>PCM bytes for <see cref="SpeechStreamEventKind.AudioDelta"/>.</summary>
    public byte[]? Pcm { get; }

    /// <summary>Text delta (<see cref="SpeechStreamEventKind.TextDelta"/>) or final text (<see cref="SpeechStreamEventKind.TextDone"/>).</summary>
    public string? Text { get; }

    /// <summary>Timing for <see cref="SpeechStreamEventKind.AudioDone"/>.</summary>
    public AudioCppTiming? Timing { get; }

    /// <summary>Parses one SSE data payload into a speech stream event.</summary>
    /// <param name="json">The event's JSON data payload.</param>
    /// <returns>The parsed event.</returns>
    /// <exception cref="JsonException">The payload carries an unknown event type.</exception>
    internal static SpeechStreamEvent Parse(string json)
    {
        return Json.Deserialize<SpeechStreamEvent>(json);
    }
}