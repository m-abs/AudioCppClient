using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mabs.AudioCpp.Models;

/// <summary>
/// Writes a stream speech request as the inner request's properties followed by
/// <c>stream_format</c> and <c>return_text</c>; reading is not supported.
/// </summary>
internal sealed class SpeechStreamRequestConverter : JsonConverter<SpeechStreamRequest>
{
    /// <inheritdoc/>
    public override SpeechStreamRequest Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException("SpeechStreamRequest can only be serialized.");
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, SpeechStreamRequest value, JsonSerializerOptions options)
    {
        value.Validate();
        writer.WriteStartObject();
        JsonElement inner = JsonSerializer.SerializeToElement(value.Request, options);
        foreach (JsonProperty property in inner.EnumerateObject())
        {
            property.WriteTo(writer);
        }

        writer.WriteString("stream_format", value.StreamFormat.Value);
        if (value.ReturnText.HasValue)
        {
            writer.WriteBoolean("return_text", value.ReturnText.Value);
        }

        writer.WriteEndObject();
    }
}