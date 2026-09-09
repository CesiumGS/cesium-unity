namespace Reinterop
{
    /// <summary>
    /// An implementation of a C# interop function that sets an item at a particular index in an array.
    /// The index is given by a parameter named `index` and the value to write is given by a parameter
    /// named `value`.
    /// </summary>
    internal class CSharpBodySetArrayItem : IGenerateCSharpBody
    {
        public IEnumerable<CSharpStatement> GenerateBody(ReinteropGenerationContext context, CSharpFunctionCallableFromCpp function)
        {
            yield return new CSharpExpressionStatement(new CSharpBinary(
                "=",
                new CSharpElementAccess(CSharpIdentifier.Thiz, [new CSharpIdentifier("index")]),
                new CSharpIdentifier("value")));
        }
    }
}
