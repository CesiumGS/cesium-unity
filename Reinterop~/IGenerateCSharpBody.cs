namespace Reinterop
{
    /// <summary>
    /// Invoked by <see cref="CSharpFunctionCallableFromCpp"/> to generate the body of a function intended
    /// to be called from C++.
    /// </summary>
    internal interface IGenerateCSharpBody
    {
        /// <summary>
        /// Generates the function body. The returned statements should assume that all parameters have already
        /// been converted to real C# types (not interop types). They do not need to catch exceptions to
        /// propagate them to the C++ side; this is handled automatically externally.
        /// </summary>
        IEnumerable<CSharpStatement> GenerateBody(ReinteropGenerationContext context, CSharpFunctionCallableFromCpp function);
    }
}
