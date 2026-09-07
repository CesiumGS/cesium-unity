using Microsoft.CodeAnalysis;

namespace Reinterop
{
    /// <summary>
    /// An implementation of a C# interop function that adds or removes a handler from an event
    /// using += / -= syntax. Events cannot have their add/remove accessors called directly as
    /// methods (that produces CS0571), so this must be used instead of <see cref="CSharpBodyInvokeMethod"/>.
    /// </summary>
    internal class CSharpBodyAddRemoveEventDelegate : IGenerateCSharpBody
    {
        private readonly IEventSymbol _event;
        private readonly bool _isAdd;

        public CSharpBodyAddRemoveEventDelegate(IEventSymbol evt, bool isAdd)
        {
            _event = evt;
            _isAdd = isAdd;
        }

        public IEnumerable<CSharpStatement> GenerateBody(CppGenerationContext context, CSharpFunctionCallableFromCpp function)
        {
            CSharpExpression target = function.Static() ? new CSharpIdentifier(function.Owner().GetFullyQualifiedName()) : new CSharpIdentifier("thiz");
            CSharpExpression accessor = new CSharpMemberAccess(target, _event.Name);
            yield return new CSharpExpressionStatement(new CSharpBinary(_isAdd ? "+=" : "-=", accessor, new CSharpIdentifier(function.Parameters().Last().Name)));
        }
    }
}
