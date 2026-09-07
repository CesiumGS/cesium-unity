namespace Reinterop
{
    internal class CSharpBodyAddRemoveDelegate : IGenerateCSharpBody
    {
        private readonly string _operator;

        public CSharpBodyAddRemoveDelegate(string op)
        {
            _operator = op;
        }

        public IEnumerable<CSharpStatement> GenerateBody(CppGenerationContext context, CSharpFunctionCallableFromCpp function)
        {
            yield return new CSharpReturn(new CSharpBinary(
                _operator,
                new CSharpIdentifier("thiz"),
                new CSharpIdentifier("rhs")));
        }
    }
}
