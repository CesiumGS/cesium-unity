namespace Reinterop
{
    /// <summary>
    /// An implementation of a C# interop function that constructs and returns an instance of
    /// <see cref="CSharpFunctionCallableFromCpp.Owner"/>. The constructor is called passing
    /// <see cref="CSharpFunctionCallableFromCpp.Parameters"/>.
    /// </summary>
    internal class CSharpBodyConstructInstance : IGenerateCSharpBody
    {
        public IEnumerable<CSharpStatement> GenerateBody(ReinteropGenerationContext context, CSharpFunctionCallableFromCpp function)
        {
            yield return new CSharpReturn(new CSharpNew(
                function.Owner(),
                function.Parameters().Select(p => new CSharpIdentifier(p.Name)).ToArray()));
        }
    }
}
