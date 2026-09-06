using Microsoft.CodeAnalysis;

namespace Reinterop
{
    /// <summary>
    /// An implementation of a C# interop function that invokes the method <see cref="CSharpFunctionCallableFromCpp.Name"/>
    /// on <see cref="CSharpFunctionCallableFromCpp.Owner"/>. It is called with <see cref="CSharpFunctionCallableFromCpp.Parameters"/>
    /// and expected to return <see cref="CSharpFunctionCallableFromCpp.ReturnType"/>. If
    /// <see cref="CSharpFunctionCallableFromCpp.Static"/> is `true`, the method is invoked as a static method; otherwise, it is
    /// invoked on an instance of <see cref="CSharpFunctionCallableFromCpp.Owner"/>. If
    /// <see cref="CSharpFunctionCallableFromCpp.TypeArguments"/> is not empty, the method is a generic method and is invoked with the
    /// specified type arguments.
    /// </summary>
    internal class CSharpBodyInvokeMethod : IGenerateCSharpBody
    {
        public IEnumerable<CSharpStatement> GenerateBody(CppGenerationContext context, CSharpFunctionCallableFromCpp function)
        {
            CSharpExpression target = function.Static() ? new CSharpIdentifier(function.Owner().GetFullyQualifiedName()) : new CSharpIdentifier("thiz");
            string methodName = function.Name()!;
            if (function.TypeArguments().Count > 0)
                methodName += "<" + string.Join(",", function.TypeArguments().Select(t => t.GetFullyQualifiedName())) + ">";
            CSharpCall callExpression = new CSharpCall(new CSharpMemberAccess(target, methodName), function.Parameters().Select(p => new CSharpIdentifier(p.Name)).ToArray());
            if (function.ReturnType().SpecialType == SpecialType.System_Void)
                yield return new CSharpExpressionStatement(callExpression);
            else
                yield return new CSharpReturn(callExpression);
        }
    }
}
