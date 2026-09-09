namespace Reinterop
{
    /// <summary>
    /// A C# interop function body that disposes of a delegate instance created from a C++ `std::function` pointer.
    /// </summary>
    internal class CSharpBodyDisposeDelegate : IGenerateCSharpBody
    {
        private readonly CSharpType _nativeFunctionType;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="nativeFunctionType">The C# type that holds the `std::function` pointer and invokes the function when its `Invoke` method is called.</param>
        public CSharpBodyDisposeDelegate(CSharpType nativeFunctionType)
        {
            _nativeFunctionType = nativeFunctionType;
        }

        public IEnumerable<CSharpStatement> GenerateBody(ReinteropGenerationContext context, CSharpFunctionCallableFromCpp function)
        {
            yield return new CSharpVariableDeclaration(
                function.Owner(),
                "delegateObject",
                CSharpIdentifier.Thiz);
            yield return new CSharpIf(
                new CSharpIs(
                    new CSharpMemberAccess(new CSharpIdentifier("delegateObject"), "Target"),
                    _nativeFunctionType,
                    "nativeFunction"),
                [new CSharpExpressionStatement(new CSharpCall(
                    new CSharpMemberAccess(new CSharpIdentifier("nativeFunction"), "Dispose"),
                    []))]);
        }
    }
}
