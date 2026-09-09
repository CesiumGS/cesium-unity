namespace Reinterop
{
    /// <summary>
    /// A C# interop function body that creates a delegate instance from a C++ `std::function` pointer.
    /// </summary>
    internal class CSharpBodyCreateDelegate : IGenerateCSharpBody
    {
        private readonly CSharpType _nativeFunctionType;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="nativeFunctionType">The C# type that holds the `std::function` pointer and invokes the function when its `Invoke` method is called.</param>
        public CSharpBodyCreateDelegate(CSharpType nativeFunctionType)
        {
            _nativeFunctionType = nativeFunctionType;
        }

        public IEnumerable<CSharpStatement> GenerateBody(ReinteropGenerationContext context, CSharpFunctionCallableFromCpp function)
        {
            yield return new CSharpVariableDeclaration(
                _nativeFunctionType,
                "receiver",
                new CSharpNew(_nativeFunctionType, [new CSharpIdentifier("pCallbackFunction")]));
            yield return new CSharpReturn(new CSharpNew(
                function.ReturnType(),
                [new CSharpMemberAccess(new CSharpIdentifier("receiver"), "Invoke")]));
        }
    }
}
