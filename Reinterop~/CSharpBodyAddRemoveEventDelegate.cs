using Microsoft.CodeAnalysis;

namespace Reinterop
{
    /// <summary>
    /// An implementation of a C# interop function that adds or removes a handler from an event
    /// using += / -= syntax.
    /// </summary>
    internal class CSharpBodyAddRemoveEventDelegate : IGenerateCSharpBody
    {
        private readonly IEventSymbol _event;
        private readonly bool _isAdd;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="evt">The event to add or remove a handler from.</param>
        /// <param name="isAdd">True to add a handler, false to remove it.</param>
        public CSharpBodyAddRemoveEventDelegate(IEventSymbol evt, bool isAdd)
        {
            _event = evt;
            _isAdd = isAdd;
        }

        public IEnumerable<CSharpStatement> GenerateBody(ReinteropGenerationContext context, CSharpFunctionCallableFromCpp function)
        {
            CSharpExpression target = function.Static() ? new CSharpIdentifier(function.Owner()) : CSharpIdentifier.Thiz;
            CSharpExpression accessor = new CSharpMemberAccess(target, _event.Name);
            yield return new CSharpExpressionStatement(new CSharpBinary(_isAdd ? "+=" : "-=", accessor, new CSharpIdentifier(function.Parameters().Last().Name)));
        }
    }
}
