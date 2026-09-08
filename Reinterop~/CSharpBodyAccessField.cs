using Microsoft.CodeAnalysis;

namespace Reinterop
{
    /// <summary>
    /// An implementation of a C# interop function that reads or writes a field.
    /// </summary>
    internal class CSharpBodyAccessField : IGenerateCSharpBody
    {
        private readonly IFieldSymbol _field;
        private readonly bool _isGetter;

        public CSharpBodyAccessField(IFieldSymbol field, bool isGetter)
        {
            _field = field;
            _isGetter = isGetter;
        }

        public IEnumerable<CSharpStatement> GenerateBody(CppGenerationContext context, CSharpFunctionCallableFromCpp function)
        {
            CSharpExpression target = function.Static() ? new CSharpIdentifier(function.Owner()) : CSharpIdentifier.Thiz;
            CSharpExpression accessor = new CSharpMemberAccess(target, _field.Name);

            if (_isGetter)
            {
                yield return new CSharpReturn(accessor);
                yield break;
            }

            yield return new CSharpExpressionStatement(new CSharpBinary("=", accessor, new CSharpIdentifier(function.Parameters().Single().Name)));
        }
    }
}
