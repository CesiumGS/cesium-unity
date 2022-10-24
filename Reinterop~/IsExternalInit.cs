namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// This type must be defined in order to use C# 9's `init` properties
    /// in .NET versions prior to version 5. See
    /// https://github.com/dotnet/roslyn/issues/45510
    /// </summary>
    internal static class IsExternalInit
    {
    }
}
