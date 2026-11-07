using System;
using System.Text.Json.Serialization;

using Mabs.AudioCpp.Utils.Converters;

namespace Mabs.AudioCpp.Models;

/// <summary>A <c>stream_format</c> value: <c>sse</c> events or raw chunked audio.</summary>
[JsonConverter(typeof(StreamFormatConverter))]
public readonly struct StreamFormat : IEquatable<StreamFormat>
{
    /// <summary>Server-sent events.</summary>
    public static readonly StreamFormat Sse = new("sse");

    /// <summary>Chunked raw audio bytes.</summary>
    public static readonly StreamFormat Audio = new("audio");

    /// <summary>Creates a format from its wire string.</summary>
    /// <param name="value">The wire string, e.g. <c>sse</c>.</param>
    public StreamFormat(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>The wire string.</summary>
    public string Value { get; }

    /// <summary>Implicit conversion from string.</summary>
    /// <param name="value">The wire string.</param>
    /// <returns>The format.</returns>
    public static implicit operator StreamFormat(string value)
    {
        return new StreamFormat(value);
    }

    /// <summary>Implicit conversion to string.</summary>
    /// <param name="format">The format.</param>
    /// <returns>The wire string.</returns>
    public static implicit operator string(StreamFormat format)
    {
        return format.Value;
    }

    /// <inheritdoc/>
    public bool Equals(StreamFormat other)
    {
        return string.Equals(Value, other.Value, StringComparison.Ordinal);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is StreamFormat other && Equals(other);
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
    /// Determines whether two <see cref="StreamFormat"/> instances are equal.
    /// </summary>
    /// <param name="left">The left-hand side <see cref="StreamFormat"/>.</param>
    /// <param name="right">The right-hand side <see cref="StreamFormat"/>.</param>
    /// <returns><c>true</c> if the instances are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(StreamFormat left, StreamFormat right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two <see cref="StreamFormat"/> instances are not equal.
    /// </summary>
    /// <param name="left">The left-hand side <see cref="StreamFormat"/>.</param>
    /// <param name="right">The right-hand side <see cref="StreamFormat"/>.</param>
    /// <returns><c>true</c> if the instances are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(StreamFormat left, StreamFormat right)
    {
        return !(left == right);
    }
}