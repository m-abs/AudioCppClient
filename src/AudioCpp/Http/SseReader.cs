using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Mabs.AudioCpp.Models;

namespace Mabs.AudioCpp.Http;

/// <summary>
/// One parsed server-sent event: the optional <c>event:</c> field name and the
/// concatenated <c>data:</c> payload. The audio.cpp server puts the discriminating
/// <c>"type"</c> inside the JSON payload, so <see cref="EventType"/> is generally
/// <see langword="null"/>; the <c>[DONE]</c> sentinel terminates the stream and is
/// never surfaced as an event.
/// </summary>
public readonly struct SseEvent
{
    /// <summary>Initializes a new instance of the <see cref="SseEvent"/> struct.</summary>
    public SseEvent(string? eventType, string data)
    {
        EventType = eventType;
        Data = data;
    }

    /// <summary>The <c>event:</c> field value, when present.</summary>
    public string? EventType { get; }

    /// <summary>The <c>data:</c> payload (UTF-8 decoded).</summary>
    public string Data { get; }
}

/// <summary>
/// Incremental reader for the audio.cpp server's <c>text/event-stream</c> responses.
/// </summary>
/// <remarks>
/// <para>
/// The reader consumes a stream without requiring the connection to close. Events are
/// framed the way the server writes them (<c>data: &lt;json&gt;\n\n</c>, terminated by
/// <c>data: [DONE]\n\n</c>); comment lines (<c>:</c> prefix) and <c>event:</c> fields
/// follow the SSE rules, and multi-line <c>data:</c> payloads are joined with <c>\n</c>
/// per spec even though the server never emits them.
/// </para>
/// <para>
/// Size limit: a single line may be at most the configured maximum (16 MiB by default).
/// The server documents that <c>task.stream.done</c> carries the whole result audio on
/// one line, about 640 KB of Base64 per 10 s of 24 kHz audio, so callers reading long
/// spoken replies should size this via the constructor.
/// </para>
/// </remarks>
public sealed class SseReader
{
    /// <summary>Default maximum size of one line: 16 MiB.</summary>
    public const int DefaultMaxLineBytes = 16 * 1024 * 1024;

    /// <summary>The literal the server writes to terminate a healthy stream.</summary>
    public const string DoneSentinel = "[DONE]";

    private readonly Stream _stream;
    private readonly int _maxLineBytes;
    private readonly byte[] _buffer;
    private readonly List<byte> _line = new();
    private int _position;
    private int _bytesFilled;

    /// <summary>Initializes a new instance of the <see cref="SseReader"/> class.Creates a reader over <paramref name="stream"/> with the default line limit.</summary>
    public SseReader(Stream stream)
        : this(stream, DefaultMaxLineBytes)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="SseReader"/> class.Creates a reader with an explicit maximum line size.</summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="maxLineBytes"/> is not positive.
    /// </exception>
    public SseReader(Stream stream, int maxLineBytes)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        if (maxLineBytes <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxLineBytes));
        }

        _maxLineBytes = maxLineBytes;
        _buffer = new byte[81920];
    }

    /// <summary>
    /// Throws <see cref="AudioCppException"/> when <paramref name="data"/> is an SSE
    /// error event (<c>{"type":"error","error":{"message":…}}</c>). SSE errors carry no
    /// error type or HTTP status per the server contract.
    /// </summary>
    public static void ThrowIfErrorEvent(string data)
    {
        if (data.Length == 0 || data[0] != '{')
        {
            return;
        }

        using (System.Text.Json.JsonDocument doc = System.Text.Json.JsonDocument.Parse(data))
        {
            if (doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Object &&
                doc.RootElement.TryGetProperty("type", out System.Text.Json.JsonElement type) &&
                type.ValueKind == System.Text.Json.JsonValueKind.String &&
                type.GetString() == "error")
            {
                string message = "server reported an error over SSE";
                if (doc.RootElement.TryGetProperty("error", out System.Text.Json.JsonElement error))
                {
                    if (error.ValueKind == System.Text.Json.JsonValueKind.Object &&
                        error.TryGetProperty("message", out System.Text.Json.JsonElement msg) &&
                        msg.ValueKind == System.Text.Json.JsonValueKind.String)
                    {
                        message = msg.GetString()!;
                    }
                    else if (error.ValueKind == System.Text.Json.JsonValueKind.String)
                    {
                        message = error.GetString()!;
                    }
                }

                throw new AudioCppException(message, errorType: null, statusCode: null);
            }
        }
    }

    /// <summary>
    /// Reads events until <c>data: [DONE]</c>, the stream end, or cancellation.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task ReadEventsAsync(Func<SseEvent, Task> onEvent, CancellationToken cancellationToken = default)
    {
        if (onEvent == null)
        {
            throw new ArgumentNullException(nameof(onEvent));
        }

        while (true)
        {
            SseEvent? next = await TryReadEventAsync(cancellationToken).ConfigureAwait(false);
            if (next == null)
            {
                return;
            }

            await onEvent(next.Value).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Enumerates events lazily. Available on all target frameworks because the
    /// library references <c>Microsoft.Bcl.AsyncInterfaces</c> on netstandard2.0.
    /// </summary>
    /// <returns></returns>
    public async IAsyncEnumerable<SseEvent> EnumerateAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        while (true)
        {
            SseEvent? next = await TryReadEventAsync(cancellationToken).ConfigureAwait(false);
            if (next == null)
            {
                yield break;
            }

            yield return next.Value;
        }
    }

    /// <summary>
    /// Reads the next event, or <see langword="null"/> once the stream terminates
    /// (<c>[DONE]</c> or connection end).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<SseEvent?> TryReadEventAsync(CancellationToken cancellationToken = default)
    {
        string? eventType = null;
        string? data = null;
        bool done = false;
        while (true)
        {
            string? line = await ReadLineAsync(cancellationToken).ConfigureAwait(false);
            if (line == null)
            {
                // Connection closed without [DONE]; the server writes this shape after
                // an SSE error event, so a pending event is still surfaced at EOF.
                break;
            }

            if (line.Length == 0)
            {
                if (!string.IsNullOrEmpty(data))
                {
                    done = data == DoneSentinel;
                    break; // blank line terminates the event
                }

                data = null; // spec: an empty data buffer is not dispatched; reset it
                continue; // keep-alive / leading separator
            }

            if (line[0] == ':')
            {
                continue; // comment
            }

            foreach (string field in SplitFields(line))
            {
                int colon = field.IndexOf(':');
                string name;
                string value;
                if (colon < 0)
                {
                    name = field;
                    value = string.Empty;
                }
                else
                {
                    name = field.Substring(0, colon);
                    value = field.Substring(colon + 1);
                    if (value.Length > 0 && value[0] == ' ')
                    {
                        value = value.Substring(1);
                    }
                }

                if (name == "data")
                {
                    data = data == null ? value : data + "\n" + value;
                }
                else if (name == "event")
                {
                    eventType = value;
                }
            }
        }

        if (done || data == null || data.Length == 0 || data == DoneSentinel)
        {
            return null;
        }

        return new SseEvent(eventType, data);
    }

    internal static AudioCppTiming? ReadTiming(System.Text.Json.JsonElement root, string property)
    {
        if (!root.TryGetProperty(property, out System.Text.Json.JsonElement timing) ||
            timing.ValueKind != System.Text.Json.JsonValueKind.Object)
        {
            return null;
        }

        bool ttftPresent = timing.TryGetProperty("ttft_ms", out System.Text.Json.JsonElement t) &&
            t.ValueKind != System.Text.Json.JsonValueKind.Null;
        return new AudioCppTiming
        {
            WallMs = ReadDouble(timing, "wall_ms"),
            AudioDurationMs = ReadDouble(timing, "audio_duration_ms"),
            Rtf = ReadDouble(timing, "rtf"),
            TtftMs = ReadDouble(timing, "ttft_ms"),
            TtftPresent = ttftPresent,
            FirstAudioBeforeInputEnd = ReadBool(timing, "first_audio_before_input_end"),
            OverlapMs = ReadDouble(timing, "overlap_ms"),
            RequestStartToFirstAudioMs = ReadDouble(timing, "request_start_to_first_audio_ms"),
            InputEndMs = ReadDouble(timing, "input_end_ms"),
        };
    }

    internal static double? ReadDouble(System.Text.Json.JsonElement obj, string property)
    {
        return obj.TryGetProperty(property, out System.Text.Json.JsonElement value) && value.ValueKind == System.Text.Json.JsonValueKind.Number ? value.GetDouble() : (double?)null;
    }

    internal static int? ReadInt(System.Text.Json.JsonElement obj, string property)
    {
        return obj.TryGetProperty(property, out System.Text.Json.JsonElement value) && value.ValueKind == System.Text.Json.JsonValueKind.Number ? value.GetInt32() : (int?)null;
    }

    internal static long? ReadLong(System.Text.Json.JsonElement obj, string property)
    {
        return obj.TryGetProperty(property, out System.Text.Json.JsonElement value) && value.ValueKind == System.Text.Json.JsonValueKind.Number && value.TryGetInt64(out long parsed) ? parsed : (long?)null;
    }

    internal static string? ReadString(System.Text.Json.JsonElement obj, string property)
    {
        return obj.TryGetProperty(property, out System.Text.Json.JsonElement value) && value.ValueKind == System.Text.Json.JsonValueKind.String ? value.GetString() : null;
    }

    internal static bool? ReadBool(System.Text.Json.JsonElement obj, string property)
    {
        if (!obj.TryGetProperty(property, out System.Text.Json.JsonElement value))
        {
            return null;
        }

        if (value.ValueKind == System.Text.Json.JsonValueKind.True)
        {
            return true;
        }

        return value.ValueKind == System.Text.Json.JsonValueKind.False ? (bool?)false : null;
    }

    internal static byte[]? ReadBase64(System.Text.Json.JsonElement obj, string property)
    {
        string? text = ReadString(obj, property);
        if (text == null)
        {
            return null;
        }

        return ReadBase64Payload(text);
    }

    /// <summary>Decodes a bare Base64 string or a <c>data:…;base64,…</c> URI.</summary>
    /// <returns></returns>
    internal static byte[] ReadBase64Payload(string text)
    {
        if (text.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            int comma = text.IndexOf(',');
            if (comma >= 0)
            {
                text = text.Substring(comma + 1);
            }
        }

        return Convert.FromBase64String(text);
    }

    internal static List<AudioCppSpeakerTurn>? ReadSpeakerTurns(System.Text.Json.JsonElement root, string property)
    {
        if (!root.TryGetProperty(property, out System.Text.Json.JsonElement array) ||
            array.ValueKind != System.Text.Json.JsonValueKind.Array)
        {
            return null;
        }

        List<AudioCppSpeakerTurn> turns = new();
        foreach (System.Text.Json.JsonElement element in array.EnumerateArray())
        {
            turns.Add(new AudioCppSpeakerTurn
            {
                StartSample = ReadLong(element, "start_sample"),
                EndSample = ReadLong(element, "end_sample"),
                SpeakerId = ReadString(element, "speaker_id"),
                Confidence = ReadDouble(element, "confidence"),
                Text = ReadString(element, "text"),
            });
        }

        return turns;
    }

    internal static List<AudioCppSegment>? ReadSegments(System.Text.Json.JsonElement root, string property)
    {
        if (!root.TryGetProperty(property, out System.Text.Json.JsonElement array) ||
            array.ValueKind != System.Text.Json.JsonValueKind.Array)
        {
            return null;
        }

        List<AudioCppSegment> segments = new();
        foreach (System.Text.Json.JsonElement element in array.EnumerateArray())
        {
            segments.Add(new AudioCppSegment
            {
                StartSample = ReadLong(element, "start_sample"),
                EndSample = ReadLong(element, "end_sample"),
                Confidence = ReadDouble(element, "confidence"),
                Text = ReadString(element, "text"),
            });
        }

        return segments;
    }

    internal static List<AudioCppWord>? ReadWords(System.Text.Json.JsonElement root, string property)
    {
        if (!root.TryGetProperty(property, out System.Text.Json.JsonElement array) ||
            array.ValueKind != System.Text.Json.JsonValueKind.Array)
        {
            return null;
        }

        List<AudioCppWord> words = new();
        foreach (System.Text.Json.JsonElement element in array.EnumerateArray())
        {
            words.Add(new AudioCppWord
            {
                Word = ReadString(element, "word"),
                Start = ReadDouble(element, "start"),
                End = ReadDouble(element, "end"),
                StartSample = ReadLong(element, "start_sample"),
                EndSample = ReadLong(element, "end_sample"),
                Confidence = ReadDouble(element, "confidence"),
            });
        }

        return words;
    }

    internal static List<AudioArtifact>? ReadArtifacts(System.Text.Json.JsonElement root, string property)
    {
        if (!root.TryGetProperty(property, out System.Text.Json.JsonElement array) ||
            array.ValueKind != System.Text.Json.JsonValueKind.Array)
        {
            return null;
        }

        List<AudioArtifact> artifacts = new();
        foreach (System.Text.Json.JsonElement element in array.EnumerateArray())
        {
            Dictionary<string, string>? meta = null;
            if (element.TryGetProperty("meta", out System.Text.Json.JsonElement metaElement) &&
                metaElement.ValueKind == System.Text.Json.JsonValueKind.Object)
            {
                meta = new Dictionary<string, string>(StringComparer.Ordinal);
                foreach (System.Text.Json.JsonProperty field in metaElement.EnumerateObject())
                {
                    meta[field.Name] = field.Value.ValueKind == System.Text.Json.JsonValueKind.String
                        ? field.Value.GetString()!
                        : field.Value.GetRawText();
                }
            }

            artifacts.Add(new AudioArtifact
            {
                Id = ReadString(element, "id") ?? string.Empty,
                Kind = ReadString(element, "kind") ?? "custom",
                Payload = ReadBase64(element, "payload") ?? Array.Empty<byte>(),
                Meta = meta,
            });
        }

        return artifacts;
    }

    internal static List<NamedAudioOutput>? ReadNamedAudioOutputs(System.Text.Json.JsonElement root, string property)
    {
        if (!root.TryGetProperty(property, out System.Text.Json.JsonElement array) ||
            array.ValueKind != System.Text.Json.JsonValueKind.Array)
        {
            return null;
        }

        List<NamedAudioOutput> outputs = new();
        foreach (System.Text.Json.JsonElement element in array.EnumerateArray())
        {
            outputs.Add(new NamedAudioOutput
            {
                Id = ReadString(element, "id") ?? string.Empty,
                Audio = ReadBase64(element, "audio"),
                SampleRate = ReadInt(element, "sample_rate"),
                Channels = ReadInt(element, "channels"),
                Format = ReadString(element, "format"),
            });
        }

        return outputs;
    }

    internal static TaskResult ReadTaskResult(System.Text.Json.JsonElement element)
    {
        return new TaskResult
        {
            Text = ReadString(element, "text"),
            Language = ReadString(element, "language"),
            Audio = ReadBase64(element, "audio"),
            SampleRate = ReadInt(element, "sample_rate"),
            Channels = ReadInt(element, "channels"),
            NamedAudioOutputs = ReadNamedAudioOutputs(element, "named_audio_outputs"),
            Artifacts = ReadArtifacts(element, "artifacts"),
            Segments = ReadSegments(element, "segments"),
            SpeakerTurns = ReadSpeakerTurns(element, "speaker_turns"),
            Words = ReadWords(element, "words"),
            Timing = ReadTiming(element, "timing"),
        };
    }

    private static IEnumerable<string> SplitFields(string line)
    {
        // An SSE line may carry several fields separated by \r; the audio.cpp server
        // writes one field per line, but split defensively.
        if (line.IndexOf('\r') < 0)
        {
            yield return line;
            yield break;
        }

        foreach (string part in line.Split('\r'))
        {
            if (part.Length > 0)
            {
                yield return part;
            }
        }
    }

    private async Task<string?> ReadLineAsync(CancellationToken cancellationToken)
    {
        _line.Clear();
        while (true)
        {
            if (_position >= _bytesFilled)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _bytesFilled = await _stream.ReadAsync(_buffer, 0, _buffer.Length, cancellationToken).ConfigureAwait(false);
                _position = 0;
                if (_bytesFilled == 0)
                {
                    return _line.Count == 0 ? null : FlushLine();
                }
            }

            for (; _position < _bytesFilled; _position++)
            {
                byte b = _buffer[_position];
                if (b == (byte)'\n')
                {
                    _position++;
                    return FlushLine();
                }

                _line.Add(b);
                if (_line.Count > _maxLineBytes)
                {
                    throw new InvalidOperationException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "SSE line exceeded the {0} byte limit; increase the max line size to read large task.stream.done results.",
                            _maxLineBytes));
                }
            }
        }
    }

    private string FlushLine()
    {
        // Trim one trailing \r for \r\n framing.
        int count = _line.Count;
        if (count > 0 && _line[count - 1] == (byte)'\r')
        {
            count--;
        }

        byte[] bytes = _line.ToArray();
        _line.Clear();
        return Encoding.UTF8.GetString(bytes, 0, count);
    }
}