using System;
using System.Collections.Generic;
using System.Text.Json;

using mabs.AudioCpp.Models;

using Xunit;

namespace mabs.AudioCpp.Tests;

/// <summary>Golden serialization of SpeechRequest bodies.</summary>
public class SpeechRequestSerializationTests
{
    private static string Serialize(SpeechRequest request)
    {
        using System.IO.MemoryStream stream = new();
        using (Utf8JsonWriter writer = new(stream))
        {
            request.WriteTo(writer);
        }

        return System.Text.Encoding.UTF8.GetString(stream.ToArray());
    }

    [Fact]
    public void MinimalBody()
    {
        string json = Serialize(new SpeechRequest { Model = "kokoro", Input = "hello" });
        Assert.Equal("{\"model\":\"kokoro\",\"input\":\"hello\"}", json);
    }

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

    [Fact]
    public void InstructPreferredThenInstructions()
    {
        string json = Serialize(new SpeechRequest { Model = "m", Input = "t", Instruct = "calm" });
        Assert.Contains("\"instruct\":\"calm\"", json);
        Assert.DoesNotContain("instructions", json);

        json = Serialize(new SpeechRequest { Model = "m", Input = "t", Instructions = "speak" });
        Assert.Contains("\"instructions\":\"speak\"", json);
    }

    [Fact]
    public void SpeedAndSpeakingRateBothSetThrows()
    {
        SpeechRequest request = new() { Model = "m", Input = "t", Speed = 1.1, SpeakingRate = 1.2 };
        Assert.Throws<ArgumentException>(() => request.Validate());
    }

    [Fact]
    public void NonPositiveSpeedThrows()
    {
        Assert.Throws<ArgumentException>(() => new SpeechRequest { Model = "m", Input = "t", Speed = 0 }.Validate());
    }

    [Fact]
    public void StringSeedPreserved()
    {
        string json = Serialize(new SpeechRequest { Model = "m", Input = "t", Seed = "18446744073709551615" });
        Assert.Contains("\"seed\":\"18446744073709551615\"", json);
    }

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

    [Fact]
    public void MissingModelOrInputThrows()
    {
        Assert.Throws<ArgumentException>(() => new SpeechRequest { Input = "t" }.Validate());
        Assert.Throws<ArgumentException>(() => new SpeechRequest { Model = "m" }.Validate());

        // Empty input string is allowed (the server validates content; null does not serialize).
        new SpeechRequest { Model = "m", Input = string.Empty }.Validate();
    }

    [Fact]
    public void StreamRequestAddsStreamFormatAndReturnText()
    {
        using System.IO.MemoryStream stream = new();
        using (Utf8JsonWriter writer = new(stream))
        {
            new SpeechStreamRequest
            {
                Request = new SpeechRequest { Model = "m", Input = "t", ResponseFormat = ResponseFormat.Pcm },
                StreamFormat = StreamFormat.Sse,
                ReturnText = true,
            }.WriteTo(writer);
        }

        string json = System.Text.Encoding.UTF8.GetString(stream.ToArray());
        Assert.Equal("{\"model\":\"m\",\"input\":\"t\",\"response_format\":\"pcm\",\"stream_format\":\"sse\",\"return_text\":true}", json);
    }

    [Fact]
    public void ReturnTextWithoutSseThrows()
    {
        SpeechStreamRequest request = new()
        {
            Request = new SpeechRequest { Model = "m", Input = "t" },
            StreamFormat = StreamFormat.Audio,
            ReturnText = true,
        };
        using System.IO.MemoryStream stream = new();
        using Utf8JsonWriter writer = new(stream);
        Assert.Throws<ArgumentException>(() => request.WriteTo(writer));
    }

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

    [Fact]
    public void LiveSpeechRejectsOutOfRange()
    {
        Assert.Throws<ArgumentException>(() => new LiveSpeechRequest { Model = "m", SampleRate = 100 }.BuildQueryString());
        Assert.Throws<ArgumentException>(() => new LiveSpeechRequest { Model = "m", Channels = 17 }.BuildQueryString());
        Assert.Throws<ArgumentException>(() => new LiveSpeechRequest { Input = "x" }.BuildQueryString());
    }

    [Fact]
    public void TaskRequestRejectsBothAudioForms()
    {
        TaskRequestBody body = new()
        {
            AudioPath = "/tmp/a.wav",
            AudioBase64 = new byte[] { 1 },
        };
        Assert.Throws<ArgumentException>(() => body.Validate());
    }

    [Fact]
    public void TaskRequestSerializesArtifacts()
    {
        using System.IO.MemoryStream stream = new();
        using (Utf8JsonWriter writer = new(stream))
        {
            new TaskRequestBody
            {
                AudioPath = "/in.wav",
                Artifacts = new List<AudioArtifactInput>
                {
                    new() {
                        Id = "example.state",
                        Kind = ArtifactKind.Custom,
                        Payload = new byte[] { 1, 2, 3 },
                        Meta = new Dictionary<string, string> { ["format"] = "example/1" },
                    },
                },
                Options = new Dictionary<string, string> { ["seed"] = "42" },
            }.WriteTo(writer);
        }

        string json = System.Text.Encoding.UTF8.GetString(stream.ToArray());
        Assert.Equal(
            "{\"audio\":\"/in.wav\",\"artifacts\":[{\"id\":\"example.state\",\"kind\":\"custom\",\"payload\":\"AQID\",\"meta\":{\"format\":\"example/1\"}}],\"options\":{\"seed\":\"42\"}}",
            json);
    }

    [Fact]
    public void ArtifactRejectsUnknownKindAndBothPayloadPath()
    {
        Assert.Throws<ArgumentException>(() => new AudioArtifactInput { Id = "a", Kind = "tokens", Path = "/x" }.WriteTo(NullWriter()));
        Assert.Throws<ArgumentException>(() => new AudioArtifactInput { Id = "a", Kind = ArtifactKind.Custom, Payload = new byte[] { 1 }, Path = "/x" }.WriteTo(NullWriter()));
    }

    [Fact]
    public void RecipeRequestSerializesRolesAndStreamFlags()
    {
        using System.IO.MemoryStream stream = new();
        using (Utf8JsonWriter writer = new(stream))
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
            request.WriteTo(writer);
        }

        string json = System.Text.Encoding.UTF8.GetString(stream.ToArray());
        Assert.Equal(
            "{\"recipe\":\"speaking-avatar\",\"stream\":true,\"include_artifacts\":true,\"models\":{\"tts\":\"chat-voice\",\"avatar\":\"chat-avatar\"},\"request\":{\"text\":\"Hello!\",\"options\":{\"avatar.reference_image_file\":\"/p.png\"}}}",
            json);
    }

    [Fact]
    public void ModelLoadValidatesPathXorPackage()
    {
        Assert.Throws<ArgumentException>(() => new ModelLoadRequest { Id = "x", Path = "/a", PackageId = "p" }.Validate());
        Assert.Throws<ArgumentException>(() => new ModelLoadRequest { Id = "x" }.Validate());
        new ModelLoadRequest { Id = "x", Path = "/a" }.Validate();
        new ModelLoadRequest { Id = "x", PackageId = "pkg" }.Validate();
    }

    [Fact]
    public void TranscriptionInputValidatesOneOfForms()
    {
        Assert.Throws<ArgumentException>(() => new TranscriptionRequest { Model = "m" }.Validate());
        Assert.Throws<ArgumentException>(() => new TranscriptionRequest { Model = "m", AudioPath = "/a.wav", AudioFile = new System.IO.MemoryStream(), AudioFileName = "a.wav" }.Validate());
        new TranscriptionRequest { Model = "m", AudioPath = "/a.wav" }.Validate();
    }

    private static Utf8JsonWriter NullWriter()
    {
        return new Utf8JsonWriter(new System.IO.MemoryStream());
    }
}