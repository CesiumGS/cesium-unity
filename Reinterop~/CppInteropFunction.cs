namespace Reinterop
{
    /// <summary>
    /// One parameter of a wrapped, interop-backed C++ function, as seen by the C++ caller (i.e. its
    /// type has already been run through <see cref="CppType.AsParameterType"/>).
    /// </summary>
    internal record CppInteropParameter(string Name, CppType Type, CppExpression CallSite)
    {
        public CppInteropParameter(string name, CppType type) : this(name, type, new CppIdentifier(name))
        {
        }
    }

    /// <summary>
    /// Describes an interop-backed C++ function - a wrapped method, property accessor, constructor, or
    /// field accessor - purely in terms of its parameters and return type, and derives everything
    /// needed to call across to the managed (C#) side and back: the interop parameter list (accounting
    /// for the implicit "thiz" and "reinteropException" parameters and the struct-return rewrite), the
    /// call arguments, and the call+return body.
    /// </summary>
    internal class CppInteropFunction
    {
        public CppGenerationContext Context { get; }

        public string Name { get; }

        /// <summary>The name of the interop function pointer field, e.g. "CallFoo_1a2b3c" or "Field_get_Bar".</summary>
        public string FunctionPointerName
        {
            get
            {
                string safeName = Interop.MakeSafeIdentifier(Name);
                return $"Call_{safeName}_{Interop.HashParameters(Parameters(), TypeArguments())}";
            }
        }

        /// <summary>
        /// The C++ type that owns this function.
        /// </summary>
        public CppType Owner { get; }

        private List<CppInteropParameter> _typeParameters = new List<CppInteropParameter>();

        /// <summary>
        /// Gets or sets the generic type parameters to this function. If this is not a generic function, the list is empty.
        /// </summary>
        public List<CppInteropParameter> TypeParameters() { return _typeParameters; }

        /// <summary>
        /// Sets the generic type parameters to this function. If this is not a generic function, the list should be empty.
        /// </summary>
        public CppInteropFunction TypeParameters(IEnumerable<CppInteropParameter> typeParameters)
        {
            _typeParameters = typeParameters.ToList();
            return this;
        }

        private List<CppType> _typeArguments = new List<CppType>();

        /// <summary>
        /// Gets or sets the generic type arguments to this function. These are the types that fill in the
        /// <see cref="TypeParameters"/> in the instantiated generic.
        /// </summary>
        public List<CppType> TypeArguments() { return _typeArguments; }

        /// <summary>
        /// Sets the generic type arguments to this function. These are the types that fill in the
        /// <see cref="TypeParameters"/> in the instantiated generic.
        /// </summary>
        public CppInteropFunction TypeArguments(IEnumerable<CppType> typeArguments)
        {
            _typeArguments = typeArguments.ToList();
            return this;
        }

        private List<CppInteropParameter> _parameters = new List<CppInteropParameter>();

        /// <summary>
        /// Gets the parameters to the function.
        /// </summary>
        public List<CppInteropParameter> Parameters() { return _parameters; }
        
        /// <summary>
        /// Sets the parameters to the function. The underlying <see cref="CppType"/> should almost always
        /// by the result of calling <see cref="CppType.AsParameterType"/>. The implicit "this" parameter should
        /// _not_ be included.
        /// </summary>
        public CppInteropFunction Parameters(IEnumerable<CppInteropParameter> parameters)
        {
            _parameters = parameters.ToList();
            return this;
        }

        private CppType _returnType = CppType.Void;

        /// <summary>
        /// Gets the function's return type. This should almost always be the result of
        /// calling <see cref="CppType.AsReturnType"/>.
        /// </summary>
        public CppType ReturnType() { return _returnType; }

        /// <summary>
        /// Sets the function's return type. This should almost always be the result of
        /// calling <see cref="CppType.AsReturnType"/>.
        /// </summary>
        public CppInteropFunction ReturnType(CppType returnType)
        {
            _returnType = returnType;
            return this;
        }

        private bool _static = false;

        /// <summary>
        /// Gets a value indicating whether this is a static member. `true` if it is static, `false` if it is an
        /// instance member that must be called with a "this" pointer. Note that constructors are considered
        /// static, since they don't have a "this" pointer yet.
        /// </summary>
        public bool Static() { return _static; }

        /// <summary>
        /// Sets a value indicating whether this is a static member. `true` if it is static, `false` if it is an
        /// instance member that must be called with a "this" pointer. Note that constructors are considered
        /// static, since they don't have a "this" pointer yet.
        /// </summary>
        public CppInteropFunction Static(bool isStatic = true)
        {
            _static = isStatic;
            return this;
        }

        private bool _private = false;

        /// <summary>
        /// Gets a value indicating whether this function is private. This controls whether the function's own declaration (not the interop function pointer field,
        /// which is always private) is marked as private in the generated C++ code.
        /// </summary>
        public bool Private() { return _private; }

        /// <summary>
        /// Sets a value indicating whether this function is private. This controls whether the function's own declaration (not the interop function pointer field,
        /// which is always private) is marked as private in the generated C++ code.
        /// </summary>
        public CppInteropFunction Private(bool isPrivate = true)
        {
            _private = isPrivate;
            return this;
        }

        private CppInteropFunction? _specializes = null;

        /// <summary>
        /// Gets the generic function that this function specializes, if any. Specializations don't get their own declaration, but instead
        /// reuse the template declaration of the generic function.
        /// </summary>
        public CppInteropFunction? Specializes() { return _specializes; }

        /// <summary>
        /// Sets the generic function that this function specializes, if any. Specializations don't get their own declaration, but instead
        /// reuse the template declaration of the generic function.
        /// </summary>
        public CppInteropFunction Specializes(CppInteropFunction? specializes)
        {
            _specializes = specializes;
            return this;
        }

        /// <summary>
        /// Determines if this function's return type necessitates rewriting the interop function to return via an out-parameter instead of directly.
        /// </summary>
        public bool NeedsStructReturnRewrite
        {
            get { return Interop.NeedsStructReturnRewrite(ReturnType()); }
        }

        /// <summary>
        /// Gets the return type of the interop function pointer. Usually this is simply `ReturnType().AsInteropType()`, but
        /// if `NeedsStructReturnRewrite` is true, this will instead be `CppType.Void` (for blittable structs) or `CppType.UInt8` (for nullable
        /// blittable structs and primitives), and the interop function will return via an out-parameter of type `ReturnType()`.
        /// </summary>
        public CppType InteropReturnType
        {
            get
            {
                if (!NeedsStructReturnRewrite)
                    return ReturnType().AsInteropType();

                return ReturnType().Kind == InteropTypeKind.Nullable
                    ? CppType.UInt8 // For the true/false indicating whether the nullable has a value.
                    : CppType.Void;
            }
        }

        /// <summary>
        /// Gets the full interop parameter list, in call order. These are the parameters to the C++ function pointer created from the C#
        /// delegate. It includes the implicit "thiz" (if this is a non-static function), then <see cref="Parameters"/> (as their
        /// <see cref="CppType.AsInteropType"/>), then "pReturnValue" (if <see cref="NeedsStructReturnRewrite"/>), then "reinteropException".
        /// </summary>
        private IReadOnlyList<(string ParameterName, string CallSiteName, CppType Type, CppType InteropType)> InteropParameters
        {
            get
            {
                IEnumerable<CppInteropParameter> allParameters = Parameters();
                if (!Static())
                    allParameters = new[] { new CppInteropParameter("thiz", Owner.AsParameterType(), new CppRaw("(*this)")) }.Concat(allParameters);

                IEnumerable<(string ParameterName, string CallSiteName, CppType Type, CppType InteropType)> interopParameters =
                    allParameters.Select(parameter => (
                        ParameterName: parameter.Name,
                        CallSiteName: CppPrinter.Print(parameter.CallSite),
                        Type: parameter.Type,
                        InteropType: parameter.Type.AsInteropType()));

                CppType returnType = ReturnType();
                CppType interopReturnType = returnType.AsInteropType();
                bool hasStructRewrite = Interop.RewriteStructReturn(ref interopParameters, ref returnType, ref interopReturnType);

                return interopParameters
                    .Concat(new[] { (ParameterName: "reinteropException", CallSiteName: "", Type: CppType.VoidPointerPointer, InteropType: CppType.VoidPointerPointer) })
                    .ToArray();
            }
        }

        /// <summary>
        /// Gets the interop types of the <see cref="Parameters"/>. This is before implicit parameters such as "thiz" and "reinteropException" are added,
        /// and before the struct return rewrite (if there is one).
        /// </summary>
        public IEnumerable<CppType> ParameterInteropTypes => Parameters().Select(parameter => parameter.Type.AsInteropType());

        /// <summary>
        /// The interop function pointer's parameter list, e.g. "void* thiz, std::int32_t x, void** reinteropException".
        /// </summary>
        private string InteropParameterListDeclaration() =>
            string.Join(", ", InteropParameters.Select(parameter => $"{parameter.InteropType.GetFullyQualifiedName()} {parameter.ParameterName}"));

        public CppInteropFunction(
            CppGenerationContext context,
            CppType owner,
            string name)
        {
            this.Context = context;
            this.Owner = owner;
            this.Name = name;
        }

        /// <summary>The wrapped function's own parameter list, e.g. "int x, float y".</summary>
        public string ParameterListDeclaration() =>
            string.Join(", ", Parameters().Select(parameter => $"{parameter.Type.GetFullyQualifiedName()} {parameter.Name}"));

        /// <summary>The interop function pointer's parameter *types* only (no names), e.g. for a function pointer type signature.</summary>
        private string InteropParameterTypeList() =>
            string.Join(", ", InteropParameters.Select(parameter => parameter.InteropType.GetFullyQualifiedName()));

        /// <summary>The types referenced by <see cref="Parameters"/>, for TypeDeclarationsReferenced.</summary>
        public IEnumerable<CppType> ParameterTypes => Parameters().Select(parameter => parameter.Type);

        // /// <summary>The types referenced by <see cref="InteropParameters"/>, for TypeDeclarationsReferenced.</summary>
        // public IEnumerable<CppType> InteropParameterTypes => InteropParameters.Select(parameter => parameter.InteropType);

        /// <summary>
        /// The call arguments to pass to <see cref="CppInterop.CallManagedFunction"/>: each parameter's
        /// value converted to its interop type, or (for a struct-return rewrite) an out-parameter of
        /// type <paramref name="outParameterTypeName"/>.
        /// </summary>
        public IReadOnlyList<CppArgument> CallArguments(string? outParameterTypeName = null) =>
            InteropParameters
                .Where(parameter => parameter.ParameterName != "reinteropException")
                .Select(parameter => parameter.ParameterName == "pReturnValue"
                    ? CppArgument.OutParameter(outParameterTypeName ?? ReturnType().GetFullyQualifiedName(), "result")
                    : CppArgument.Value(parameter.Type.GetConversionToInteropType(Context, parameter.CallSiteName)))
                .ToArray();

        /// <summary>
        /// Builds the call+return body, automatically choosing between a void call, a value-returning
        /// call, a struct-return-rewrite call (with the result produced via an out-parameter of type
        /// <paramref name="outParameterTypeName"/>, which defaults to <see cref="ReturnType"/> itself),
        /// and a Nullable-wrapped struct-return-rewrite call (which additionally returns a "resultIsValid"
        /// flag, becoming "resultIsValid ? std::make_optional(...) : std::nullopt").
        /// </summary>
        public IReadOnlyList<CppStatement> Body(string? outParameterTypeName = null, string resultVariableName = "result")
        {
            CppExpression functionPointer = new CppIdentifier(FunctionPointerName);

            if (NeedsStructReturnRewrite && ReturnType().Kind == InteropTypeKind.Nullable)
            {
                CppType elementType = ReturnType().GenericArguments!.First();
                IReadOnlyList<CppArgument> nullableArguments = CallArguments(outParameterTypeName ?? elementType.GetFullyQualifiedName());
                string convertedResult = ReturnType().GetConversionFromInteropType(Context, resultVariableName);
                return CppInterop.CallManagedFunction(
                    functionPointer, nullableArguments,
                    resultTypeName: "auto",
                    returnExpression: new CppRaw($"resultIsValid ? std::make_optional(std::move({convertedResult})) : std::nullopt"),
                    resultVariableName: "resultIsValid");
            }

            IReadOnlyList<CppArgument> arguments = CallArguments(outParameterTypeName);

            bool isVoid = ReturnType().Name == "void" && !ReturnType().Flags.HasFlag(CppTypeFlags.Pointer);
            if (isVoid)
                return CppInterop.CallManagedFunction(functionPointer, arguments);

            return CppInterop.CallManagedFunction(
                functionPointer, arguments,
                resultTypeName: NeedsStructReturnRewrite ? null : "auto",
                returnExpression: new CppRaw(ReturnType().GetConversionFromInteropType(Context, resultVariableName)),
                resultVariableName: resultVariableName);
        }

        /// <summary>
        /// Gets whether this function is an unspecialized generic, i.e., it has generic <see cref="TypeParameters"/>
        /// but any of the corresponding <see cref="TypeArguments"/> are just a
        /// <see cref="InteropTypeKind.GenericParameter"/>. We can't generate interop for an unspecialized generic, but
        /// we can still add its declaration to the generation and we can add interop for any specializations.
        /// </summary>
        public bool IsUnspecializedGeneric => TypeArguments().Any(t => t.Kind == InteropTypeKind.GenericParameter);

        /// <summary>
        /// Adds everything needed to expose this interop function: the interop function pointer field
        /// (declaration, out-of-line definition initialized to nullptr, and startup init registration),
        /// the wrapped function's own declaration (unless <see cref="WithoutDeclaration"/> was used), and
        /// its definition.
        /// </summary>
        public void AddToGeneration(
            GeneratedResult result,
            string? csharpName,
            string? csharpContent,
            IReadOnlyList<CppStatement>? body)
        {
            if (!IsUnspecializedGeneric && csharpName != null && csharpContent != null)
                AddInteropFunctionPointer(result, result.CppDefinition.Type.GetFullyQualifiedName(false), csharpName, csharpContent);

            // Don't add a declaration for a specialization. Use the generic template declaration instead.
            if (Specializes() == null)
                AddDeclaration(result);

            if (!IsUnspecializedGeneric && body != null)
                AddDefinition(result, body);
        }

        /// <summary>
        /// Declares the private static interop function pointer field, defines it (initialized to
        /// nullptr), and registers it for initialization at startup - the boilerplate that's identical,
        /// aside from a per-call-site convention or two, for every method, property accessor,
        /// constructor, and field accessor. Exposed separately (rather than only through
        /// <see cref="AddToGeneration"/>) for constructors, whose own declaration/definition don't fit
        /// that method's shape (no return type, and an initializer-list body).
        /// </summary>
        /// <param name="qualifiedDefinitionName">
        /// The type name to qualify the out-of-line field pointer definition with, e.g.
        /// "MyNamespace::MyClass" or (for constructors of generic types) "MyClass&lt;T&gt;".
        /// </param>
        public void AddInteropFunctionPointer(
            GeneratedResult result,
            string qualifiedDefinitionName,
            string csharpName,
            string csharpContent)
        {
            GeneratedCppDeclaration declaration = result.CppDeclaration;
            GeneratedCppDefinition definition = result.CppDefinition;
            GeneratedInit init = result.Init;

            IEnumerable<CppType> fieldPointerTypes = new[] { InteropReturnType }.Concat(ParameterInteropTypes);

            declaration.Elements.Add(new(
                Content: $"static {InteropReturnType.GetFullyQualifiedName()} (*{FunctionPointerName})({InteropParameterListDeclaration()});",
                IsPrivate: true,
                TypeDeclarationsReferenced: fieldPointerTypes));

            definition.Elements.Add(new(
                Content: $"{InteropReturnType.GetFullyQualifiedName()} (*{qualifiedDefinitionName}::{FunctionPointerName})({InteropParameterListDeclaration()}) = nullptr;",
                TypeDeclarationsReferenced: fieldPointerTypes));

            init.Functions.Add(new(
                CppName: $"{definition.Type.GetFullyQualifiedName()}::{FunctionPointerName}",
                CppTypeSignature: $"{InteropReturnType.GetFullyQualifiedName()} (*)({InteropParameterTypeList()})",
                CppTypeDefinitionsReferenced: new[] { definition.Type },
                CppTypeDeclarationsReferenced: fieldPointerTypes,
                CSharpName: csharpName,
                CSharpContent: csharpContent
            ));
        }

        private void AddDeclaration(GeneratedResult result)
        {
            string modifiers = Static() ? "static " : "";
            string afterModifiers = Static() ? "" : " const";

            if (IsUnspecializedGeneric)
            {
                modifiers = "template <" + string.Join(", ", TypeParameters().Select(t => "typename " + t.Type.GetFullyQualifiedName())) + ">\n" + modifiers;
            }

            result.CppDeclaration.Elements.Add(new(
                Content: $"{modifiers}{ReturnType().GetFullyQualifiedName()} {Name}({ParameterListDeclaration()}){afterModifiers};",
                TypeDeclarationsReferenced: new[] { ReturnType() }.Concat(ParameterTypes),
                IsPrivate: Private()
            ));
        }

        private void AddDefinition(GeneratedResult result, IReadOnlyList<CppStatement> body)
        {
            GeneratedCppDefinition definition = result.CppDefinition;
            string afterModifiers = Static() ? "" : " const";
            string typeTemplateSpecialization = GetTypeTemplateSpecialization(definition.Type);
            string templatePrefix = "";
            string templateSpecialization = "";
            string parameters;

            CppInteropFunction? specializes = Specializes();
            if (specializes != null)
            {
                // This is a specialization of a generic. We need to declare it as such.
                templatePrefix = "template <> ";
                templateSpecialization = $"<{string.Join(", ", TypeArguments().Select(t => t.GetFullyQualifiedName()))}>";

                // We also need to pass every generic parameter as a const reference in order to match the unspecialized template.
                // This isn't ideal (passing a parameter as `const int&`, for example, is a bit weird), but it works and
                // isn't a big enough problem to bother fixing right now.
                List<CppInteropParameter> genericFunctionParameters = specializes.Parameters();
                parameters = string.Join(", ", Parameters().Select((parameter, index) => {
                    if (genericFunctionParameters[index].Type.Kind == InteropTypeKind.GenericParameter)
                        return $"{parameter.Type.AsConstReference().GetFullyQualifiedName()} {parameter.Name}";
                    else
                        return $"{parameter.Type.GetFullyQualifiedName()} {parameter.Name}";
                }));
            }
            else
            {
                parameters = ParameterListDeclaration();
            }

            definition.Elements.Add(new(
                Content:
                    $$"""
                    {{templatePrefix}}{{ReturnType().GetFullyQualifiedName()}} {{definition.Type.Name}}{{typeTemplateSpecialization}}::{{Name}}{{templateSpecialization}}({{parameters}}){{afterModifiers}} {
                        {{new[] { CppPrinter.Print(body) }.JoinAndIndent("    ")}}
                    }
                    """,
                TypeDefinitionsReferenced: new[]
                {
                    definition.Type,
                    ReturnType(),
                    CppObjectHandle.GetCppType(Context),
                    CppReinteropException.GetCppType(Context)
                }.Concat(ParameterTypes)
            ));
        }

        /// <summary>
        /// The "&lt;T, U&gt;" suffix needed to qualify a member of a generic type's out-of-line
        /// definition, e.g. "MyClass&lt;T&gt;::Method(...)" - empty if the type isn't generic.
        /// </summary>
        public static string GetTypeTemplateSpecialization(CppType type) =>
            type.GenericArguments != null && type.GenericArguments.Count > 0
                ? "<" + string.Join(", ", type.GenericArguments.Select(t => t.GetFullyQualifiedName())) + ">"
                : "";

    }
}
