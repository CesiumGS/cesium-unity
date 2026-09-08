namespace Reinterop
{
    /// <summary>
    /// An implementation of a C# interop function that combines two delegates (using the `+` operator), or removes
    /// a delegate from a combined delegate (using the `-` operator).
    /// </summary>
    internal class CSharpBodyAddRemoveDelegate : IGenerateCSharpBody
    {
        private readonly string _operator;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="op">The operator to use (`"+"` to combine delegates, `"-"` to remove a delegate).</param>
        public CSharpBodyAddRemoveDelegate(string op)
        {
            _operator = op;
        }

        public IEnumerable<CSharpStatement> GenerateBody(CppGenerationContext context, CSharpFunctionCallableFromCpp function)
        {
            yield return new CSharpReturn(new CSharpBinary(
                _operator,
                CSharpIdentifier.Thiz,
                new CSharpIdentifier("rhs")));
        }
    }
}
