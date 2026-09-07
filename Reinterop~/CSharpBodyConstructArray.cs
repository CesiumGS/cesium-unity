namespace Reinterop
{
    internal class CSharpBodyConstructArray : IGenerateCSharpBody
    {
        public IEnumerable<CSharpStatement> GenerateBody(CppGenerationContext context, CSharpFunctionCallableFromCpp function)
        {
            CSharpType? arrayElementType = function.Owner().ArrayElementType;
            if (arrayElementType == null)
                throw new InvalidOperationException($"Owner {function.Owner().GetFullyQualifiedName()} does not have an ArrayElementType.");
            yield return new CSharpReturn(new CSharpArrayNew(arrayElementType.GetFullyQualifiedName(), [new CSharpIdentifier("size")]));
        }
    }
}
