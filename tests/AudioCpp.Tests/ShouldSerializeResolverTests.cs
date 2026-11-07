using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

using Mabs.AudioCpp.Utils;

using Xunit;

namespace Mabs.AudioCpp.Tests;

/// <summary>Wire behavior of <see cref="ShouldSerializeJsonTypeInfoResolver"/>, which drives every request body.</summary>
public class ShouldSerializeResolverTests
{
    private sealed record ProbeRequest
    {
        [JsonPropertyName("model")]
        public string? Model { get; init; }

        [JsonPropertyName("options")]
        public Dictionary<string, string>? Options { get; init; }

        [JsonPropertyName("include_artifacts")]
        public bool IncludeArtifacts { get; init; }

        [JsonIgnore]
        public bool Computed => true;

        public bool ShouldSerializeOptions() => Options is { Count: > 0 };

        public bool ShouldSerializeIncludeArtifacts() => true;
    }

    /// <summary>An empty options map is omitted by <c>ShouldSerializeOptions()</c>, and a snake_case property is still resolved by its CLR name.</summary>
    [Fact]
    public void EmptyOptionsOmittedAndSnakeCasePropertyResolvedByClrName()
    {
        string json = JsonSerializer.Serialize(new ProbeRequest { Model = "m", Options = [] }, Json.SerializerOptions);
        Assert.Equal("{\"model\":\"m\",\"include_artifacts\":false}", json);
    }

    /// <summary>A non-empty options map is written after the typed fields.</summary>
    [Fact]
    public void NonEmptyOptionsAreWritten()
    {
        string json = JsonSerializer.Serialize(new ProbeRequest { Model = "m", Options = new Dictionary<string, string> { ["seed"] = "42" } }, Json.SerializerOptions);
        Assert.Equal("{\"model\":\"m\",\"options\":{\"seed\":\"42\"},\"include_artifacts\":false}", json);
    }

    /// <summary>A null property is omitted by the default ignore condition and a <c>[JsonIgnore]</c> property never appears.</summary>
    [Fact]
    public void NullAndIgnoredPropertiesAreOmitted()
    {
        string json = JsonSerializer.Serialize(new ProbeRequest(), Json.SerializerOptions);
        Assert.Equal("{\"include_artifacts\":false}", json);
    }
}