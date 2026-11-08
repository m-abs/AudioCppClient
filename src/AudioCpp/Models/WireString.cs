using System;
using System.Text.Json.Serialization;

using Mabs.AudioCpp.Utils.Converters;

namespace Mabs.AudioCpp.Models;

/// <summary>
/// A wire-string value used by both <c>response_format</c> and <c>stream_format</c>. Use the
/// static instances or construct from any string the server accepts (an unknown value is
/// rejected by the server).
/// </summary>
[JsonConverter(typeof(WireStringConverter))]
public readonly struct WireString : IEquatable<WireString>
{
    /// <summary>WAV bytes (<c>response_format</c> default).</summary>
    public static readonly WireString WAV = new("wav");

    /// <summary>JSON object carrying Base64 WAV, or the JSON response envelope.</summary>
    public static readonly WireString JSON = new("json");

    /// <summary>Raw PCM16 bytes.</summary>
    public static readonly WireString PCM = new("pcm");

    /// <summary>MP3 bytes (only builds with the <c>mp3_encode</c> frontend module).</summary>
    public static readonly WireString MP3 = new("mp3");

    /// <summary>Server-sent events.</summary>
    public static readonly WireString SSE = new("sse");

    /// <summary>Chunked raw audio bytes.</summary>
    public static readonly WireString Audio = new("audio");

    /// <summary>Creates a value from its wire string.</summary>
    /// <param name="value">The wire string, e.g. <c>wav</c>.</param>
    public WireString(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>The wire string.</summary>
    public string Value { get; }

    /// <summary>Implicit conversion from string.</summary>
    /// <param name="value">The wire string.</param>
    /// <returns>The value.</returns>
    public static implicit operator WireString(string value)
    {
        return new WireString(value);
    }

    /// <summary>Implicit conversion to string.</summary>
    /// <param name="value">The wire string value.</param>
    /// <returns>The wire string.</returns>
    public static implicit operator string(WireString value)
    {
        return value.Value;
    }

    /// <inheritdoc/>
    public bool Equals(WireString other)
    {
        return string.Equals(Value, other.Value, StringComparison.Ordinal);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is WireString other && Equals(other);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return Value == null ? 0 : Value.GetHashCode();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return Value;
    }

    /// <summary>
    /// Determines whether two <see cref="WireString"/> instances are equal.
    /// </summary>
    /// <param name="left">The left-hand side <see cref="WireString"/>.</param>
    /// <param name="right">The right-hand side <see cref="WireString"/>.</param>
    /// <returns><see langword="true"/> if the instances are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(WireString left, WireString right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two <see cref="WireString"/> instances are not equal.
    /// </summary>
    /// <param name="left">The left-hand side <see cref="WireString"/>.</param>
    /// <param name="right">The right-hand side <see cref="WireString"/>.</param>
    /// <returns><see langword="true"/> if the instances are not equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(WireString left, WireString right)
    {
        return !(left == right);
    }
}