namespace Reinterop
{
    /// <summary>
    /// A C# interop function body that creates a delegate instance from a C++ std::function pointer.
    /// </summary>
    internal class CSharpBodyCreateDelegate : IGenerateCSharpBody
    {
        private readonly CSharpType _nativeFunctionType;

        public CSharpBodyCreateDelegate(CSharpType nativeFunctionType)
        {
            _nativeFunctionType = nativeFunctionType;
        }

        public IEnumerable<CSharpStatement> GenerateBody(CppGenerationContext context, CSharpFunctionCallableFromCpp function)
        {
            yield return new CSharpVariableDeclaration(
                _nativeFunctionType.GetFullyQualifiedName(),
                "receiver",
                new CSharpNew(_nativeFunctionType, [new CSharpIdentifier("pCallbackFunction")]));
            yield return new CSharpReturn(new CSharpNew(
                function.ReturnType(),
                [new CSharpMemberAccess(new CSharpIdentifier("receiver"), "Invoke")]));
        }
    }
}
