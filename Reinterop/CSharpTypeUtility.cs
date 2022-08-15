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

        public static string GetAccessString(Accessibility access)
        {
            if (access == Accessibility.Public)
                return "public";
            else if (access == Accessibility.Private)
                return "private";
            else if (access == Accessibility.Protected)
                return "protected";
            else if (access == Accessibility.Internal)
                return "internal";
            else if (access == Accessibility.ProtectedAndInternal)
                return "private protected";
            else if (access == Accessibility.ProtectedOrInternal)
                return "protected internal";
            else
                return "";
        }
    }
}
