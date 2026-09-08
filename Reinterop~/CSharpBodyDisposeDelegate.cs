namespace Reinterop
{
    internal class CSharpBodyDisposeDelegate : IGenerateCSharpBody
    {
        private readonly string _nativeFunctionTypeName;

        public CSharpBodyDisposeDelegate(string nativeFunctionTypeName)
        {
            _nativeFunctionTypeName = nativeFunctionTypeName;
        }

        public IEnumerable<CSharpStatement> GenerateBody(CppGenerationContext context, CSharpFunctionCallableFromCpp function)
        {
            yield return new CSharpVariableDeclaration(
                function.Owner(),
                "delegateObject",
                CSharpIdentifier.Thiz);
            yield return new CSharpIf(
                new CSharpIs(
                    new CSharpMemberAccess(new CSharpIdentifier("delegateObject"), "Target"),
                    _nativeFunctionTypeName,
                    "nativeFunction"),
                [new CSharpExpressionStatement(new CSharpCall(
                    new CSharpMemberAccess(new CSharpIdentifier("nativeFunction"), "Dispose"),
                    []))]);
        }
    }
}
