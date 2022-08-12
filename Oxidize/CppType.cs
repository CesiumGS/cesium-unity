using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Oxidize
{
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
        public readonly InteropTypeKind Kind;
        public readonly IReadOnlyCollection<string> Namespaces;
        public readonly string Name;
        public readonly IReadOnlyCollection<CppType>? GenericArguments;
        public readonly CppTypeFlags Flags;
        public readonly string? HeaderOverride;

        private static readonly string[] StandardNamespace = { "std" };
        private static readonly string[] NoNamespace = { };

        private const string IncludeCStdInt = "<cstdint>";
        private const string IncludeCStdDef = "<cstddef>";

        public static readonly CppType Int8 = CreatePrimitiveType(StandardNamespace, "int8_t", 0, IncludeCStdInt);
        public static readonly CppType Int16 = CreatePrimitiveType(StandardNamespace, "int16_t", 0, IncludeCStdInt);
        public static readonly CppType Int32 = CreatePrimitiveType(StandardNamespace, "int32_t", 0, IncludeCStdInt);
        public static readonly CppType Int64 = CreatePrimitiveType(StandardNamespace, "int64_t", 0, IncludeCStdInt);
        public static readonly CppType UInt16 = CreatePrimitiveType(StandardNamespace, "uint16_t", 0, IncludeCStdInt);
        public static readonly CppType UInt8 = CreatePrimitiveType(StandardNamespace, "uint8_t", 0, IncludeCStdInt);
        public static readonly CppType UInt32 = CreatePrimitiveType(StandardNamespace, "uint32_t", 0, IncludeCStdInt);
        public static readonly CppType UInt64 = CreatePrimitiveType(StandardNamespace, "uint64_t", 0, IncludeCStdInt);
        public static readonly CppType Boolean = CreatePrimitiveType(NoNamespace, "bool");
        public static readonly CppType Single = CreatePrimitiveType(NoNamespace, "float");
        public static readonly CppType Double = CreatePrimitiveType(NoNamespace, "double");
        public static readonly CppType VoidPointer = CreatePrimitiveType(NoNamespace, "void", CppTypeFlags.Pointer);
        public static readonly CppType Void = CreatePrimitiveType(NoNamespace, "void");
        public static readonly CppType NullPointer = CreatePrimitiveType(StandardNamespace, "nullptr_t", 0, IncludeCStdDef);

        public static CppType FromCSharp(CppGenerationContext context, ITypeSymbol type)
        {
            IPointerTypeSymbol? pointer = type as IPointerTypeSymbol;
            if (pointer != null)
            {
                CppType original = FromCSharp(context, pointer.PointedAtType);
                return original.AsPointer();
            }

            InteropTypeKind kind = Interop.DetermineTypeKind(context.Compilation, type);
            if (kind == InteropTypeKind.GenericParameter)
                return new CppType(InteropTypeKind.GenericParameter, NoNamespace, type.Name, null, 0);

            if (kind == InteropTypeKind.Primitive)
            {
                switch (type.SpecialType)
                {
                    case SpecialType.System_SByte:
                        return Int8;
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
                    case SpecialType.System_Byte:
                        return UInt8;
                    case SpecialType.System_UInt16:
                        return UInt16;
                    case SpecialType.System_UInt32:
                        return UInt32;
                    case SpecialType.System_UInt64:
                        return UInt64;
                    case SpecialType.System_Boolean:
                        return Boolean;
                    case SpecialType.System_IntPtr:
                        return VoidPointer;
                    case SpecialType.System_Void:
                        return Void;
                }
            }

            List<string> namespaces = new List<string>();

            INamespaceSymbol ns = type.ContainingNamespace;
            while (ns != null)
            {
                if (ns.Name.Length > 0)
                    namespaces.Add(ns.Name);
                ns = ns.ContainingNamespace;
            }

            if (context.BaseNamespace.Length > 0)
            {
                namespaces.Add(context.BaseNamespace);
            }

            namespaces.Reverse();

            // If the first two namespaces are identical, remove the duplication.
            // This is to avoid `Oxidize::Oxidize`.
            if (namespaces.Count >= 2 && namespaces[0] == namespaces[1])
                namespaces.RemoveAt(0);

            List<CppType>? genericArguments = null;

            string name = type.Name;

            INamedTypeSymbol? named = type as INamedTypeSymbol;
            if (named != null && named.IsGenericType)
            {
                genericArguments = named.TypeArguments.Select(symbol => CppType.FromCSharp(context, symbol)).ToList();

                // Add the number of generic arguments as a suffix, because C++ (unlike C#) doesn't make it
                // easy to overload based on the number.
                name += named.Arity;
            }

            return new CppType(kind, namespaces, name, genericArguments, 0);
        }

        public CppType(
            InteropTypeKind kind,
            IReadOnlyCollection<string> namespaces,
            string name,
            IReadOnlyCollection<CppType>? genericArguments,
            CppTypeFlags flags,
            string? headerOverride = null)
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

            if (headerOverride != null)
            {
                // If the header name is not wrapped in quotes or angle brackets, wrap it in quotes.
                if (!headerOverride.StartsWith("<") && !headerOverride.StartsWith("\""))
                    headerOverride = '"' + headerOverride + '"';
                this.HeaderOverride = headerOverride;
            }
        }

        public bool CanBeForwardDeclared
        {
            get
            {
                // TODO: currently any type that uses a custom header cannot be forward declared, but this may need more nuance.
                return this.HeaderOverride == null;
            }
        }

        public string GetFullyQualifiedNamespace(bool startWithGlobal = true)
        {
            string ns = string.Join("::", Namespaces);
            if (!startWithGlobal)
                return ns;

            if (ns.Length > 0)
                return "::" + ns;
            else
                return "";
        }

        public string GetFullyQualifiedName(bool startWithGlobal = true)
        {
            string template = "";
            if (this.GenericArguments != null && this.GenericArguments.Count > 0)
            {
                template = $"<{string.Join(", ", this.GenericArguments.Select(arg => arg.GetFullyQualifiedName()))}>";
            }

            string modifier = Flags.HasFlag(CppTypeFlags.Const) ? "const " : "";
            string suffix = Flags.HasFlag(CppTypeFlags.Pointer)
                ? "*"
                : Flags.HasFlag(CppTypeFlags.Reference)
                    ? "&"
                    : "";
            string ns = GetFullyQualifiedNamespace(startWithGlobal);
            if (ns.Length > 0)
                return $"{modifier}{ns}::{Name}{template}{suffix}";
            else
                return $"{modifier}{Name}{template}{suffix}";
        }

        public void AddForwardDeclarationsToSet(ISet<string> forwardDeclarations)
        {
            // Primitives do not need to be forward declared
            if (Kind == InteropTypeKind.Primitive)
                return;

            string template = "";
            if (this.GenericArguments != null && this.GenericArguments.Count > 0)
            {
                foreach (CppType genericType in this.GenericArguments)
                {
                    genericType.AddForwardDeclarationsToSet(forwardDeclarations);
                }

                template = $"template <{string.Join(", ", this.GenericArguments.Select((_, index) => $"typename T{index}"))}> ";
            }

            string ns = GetFullyQualifiedNamespace(false);
            if (ns != null)
            {
                string typeType;
                if (Kind == InteropTypeKind.BlittableStruct || Kind == InteropTypeKind.NonBlittableStructWrapper)
                    typeType = "struct";
                else if (Kind == InteropTypeKind.Enum)
                    typeType = "enum class";
                else
                    typeType = "class";
                forwardDeclarations.Add(
                    $$"""
                    namespace {{ns}} {
                    {{template}}{{typeType}} {{Name}};
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
            if (this.HeaderOverride != null)
            {
                includes.Add(this.HeaderOverride);
                return;
            }

            if (Kind == InteropTypeKind.Primitive)
            {
                // Special case for primitives in <cstdint>.
                if (Namespaces == StandardNamespace)
                {
                    includes.Add(IncludeCStdInt);
                }
                return;
            }

            if (this.GenericArguments != null && this.GenericArguments.Count > 0)
            {
                foreach (CppType genericType in this.GenericArguments)
                {
                    genericType.AddIncludesToSet(includes, forHeader);
                }

            }

            bool canBeForwardDeclared = true; // Flags.HasFlag(CppTypeFlags.Reference) || Flags.HasFlag(CppTypeFlags.Pointer);
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

        public CppType AsPointer()
        {
            return new CppType(Kind, Namespaces, Name, GenericArguments, Flags | CppTypeFlags.Pointer, HeaderOverride);
        }

        public CppType AsReference()
        {
            return new CppType(Kind, Namespaces, Name, GenericArguments, Flags | CppTypeFlags.Reference, HeaderOverride);
        }

        public CppType AsConstReference()
        {
            return new CppType(Kind, Namespaces, Name, GenericArguments, Flags | CppTypeFlags.Const | CppTypeFlags.Reference & ~CppTypeFlags.Pointer, HeaderOverride);
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
                case InteropTypeKind.ClassWrapper:
                case InteropTypeKind.BlittableStruct:
                case InteropTypeKind.NonBlittableStructWrapper:
                case InteropTypeKind.Delegate:
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
            if (this.Kind == InteropTypeKind.Primitive || this.Kind == InteropTypeKind.BlittableStruct)
                return this;
            else if (this.Kind == InteropTypeKind.Enum)
                return UInt32;

            return VoidPointer;
        }

        /// <summary>
        /// Gets an expression that converts this type to the
        /// {@link AsInteropType}.
        /// </summary>
        public string GetConversionToInteropType(CppGenerationContext context, string variableName)
        {
            switch (this.Kind)
            {
                case InteropTypeKind.ClassWrapper:
                case InteropTypeKind.NonBlittableStructWrapper:
                case InteropTypeKind.Delegate:
                    return $"{variableName}.GetHandle().GetRaw()";
                case InteropTypeKind.Enum:
                    return $"::std::uint32_t({variableName})";
                case InteropTypeKind.Primitive:
                case InteropTypeKind.BlittableStruct:
                case InteropTypeKind.Unknown:
                default:
                    return variableName;
            }
        }

        public string GetConversionFromInteropType(CppGenerationContext context, string variableName)
        {
            switch (this.Kind)
            {
                case InteropTypeKind.ClassWrapper:
                case InteropTypeKind.NonBlittableStructWrapper:
                case InteropTypeKind.Delegate:
                    return $"{GetFullyQualifiedName()}({CppObjectHandle.GetCppType(context).GetFullyQualifiedName()}({variableName}))";
                case InteropTypeKind.Enum:
                    return $"{this.GetFullyQualifiedName()}({variableName})";
                case InteropTypeKind.Primitive:
                case InteropTypeKind.BlittableStruct:
                case InteropTypeKind.Unknown:
                default:
                    return variableName;
            }
        }

        private static CppType CreatePrimitiveType(IReadOnlyCollection<string> cppNamespaces, string cppTypeName, CppTypeFlags flags = 0, string? headerOverride = null)
        {
            return new CppType(InteropTypeKind.Primitive, cppNamespaces, cppTypeName, null, flags, headerOverride);
        }
    }
}
