using Microsoft.CodeAnalysis;

namespace Reinterop
{
    [Flags]
    internal enum CppTypeFlags
    {
        Pointer = 1,
        Reference = 2,
        Const = 4,
        DoublePointer = 9, // A double pointer is also a pointer
        RValueReference = 18, // An r-value reference is also a reference
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
        private static readonly string[] FlagsNamespace = { "flags" };
        private static readonly string[] NoNamespace = { };

        private const string IncludeCStdInt = "<cstdint>";
        private const string IncludeCStdDef = "<cstddef>";
        private const string IncludeEnumFlags = "<flags/flags.hpp>";

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
        public static readonly CppType VoidPointerPointer = CreatePrimitiveType(NoNamespace, "void", CppTypeFlags.DoublePointer);
        public static readonly CppType Void = CreatePrimitiveType(NoNamespace, "void");
        public static readonly CppType NullPointer = CreatePrimitiveType(StandardNamespace, "nullptr_t", 0, IncludeCStdDef);

        public static CppType FromCSharp(CppGenerationContext context, CSharpType type)
        {
            if (type.Name == "Nullable" && type.TypeArguments.Count == 1)
            {
                CppType nullabledType = CppType.FromCSharp(context, type.TypeArguments[0]);
                if (nullabledType.Kind == InteropTypeKind.BlittableStruct || nullabledType.Kind == InteropTypeKind.Primitive)
                    return new CppType(InteropTypeKind.Nullable, [ "std" ], "optional", [ nullabledType ], 0, "<optional>");
            }

            IArrayTypeSymbol? arrayType = type as IArrayTypeSymbol;
            if (type.ArrayElementType != null)
            {
                CppType original = FromCSharp(context, type.ArrayElementType);
                return new CppType(InteropTypeKind.ClassWrapper, Interop.BuildNamespace(context.BaseNamespace, "System"), "Array1", [ original ], 0);
            }

            if (type.Kind == InteropTypeKind.GenericParameter)
                return new CppType(InteropTypeKind.GenericParameter, NoNamespace, type.Name, null, 0);

            if (type.Kind == InteropTypeKind.Primitive)
            {
                CppType? specialType = null;
                switch (type.SpecialType)
                {
                    case SpecialType.System_SByte:
                        specialType = Int8;
                        break;
                    case SpecialType.System_Int16:
                        specialType = Int16;
                        break;
                    case SpecialType.System_Int32:
                        specialType = Int32;
                        break;
                    case SpecialType.System_Int64:
                        specialType = Int64;
                        break;
                    case SpecialType.System_Single:
                        specialType = Single;
                        break;
                    case SpecialType.System_Double:
                        specialType = Double;
                        break;
                    case SpecialType.System_Byte:
                        specialType = UInt8;
                        break;
                    case SpecialType.System_UInt16:
                        specialType = UInt16;
                        break;
                    case SpecialType.System_UInt32:
                        specialType = UInt32;
                        break;
                    case SpecialType.System_UInt64:
                        specialType = UInt64;
                        break;
                    case SpecialType.System_Boolean:
                        specialType = Boolean;
                        break;
                    case SpecialType.System_IntPtr:
                        specialType = VoidPointer;
                        break;
                    case SpecialType.System_Void:
                        specialType = Void;
                        break;
                }
                if (specialType != null)
                {
                    if (type.Flags.HasFlag(CSharpTypeFlags.Pointer))
                        specialType = specialType.AsPointer();
                    return specialType;
                }
            }

            List<string> namespaces = [.. type.Namespaces];

            if (context.BaseNamespace.Length > 0)
                namespaces.Insert(0, context.BaseNamespace);

            // If the first two namespaces are identical, remove the duplication.
            // This is to avoid `Reinterop::Reinterop`.
            if (namespaces.Count >= 2 && namespaces[0] == namespaces[1])
                namespaces.RemoveAt(0);

            List<CppType>? genericArguments = null;

            string name = type.Name;

            if (type.TypeArguments.Count > 0 || (type.ContainingType != null && type.ContainingType.TypeArguments.Count > 0))
            {
                genericArguments = type.TypeArguments.Select(t => CppType.FromCSharp(context, t)).ToList();

                // Add the number of generic arguments as a suffix, because C++ (unlike C#) doesn't make it
                // easy to overload based on the number.
                name += type.TypeArguments.Count;
            }

            CppTypeFlags flags = 0;
            if (type.Flags.HasFlag(CSharpTypeFlags.Pointer))
                flags |= CppTypeFlags.Pointer;

            return new CppType(type.Kind, namespaces, name, genericArguments, flags);
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
            return startWithGlobal ? "::" + ns : ns;
        }

        public string GetFullyQualifiedName(bool startWithGlobal = true)
        {
            string template = "";
            if (this.GenericArguments != null && this.GenericArguments.Count > 0)
            {
                template = $"<{string.Join(", ", this.GenericArguments.Select(arg => arg.GetFullyQualifiedName()))}>";
            }

            string modifier = Flags.HasFlag(CppTypeFlags.Const) ? "const " : "";
            string suffix = "";
            if (Flags.HasFlag(CppTypeFlags.DoublePointer))
                suffix = "**";
            else if (Flags.HasFlag(CppTypeFlags.Pointer))
                suffix = "*";
            else if (Flags.HasFlag(CppTypeFlags.RValueReference))
                suffix = "&&";
            else if (Flags.HasFlag(CppTypeFlags.Reference))
                suffix = "&";
            
            // Template parameters and unqualified primitives (void, bool, float, etc.) cannot be globally qualified, i.e., `::void`, `::bool`, or `::T`.
            if (Namespaces.Count == 0 && (Kind == InteropTypeKind.Primitive || Kind == InteropTypeKind.GenericParameter))
                return $"{modifier}{Name}{template}{suffix}";

            string ns = GetFullyQualifiedNamespace(startWithGlobal);
            if (ns.EndsWith("::") || ns.Length == 0)
                return $"{modifier}{ns}{Name}{template}{suffix}";
            else
                return $"{modifier}{ns}::{Name}{template}{suffix}";
        }

        public void AddForwardDeclarationsToSet(ISet<string> forwardDeclarations)
        {
            // Primitives and generic parameters do not need to be forward declared
            if (Kind == InteropTypeKind.Primitive || Kind == InteropTypeKind.GenericParameter)
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
                string suffix = "";
                if (Kind == InteropTypeKind.BlittableStruct || Kind == InteropTypeKind.NonBlittableStructWrapper)
                    typeType = "struct";
                else if (Kind == InteropTypeKind.Enum)
                    typeType = "enum class";
                else if (Kind == InteropTypeKind.EnumFlags) {
                    typeType = "enum class";
                    // TODO: What if the original C# enum was some other 
                    // integral type though?
                    suffix = " : uint32_t";
                } else
                    typeType = "class";
                forwardDeclarations.Add(
                    $$"""
                    namespace {{ns}} {
                    {{template}}{{typeType}} {{Name}}{{suffix}};
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
            if (this.GenericArguments != null && this.GenericArguments.Count > 0)
            {
                foreach (CppType genericType in this.GenericArguments)
                {
                    genericType.AddIncludesToSet(includes, forHeader);
                }

            }

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

        public CppType AsRValueReference()
        {
            return new CppType(Kind, Namespaces, Name, GenericArguments, Flags | CppTypeFlags.RValueReference & ~CppTypeFlags.Const, HeaderOverride);
        }

        public CppType AsConstPointer()
        {
            return new CppType(Kind, Namespaces, Name, GenericArguments, Flags | CppTypeFlags.Const | CppTypeFlags.Pointer & ~CppTypeFlags.Reference, HeaderOverride);
        }

        public CppType AsEnumFlags() {
          return new CppType(InteropTypeKind.EnumFlags, FlagsNamespace, "flags", new CppType[]{ this }, 0, IncludeEnumFlags);
        }

        /// <summary>
        /// Gets a version of this type suitable for use as the return value
        /// of a wrapped method. This simply returns the type unmodified.
        /// </summary>
        public CppType AsReturnType()
        {
            if (this.Kind == InteropTypeKind.EnumFlags)
                return this.AsEnumFlags();

            // All other types are returned by value.
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
                case InteropTypeKind.Nullable:
                    return this.AsConstReference();
                case InteropTypeKind.GenericParameter:
                    // TODO: ideally, we wouldn't pass primitives by const reference, but
                    // we can't easily tell from the generic parameter. So just pass all parameters
                    // of generic type by const reference.
                    return this.AsConstReference();
                case InteropTypeKind.EnumFlags:
                    // Allows enums to be combined together as flags.
                    return this.AsEnumFlags();
            }

            return this;
        }

        /// <summary>
        /// Gets a version of this type suitable for use as a wrapped function
        /// parameter that can be std::move'd. For classes and structs, this returns an
        /// r-value reference (`&&`) to the type.
        /// </summary>
        public CppType AsMovableParameterType()
        {
            switch (this.Kind)
            {
                case InteropTypeKind.ClassWrapper:
                case InteropTypeKind.BlittableStruct:
                case InteropTypeKind.NonBlittableStructWrapper:
                case InteropTypeKind.Delegate:
                case InteropTypeKind.Nullable:
                    return this.AsRValueReference();
                case InteropTypeKind.GenericParameter:
                    // TODO: ideally, we wouldn't pass primitives by reference, but
                    // we can't easily tell from the generic parameter. So just pass all parameters
                    // of generic type by reference.
                    return this.AsRValueReference();
                case InteropTypeKind.EnumFlags:
                    // Allows enums to be combined together as flags.
                    return this.AsEnumFlags();
            }

            return this;
        }

        /// <summary>
        /// Gets a version of this type without any const, pointer, or reference qualifications.
        /// </summary>
        public CppType AsSimpleType()
        {
            if (this.Flags != 0)
                return new CppType(this.Kind, this.Namespaces, this.Name, this.GenericArguments, 0, this.HeaderOverride);
            else
                return this;
        }

        /// <summary>
        /// Gets the version of this type that should be used in a function
        /// pointer that will call into the managed side.
        /// </summary>
        public CppType AsInteropType()
        {
            if (this == Boolean)
            {
                // C++ doesn't specify the size of a bool, and C# uses
                // different sizes in different contexts. So we explicitly
                // marshal bools as uint8_t.
                return UInt8;
            }
            else if (this.Kind == InteropTypeKind.Primitive)
                return this;
            else if (this.Kind == InteropTypeKind.BlittableStruct)
            {
                // If this is a parameter, it will be a const reference; turn it into a const pointer.
                // Otherwise, it's a return value; just return the simple type for now.
                if (this.Flags.HasFlag(CppTypeFlags.Const) && this.Flags.HasFlag(CppTypeFlags.Reference))
                    return this.AsConstPointer();
                return this.AsSimpleType();
            }
            else if (this.Kind == InteropTypeKind.Nullable)
            {
                // If this is a parameter, it will be a const reference; turn it into a const pointer.
                // Otherwise, it's a return value; just return the simple type for now.
                CppType underlying = this.GenericArguments.FirstOrDefault();
                if (this.Flags.HasFlag(CppTypeFlags.Const) && this.Flags.HasFlag(CppTypeFlags.Reference))
                    return underlying.AsConstPointer();
                return underlying.AsSimpleType();
            }
            else if (this.Kind == InteropTypeKind.Enum || this.Kind == InteropTypeKind.EnumFlags)
                return UInt32;

            return VoidPointer;
        }

        /// <summary>
        /// Gets an expression that converts this type to the
        /// {@link AsInteropType}.
        /// </summary>
        public string GetConversionToInteropType(CppGenerationContext context, string variableName)
        {
            if (this == Boolean)
            {
                return $"{variableName} ? 1 : 0";
            }

            switch (this.Kind)
            {
                case InteropTypeKind.ClassWrapper:
                case InteropTypeKind.NonBlittableStructWrapper:
                case InteropTypeKind.Delegate:
                    // If this is a reference, we can count on it continuing to
                    // exist for the duration of the relevant function call, so just
                    // get the raw handle.
                    //
                    // But if it's not a reference, this is a temporary variable storing
                    // a return value, and the handle value must outlive the ObjectHandle
                    // instance. So, release it from this instance.
                    if (this.Flags.HasFlag(CppTypeFlags.Reference))
                        return $"{variableName}.GetHandle().GetRaw()";
                    else
                        return $"{variableName}.GetHandle().Release()";
                case InteropTypeKind.Enum:
                    return $"::std::uint32_t({variableName})";
                case InteropTypeKind.EnumFlags:
                    return $"{variableName}.underlying_value()";
                case InteropTypeKind.Primitive:
                case InteropTypeKind.BlittableStruct:
                    if (this.Flags.HasFlag(CppTypeFlags.Reference))
                        return $"&{variableName}";
                    else
                        return variableName;
                case InteropTypeKind.Nullable:
                    if (this.Flags.HasFlag(CppTypeFlags.Reference))
                        return $"{variableName}.has_value() ? &{variableName}.value() : nullptr";
                    else
                        return variableName;
                case InteropTypeKind.Unknown:
                default:
                    return variableName;
            }
        }

        /// <summary>
        /// Gets an expression that converts this type to the
        /// {@link AsInteropType}.
        /// </summary>
        public CppExpression GetConversionToInteropTypeExpression(CppGenerationContext context, string variableName)
        {
            if (this == Boolean)
            {
                return new CppTernary(
                    new CppIdentifier(variableName),
                    new CppLiteral("1"),
                    new CppLiteral("0")
                );
            }

            switch (this.Kind)
            {
                case InteropTypeKind.ClassWrapper:
                case InteropTypeKind.NonBlittableStructWrapper:
                case InteropTypeKind.Delegate:
                    // If this is a reference, we can count on it continuing to
                    // exist for the duration of the relevant function call, so just
                    // get the raw handle.
                    //
                    // But if it's not a reference, this is a temporary variable storing
                    // a return value, and the handle value must outlive the ObjectHandle
                    // instance. So, release it from this instance.
                    CppExpression handleExpression = new CppCall(new CppMemberAccess(new CppIdentifier(variableName), "GetHandle"), []);
                    if (this.Flags.HasFlag(CppTypeFlags.Reference))
                    {
                        CppExpression rawExpression = new CppMemberAccess(handleExpression, "GetRaw");
                        return new CppCall(rawExpression, []);
                    }
                    else
                    {
                        CppExpression releaseExpression = new CppMemberAccess(handleExpression, "Release");
                        return new CppCall(releaseExpression, []);
                    }   
                case InteropTypeKind.Enum:
                    return new CppCast(CppType.UInt32, new CppIdentifier(variableName));
                case InteropTypeKind.EnumFlags:
                    return new CppCall(new CppMemberAccess(new CppIdentifier(variableName), "underlying_value"), []);
                case InteropTypeKind.Primitive:
                case InteropTypeKind.BlittableStruct:
                    if (this.Flags.HasFlag(CppTypeFlags.Reference))
                        return new CppUnary("&", new CppIdentifier(variableName));
                    else
                        return new CppIdentifier(variableName);
                case InteropTypeKind.Nullable:
                    if (this.Flags.HasFlag(CppTypeFlags.Reference))
                        return new CppTernary(
                            new CppCall(new CppMemberAccess(new CppIdentifier(variableName), "has_value"), []),
                            new CppUnary("&", new CppCall(new CppMemberAccess(new CppIdentifier(variableName), "value"), [])),
                            new CppIdentifier("nullptr")
                        );
                    else
                        return new CppIdentifier(variableName);
                case InteropTypeKind.Unknown:
                default:
                    return new CppIdentifier(variableName);
            }
        }

        public string GetConversionFromInteropType(CppGenerationContext context, string variableName)
        {
            if (this == Boolean)
            {
                return $"!!{variableName}";
            }

            switch (this.Kind)
            {
                case InteropTypeKind.ClassWrapper:
                case InteropTypeKind.NonBlittableStructWrapper:
                case InteropTypeKind.Delegate:
                    return $"{this.AsSimpleType().GetFullyQualifiedName()}({CppObjectHandle.GetCppType(context).GetFullyQualifiedName()}({variableName}))";
                case InteropTypeKind.Enum:
                    return $"{this.AsSimpleType().GetFullyQualifiedName()}({variableName})";
                case InteropTypeKind.EnumFlags:
                    return $"{this.GenericArguments.ElementAt(0).AsSimpleType().GetFullyQualifiedName()}({variableName})";
                case InteropTypeKind.Nullable:
                    if (this.Flags.HasFlag(CppTypeFlags.Reference))
                        // parameter
                        return $"{variableName} == nullptr ? std::nullopt : std::make_optional(*{variableName})";
                    else
                        // return value
                        return variableName;
                case InteropTypeKind.BlittableStruct:
                    if (this.Flags.HasFlag(CppTypeFlags.Reference))
                        // parameter
                        return $"*{variableName}";
                    else
                        // return value
                        return variableName;
                case InteropTypeKind.Primitive:
                case InteropTypeKind.Unknown:
                default:
                    return variableName;
            }
        }

        public CppExpression GetConversionFromInteropTypeExpression(CppGenerationContext context, CppExpression inputExpression)
        {
            if (this == Boolean)
            {
                return new CppUnary("!", new CppUnary("!", inputExpression));
            }

            switch (this.Kind)
            {
                case InteropTypeKind.ClassWrapper:
                case InteropTypeKind.NonBlittableStructWrapper:
                case InteropTypeKind.Delegate:
                    CppCall constructHandle = new CppCall(new CppIdentifier(CppObjectHandle.GetCppType(context).GetFullyQualifiedName()), [inputExpression]);
                    return new CppCall(new CppIdentifier(this.AsSimpleType().GetFullyQualifiedName()), [constructHandle]);
                case InteropTypeKind.Enum:
                    return new CppCall(new CppIdentifier(this.AsSimpleType().GetFullyQualifiedName()), [inputExpression]);
                case InteropTypeKind.EnumFlags:
                    return new CppCall(new CppIdentifier(this.GenericArguments.ElementAt(0).AsSimpleType().GetFullyQualifiedName()), [inputExpression]);
                case InteropTypeKind.Nullable:
                    if (this.Flags.HasFlag(CppTypeFlags.Reference))
                        // parameter
                        return new CppTernary(
                            new CppBinary("==", inputExpression, new CppRaw("nullptr")),
                            new CppRaw("std::nullopt"),
                            new CppCall(new CppIdentifier("std::make_optional"), [new CppUnary("*", inputExpression)])
                        );
                    else
                        // return value
                        return inputExpression;
                case InteropTypeKind.BlittableStruct:
                    if (this.Flags.HasFlag(CppTypeFlags.Reference))
                        // parameter
                        return new CppUnary("*", inputExpression);
                    else
                        // return value
                        return inputExpression;
                case InteropTypeKind.Primitive:
                case InteropTypeKind.Unknown:
                default:
                    return inputExpression;
            }
        }

        private static CppType CreatePrimitiveType(IReadOnlyCollection<string> cppNamespaces, string cppTypeName, CppTypeFlags flags = 0, string? headerOverride = null)
        {
            return new CppType(InteropTypeKind.Primitive, cppNamespaces, cppTypeName, null, flags, headerOverride);
        }
    }
}
