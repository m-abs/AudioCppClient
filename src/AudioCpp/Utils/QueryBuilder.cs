using System;
using System.Text;

namespace Mabs.AudioCpp.Utils;

/// <summary>Minimal URL query-string builder (application/x-www-form-urlencoded escaping).</summary>
internal sealed class QueryBuilder
{
    private readonly StringBuilder _builder = new();

    /// <summary>Appends one <c>key=value</c> pair; empty values are skipped.</summary>
    /// <param name="key">The query key.</param>
    /// <param name="value">The query value; nothing is appended when null or empty.</param>
    internal void Add(string key, string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        _builder.Append(_builder.Length == 0 ? "?" : "&");
        _builder.Append(Uri.EscapeDataString(key));
        _builder.Append('=');
        _builder.Append(Uri.EscapeDataString(value));
    }

    /// <inheritdoc/>
    public override string ToString() => _builder.ToString();
}