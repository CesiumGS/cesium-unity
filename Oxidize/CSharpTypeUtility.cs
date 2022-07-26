using Microsoft.CodeAnalysis;

namespace Oxidize
{
    internal class CSharpTypeUtility
    {
        public static string GetNameForDelegate(CSharpType type, IMethodSymbol method)
        {
            // TODO: include parameters in name to avoid conflicts on overloads.
            string name = method.Name;
            if (name == ".ctor")
                name = "Constructor";
            return $"{type.GetFullyQualifiedName().Replace(".", "_")}_{name}";
        }
    }
}
