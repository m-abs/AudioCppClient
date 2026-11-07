using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Mabs.AudioCpp.Http;

/// <summary>
/// One parsed server-sent event: the optional <c>event:</c> field name and the
/// concatenated <c>data:</c> payload. The audio.cpp server puts the discriminating
/// <c>"type"</c> inside the JSON payload, so <see cref="EventType"/> is generally
/// <see langword="null"/>; the <c>[DONE]</c> sentinel terminates the stream and is
/// never surfaced as an event. Callers parse <see cref="Data"/> themselves, typically
/// via <c>Json.Deserialize&lt;T&gt;</c> from <c>Mabs.AudioCpp.Models</c>.
/// </summary>
public readonly struct SseEvent
{
    /// <summary>Stores the parsed event fields.</summary>
    /// <param name="eventType">The <c>event:</c> field value, or <see langword="null"/> when absent.</param>
    /// <param name="data">The concatenated <c>data:</c> payload.</param>
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
/// The reader performs SSE framing only; event payloads are surfaced as raw JSON text
/// for callers to deserialize.
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
    private readonly List<byte> _line = [];
    private int _position;
    private int _bytesFilled;

    /// <summary>Creates a reader over <paramref name="stream"/> with the default line limit.</summary>
    /// <param name="stream">The response content stream to read events from.</param>
    public SseReader(Stream stream)
        : this(stream, DefaultMaxLineBytes)
    {
    }

    /// <summary>Creates a reader with an explicit maximum line size.</summary>
    /// <param name="stream">The response content stream to read events from.</param>
    /// <param name="maxLineBytes">Maximum size of one line, in bytes; must be positive.</param>
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
    /// error type or HTTP status per the server contract, and the server closes the
    /// connection after an error event without writing <c>[DONE]</c>.
    /// </summary>
    /// <param name="data">The <c>data:</c> payload of one event.</param>
    public static void ThrowIfErrorEvent(string data)
    {
        if (data.Length == 0 || data[0] != '{')
        {
            return;
        }

        using JsonDocument doc = JsonDocument.Parse(data);
        if (doc.RootElement.ValueKind == JsonValueKind.Object &&
            doc.RootElement.TryGetProperty("type", out JsonElement type) &&
            type.ValueKind == JsonValueKind.String &&
            type.GetString() == "error")
        {
            string message = "server reported an error over SSE";
            if (doc.RootElement.TryGetProperty("error", out JsonElement error))
            {
                if (error.ValueKind == JsonValueKind.Object &&
                    error.TryGetProperty("message", out JsonElement msg) &&
                    msg.ValueKind == JsonValueKind.String)
                {
                    message = msg.GetString()!;
                }
                else if (error.ValueKind == JsonValueKind.String)
                {
                    message = error.GetString()!;
                }
            }

            throw new AudioCppException(message, errorType: null, statusCode: null);
        }
    }

    /// <summary>
    /// Reads events until <c>data: [DONE]</c>, the stream end, or cancellation.
    /// </summary>
    /// <param name="onEvent">Callback invoked for each event; iteration stops if it throws.</param>
    /// <param name="cancellationToken">Cancellation token observed while reading the stream.</param>
    /// <returns>Completes when the stream terminates or the callback stops consuming events.</returns>
    public async Task ReadEventsAsync(Func<SseEvent, Task> onEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(onEvent);

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
    /// Enumerates events lazily. Every streaming method also has a callback overload
    /// for callers without <c>await foreach</c> language support.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token observed while reading the stream.</param>
    /// <returns>The event sequence; iteration ends when <c>[DONE]</c> or connection close is observed.</returns>
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
    /// <param name="cancellationToken">Cancellation token observed while reading.</param>
    /// <returns>The next event, or <see langword="null"/> at stream termination.</returns>
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

    /// <summary>Splits one line into SSE fields on <c>\r</c> (defensive; the server writes one field per line).</summary>
    /// <param name="line">The raw line.</param>
    /// <returns>The non-empty field segments.</returns>
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

    /// <summary>Reads one line (UTF-8 decoded, line terminators trimmed), or <see langword="null"/> at clean EOF.</summary>
    /// <param name="cancellationToken">Cancellation token observed while reading.</param>
    /// <returns>The line, or <see langword="null"/> when the stream ends with no pending bytes.</returns>
    /// <exception cref="InvalidOperationException">The line exceeded the configured maximum size.</exception>
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

    /// <summary>Decodes the buffered line (trimming one trailing <c>\r</c>) and clears the buffer.</summary>
    /// <returns>The line text.</returns>
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