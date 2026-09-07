namespace Reinterop
{
    internal class CSharpBodyCreateDelegate : IGenerateCSharpBody
    {
        private readonly CSharpType _delegateType;
        private readonly string _nativeFunctionTypeName;

        public CSharpBodyCreateDelegate(CSharpType delegateType, string nativeFunctionTypeName)
        {
            _delegateType = delegateType;
            _nativeFunctionTypeName = nativeFunctionTypeName;
        }

        public IEnumerable<CSharpStatement> GenerateBody(CppGenerationContext context, CSharpFunctionCallableFromCpp function)
        {
            yield return new CSharpVariableDeclaration(
                "var",
                "receiver",
                new CSharpNew(_nativeFunctionTypeName, [new CSharpIdentifier("pCallbackFunction")]));
            yield return new CSharpReturn(new CSharpNew(
                _delegateType.GetFullyQualifiedName(),
                [new CSharpMemberAccess(new CSharpIdentifier("receiver"), "Invoke")]));
        }
    }
}
