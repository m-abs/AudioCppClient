using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

using Mabs.AudioCpp.Utils;

namespace Mabs.AudioCpp.Models;

/// <summary>Body of <c>POST /v1/audio/speech</c> (non-streaming) and the base of stream requests.</summary>
public sealed record SpeechRequest
{
    /// <summary>Configured model id. Required.</summary>
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>Text to speak. Required.</summary>
    [JsonPropertyName("input")]
    public string? Input { get; set; }

    /// <summary>Language code.</summary>
    [JsonPropertyName("language")]
    public string? Language { get; set; }

    /// <summary>Voice preset name, voice-dir name, or native voice id. Empty/null is omitted.</summary>
    [JsonPropertyName("voice")]
    public string? Voice { get; set; }

    /// <summary>Voice reference: a server-local path string or a <see cref="VoiceRefObject"/>.</summary>
    [JsonPropertyName("voice_ref")]
    public object? VoiceRef { get; set; }

    /// <summary>Transcript of the voice reference audio.</summary>
    [JsonPropertyName("reference_text")]
    public string? ReferenceText { get; set; }

    /// <summary>Model option <c>instruct</c>. Mutually exclusive with <see cref="Instructions"/> (server-side 400 mirrored client-side).</summary>
    [JsonPropertyName("instruct")]
    public string? Instruct { get; set; }

    /// <summary>OpenAI-style top-level instructions; the server maps them to model option <c>instruction</c>. Mutually exclusive with <see cref="Instruct"/>.</summary>
    [JsonPropertyName("instructions")]
    public string? Instructions { get; set; }

    /// <summary>Positive speech-rate multiplier. Mutually exclusive with <see cref="SpeakingRate"/>.</summary>
    [JsonPropertyName("speed")]
    public double? Speed { get; set; }

    /// <summary>Alias of <see cref="Speed"/>. Mutually exclusive with it on this client.</summary>
    [JsonPropertyName("speaking_rate")]
    public double? SpeakingRate { get; set; }

    /// <summary>
    /// Sampling seed. Sent as a JSON string when set: the documented full-uint64-safe
    /// spelling (JSON numeric seeds may lose precision above 2^53).
    /// </summary>
    [JsonPropertyName("seed")]
    public string? Seed { get; set; }

    /// <summary>Sampling temperature.</summary>
    [JsonPropertyName("temperature")]
    public double? Temperature { get; set; }

    /// <summary>Top-k sampling.</summary>
    [JsonPropertyName("top_k")]
    public int? TopK { get; set; }

    /// <summary>Top-p (nucleus) sampling.</summary>
    [JsonPropertyName("top_p")]
    public double? TopP { get; set; }

    /// <summary>Maximum generated tokens.</summary>
    [JsonPropertyName("max_tokens")]
    public int? MaxTokens { get; set; }

    /// <summary>Maximum inference steps (diffusion families).</summary>
    [JsonPropertyName("max_steps")]
    public int? MaxSteps { get; set; }

    /// <summary>Repetition penalty.</summary>
    [JsonPropertyName("repetition_penalty")]
    public double? RepetitionPenalty { get; set; }

    /// <summary>Classifier-free guidance scale.</summary>
    [JsonPropertyName("guidance_scale")]
    public double? GuidanceScale { get; set; }

    /// <summary>Number of inference steps (diffusion families).</summary>
    [JsonPropertyName("num_inference_steps")]
    public int? NumInferenceSteps { get; set; }

    /// <summary><c>wav</c> (default), <c>json</c>, <c>pcm</c>, <c>mp3</c> where the frontend is compiled.</summary>
    [JsonPropertyName("response_format")]
    public ResponseFormat? ResponseFormat { get; set; }

    /// <summary>Upper bound on waiting for a busy model, in milliseconds.</summary>
    [JsonPropertyName("busy_timeout_ms")]
    public long? BusyTimeoutMs { get; set; }

    /// <summary>Model-family-specific options: the escape hatch mirroring the server's stringly-typed option map.</summary>
    [JsonPropertyName("options")]
    public Dictionary<string, string>? Options { get; set; }

    /// <summary>Omits the options map when it is empty, as the wire format expects.</summary>
    /// <returns>Whether the options map has entries.</returns>
    public bool ShouldSerializeOptions()
    {
        return Options != null && Options.Count > 0;
    }

    /// <summary>Validates cross-field rules (required fields, instruct XOR instructions, speed XOR speaking_rate, option collisions).</summary>
    public void Validate()
    {
        if (string.IsNullOrEmpty(Model))
        {
            throw new ArgumentException("speech requires a model id", nameof(Model));
        }

        if (Input == null)
        {
            throw new ArgumentException("speech requires input text", nameof(Input));
        }

        if (Instruct != null && Instructions != null)
        {
            throw new ArgumentException("instruct and instructions cannot both be set");
        }

        if (Speed.HasValue && SpeakingRate.HasValue)
        {
            throw new ArgumentException("speed and speaking_rate cannot both be set");
        }

        if (Speed is { } speed && (speed <= 0 || double.IsNaN(speed) || double.IsInfinity(speed)))
        {
            throw new ArgumentException("speed must be a positive finite number");
        }

        if (SpeakingRate is { } rate && (rate <= 0 || double.IsNaN(rate) || double.IsInfinity(rate)))
        {
            throw new ArgumentException("speaking_rate must be a positive finite number");
        }

        if (VoiceRef != null && VoiceRef is not string && VoiceRef is not VoiceRefObject)
        {
            throw new ArgumentException("VoiceRef must be a string path or a VoiceRefObject", nameof(VoiceRef));
        }

        if (Options != null)
        {
            if (Instruct != null)
            {
                RequestJson.ThrowOnOptionCollision(Options, "instruct");
            }

            if (Instructions != null)
            {
                RequestJson.ThrowOnOptionCollision(Options, "instruction");
            }

            if (Speed.HasValue)
            {
                RequestJson.ThrowOnOptionCollision(Options, "speed");
            }

            if (SpeakingRate.HasValue)
            {
                RequestJson.ThrowOnOptionCollision(Options, "speaking_rate");
            }

            if (ReferenceText != null)
            {
                RequestJson.ThrowOnOptionCollision(Options, "reference_text");
            }

            if (Seed != null)
            {
                RequestJson.ThrowOnOptionCollision(Options, "seed");
            }

            if (Temperature.HasValue)
            {
                RequestJson.ThrowOnOptionCollision(Options, "temperature");
            }

            if (TopK.HasValue)
            {
                RequestJson.ThrowOnOptionCollision(Options, "top_k");
            }

            if (TopP.HasValue)
            {
                RequestJson.ThrowOnOptionCollision(Options, "top_p");
            }

            if (MaxTokens.HasValue)
            {
                RequestJson.ThrowOnOptionCollision(Options, "max_tokens");
            }

            if (MaxSteps.HasValue)
            {
                RequestJson.ThrowOnOptionCollision(Options, "max_steps");
            }

            if (RepetitionPenalty.HasValue)
            {
                RequestJson.ThrowOnOptionCollision(Options, "repetition_penalty");
            }

            if (GuidanceScale.HasValue)
            {
                RequestJson.ThrowOnOptionCollision(Options, "guidance_scale");
            }

            if (NumInferenceSteps.HasValue)
            {
                RequestJson.ThrowOnOptionCollision(Options, "num_inference_steps");
            }
        }
    }
}