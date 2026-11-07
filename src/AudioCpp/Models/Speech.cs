using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Mabs.AudioCpp.Models;

/// <summary>
/// A <c>response_format</c> value. The wire type is a string; use the static instances or
/// construct from any string the server accepts (an unknown value is rejected by the server).
/// </summary>
public readonly struct ResponseFormat : IEquatable<ResponseFormat>
{
    /// <summary>WAV bytes (default).</summary>
    public static readonly ResponseFormat Wav = new("wav");

    /// <summary>JSON object carrying Base64 WAV.</summary>
    public static readonly ResponseFormat Json = new("json");

    /// <summary>Raw PCM16 bytes.</summary>
    public static readonly ResponseFormat Pcm = new("pcm");

    /// <summary>MP3 bytes (only builds with the <c>mp3_encode</c> frontend module).</summary>
    public static readonly ResponseFormat Mp3 = new("mp3");

    /// <summary>Creates a format from its wire string.</summary>
    /// <param name="value">The wire string, e.g. <c>wav</c>.</param>
    public ResponseFormat(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>The wire string.</summary>
    public string Value { get; }

    /// <summary>Implicit conversion from string.</summary>
    /// <param name="value">The wire string.</param>
    /// <returns>The format.</returns>
    public static implicit operator ResponseFormat(string value) => new(value);

    /// <summary>Implicit conversion to string.</summary>
    /// <param name="format">The format.</param>
    /// <returns>The wire string.</returns>
    public static implicit operator string(ResponseFormat format) => format.Value;

    /// <inheritdoc/>
    public bool Equals(ResponseFormat other) => string.Equals(Value, other.Value, StringComparison.Ordinal);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ResponseFormat other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => Value == null ? 0 : Value.GetHashCode();

    /// <inheritdoc/>
    public override string ToString() => Value;
}

/// <summary>A <c>stream_format</c> value: <c>sse</c> events or raw chunked audio.</summary>
public readonly struct StreamFormat : IEquatable<StreamFormat>
{
    /// <summary>Server-sent events.</summary>
    public static readonly StreamFormat Sse = new("sse");

    /// <summary>Chunked raw audio bytes.</summary>
    public static readonly StreamFormat Audio = new("audio");

    /// <summary>Creates a format from its wire string.</summary>
    /// <param name="value">The wire string, e.g. <c>sse</c>.</param>
    public StreamFormat(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>The wire string.</summary>
    public string Value { get; }

    /// <summary>Implicit conversion from string.</summary>
    /// <param name="value">The wire string.</param>
    /// <returns>The format.</returns>
    public static implicit operator StreamFormat(string value) => new(value);

    /// <summary>Implicit conversion to string.</summary>
    /// <param name="format">The format.</param>
    /// <returns>The wire string.</returns>
    public static implicit operator string(StreamFormat format) => format.Value;

    /// <inheritdoc/>
    public bool Equals(StreamFormat other) => string.Equals(Value, other.Value, StringComparison.Ordinal);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is StreamFormat other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => Value == null ? 0 : Value.GetHashCode();

    /// <inheritdoc/>
    public override string ToString() => Value;
}

/// <summary>
/// A <c>voice_ref</c> object: <c>{"type":"path","path":…}</c> or
/// <c>{"type":"base64","data":…}</c> (decoded payload limited to 5 MiB by the server).
/// Plain server-local path strings are also accepted by the wire; see
/// <see cref="SpeechRequest.VoiceRef"/>.
/// </summary>
public sealed class VoiceRefObject
{
    /// <summary>Creates a server-local path reference.</summary>
    /// <param name="path">Server-local WAV path.</param>
    /// <returns>The reference.</returns>
    public static VoiceRefObject FromPath(string path) =>
        new()
        { Type = "path", Path = path ?? throw new ArgumentNullException(nameof(path)) };

    /// <summary>Creates an inline Base64 WAV reference (bare Base64 or a data-URI; a data-URI is also accepted by the server).</summary>
    /// <param name="data">Base64 text.</param>
    /// <returns>The reference.</returns>
    public static VoiceRefObject FromBase64(string data) =>
        new()
        { Type = "base64", Data = data ?? throw new ArgumentNullException(nameof(data)) };

    /// <summary>Creates an inline reference from WAV bytes.</summary>
    /// <param name="wavBytes">WAV bytes to Base64-encode.</param>
    /// <returns>The reference.</returns>
    public static VoiceRefObject FromBytes(byte[] wavBytes) =>
        new()
        { Type = "base64", Data = Convert.ToBase64String(wavBytes ?? throw new ArgumentNullException(nameof(wavBytes))) };

    /// <summary><c>path</c> or <c>base64</c>.</summary>
    public string Type { get; set; } = "path";

    /// <summary>Server-local path; set when <see cref="Type"/> is <c>path</c>.</summary>
    public string? Path { get; set; }

    /// <summary>Base64 payload; set when <see cref="Type"/> is <c>base64</c>.</summary>
    public string? Data { get; set; }

    /// <summary>Writes the reference object.</summary>
    /// <param name="writer">The JSON writer.</param>
    internal void WriteTo(Utf8JsonWriter writer)
    {
        writer.WriteStartObject();
        writer.WriteString("type", Type);
        if (Type == "path")
        {
            writer.WriteString("path", Path);
        }
        else
        {
            writer.WriteString("data", Data);
        }

        writer.WriteEndObject();
    }
}

/// <summary>Shared request-serialization helpers.</summary>
internal static class RequestJson
{
    /// <summary>Writes a string field only when it is set.</summary>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="name">The field name.</param>
    /// <param name="value">The value; nothing is written when null or empty.</param>
    internal static void WriteStringIfSet(Utf8JsonWriter writer, string name, string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            writer.WriteString(name, value);
        }
    }

    /// <summary>Writes a numeric field only when it has a value.</summary>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="name">The field name.</param>
    /// <param name="value">The value; nothing is written when null.</param>
    internal static void WriteNumberIfSet(Utf8JsonWriter writer, string name, double? value)
    {
        if (value.HasValue)
        {
            writer.WriteNumber(name, value.Value);
        }
    }

    /// <summary>Writes a numeric field only when it has a value.</summary>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="name">The field name.</param>
    /// <param name="value">The value; nothing is written when null.</param>
    internal static void WriteNumberIfSet(Utf8JsonWriter writer, string name, int? value)
    {
        if (value.HasValue)
        {
            writer.WriteNumber(name, value.Value);
        }
    }

    /// <summary>Writes a numeric field only when it has a value.</summary>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="name">The field name.</param>
    /// <param name="value">The value; nothing is written when null.</param>
    internal static void WriteNumberIfSet(Utf8JsonWriter writer, string name, long? value)
    {
        if (value.HasValue)
        {
            writer.WriteNumber(name, value.Value);
        }
    }

    /// <summary>Writes the options map; omitted when null or empty.</summary>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="options">The options map.</param>
    internal static void WriteOptions(Utf8JsonWriter writer, IDictionary<string, string>? options)
    {
        if (options == null || options.Count == 0)
        {
            return;
        }

        writer.WriteStartObject("options");
        foreach (KeyValuePair<string, string> pair in options)
        {
            writer.WriteString(pair.Key, pair.Value);
        }

        writer.WriteEndObject();
    }

    /// <summary>
    /// Throws when an options-map key collides with a typed field that is set. The server
    /// treats aliases as non-merging, so a silent overwrite would be a footgun; the caller
    /// must set exactly one of the two spellings.
    /// </summary>
    /// <param name="options">The options map.</param>
    /// <param name="key">The key to check.</param>
    /// <exception cref="ArgumentException">The key is present in the options map.</exception>
    internal static void ThrowOnOptionCollision(IDictionary<string, string>? options, string key)
    {
        if (options != null && options.ContainsKey(key))
        {
            throw new ArgumentException(
                "options key '" + key + "' collides with the typed field of the same name; set only one of them");
        }
    }
}

/// <summary>Body of <c>POST /v1/audio/speech</c> (non-streaming) and the base of stream requests.</summary>
public sealed class SpeechRequest
{
    /// <summary>Configured model id. Required.</summary>
    public string? Model { get; set; }

    /// <summary>Text to speak. Required.</summary>
    public string? Input { get; set; }

    /// <summary>Language code.</summary>
    public string? Language { get; set; }

    /// <summary>Voice preset name, voice-dir name, or native voice id. Empty/null is omitted.</summary>
    public string? Voice { get; set; }

    /// <summary>Voice reference: a server-local path string or a <see cref="VoiceRefObject"/>.</summary>
    public object? VoiceRef { get; set; }

    /// <summary>Transcript of the voice reference audio.</summary>
    public string? ReferenceText { get; set; }

    /// <summary>Model option <c>instruct</c>. Mutually exclusive with <see cref="Instructions"/> (server-side 400 mirrored client-side).</summary>
    public string? Instruct { get; set; }

    /// <summary>OpenAI-style top-level instructions; the server maps them to model option <c>instruction</c>. Mutually exclusive with <see cref="Instruct"/>.</summary>
    public string? Instructions { get; set; }

    /// <summary>Positive speech-rate multiplier. Mutually exclusive with <see cref="SpeakingRate"/>.</summary>
    public double? Speed { get; set; }

    /// <summary>Alias of <see cref="Speed"/>. Mutually exclusive with it on this client.</summary>
    public double? SpeakingRate { get; set; }

    /// <summary>
    /// Sampling seed. Sent as a JSON string when set: the documented full-uint64-safe
    /// spelling (JSON numeric seeds may lose precision above 2^53).
    /// </summary>
    public string? Seed { get; set; }

    /// <summary>Sampling temperature.</summary>
    public double? Temperature { get; set; }

    /// <summary>Top-k sampling.</summary>
    public int? TopK { get; set; }

    /// <summary>Top-p (nucleus) sampling.</summary>
    public double? TopP { get; set; }

    /// <summary>Maximum generated tokens.</summary>
    public int? MaxTokens { get; set; }

    /// <summary>Maximum inference steps (diffusion families).</summary>
    public int? MaxSteps { get; set; }

    /// <summary>Repetition penalty.</summary>
    public double? RepetitionPenalty { get; set; }

    /// <summary>Classifier-free guidance scale.</summary>
    public double? GuidanceScale { get; set; }

    /// <summary>Number of inference steps (diffusion families).</summary>
    public int? NumInferenceSteps { get; set; }

    /// <summary><c>wav</c> (default), <c>json</c>, <c>pcm</c>, <c>mp3</c> where the frontend is compiled.</summary>
    public ResponseFormat? ResponseFormat { get; set; }

    /// <summary>Upper bound on waiting for a busy model, in milliseconds.</summary>
    public long? BusyTimeoutMs { get; set; }

    /// <summary>Model-family-specific options: the escape hatch mirroring the server's stringly-typed option map.</summary>
    public Dictionary<string, string>? Options { get; set; }

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

    /// <summary>Serializes the full request object (validates first).</summary>
    /// <param name="writer">The JSON writer.</param>
    internal void WriteTo(Utf8JsonWriter writer)
    {
        Validate();
        writer.WriteStartObject();
        WriteFields(writer);
        writer.WriteEndObject();
    }

    /// <summary>Writes the request fields without surrounding braces (for stream/live bodies that add fields).</summary>
    /// <param name="writer">The JSON writer.</param>
    internal void WriteFields(Utf8JsonWriter writer)
    {
        writer.WriteString("model", Model);
        writer.WriteString("input", Input);
        RequestJson.WriteStringIfSet(writer, "language", Language);
        RequestJson.WriteStringIfSet(writer, "voice", Voice);
        WriteVoiceRef(writer);
        RequestJson.WriteStringIfSet(writer, "reference_text", ReferenceText);
        if (Instruct != null)
        {
            writer.WriteString("instruct", Instruct);
        }
        else if (Instructions != null)
        {
            writer.WriteString("instructions", Instructions);
        }

        if (Speed.HasValue)
        {
            writer.WriteNumber("speed", Speed.Value);
        }
        else if (SpeakingRate.HasValue)
        {
            writer.WriteNumber("speaking_rate", SpeakingRate.Value);
        }

        if (Seed != null)
        {
            writer.WriteString("seed", Seed);
        }

        RequestJson.WriteNumberIfSet(writer, "temperature", Temperature);
        RequestJson.WriteNumberIfSet(writer, "top_k", TopK);
        RequestJson.WriteNumberIfSet(writer, "top_p", TopP);
        RequestJson.WriteNumberIfSet(writer, "max_tokens", MaxTokens);
        RequestJson.WriteNumberIfSet(writer, "max_steps", MaxSteps);
        RequestJson.WriteNumberIfSet(writer, "repetition_penalty", RepetitionPenalty);
        RequestJson.WriteNumberIfSet(writer, "guidance_scale", GuidanceScale);
        RequestJson.WriteNumberIfSet(writer, "num_inference_steps", NumInferenceSteps);
        if (ResponseFormat.HasValue)
        {
            writer.WriteString("response_format", ResponseFormat.Value.Value);
        }

        RequestJson.WriteNumberIfSet(writer, "busy_timeout_ms", BusyTimeoutMs);
        RequestJson.WriteOptions(writer, Options);
    }

    /// <summary>Writes the voice-ref field: a string path or an object, depending on <see cref="VoiceRef"/>.</summary>
    /// <param name="writer">The JSON writer.</param>
    /// <exception cref="ArgumentException">
    /// <see cref="VoiceRef"/> is neither a string nor a <see cref="VoiceRefObject"/>.
    /// </exception>
    internal void WriteVoiceRef(Utf8JsonWriter writer)
    {
        if (VoiceRef == null)
        {
            return;
        }

        switch (VoiceRef)
        {
            case string path:
                writer.WriteString("voice_ref", path);
                break;
            case VoiceRefObject obj:
                writer.WritePropertyName("voice_ref");
                obj.WriteTo(writer);
                break;
            default:
                throw new ArgumentException("VoiceRef must be a string path or a VoiceRefObject", nameof(VoiceRef));
        }
    }
}

/// <summary>Body of a streaming speech request (<c>stream_format</c> always present).</summary>
public sealed class SpeechStreamRequest
{
    /// <summary>The underlying request; <see cref="SpeechRequest.ResponseFormat"/> should be <c>pcm</c> — streaming speech only supports PCM output.</summary>
    public SpeechRequest Request { get; set; } = new SpeechRequest();

    /// <summary><c>sse</c> for event framing; <c>audio</c> for raw chunked PCM.</summary>
    public StreamFormat StreamFormat { get; set; } = StreamFormat.Sse;

    /// <summary>
    /// Ask the server to also stream the text a speech-to-speech model writes while
    /// speaking. Requires <c>stream_format=sse</c>. On the non-live
    /// <c>/v1/audio/speech</c> route the field is accepted only by the live handler;
    /// set it on <see cref="LiveSpeechRequest"/> for that behavior.
    /// </summary>
    public bool? ReturnText { get; set; }

    /// <summary>Writes the stream request object (validates the inner request first).</summary>
    /// <param name="writer">The JSON writer.</param>
    /// <exception cref="ArgumentException">
    /// <see cref="ReturnText"/> is set with a non-<c>sse</c> stream format.
    /// </exception>
    internal void WriteTo(Utf8JsonWriter writer)
    {
        Request.Validate();
        if (ReturnText == true && StreamFormat != StreamFormat.Sse)
        {
            throw new ArgumentException("return_text needs stream_format=sse");
        }

        writer.WriteStartObject();
        Request.WriteFields(writer);
        writer.WriteString("stream_format", StreamFormat.Value);
        if (ReturnText.HasValue)
        {
            writer.WriteBoolean("return_text", ReturnText.Value);
        }

        writer.WriteEndObject();
    }
}

/// <summary>
/// Parameters of <c>POST /v1/audio/speech/live</c>: chunked PCM body in, SSE or raw audio
/// out. All request fields travel as query parameters because the body carries audio.
/// </summary>
public sealed class LiveSpeechRequest
{
    /// <summary>Configured streaming model id. Required.</summary>
    public string? Model { get; set; }

    /// <summary>
    /// Text for a text-to-speech model (required for it); speech-to-speech, <c>vc</c> and
    /// <c>svc</c> models treat it as an optional prompt.
    /// </summary>
    public string? Input { get; set; }

    /// <summary>Language code.</summary>
    public string? Language { get; set; }

    /// <summary>Voice preset / native voice id.</summary>
    public string? Voice { get; set; }

    /// <summary>Sample rate of the PCM body; 1000–384000. Server default 16000.</summary>
    public int? SampleRate { get; set; } = 16000;

    /// <summary>Interleaved channel count of the PCM body; 1–16. Server default 1.</summary>
    public int? Channels { get; set; } = 1;

    /// <summary><c>s16le</c> (default) or <c>f32le</c>.</summary>
    public string? SampleFormat { get; set; }

    /// <summary><c>sse</c> (default) or <c>audio</c>. The route only supports <c>response_format=pcm</c>.</summary>
    public StreamFormat StreamFormat { get; set; } = StreamFormat.Sse;

    /// <summary>Stream the text the model writes as it speaks; requires <see cref="StreamFormat"/> = <c>sse</c>.</summary>
    public bool? ReturnText { get; set; }

    /// <summary>Upper bound on waiting for a busy model, in milliseconds.</summary>
    public long? BusyTimeoutMs { get; set; }

    /// <summary>Sampling seed as decimal text (full uint64 range).</summary>
    public string? Seed { get; set; }

    /// <summary>
    /// Extra query-passed options the live handler recognizes: <c>temperature</c>,
    /// <c>top_k</c>, <c>top_p</c>, <c>max_tokens</c>, <c>max_steps</c>,
    /// <c>repetition_penalty</c>, <c>guidance_scale</c>, <c>num_inference_steps</c>,
    /// <c>system_prompt</c>, <c>instructions</c> (maps to <c>system_prompt</c>),
    /// <c>voice_id</c>, <c>reference_text</c>, <c>text_temperature</c>, <c>text_top_k</c>,
    /// <c>do_sample</c>.
    /// </summary>
    public Dictionary<string, string>? Options { get; set; }

    /// <summary>Validates the request and builds the query string.</summary>
    /// <returns>The encoded query string with a leading <c>?</c>.</returns>
    public string BuildQueryString()
    {
        if (string.IsNullOrEmpty(Model))
        {
            throw new ArgumentException("live speech requires a model id", nameof(Model));
        }

        if (SampleRate is < 1000 or > 384000)
        {
            throw new ArgumentException("sample_rate must be between 1000 and 384000");
        }

        if (Channels is < 1 or > 16)
        {
            throw new ArgumentException("channels must be between 1 and 16");
        }

        if (ReturnText == true && StreamFormat != StreamFormat.Sse)
        {
            throw new ArgumentException("return_text needs stream_format=sse");
        }

        QueryBuilder query = new();
        query.Add("model", Model);
        query.Add("input", Input);
        query.Add("language", Language);
        query.Add("voice", Voice);
        query.Add("sample_rate", SampleRate?.ToString(CultureInfo.InvariantCulture));
        query.Add("channels", Channels?.ToString(CultureInfo.InvariantCulture));
        query.Add("sample_format", SampleFormat);
        query.Add("stream_format", StreamFormat.Value);
        if (ReturnText.HasValue)
        {
            query.Add("return_text", ReturnText.Value ? "true" : "false");
        }

        query.Add("busy_timeout_ms", BusyTimeoutMs?.ToString(CultureInfo.InvariantCulture));
        query.Add("seed", Seed);
        if (Options != null)
        {
            foreach (KeyValuePair<string, string> pair in Options)
            {
                query.Add(pair.Key, pair.Value);
            }
        }

        return query.ToString();
    }
}

/// <summary>Non-streaming speech result: audio bytes, content type, and timing headers.</summary>
public sealed class SpeechResult
{
    /// <summary>Audio bytes (WAV by default).</summary>
    public byte[] Audio { get; internal set; } = Array.Empty<byte>();

    /// <summary>Response content type, for example <c>audio/wav</c>.</summary>
    public string? ContentType { get; internal set; }

    /// <summary>Timing reported by the <c>X-AudioCPP-Wall-Ms</c>, <c>X-AudioCPP-Audio-Duration-Ms</c> and <c>X-AudioCPP-RTF</c> headers.</summary>
    public AudioCppTiming? Timing { get; internal set; }
}

/// <summary>Parsed <c>response_format=json</c> speech response.</summary>
public sealed class SpeechJsonResult
{
    /// <summary>Base64 WAV text as delivered.</summary>
    public string AudioBase64 { get; internal set; } = string.Empty;

    /// <summary>Decoded WAV bytes.</summary>
    public byte[] Audio => Http.SseReader.ReadBase64Payload(AudioBase64);

    /// <summary>Format tag; the server writes <c>wav</c>.</summary>
    public string? Format { get; internal set; }

    /// <summary>Run timing (<c>wall_ms</c>, <c>audio_duration_ms</c>, <c>rtf</c>).</summary>
    public AudioCppTiming? Timing { get; internal set; }
}

/// <summary>Kind of a <see cref="SpeechStreamEvent"/>.</summary>
public enum SpeechStreamEventKind
{
    /// <summary>A chunk of PCM16 audio.</summary>
    AudioDelta,

    /// <summary>Final audio event carrying timing; a healthy stream sends <c>[DONE]</c> after it.</summary>
    AudioDone,

    /// <summary>Text written since the previous event (live streams with <c>return_text</c>).</summary>
    TextDelta,

    /// <summary>The whole text, before <see cref="SpeechStreamEventKind.AudioDone"/>.</summary>
    TextDone,
}

/// <summary>A speech stream event.</summary>
public readonly struct SpeechStreamEvent
{
    private SpeechStreamEvent(SpeechStreamEventKind kind, byte[]? pcm, string? text, AudioCppTiming? timing)
    {
        Kind = kind;
        Pcm = pcm;
        Text = text;
        Timing = timing;
    }

    /// <summary>Which event this is.</summary>
    public SpeechStreamEventKind Kind { get; }

    /// <summary>PCM bytes for <see cref="SpeechStreamEventKind.AudioDelta"/>.</summary>
    public byte[]? Pcm { get; }

    /// <summary>Text delta (<see cref="SpeechStreamEventKind.TextDelta"/>) or final text (<see cref="SpeechStreamEventKind.TextDone"/>).</summary>
    public string? Text { get; }

    /// <summary>Timing for <see cref="SpeechStreamEventKind.AudioDone"/>.</summary>
    public AudioCppTiming? Timing { get; }

    /// <summary>Parses one SSE data payload into a speech stream event.</summary>
    /// <param name="json">The event's JSON data payload.</param>
    /// <returns>The parsed event.</returns>
    /// <exception cref="JsonException">The payload carries an unknown event type.</exception>
    internal static SpeechStreamEvent Parse(string json)
    {
        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;
        string? type = Http.SseReader.ReadString(root, "type");
        switch (type)
        {
            case "speech.audio.delta":
                return new SpeechStreamEvent(SpeechStreamEventKind.AudioDelta, Http.SseReader.ReadBase64(root, "audio") ?? Array.Empty<byte>(), null, null);
            case "speech.audio.done":
                return new SpeechStreamEvent(SpeechStreamEventKind.AudioDone, null, null, Http.SseReader.ReadTiming(root, "timing"));
            case "speech.text.delta":
                return new SpeechStreamEvent(SpeechStreamEventKind.TextDelta, null, Http.SseReader.ReadString(root, "delta") ?? string.Empty, null);
            case "speech.text.done":
                return new SpeechStreamEvent(SpeechStreamEventKind.TextDone, null, Http.SseReader.ReadString(root, "text") ?? string.Empty, null);
            default:
                throw new JsonException("unknown speech stream event type: " + (type ?? "<none>"));
        }
    }
}

/// <summary>Minimal URL query-string builder (application/x-www-form-urlencoded escaping).</summary>
internal sealed class QueryBuilder
{
    private readonly StringBuilder _builder = new();

    /// <summary>Appends one <c>key=value</c> pair; empty values are skipped.</summary>
    /// <param name="key">The query key.</param>
    /// <param name="value">The query value; nothing is appended when null or empty.</param>
    internal void Add(string key, string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        _builder.Append(_builder.Length == 0 ? "?" : "&");
        _builder.Append(Uri.EscapeDataString(key));
        _builder.Append('=');
        _builder.Append(Uri.EscapeDataString(value));
    }

    /// <inheritdoc/>
    public override string ToString() => _builder.ToString();
}