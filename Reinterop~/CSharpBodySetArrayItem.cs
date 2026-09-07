namespace Reinterop
{
    internal class CSharpBodySetArrayItem : IGenerateCSharpBody
    {
        public IEnumerable<CSharpStatement> GenerateBody(CppGenerationContext context, CSharpFunctionCallableFromCpp function)
        {
            yield return new CSharpExpressionStatement(new CSharpBinary(
                "=",
                new CSharpElementAccess(new CSharpIdentifier("thiz"), [new CSharpIdentifier("index")]),
                new CSharpIdentifier("value")));
        }
    }
}
