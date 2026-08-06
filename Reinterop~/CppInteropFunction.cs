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
        private readonly CppGenerationContext _context;

        /// <summary>The name of the interop function pointer field, e.g. "CallFoo_1a2b3c" or "Field_get_Bar".</summary>
        public string Name { get; }

        /// <summary>The function's own parameters, as seen by its C++ caller (excludes "thiz").</summary>
        public IReadOnlyList<CppInteropParameter> Parameters { get; }

        /// <summary>The function's own return type, as seen by its C++ caller.</summary>
        public CppType ReturnType { get; }

        /// <summary>The interop function pointer's return type, after any struct-return rewrite.</summary>
        public CppType InteropReturnType { get; }

        /// <summary>True if the result is produced via a "pReturnValue" out-parameter instead of being returned directly.</summary>
        public bool HasStructRewrite { get; }

        /// <summary>
        /// The full interop parameter list, in call order: an implicit "thiz" (if this is an instance
        /// function), then <see cref="Parameters"/>, then "pReturnValue" (if <see cref="HasStructRewrite"/>),
        /// then "reinteropException".
        /// </summary>
        public IReadOnlyList<(string ParameterName, string CallSiteName, CppType Type, CppType InteropType)> InteropParameters { get; }

        /// <param name="instanceType">
        /// The type of "thiz", if this is an instance (non-static) function. Its call-site expression
        /// is always "(*this)". Null for static functions and constructors.
        /// </param>
        public CppInteropFunction(
            CppGenerationContext context,
            string name,
            IReadOnlyList<CppInteropParameter> parameters,
            CppType returnType,
            CppType? instanceType = null)
        {
            _context = context;
            Name = name;
            Parameters = parameters;
            ReturnType = returnType;
            _isInstance = instanceType != null;

            IEnumerable<CppInteropParameter> allParameters = parameters;
            if (instanceType != null)
                allParameters = new[] { new CppInteropParameter("thiz", instanceType, new CppRaw("(*this)")) }.Concat(allParameters);

            IEnumerable<(string ParameterName, string CallSiteName, CppType Type, CppType InteropType)> interopParameters =
                allParameters.Select(parameter => (
                    ParameterName: parameter.Name,
                    CallSiteName: CppPrinter.Print(parameter.CallSite),
                    Type: parameter.Type,
                    InteropType: parameter.Type.AsInteropType()));

            CppType interopReturnType = returnType.AsInteropType();
            HasStructRewrite = Interop.RewriteStructReturn(ref interopParameters, ref returnType, ref interopReturnType);
            InteropReturnType = interopReturnType;

            InteropParameters = interopParameters
                .Concat(new[] { (ParameterName: "reinteropException", CallSiteName: "", Type: CppType.VoidPointerPointer, InteropType: CppType.VoidPointerPointer) })
                .ToArray();
        }

        private readonly bool _isInstance;
        private bool _isPrivate;
        private bool _skipDeclaration;
        private string _templatePrefix = "";
        private string _templateSpecialization = "";

        /// <summary>Marks the wrapped function's own declaration (not the interop pointer field, which is always private) as private.</summary>
        public CppInteropFunction AsPrivate()
        {
            _isPrivate = true;
            return this;
        }

        /// <summary>
        /// Skips the wrapped function's own declaration in <see cref="AddToGeneration"/> - for a generic
        /// method specialization, which reuses the template declaration added separately, once, for the
        /// first specialization of that template.
        /// </summary>
        public CppInteropFunction WithoutDeclaration()
        {
            _skipDeclaration = true;
            return this;
        }

        /// <summary>Marks the wrapped function's own definition as a generic method specialization, e.g. "template &lt;&gt; ... Method&lt;int&gt;(...)".</summary>
        public CppInteropFunction AsTemplateSpecialization(IEnumerable<CppType> templateArguments)
        {
            _templatePrefix = "template <> ";
            _templateSpecialization = $"<{string.Join(", ", templateArguments.Select(t => t.GetFullyQualifiedName()))}>";
            return this;
        }

        /// <summary>The wrapped function's own parameter list, e.g. "int x, float y".</summary>
        public string ParameterListDeclaration() =>
            string.Join(", ", Parameters.Select(parameter => $"{parameter.Type.GetFullyQualifiedName()} {parameter.Name}"));

        /// <summary>The interop function pointer's parameter list, e.g. "void* thiz, std::int32_t x, void** reinteropException".</summary>
        public string InteropParameterListDeclaration() =>
            string.Join(", ", InteropParameters.Select(parameter => $"{parameter.InteropType.GetFullyQualifiedName()} {parameter.ParameterName}"));

        /// <summary>The interop function pointer's parameter *types* only (no names), e.g. for a function pointer type signature.</summary>
        public string InteropParameterTypeList() =>
            string.Join(", ", InteropParameters.Select(parameter => parameter.InteropType.GetFullyQualifiedName()));

        /// <summary>The types referenced by <see cref="Parameters"/>, for TypeDeclarationsReferenced.</summary>
        public IEnumerable<CppType> ParameterTypes => Parameters.Select(parameter => parameter.Type);

        /// <summary>The interop types of <see cref="Parameters"/> (excluding "thiz"/"reinteropException"/"pReturnValue"), for TypeDeclarationsReferenced.</summary>
        public IEnumerable<CppType> ParameterInteropTypes => Parameters.Select(parameter => parameter.Type.AsInteropType());

        /// <summary>The types referenced by <see cref="InteropParameters"/>, for TypeDeclarationsReferenced.</summary>
        public IEnumerable<CppType> InteropParameterTypes => InteropParameters.Select(parameter => parameter.InteropType);

        /// <summary>
        /// The call arguments to pass to <see cref="CppInterop.CallManagedFunction"/>: each parameter's
        /// value converted to its interop type, or (for a struct-return rewrite) an out-parameter of
        /// type <paramref name="outParameterTypeName"/>.
        /// </summary>
        public IReadOnlyList<CppArgument> CallArguments(string? outParameterTypeName = null) =>
            InteropParameters
                .Where(parameter => parameter.ParameterName != "reinteropException")
                .Select(parameter => parameter.ParameterName == "pReturnValue"
                    ? CppArgument.OutParameter(outParameterTypeName ?? ReturnType.GetFullyQualifiedName(), "result")
                    : CppArgument.Value(parameter.Type.GetConversionToInteropType(_context, parameter.CallSiteName)))
                .ToArray();

        /// <summary>
        /// Builds the call+return body, automatically choosing between a void call, a value-returning
        /// call, and a struct-return-rewrite call (with the result produced via an out-parameter of type
        /// <paramref name="outParameterTypeName"/>, which defaults to <see cref="ReturnType"/> itself).
        /// Returns null for the one shape not modeled here: a Nullable-wrapped struct-return rewrite,
        /// which returns a "resultIsValid" flag rather than using an exception out-parameter alone -
        /// callers must still build that shape by hand (and pass a null body to <see cref="AddToGeneration"/>).
        /// </summary>
        public IReadOnlyList<CppStatement>? Body(string? outParameterTypeName = null, string resultVariableName = "result")
        {
            if (HasStructRewrite && ReturnType.Kind == InteropTypeKind.Nullable)
                return null;

            CppExpression functionPointer = new CppIdentifier(Name);
            IReadOnlyList<CppArgument> arguments = CallArguments(outParameterTypeName);

            bool isVoid = ReturnType.Name == "void" && !ReturnType.Flags.HasFlag(CppTypeFlags.Pointer);
            if (isVoid)
                return CppInterop.CallManagedFunction(functionPointer, arguments);

            return CppInterop.CallManagedFunction(
                functionPointer, arguments,
                resultTypeName: HasStructRewrite ? null : "auto",
                returnExpression: new CppRaw(ReturnType.GetConversionFromInteropType(_context, resultVariableName)),
                resultVariableName: resultVariableName);
        }

        /// <summary>
        /// Adds everything needed to expose this interop function: the interop function pointer field
        /// (declaration, out-of-line definition initialized to nullptr, and startup init registration),
        /// the wrapped function's own declaration (unless <see cref="WithoutDeclaration"/> was used), and
        /// (if <paramref name="body"/> is non-null) its definition. A null <paramref name="body"/> (see
        /// <see cref="Body"/>) means the caller still needs to add a hand-built definition of its own.
        /// </summary>
        public void AddToGeneration(
            GeneratedResult result,
            string name,
            string csharpName,
            string csharpContent,
            IReadOnlyList<CppStatement>? body)
        {
            AddInteropFunctionPointer(result, result.CppDefinition.Type.GetFullyQualifiedName(false), csharpName, csharpContent);

            if (!_skipDeclaration)
                AddDeclaration(result, name);

            if (body != null)
                AddDefinition(result, name, body);
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
                Content: $"static {InteropReturnType.GetFullyQualifiedName()} (*{Name})({InteropParameterListDeclaration()});",
                IsPrivate: true,
                TypeDeclarationsReferenced: fieldPointerTypes));

            definition.Elements.Add(new(
                Content: $"{InteropReturnType.GetFullyQualifiedName()} (*{qualifiedDefinitionName}::{Name})({InteropParameterListDeclaration()}) = nullptr;",
                TypeDeclarationsReferenced: fieldPointerTypes));

            init.Functions.Add(new(
                CppName: $"{definition.Type.GetFullyQualifiedName()}::{Name}",
                CppTypeSignature: $"{InteropReturnType.GetFullyQualifiedName()} (*)({InteropParameterTypeList()})",
                CppTypeDefinitionsReferenced: new[] { definition.Type },
                CppTypeDeclarationsReferenced: fieldPointerTypes,
                CSharpName: csharpName,
                CSharpContent: csharpContent
            ));
        }

        private void AddDeclaration(GeneratedResult result, string name)
        {
            string modifiers = _isInstance ? "" : "static ";
            string afterModifiers = _isInstance ? " const" : "";

            result.CppDeclaration.Elements.Add(new(
                Content: $"{modifiers}{ReturnType.GetFullyQualifiedName()} {name}({ParameterListDeclaration()}){afterModifiers};",
                TypeDeclarationsReferenced: new[] { ReturnType }.Concat(ParameterTypes),
                IsPrivate: _isPrivate
            ));
        }

        private void AddDefinition(GeneratedResult result, string name, IReadOnlyList<CppStatement> body)
        {
            GeneratedCppDefinition definition = result.CppDefinition;
            string afterModifiers = _isInstance ? " const" : "";
            string typeTemplateSpecialization = GetTypeTemplateSpecialization(definition.Type);

            definition.Elements.Add(new(
                Content:
                    $$"""
                    {{_templatePrefix}}{{ReturnType.GetFullyQualifiedName()}} {{definition.Type.Name}}{{typeTemplateSpecialization}}::{{name}}{{_templateSpecialization}}({{ParameterListDeclaration()}}){{afterModifiers}} {
                        {{new[] { CppPrinter.Print(body) }.JoinAndIndent("    ")}}
                    }
                    """,
                TypeDefinitionsReferenced: new[]
                {
                    definition.Type,
                    ReturnType,
                    CppObjectHandle.GetCppType(_context),
                    CppReinteropException.GetCppType(_context)
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
