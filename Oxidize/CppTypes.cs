using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Oxidize
{
    internal class CppTypes
    {
        public static string GetNamespace(string baseNamespace, ITypeSymbol type)
        {
            List<string> parts = new List<string>();
            if (baseNamespace.Length > 0)
            {
                parts.Add(baseNamespace);
            }

            INamespaceSymbol ns = type.ContainingNamespace;
            while (ns != null)
            {
                if (ns.Name.Length > 0)
                {
                    parts.Add(ns.Name);
                }
                ns = ns.ContainingNamespace;
            }

            return string.Join("::", parts);
        }

        public static string GetFullyQualifiedTypeName(string baseNamespace, ITypeSymbol type)
        {
            return "::" + GetNamespace(baseNamespace, type) + "::" + type.Name;
        }
    }
}
