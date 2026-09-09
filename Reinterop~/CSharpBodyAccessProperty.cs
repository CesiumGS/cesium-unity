using Microsoft.CodeAnalysis;

namespace Reinterop
{
    /// <summary>
    /// An implementation of a C# interop function that invokes a property accessor using
    /// property or indexer syntax.
    /// </summary>
    internal class CSharpBodyAccessProperty : IGenerateCSharpBody
    {
        private readonly IPropertySymbol _property;
        private readonly bool _isGetter;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="property">The property to read or write. If the property reports that it `IsIndexer`, it is accessed using indexer syntax.</param>
        /// <param name="isGetter">True to read the property, false to write it.</param>
        public CSharpBodyAccessProperty(IPropertySymbol property, bool isGetter)
        {
            _property = property;
            _isGetter = isGetter;
        }

        public IEnumerable<CSharpStatement> GenerateBody(ReinteropGenerationContext context, CSharpFunctionCallableFromCpp function)
        {
            CSharpExpression target = function.Static() ? new CSharpIdentifier(function.Owner()) : CSharpIdentifier.Thiz;
            CSharpExpression accessor = _property.IsIndexer
                ? new CSharpElementAccess(target, function.Parameters().Take(_property.Parameters.Length).Select(p => new CSharpIdentifier(p.Name)).ToArray())
                : new CSharpMemberAccess(target, _property.Name);

            if (_isGetter)
                yield return new CSharpReturn(accessor);
            else
                yield return new CSharpExpressionStatement(new CSharpBinary("=", accessor, new CSharpIdentifier(function.Parameters().Last().Name)));
        }
    }
}
