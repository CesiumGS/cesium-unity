using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Oxidize
{
    internal enum CppTypeKind
    {
        Unknown,
        Primitive,
        BlittableStruct,
        NonBlittableStructWrapper,
        ClassWrapper
    }

    [Flags]
    internal enum CppTypeFlags
    {
        Normal = 0,
        Pointer = 1,
        Reference = 2,
        Const = 4
    }

    /// <summary>
    /// Describes a C++ type.
    /// </summary>
    internal class CppType
    {
        public readonly CppTypeKind Kind;
        public readonly IReadOnlyCollection<string> Namespaces;
        public readonly string Name;
        public readonly IReadOnlyCollection<CppType>? GenericArguments;
        public readonly CppTypeFlags Flags;

        private static readonly string[] StandardNamespace = { "std" };
        private static readonly string[] NoNamespace = { };

        private const string IncludeCStdInt = "<cstdint>";

        public static CppType FromCSharp(CppGenerationContext context, ITypeSymbol type)
        {
            switch (type.SpecialType)
            {
                case SpecialType.System_Int16:
                    return CreatePrimitiveType(StandardNamespace, "int16_t");
                case SpecialType.System_Int32:
                    return CreatePrimitiveType(StandardNamespace, "int32_t");
                case SpecialType.System_Int64:
                    return CreatePrimitiveType(StandardNamespace, "int64_t");
                case SpecialType.System_Single:
                    return CreatePrimitiveType(NoNamespace, "float");
                case SpecialType.System_Double:
                    return CreatePrimitiveType(NoNamespace, "double");
                case SpecialType.System_UInt16:
                    return CreatePrimitiveType(StandardNamespace, "uint16_t");
                case SpecialType.System_UInt32:
                    return CreatePrimitiveType(StandardNamespace, "uint32_t");
                case SpecialType.System_UInt64:
                    return CreatePrimitiveType(StandardNamespace, "uint64_t");
            }

            List<string> namespaces = new List<string>();
            if (context.BaseNamespace.Length > 0)
            {
                namespaces.Add(context.BaseNamespace);
            }

            INamespaceSymbol ns = type.ContainingNamespace;
            while (ns != null)
            {
                if (ns.Name.Length > 0)
                {
                    namespaces.Add(ns.Name);
                }
                ns = ns.ContainingNamespace;
            }

            // TODO: generics

            return new CppType(CppTypeKind.ClassWrapper, namespaces, type.Name, null, CppTypeFlags.Normal);
        }

        public CppType(
            CppTypeKind kind,
            IReadOnlyCollection<string> namespaces,
            string name,
            IReadOnlyCollection<CppType>? genericArguments,
            CppTypeFlags flags)
        {
            this.Kind = kind;
            if (namespaces == StandardNamespace || namespaces == NoNamespace)
                this.Namespaces = (List<string>)namespaces;
            else
                this.Namespaces = new List<string>(namespaces);
            this.Name = name;
            if (genericArguments != null)
                this.GenericArguments = new List<CppType>(genericArguments);
            this.Flags = flags;
        }

        public string GetFullyQualifiedNamespace()
        {
            string ns = string.Join("::", Namespaces);
            if (ns.Length > 0)
                return "::" + ns;
            else
                return "";
        }

        public string GetFullyQualifiedName()
        {
            string modifier = Flags.HasFlag(CppTypeFlags.Const) ? "const " : "";
            string suffix = Flags.HasFlag(CppTypeFlags.Pointer)
                ? "*"
                : Flags.HasFlag(CppTypeFlags.Reference)
                    ? "&"
                    : "";
            string ns = GetFullyQualifiedNamespace();
            if (ns.Length > 0)
                return $"{modifier}{ns}::{Name}{suffix}";
            else
                return $"{modifier}{Name}{suffix}";
        }

        /// <summary>
        /// Adds the includes that are required to use this type in a generated
        /// header file as part of a method signature. If the type can be
        /// forward declared instead, this method will do nothing.
        /// </summary>
        /// <param name="includes">The set of includes to which to add this type's includes.</param>
        public void AddHeaderIncludesToCollection(ISet<string> includes)
        {
            AddIncludesToCollection(includes, true);
        }

        /// <summary>
        /// Adds the includes that are required to use this type in a generated
        /// source file.
        /// </summary>
        /// <param name="includes">The set of includes to which to add this type's includes.</param>
        public void AddSourceIncludesToCollection(ISet<string> includes)
        {
            AddIncludesToCollection(includes, false);
        }

        private void AddIncludesToCollection(ISet<string> includes, bool forHeader)
        {
            // Special case for primitives in <cstdint>.
            if (Kind == CppTypeKind.Primitive && Namespaces == StandardNamespace)
            {
                includes.Add(IncludeCStdInt);
                return;
            }

            if (!forHeader)
            {
                // Build an include name from the namespace and type names.
                string path = string.Join("/", Namespaces);
                if (path.Length > 0)
                    includes.Add($"<{path}/{Name}.h>");
                else
                    includes.Add($"<{Name}.h>");
            }

            // Add includes for generic arguments, too.
            // TODO
        }

        public CppType AsConstReference()
        {
            return new CppType(Kind, Namespaces, Name, GenericArguments, Flags | CppTypeFlags.Const | CppTypeFlags.Reference & ~CppTypeFlags.Pointer);
        }

        /// <summary>
        /// Gets a version of this type suitable for use as the return value
        /// of a wrapped method. This simply returns the type unmodified.
        /// </summary>
        public CppType AsReturnType()
        {
            // All types are returned by value.
            return this;
        }

        /// <summary>
        /// Gets a version of this type suitable for use as a wrapped function
        /// parameter. For classes and structs, this returns a const reference
        /// to the type.
        /// </summary>
        public CppType AsParameterType()
        {
            switch (this.Kind)
            {
                case CppTypeKind.ClassWrapper:
                case CppTypeKind.BlittableStruct:
                case CppTypeKind.NonBlittableStructWrapper:
                    return this.AsConstReference();
            }

            return this;
        }

        private static CppType CreatePrimitiveType(IReadOnlyCollection<string> cppNamespaces, string cppTypeName)
        {
            return new CppType(CppTypeKind.Primitive, cppNamespaces, cppTypeName, null, CppTypeFlags.Normal);
        }

        //private static bool IsBlittableStruct(CppGenerationContext options, ITypeSymbol type)
        //{
        //    if (!type.IsValueType)
        //        return false;

        //    string? primitiveType = TranslatePrimitiveType(options, type);
        //    if (primitiveType != null)
        //        return true;

        //    ImmutableArray<ISymbol> members = type.GetMembers();
        //    foreach (ISymbol member in members)
        //    {
        //        if (member.Kind != SymbolKind.Field)
        //            continue;

        //        ITypeSymbol? memberType = member as ITypeSymbol;
        //        if (memberType == null)
        //            continue;

        //        if (!IsBlittableStruct(options, memberType))
        //            return false;
        //    }

        //    return true;
        //}
    }
}
