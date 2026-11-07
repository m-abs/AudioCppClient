using System;

namespace Mabs.AudioCpp.Http
{
    /// <summary>
    /// The exception thrown when an audio.cpp server reports an error, either through a
    /// non-2xx JSON error envelope (<c>{"error":{"message":…,"type":…}}</c>) or through an
    /// SSE <c>error</c> event.
    /// </summary>
    public class AudioCppException : Exception
    {
        /// <summary>Initializes a new instance of the <see cref="AudioCppException"/> class.Initializes a new instance with all wire-reported fields.</summary>
        public AudioCppException(string message, string? errorType, int? statusCode)
            : base(message)
        {
            ErrorType = errorType;
            StatusCode = statusCode;
        }

        /// <summary>Initializes a new instance of the <see cref="AudioCppException"/> class.Initializes a new instance with a message only.</summary>
        public AudioCppException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// The server-reported error type, for example <c>server_error</c>,
        /// <c>invalid_request_error</c>, <c>server_busy</c> or <c>insufficient_memory</c>.
        /// <see langword="null"/> when the body was not an error envelope or the error
        /// arrived as an SSE event (SSE error events carry no type).
        /// </summary>
        public string? ErrorType { get; }

        /// <summary>
        /// The HTTP status code, when the error came from an HTTP response body;
        /// <see langword="null"/> for SSE error events.
        /// </summary>
        public int? StatusCode { get; }
    }
}