namespace Reinterop
{
    /// <summary>
    /// An implementation of a C# interop function that invokes the binary operator identified by <see cref="CSharpFunctionCallableFromCpp.Name"/>
    /// with the left and right operands provided by <see cref="CSharpFunctionCallableFromCpp.Parameters"/>. There must be exactly two parameters.
    /// The operator is expected to return <see cref="CSharpFunctionCallableFromCpp.ReturnType"/>. The method name is converted into an
    /// operator by <see cref="Interop.MethodNameToOperator"/>.
    /// </summary>
    internal class CSharpBodyInvokeBinaryOperator : IGenerateCSharpBody
    {
        public IEnumerable<CSharpStatement> GenerateBody(ReinteropGenerationContext context, CSharpFunctionCallableFromCpp function)
        {
            if (function.Parameters().Count != 2)
                throw new InvalidOperationException("CSharpBodyInvokeBinaryOperator requires exactly two parameters.");

            CSharpExpression leftExpr = new CSharpIdentifier(function.Parameters()[0].Name);
            CSharpExpression rightExpr = new CSharpIdentifier(function.Parameters()[1].Name);
            yield return new CSharpReturn(new CSharpBinary(Interop.MethodNameToOperator(function.Name()!), leftExpr, rightExpr));
        }
    }
}
