using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mabs.AudioCpp.Utils;

/// <summary>
/// Shared serializer settings and helpers for request bodies and parsed responses. Nulls are
/// omitted; <c>ShouldSerialize{PropertyName}()</c> methods are honored through
/// <see cref="ShouldSerializeJsonTypeInfoResolver"/>.
/// </summary>
internal static class Json
{
    /// <summary>The options used for every request write and response read.</summary>
    public static readonly JsonSerializerOptions SerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        TypeInfoResolver = new ShouldSerializeJsonTypeInfoResolver(),
    };

    /// <summary>Serializes a request body with the shared options.</summary>
    /// <param name="value">The request record.</param>
    /// <returns>The JSON text.</returns>
    public static string Serialize(object value) => JsonSerializer.Serialize(value, SerializerOptions);

    /// <summary>Deserializes a response body with the shared options.</summary>
    /// <param name="json">The JSON text.</param>
    /// <typeparam name="T">The response type.</typeparam>
    /// <returns>The deserialized value.</returns>
    public static T Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, SerializerOptions)!;

    /// <summary>Deserializes a cached response body with the shared options.</summary>
    /// <param name="json">The JSON text, or null.</param>
    /// <typeparam name="T">The response type.</typeparam>
    /// <returns>The deserialized value.</returns>
    /// <exception cref="JsonException">The body is null, or the root is not a JSON object.</exception>
    public static T ParseObject<T>(string? json)
    {
        if (json is null)
        {
            throw new JsonException("The cached JSON body is null.");
        }

        T? value = JsonSerializer.Deserialize<T>(json, SerializerOptions);
        return value ?? throw new JsonException("The cached JSON body is not a JSON object.");
    }
}