namespace Reinterop
{
    /// <summary>
    /// An implementation of a C# interop function that constructs a new array of the owner type.
    /// </summary>
    internal class CSharpBodyConstructArray : IGenerateCSharpBody
    {
        public IEnumerable<CSharpStatement> GenerateBody(CppGenerationContext context, CSharpFunctionCallableFromCpp function)
        {
            CSharpType? arrayElementType = function.Owner().ArrayElementType;
            if (arrayElementType == null)
                throw new InvalidOperationException($"Owner {function.Owner().GetFullyQualifiedName()} does not have an ArrayElementType.");
            yield return new CSharpReturn(new CSharpArrayNew(arrayElementType, [new CSharpIdentifier("size")]));
        }
    }
}
