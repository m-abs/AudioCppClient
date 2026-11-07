using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

using Mabs.AudioCpp.Models;
using Mabs.AudioCpp.Models.Alignment;
using Mabs.AudioCpp.Models.Transcription;
using Mabs.AudioCpp.Models.Transcription.Batch;
using Mabs.AudioCpp.Utils;

namespace Mabs.AudioCpp.Http;

/// <summary>Multipart request construction shared by the transcription/alignment/batch routes.</summary>
internal static class Multipart
{
    /// <summary>Builds a POST request with the single-file multipart body.</summary>
    /// <param name="uri">The target route.</param>
    /// <param name="request">The request whose <see cref="TranscriptionInputBase.AudioFile"/> is uploaded.</param>
    /// <param name="stream">Whether to add the <c>stream=true</c> form field.</param>
    /// <returns>The request, ready to send.</returns>
    internal static HttpRequestMessage Post(Uri uri, TranscriptionInputBase request, bool stream = false)
    {
        HttpRequestMessage httpRequest = new(HttpMethod.Post, uri)
        {
            Content = BuildSingle(request, stream),
        };
        return httpRequest;
    }

    /// <summary>Builds the single-file multipart body: file part plus model/language/prompt/busy_timeout fields.</summary>
    /// <param name="request">The request whose file and fields are serialized.</param>
    /// <param name="stream">Whether to add the <c>stream=true</c> form field.</param>
    /// <returns>The multipart content.</returns>
    internal static MultipartFormDataContent BuildSingle(TranscriptionInputBase request, bool stream)
    {
        MultipartFormDataContent content = [];
        AddFile(content, "file", request.AudioFile!, request.AudioFileName!);
        content.Add(new StringContent(request.Model!), "model");
        if (!string.IsNullOrEmpty(request.Language))
        {
            content.Add(new StringContent(request.Language!), "language");
        }

        string? prompt = request is TranscriptionRequest transcription ? transcription.Prompt : request is TranscriptionDetailsRequest details ? details.Prompt : null;
        if (!string.IsNullOrEmpty(prompt))
        {
            content.Add(new StringContent(prompt!), "prompt");
        }

        if (request.BusyTimeoutMs.HasValue)
        {
            content.Add(new StringContent(request.BusyTimeoutMs.Value.ToString(CultureInfo.InvariantCulture)), "busy_timeout_ms");
        }

        if (stream)
        {
            content.Add(new StringContent("true"), "stream");
        }

        return content;
    }

    /// <summary>Builds the batch multipart body: one file part per upload plus the batch fields.</summary>
    /// <param name="request">The batch request.</param>
    /// <returns>The multipart content.</returns>
    internal static MultipartFormDataContent BuildBatch(BatchTranscriptionRequest request)
    {
        MultipartFormDataContent content = [];
        foreach (AudioUpload file in request.Files)
        {
            AddFile(content, "file", file.Content, file.FileName);
        }

        content.Add(new StringContent(request.Model!, Encoding.UTF8), "model");
        if (!string.IsNullOrEmpty(request.Language))
        {
            content.Add(new StringContent(request.Language!, Encoding.UTF8), "language");
        }

        if (!string.IsNullOrEmpty(request.Prompt))
        {
            content.Add(new StringContent(request.Prompt!, Encoding.UTF8), "prompt");
        }

        if (request.BusyTimeoutMs.HasValue)
        {
            content.Add(new StringContent(request.BusyTimeoutMs.Value.ToString(CultureInfo.InvariantCulture), Encoding.UTF8), "busy_timeout_ms");
        }

        if (request.Options != null && request.Options.Count > 0)
        {
            content.Add(new StringContent(BuildOptionsJson(request.Options), Encoding.UTF8, "application/json"), "options");
        }

        return content;
    }

    /// <summary>Builds the alignment multipart body: file, model, text, and optional language.</summary>
    /// <param name="request">The alignment request.</param>
    /// <returns>The multipart content.</returns>
    internal static MultipartFormDataContent BuildAlignment(AlignmentRequest request)
    {
        MultipartFormDataContent content = [];
        AddFile(content, "file", request.AudioFile!, request.AudioFileName!);
        content.Add(new StringContent(request.Model!, Encoding.UTF8), "model");
        content.Add(new StringContent(request.Text!, Encoding.UTF8), "text");
        if (!string.IsNullOrEmpty(request.Language))
        {
            content.Add(new StringContent(request.Language!, Encoding.UTF8), "language");
        }

        return content;
    }

    /// <summary>Adds one file part as bytes with an octet-stream content type.</summary>
    /// <param name="content">The multipart body to append to.</param>
    /// <param name="name">The form-field name (<c>file</c>).</param>
    /// <param name="data">The file bytes.</param>
    /// <param name="fileName">The filename reported in the part headers.</param>
    private static void AddFile(MultipartFormDataContent content, string name, byte[] data, string fileName)
    {
        ByteArrayContent part = new(data);
        part.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Add(part, name, fileName);
    }

    /// <summary>Adds one file part as a stream with an octet-stream content type.</summary>
    /// <param name="content">The multipart body to append to.</param>
    /// <param name="name">The form-field name (<c>file</c>).</param>
    /// <param name="data">The file stream.</param>
    /// <param name="fileName">The filename reported in the part headers.</param>
    private static void AddFile(MultipartFormDataContent content, string name, Stream data, string fileName)
    {
        StreamContent part = new(data);
        part.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Add(part, name, fileName);
    }

    /// <summary>Serializes the options map to a JSON string for the batch form field.</summary>
    /// <param name="options">The options map.</param>
    /// <returns>The JSON object string.</returns>
    private static string BuildOptionsJson(Dictionary<string, string> options)
    {
        return Json.Serialize(options);
    }
}