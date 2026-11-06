// Polyfills so C# 9+ language features (records with init accessors) compile on netstandard2.0.
#if NETSTANDARD2_0
namespace System.Runtime.CompilerServices
{
    using System.ComponentModel;

    /// <summary>Required by the compiler to emit init-only property setters on older targets.</summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class IsExternalInit
    {
    }
}
#endif