using System;
using System.Collections.Generic;

namespace Mabs.AudioCpp.Utils;

/// <summary>Shared request-serialization helpers.</summary>
internal static class RequestJson
{
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