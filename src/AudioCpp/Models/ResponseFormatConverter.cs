using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mabs.AudioCpp.Models;

/// <summary>Writes and reads <see cref="ResponseFormat"/> as its wire string.</summary>
internal sealed class ResponseFormatConverter : JsonConverter<ResponseFormat>
{
    /// <inheritdoc/>
    public override ResponseFormat Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("response_format must be a string");
        }

        return new ResponseFormat(reader.GetString()!);
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, ResponseFormat value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}