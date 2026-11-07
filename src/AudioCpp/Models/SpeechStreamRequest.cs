using System;
using System.Text.Json.Serialization;

namespace Mabs.AudioCpp.Models;

/// <summary>Body of a streaming speech request (<c>stream_format</c> always present).</summary>
[JsonConverter(typeof(SpeechStreamRequestConverter))]
public sealed record SpeechStreamRequest
{
    /// <summary>The underlying request; <see cref="SpeechRequest.ResponseFormat"/> should be <c>pcm</c> — streaming speech only supports PCM output.</summary>
    [JsonPropertyName("request")]
    public SpeechRequest Request { get; set; } = new SpeechRequest();

    /// <summary><c>sse</c> for event framing; <c>audio</c> for raw chunked PCM.</summary>
    [JsonPropertyName("stream_format")]
    public StreamFormat StreamFormat { get; set; } = StreamFormat.Sse;

    /// <summary>
    /// Ask the server to also stream the text a speech-to-speech model writes while
    /// speaking. Requires <c>stream_format=sse</c>. On the non-live
    /// <c>/v1/audio/speech</c> route the field is accepted only by the live handler;
    /// set it on <see cref="LiveSpeechRequest"/> for that behavior.
    /// </summary>
    [JsonPropertyName("return_text")]
    public bool? ReturnText { get; set; }

    /// <summary>Validates the inner request and the return-text rule.</summary>
    /// <exception cref="ArgumentException">
    /// <see cref="ReturnText"/> is set with a non-<c>sse</c> stream format.
    /// </exception>
    public void Validate()
    {
        Request.Validate();
        if (ReturnText == true && StreamFormat != StreamFormat.Sse)
        {
            throw new ArgumentException("return_text needs stream_format=sse");
        }
    }
}