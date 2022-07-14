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

        private static readonly CppType Int16 = CreatePrimitiveType(StandardNamespace, "int16_t");
        private static readonly CppType Int32 = CreatePrimitiveType(StandardNamespace, "int32_t");
        private static readonly CppType Int64 = CreatePrimitiveType(StandardNamespace, "int64_t");
        private static readonly CppType UInt16 = CreatePrimitiveType(StandardNamespace, "uint16_t");
        private static readonly CppType UInt32 = CreatePrimitiveType(StandardNamespace, "uint32_t");
        private static readonly CppType UInt64 = CreatePrimitiveType(StandardNamespace, "uint64_t");
        private static readonly CppType Single = CreatePrimitiveType(NoNamespace, "float");
        private static readonly CppType Double = CreatePrimitiveType(NoNamespace, "double");
        private static readonly CppType VoidPointer = CreatePrimitiveType(NoNamespace, "void", CppTypeFlags.Pointer);
        private static readonly CppType Void = CreatePrimitiveType(NoNamespace, "void");

        public static CppType FromCSharp(CppGenerationContext context, ITypeSymbol type)
        {
            switch (type.SpecialType)
            {
                case SpecialType.System_Int16:
                    return Int16;
                case SpecialType.System_Int32:
                    return Int32;
                case SpecialType.System_Int64:
                    return Int64;
                case SpecialType.System_Single:
                    return Single;
                case SpecialType.System_Double:
                    return Double;
                case SpecialType.System_UInt16:
                    return UInt16;
                case SpecialType.System_UInt32:
                    return UInt32;
                case SpecialType.System_UInt64:
                    return UInt64;
                case SpecialType.System_IntPtr:
                    return VoidPointer;
                case SpecialType.System_Void:
                    return Void;
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

            return new CppType(CppTypeKind.ClassWrapper, namespaces, type.Name, null, 0);
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
                this.Namespaces = namespaces;
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

        public void AddForwardDeclarationsToSet(ISet<string> forwardDeclarations)
        {
            // Primitives do not need to be forward declared
            if (Kind == CppTypeKind.Primitive)
                return;

            // Non-pointer, non-reference types cannot be forward declared.
            if (!Flags.HasFlag(CppTypeFlags.Reference) && !Flags.HasFlag(CppTypeFlags.Pointer))
                return;

            string ns = GetFullyQualifiedNamespace();
            if (ns != null)
            {
                string typeType = Kind == CppTypeKind.ClassWrapper ? "class" : "struct";
                forwardDeclarations.Add(
                    $$"""
                    namespace {{ns}} {
                    {{typeType}} {{Name}};
                    }
                    """);
            }
        }

        /// <summary>
        /// Adds the includes that are required to use this type in a generated
        /// header file as part of a method signature. If the type can be
        /// forward declared instead, this method will do nothing.
        /// </summary>
        /// <param name="includes">The set of includes to which to add this type's includes.</param>
        public void AddHeaderIncludesToSet(ISet<string> includes)
        {
            AddIncludesToSet(includes, true);
        }

        /// <summary>
        /// Adds the includes that are required to use this type in a generated
        /// source file.
        /// </summary>
        /// <param name="includes">The set of includes to which to add this type's includes.</param>
        public void AddSourceIncludesToSet(ISet<string> includes)
        {
            AddIncludesToSet(includes, false);
        }

        private void AddIncludesToSet(ISet<string> includes, bool forHeader)
        {
            if (Kind == CppTypeKind.Primitive)
            {
                // Special case for primitives in <cstdint>.
                if (Namespaces == StandardNamespace)
                {
                    includes.Add(IncludeCStdInt);
                }
                return;
            }

            bool canBeForwardDeclared = Flags.HasFlag(CppTypeFlags.Reference) || Flags.HasFlag(CppTypeFlags.Pointer);
            if (!forHeader || !canBeForwardDeclared)
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

        /// <summary>
        /// Gets the version of this type that should be used in a function
        /// pointer that will call into the managed side.
        /// </summary>
        public CppType AsInteropType()
        {
            if (this.Kind == CppTypeKind.Primitive || this.Kind == CppTypeKind.BlittableStruct)
                return this;

            return VoidPointer;
        }

        private static CppType CreatePrimitiveType(IReadOnlyCollection<string> cppNamespaces, string cppTypeName, CppTypeFlags flags = 0)
        {
            return new CppType(CppTypeKind.Primitive, cppNamespaces, cppTypeName, null, flags);
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
