using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using Mabs.AudioCpp.Models;

namespace Mabs.AudioCpp.Utils.Converters;

/// <summary>Writes and reads <see cref="WireString"/> as its wire string.</summary>
internal sealed class WireStringConverter : JsonConverter<WireString>
{
    /// <inheritdoc/>
    public override WireString Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("wire string must be a string");
        }

        return new WireString(reader.GetString()!);
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, WireString value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}