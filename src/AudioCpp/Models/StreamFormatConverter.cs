using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mabs.AudioCpp.Models;

/// <summary>Writes and reads <see cref="StreamFormat"/> as its wire string.</summary>
internal sealed class StreamFormatConverter : JsonConverter<StreamFormat>
{
    /// <inheritdoc/>
    public override StreamFormat Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("stream_format must be a string");
        }

        return new StreamFormat(reader.GetString()!);
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, StreamFormat value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}