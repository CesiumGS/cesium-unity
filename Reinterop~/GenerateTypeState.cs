using Microsoft.CodeAnalysis;

namespace Reinterop
{
    internal class GenerateTypeState
    {
        /// <summary>
        /// Holds a cache of the generic template declarations generated for the current type being generated. This is
        /// used to avoid generating the same template declaration multiple times when a generic method has multiple
        /// specializations. The key is the generic method's unbound symbol (i.e. <see cref="IMethodSymbol.ConstructedFrom"/>).
        /// </summary>
        public Dictionary<IMethodSymbol, CppFunction> MethodCache;

        public GenerateTypeState()
        {
            this.MethodCache = new Dictionary<IMethodSymbol, CppFunction>(SymbolEqualityComparer.Default);
        }
    }
}
