using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace Reinterop
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

        /// <summary>
        /// Find a member on a type or any of its base classes.
        /// </summary>
        /// <param name="type">The type on which to find the member.</param>
        /// <param name="name">The name of the member.</param>
        /// <returns>The member, or null if it does not exist.</returns>
        public static ISymbol? FindMember(ITypeSymbol type, string name)
        {
            return FindMembers(type, name).FirstOrDefault();
        }

        /// <summary>
        /// Find a member on a type or any of its base classes.
        /// </summary>
        /// <param name="type">The type on which to find the member.</param>
        /// <param name="name">The name of the member.</param>
        /// <returns>The member, or null if it does not exist.</returns>
        public static IEnumerable<ISymbol> FindMembers(ITypeSymbol type, string name)
        {
            ITypeSymbol? current = type;
            while (current != null)
            {
                ImmutableArray<ISymbol> members = current.GetMembers(name);
                foreach (ISymbol symbol in members)
                {
                    yield return symbol;
                }

                current = current.BaseType;
            }
        }

        public static IPropertySymbol? GetAutoPropertyForField(IFieldSymbol field)
        {
            string fieldName = field.Name;

            IPropertySymbol? autoProperty = field.AssociatedSymbol as IPropertySymbol;

            // Unfortunately, the above will only work if the C# compiler is looking at the source code for the property.
            // So for backing fields in referenced assemblies, we need to do this more manually.
            if (autoProperty == null && fieldName.EndsWith("__BackingField"))
            {
                Match match = new Regex("<(.+)>k__BackingField").Match(fieldName);
                if (match.Success && match.Groups.Count > 1)
                {
                    autoProperty = CSharpTypeUtility.FindMember(field.ContainingType, match.Groups[1].Value) as IPropertySymbol;
                }
            }

            return autoProperty;
        }

        public static string GetFieldName(IFieldSymbol field)
        {
            IPropertySymbol? autoProperty = GetAutoPropertyForField(field);
            if (autoProperty != null)
                return autoProperty.Name;
            else
                return field.Name;
        }

        public static bool GetFieldIsPrivate(IFieldSymbol field)
        {
            IPropertySymbol? autoProperty = GetAutoPropertyForField(field);
            if (autoProperty != null)
                return autoProperty.DeclaredAccessibility != Accessibility.Public;
            else
                return field.DeclaredAccessibility != Accessibility.Public;
        }
    }
}
