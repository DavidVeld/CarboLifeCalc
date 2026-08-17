namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Marker the C# compiler emits on init-only setters, and therefore on every
    /// positional record. .NET Framework does not ship it, so the 4.8 build supplies its
    /// own; it is linked into every project by Directory.Build.props and compiled only
    /// for net48.
    /// </summary>
    internal static class IsExternalInit
    {
    }
}
