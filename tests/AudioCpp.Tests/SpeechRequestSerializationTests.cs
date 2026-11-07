using System;
using System.Collections.Generic;
using System.IO;

using Mabs.AudioCpp.Models;
using Mabs.AudioCpp.Utils;

using Xunit;

namespace Mabs.AudioCpp.Tests;

/// <summary>Golden serialization of request bodies through the shared serializer settings.</summary>
public class SpeechRequestSerializationTests
{
    /// <summary>Serializes a request body with the shared options.</summary>
    /// <param name="request">The request to serialize.</param>
    /// <returns>The JSON produced by <see cref="Json.SerializerOptions"/>.</returns>
    private static string Serialize(SpeechRequest request)
    {
        return Json.Serialize(request);
    }

    /// <summary>A minimal request serializes to exactly model and input.</summary>
    [Fact]
    public void MinimalBody()
    {
        string json = Serialize(new SpeechRequest { Model = "kokoro", Input = "hello" });
        Assert.Equal("{\"model\":\"kokoro\",\"input\":\"hello\"}", json);
    }

    /// <summary>Set fields serialize in server field order with unset fields omitted.</summary>
    [Fact]
    public void FieldOrderAndNulls()
    {
        string json = Serialize(new SpeechRequest
        {
            Model = "m",
            Input = "t",
            Language = "en",
            Voice = "am_adam",
            MaxTokens = 96,
            Seed = "1234",
        });
        Assert.Equal("{\"model\":\"m\",\"input\":\"t\",\"language\":\"en\",\"voice\":\"am_adam\",\"seed\":\"1234\",\"max_tokens\":96}", json);
    }

    /// <summary>Setting both instruct and instructions fails validation.</summary>
    [Fact]
    public void InstructAndInstructionsBothSetThrows()
    {
        SpeechRequest request = new()
        {
            Model = "m",
            Input = "t",
            Instruct = "a",
            Instructions = "b",
        };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("instruct and instructions", ex.Message);
    }

    /// <summary>Instruct serializes when set; instructions serialize only when instruct is absent.</summary>
    [Fact]
    public void InstructPreferredThenInstructions()
    {
        string json = Serialize(new SpeechRequest { Model = "m", Input = "t", Instruct = "calm" });
        Assert.Contains("\"instruct\":\"calm\"", json);
        Assert.DoesNotContain("instructions", json);

        json = Serialize(new SpeechRequest { Model = "m", Input = "t", Instructions = "speak" });
        Assert.Contains("\"instructions\":\"speak\"", json);
    }

    /// <summary>Setting both speed and speaking_rate fails validation.</summary>
    [Fact]
    public void SpeedAndSpeakingRateBothSetThrows()
    {
        SpeechRequest request = new() { Model = "m", Input = "t", Speed = 1.1, SpeakingRate = 1.2 };
        Assert.Throws<ArgumentException>(() => request.Validate());
    }

    /// <summary>A zero speed fails validation.</summary>
    [Fact]
    public void NonPositiveSpeedThrows()
    {
        Assert.Throws<ArgumentException>(() => new SpeechRequest { Model = "m", Input = "t", Speed = 0 }.Validate());
    }

    /// <summary>A seed beyond int64 range stays a string in the JSON.</summary>
    [Fact]
    public void StringSeedPreserved()
    {
        string json = Serialize(new SpeechRequest { Model = "m", Input = "t", Seed = "18446744073709551615" });
        Assert.Contains("\"seed\":\"18446744073709551615\"", json);
    }

    /// <summary>An options key colliding with a typed field fails validation.</summary>
    [Fact]
    public void OptionsCollisionThrows()
    {
        SpeechRequest request = new()
        {
            Model = "m",
            Input = "t",
            MaxTokens = 96,
            Options = new Dictionary<string, string> { ["max_tokens"] = "128" },
        };
        ArgumentException ex = Assert.Throws<ArgumentException>(() => request.Validate());
        Assert.Contains("max_tokens", ex.Message);
    }

    /// <summary>The options map serializes after the typed fields.</summary>
    [Fact]
    public void OptionsMapSerializedAfterTypedFields()
    {
        SpeechRequest request = new()
        {
            Model = "m",
            Input = "t",
            Options = new Dictionary<string, string> { ["retry_badcase"] = "false", ["chunk_seconds"] = "2" },
        };
        string json = Serialize(request);
        Assert.Equal("{\"model\":\"m\",\"input\":\"t\",\"options\":{\"retry_badcase\":\"false\",\"chunk_seconds\":\"2\"}}", json);
    }

    /// <summary>An empty options map is omitted, matching the wire format the server expects.</summary>
    [Fact]
    public void EmptyOptionsMapIsOmitted()
    {
        string json = Serialize(new SpeechRequest { Model = "m", Input = "t", Options = [] });
        Assert.Equal("{\"model\":\"m\",\"input\":\"t\"}", json);
    }

    /// <summary>Voice ref serializes as a plain string or as a typed base64/path object.</summary>
    [Fact]
    public void VoiceRefStringAndObject()
    {
        string json = Serialize(new SpeechRequest { Model = "m", Input = "t", VoiceRef = "/tmp/ref.wav" });
        Assert.Contains("\"voice_ref\":\"/tmp/ref.wav\"", json);

        json = Serialize(new SpeechRequest { Model = "m", Input = "t", VoiceRef = VoiceRefObject.FromBase64("UklGRh") });
        Assert.Contains("\"voice_ref\":{\"type\":\"base64\",\"data\":\"UklGRh\"}", json);

        json = Serialize(new SpeechRequest { Model = "m", Input = "t", VoiceRef = VoiceRefObject.FromPath("voices/alice.wav") });
        Assert.Contains("\"voice_ref\":{\"type\":\"path\",\"path\":\"voices/alice.wav\"}", json);
    }

    /// <summary>Response format and busy timeout serialize under their server field names.</summary>
    [Fact]
    public void ResponseFormatAndBusyTimeout()
    {
        SpeechRequest request = new()
        {
            Model = "m",
            Input = "t",
            ResponseFormat = ResponseFormat.Pcm,
            BusyTimeoutMs = 5000,
        };
        string json = Serialize(request);
        Assert.Contains("\"response_format\":\"pcm\"", json);
        Assert.Contains("\"busy_timeout_ms\":5000", json);
    }

    /// <summary>Missing model or input fails validation, while an empty input string is allowed.</summary>
    [Fact]
    public void MissingModelOrInputThrows()
    {
        Assert.Throws<ArgumentException>(() => new SpeechRequest { Input = "t" }.Validate());
        Assert.Throws<ArgumentException>(() => new SpeechRequest { Model = "m" }.Validate());

        // Empty input string is allowed (the server validates content; null does not serialize).
        new SpeechRequest { Model = "m", Input = string.Empty }.Validate();
    }

    /// <summary>The stream wrapper adds stream_format and return_text after the inner request fields.</summary>
    [Fact]
    public void StreamRequestAddsStreamFormatAndReturnText()
    {
        SpeechStreamRequest request = new()
        {
            Request = new SpeechRequest { Model = "m", Input = "t", ResponseFormat = ResponseFormat.Pcm },
            StreamFormat = StreamFormat.Sse,
            ReturnText = true,
        };
        string json = Json.Serialize(request);
        Assert.Equal("{\"model\":\"m\",\"input\":\"t\",\"response_format\":\"pcm\",\"stream_format\":\"sse\",\"return_text\":true}", json);
    }

    /// <summary>return_text with a non-SSE stream format fails validation.</summary>
    [Fact]
    public void ReturnTextWithoutSseThrows()
    {
        SpeechStreamRequest request = new()
        {
            Request = new SpeechRequest { Model = "m", Input = "t" },
            StreamFormat = StreamFormat.Audio,
            ReturnText = true,
        };
        Assert.Throws<ArgumentException>(() => request.Validate());
    }

    /// <summary>The live speech query string is percent-encoded and includes the default rate and format.</summary>
    [Fact]
    public void LiveSpeechQueryEncodesAndValidates()
    {
        LiveSpeechRequest request = new()
        {
            Model = "s2s-stream",
            Input = "hello world & more",
            Language = "en",
        };
        string query = request.BuildQueryString();
        Assert.StartsWith("?", query);
        Assert.Contains("model=s2s-stream", query);
        Assert.Contains("input=hello%20world%20%26%20more", query);
        Assert.Contains("sample_rate=16000", query);
        Assert.Contains("channels=1", query);
        Assert.Contains("stream_format=sse", query);
    }

    /// <summary>Out-of-range sample rate, out-of-range channels, and missing model all fail query building.</summary>
    [Fact]
    public void LiveSpeechRejectsOutOfRange()
    {
        Assert.Throws<ArgumentException>(() => new LiveSpeechRequest { Model = "m", SampleRate = 100 }.BuildQueryString());
        Assert.Throws<ArgumentException>(() => new LiveSpeechRequest { Model = "m", Channels = 17 }.BuildQueryString());
        Assert.Throws<ArgumentException>(() => new LiveSpeechRequest { Input = "x" }.BuildQueryString());
    }

    /// <summary>Supplying both audio path and audio base64 fails validation.</summary>
    [Fact]
    public void TaskRequestRejectsBothAudioForms()
    {
        TaskRequestBody body = new()
        {
            AudioPath = "/tmp/a.wav",
            AudioBase64 = [1],
        };
        Assert.Throws<ArgumentException>(() => body.Validate());
    }

    /// <summary>Artifacts serialize with base64 payload and meta, followed by the options map.</summary>
    [Fact]
    public void TaskRequestSerializesArtifacts()
    {
        TaskRequestBody body = new()
        {
            AudioPath = "/in.wav",
            Artifacts = new List<AudioArtifactInput>
            {
                new()
                {
                    Id = "example.state",
                    Kind = ArtifactKind.Custom,
                    Payload = [1, 2, 3],
                    Meta = new Dictionary<string, string> { ["format"] = "example/1" },
                },
            },
            Options = new Dictionary<string, string> { ["seed"] = "42" },
        };
        string json = Json.Serialize(body);
        Assert.Equal(
            "{\"audio\":\"/in.wav\",\"artifacts\":[{\"id\":\"example.state\",\"kind\":\"custom\",\"payload\":\"AQID\",\"meta\":{\"format\":\"example/1\"}}],\"options\":{\"seed\":\"42\"}}",
            json);
    }

    /// <summary>An unknown artifact kind or a payload combined with a path fails validation.</summary>
    [Fact]
    public void ArtifactRejectsUnknownKindAndBothPayloadPath()
    {
        Assert.Throws<ArgumentException>(() => new AudioArtifactInput { Id = "a", Kind = "tokens", Path = "/x" }.Validate());
        Assert.Throws<ArgumentException>(() => new AudioArtifactInput { Id = "a", Kind = ArtifactKind.Custom, Payload = [1], Path = "/x" }.Validate());
    }

    /// <summary>Recipe name, stream flags, model map, and inner request serialize in order.</summary>
    [Fact]
    public void RecipeRequestSerializesRolesAndStreamFlags()
    {
        RecipeRunRequest request = new()
        {
            Recipe = "speaking-avatar",
            Stream = true,
            IncludeArtifacts = true,
        };
        request.Models["tts"] = "chat-voice";
        request.Models["avatar"] = "chat-avatar";
        request.Request.Text = "Hello!";
        request.Request.Options = new Dictionary<string, string> { ["avatar.reference_image_file"] = "/p.png" };
        string json = Json.Serialize(request);
        Assert.Equal(
            "{\"recipe\":\"speaking-avatar\",\"stream\":true,\"include_artifacts\":true,\"models\":{\"tts\":\"chat-voice\",\"avatar\":\"chat-avatar\"},\"request\":{\"text\":\"Hello!\",\"options\":{\"avatar.reference_image_file\":\"/p.png\"}}}",
            json);
    }

    /// <summary>Exactly one of path or package id is required for a model load.</summary>
    [Fact]
    public void ModelLoadValidatesPathXorPackage()
    {
        Assert.Throws<ArgumentException>(() => new ModelLoadRequest { Id = "x", Path = "/a", PackageId = "p" }.Validate());
        Assert.Throws<ArgumentException>(() => new ModelLoadRequest { Id = "x" }.Validate());
        new ModelLoadRequest { Id = "x", Path = "/a" }.Validate();
        new ModelLoadRequest { Id = "x", PackageId = "pkg" }.Validate();
    }

    /// <summary>Package-id loads serialize in the client field order: id, path, package_id, family, task.</summary>
    [Fact]
    public void ModelLoadRequestSerializesPackageIdGolden()
    {
        ModelLoadRequest request = new() { Id = "sep", PackageId = "htdemucs_q8_0", Task = "sep" };
        string json = Json.Serialize(request);
        Assert.Equal(
            "{\"id\":\"sep\",\"package_id\":\"htdemucs_q8_0\",\"task\":\"sep\"}",
            json);
    }

    /// <summary>Exactly one audio form is required; supplying both fails validation.</summary>
    [Fact]
    public void TranscriptionInputValidatesOneOfForms()
    {
        Assert.Throws<ArgumentException>(() => new TranscriptionRequest { Model = "m" }.Validate());
        Assert.Throws<ArgumentException>(() => new TranscriptionRequest { Model = "m", AudioPath = "/a.wav", AudioFile = new MemoryStream(), AudioFileName = "a.wav" }.Validate());
        new TranscriptionRequest { Model = "m", AudioPath = "/a.wav" }.Validate();
    }
}