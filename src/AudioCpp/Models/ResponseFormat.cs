using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using Mabs.AudioCpp.Utils;

namespace Mabs.AudioCpp.Models;

/// <summary>
/// A <c>response_format</c> value. The wire type is a string; use the static instances or
/// construct from any string the server accepts (an unknown value is rejected by the server).
/// </summary>
[JsonConverter(typeof(ResponseFormatConverter))]
public readonly struct ResponseFormat : IEquatable<ResponseFormat>
{
    /// <summary>WAV bytes (default).</summary>
    public static readonly ResponseFormat Wav = new("wav");

    /// <summary>JSON object carrying Base64 WAV.</summary>
    public static readonly ResponseFormat Json = new("json");

    /// <summary>Raw PCM16 bytes.</summary>
    public static readonly ResponseFormat Pcm = new("pcm");

    /// <summary>MP3 bytes (only builds with the <c>mp3_encode</c> frontend module).</summary>
    public static readonly ResponseFormat Mp3 = new("mp3");

    /// <summary>Creates a format from its wire string.</summary>
    /// <param name="value">The wire string, e.g. <c>wav</c>.</param>
    public ResponseFormat(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>The wire string.</summary>
    public string Value { get; }

    /// <summary>Implicit conversion from string.</summary>
    /// <param name="value">The wire string.</param>
    /// <returns>The format.</returns>
    public static implicit operator ResponseFormat(string value)
    {
        return new ResponseFormat(value);
    }

    /// <summary>Implicit conversion to string.</summary>
    /// <param name="format">The format.</param>
    /// <returns>The wire string.</returns>
    public static implicit operator string(ResponseFormat format)
    {
        return format.Value;
    }

    /// <inheritdoc/>
    public bool Equals(ResponseFormat other)
    {
        return string.Equals(Value, other.Value, StringComparison.Ordinal);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is ResponseFormat other && Equals(other);
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
    /// Determines whether two <see cref="ResponseFormat"/> instances are equal.
    /// </summary>
    /// <param name="left">The left-hand side <see cref="ResponseFormat"/>.</param>
    /// <param name="right">The right-hand side <see cref="ResponseFormat"/>.</param>
    /// <returns><see langword="true"/> if the instances are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(ResponseFormat left, ResponseFormat right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two <see cref="ResponseFormat"/> instances are not equal.
    /// </summary>
    /// <param name="left">The left-hand side <see cref="ResponseFormat"/>.</param>
    /// <param name="right">The right-hand side <see cref="ResponseFormat"/>.</param>
    /// <returns><see langword="true"/> if the instances are not equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(ResponseFormat left, ResponseFormat right)
    {
        return !(left == right);
    }
}