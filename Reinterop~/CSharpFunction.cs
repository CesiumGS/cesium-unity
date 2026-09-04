using Microsoft.CodeAnalysis;

namespace Reinterop
{
    /// <summary>
    /// Describes a plain C# function - a method - purely in terms of its modifiers, parameters, and
    /// return type, and renders its declaration and (given a body) its definition as C# source text.
    /// </summary>
    /// <remarks>
    /// This is the C# counterpart to <see cref="CppFunction"/>. Unlike <see cref="CppInteropFunction"/>
    /// or <see cref="CSharpFunctionCallableFromCpp"/>, it has no knowledge of the interop boundary -
    /// delegates, function pointers, exception marshaling, and so on. It only knows how to render a
    /// C# function from a fluent description of its shape; interop-specific concerns are the
    /// responsibility of the caller.
    /// </remarks>
    internal class CSharpFunction
    {
        public CppGenerationContext Context { get; }

        /// <summary>
        /// The C# type that owns this function.
        /// </summary>
        public CSharpType Owner { get; }

        public string Name { get; }

        private List<CSharpType> _typeArguments = new List<CSharpType>();

        /// <summary>
        /// Gets the generic type parameters to this function. If this is not a generic function, the list is empty.
        /// </summary>
        public List<CSharpType> TypeArguments() { return _typeArguments; }

        /// <summary>
        /// Sets the generic type parameters to this function. If this is not a generic function, the list should be empty.
        /// </summary>
        public CSharpFunction TypeArguments(IEnumerable<CSharpType> typeArguments)
        {
            _typeArguments = typeArguments.ToList();
            return this;
        }

        /// <summary>
        /// Sets the generic type arguments to this function from a list of Roslyn type parameters. If this is not a
        /// generic function, the list should be empty.
        /// </summary>
        public CSharpFunction TypeArguments(IEnumerable<ITypeParameterSymbol> typeParameters)
        {
            return TypeArguments(typeParameters.Select(parameter => CSharpType.FromSymbol(Context, parameter)));
        }

        private List<CSharpParameter> _parameters = new List<CSharpParameter>();

        /// <summary>
        /// Gets the parameters to the function.
        /// </summary>
        public List<CSharpParameter> Parameters() { return _parameters; }

        /// <summary>
        /// Sets the parameters to the function.
        /// </summary>
        public CSharpFunction Parameters(IEnumerable<CSharpParameter> parameters)
        {
            _parameters = parameters.ToList();
            return this;
        }

        /// <summary>
        /// Sets the parameters to the function from a list of Roslyn parameters.
        /// </summary>
        public CSharpFunction Parameters(IEnumerable<IParameterSymbol> parameters)
        {
            return Parameters(parameters.Select(parameter => new CSharpParameter(CSharpType.FromSymbol(Context, parameter.Type), parameter.Name)));
        }

        private CSharpType _returnType;

        /// <summary>
        /// Gets the function's return type.
        /// </summary>
        public CSharpType ReturnType() { return _returnType; }

        /// <summary>
        /// Sets the function's return type.
        /// </summary>
        public CSharpFunction ReturnType(CSharpType returnType)
        {
            _returnType = returnType;
            return this;
        }

        /// <summary>
        /// Sets the function's return type from a Roslyn type.
        /// </summary>
        public CSharpFunction ReturnType(ITypeSymbol returnType)
        {
            return ReturnType(CSharpType.FromSymbol(Context, returnType));
        }

        private bool _static = false;

        /// <summary>
        /// Gets a value indicating whether this is a static member.
        /// </summary>
        public bool Static() { return _static; }

        /// <summary>
        /// Sets a value indicating whether this is a static member.
        /// </summary>
        public CSharpFunction Static(bool isStatic)
        {
            _static = isStatic;
            return this;
        }

        private bool _private = false;

        /// <summary>
        /// Gets a value indicating whether this function is private. If `false`, no accessibility modifier
        /// is emitted, which C# treats as private for a class member anyway.
        /// </summary>
        public bool Private() { return _private; }

        /// <summary>
        /// Sets a value indicating whether this function is private.
        /// </summary>
        public CSharpFunction Private(bool isPrivate)
        {
            _private = isPrivate;
            return this;
        }

        private bool _unsafe = false;

        /// <summary>
        /// Gets a value indicating whether this function is marked as `unsafe` in the generated C# code.
        /// </summary>
        public bool Unsafe() { return _unsafe; }

        /// <summary>
        /// Sets a value indicating whether this function is marked as `unsafe` in the generated C# code.
        /// </summary>
        public CSharpFunction Unsafe(bool isUnsafe)
        {
            _unsafe = isUnsafe;
            return this;
        }

        private List<string> _attributes = new List<string>();

        /// <summary>
        /// Gets the attributes (e.g. `"[AOT.MonoPInvokeCallback(typeof(Foo))]"`) applied to this function, each
        /// rendered exactly as given on its own line immediately above the function's declaration.
        /// </summary>
        public List<string> Attributes() { return _attributes; }

        /// <summary>
        /// Sets the attributes applied to this function.
        /// </summary>
        public CSharpFunction Attributes(IEnumerable<string> attributes)
        {
            _attributes = attributes.ToList();
            return this;
        }

        private List<CSharpStatement>? _body = null;

        /// <summary>
        /// Gets the statements of this function's body. See <see cref="Body(IEnumerable{CSharpStatement}?)"/> for details.
        /// </summary>
        public List<CSharpStatement>? Body() { return _body; }

        /// <summary>
        /// Sets the statements of this function's body, used by <see cref="Print"/>. If left unset (or set to
        /// null), <see cref="Print"/> renders a bodyless declaration terminated with `;` instead, e.g. for an
        /// abstract or partial method.
        /// </summary>
        public CSharpFunction Body(IEnumerable<CSharpStatement>? body)
        {
            _body = body?.ToList();
            return this;
        }

        public CSharpFunction(CppGenerationContext context, CSharpType owner, string name)
        {
            this.Context = context;
            this.Owner = owner;
            this.Name = name;
            this._returnType = CSharpType.FromSymbol(context, context.Compilation.GetSpecialType(SpecialType.System_Void));
        }

        /// <summary>The generic type parameter list, e.g. "&lt;T, U&gt;" - empty if this function isn't generic.</summary>
        public string TypeArgumentListDeclaration() =>
            TypeArguments().Count > 0 ? "<" + string.Join(", ", TypeArguments()) + ">" : "";

        /// <summary>The function's own parameter list, e.g. "int x, float y".</summary>
        public string ParameterListDeclaration() =>
            string.Join(", ", Parameters().Select(parameter => $"{parameter.Type.GetFullyQualifiedName()} {parameter.Name}"));

        /// <summary>
        /// Gets a declaration for a delegate that matches this function's signature. The returned string will include
        /// the `unsafe` keyword if <see cref="Unsafe"/> is true, as well as the `delegate` keyword, but it will not include
        /// any access modifiers like `private` or `public`, nor will it include `static`.
        /// </summary>
        /// <param name="delegateTypeName">The name of the delegate.</param>
        public string GetDelegateDeclaration(string delegateTypeName)
        {
            string modifiers = Unsafe() ? "unsafe " : "";
            return $"{modifiers}delegate {ReturnType().GetFullyQualifiedName()} {delegateTypeName}({ParameterListDeclaration()})";
        }

        /// <summary>
        /// Renders this function's declaration and, if <see cref="Body"/> was set, its body, as C# source text.
        /// </summary>
        public string Print()
        {
            string attributes = Attributes().Count > 0 ? string.Join(Environment.NewLine, Attributes()) + Environment.NewLine : "";
            string modifiers = string.Join(" ", GetModifiers());
            if (modifiers.Length > 0)
                modifiers += " ";

            string signature = $"{modifiers}{ReturnType().GetFullyQualifiedName()} {Name}{TypeArgumentListDeclaration()}({ParameterListDeclaration()})";

            if (Body() == null)
                return $"{attributes}{signature};";

            return $$"""
                {{attributes}}{{signature}}
                {
                    {{CSharpPrinter.Print(Body()!, "    ")}}
                }
                """;
        }

        private IEnumerable<string> GetModifiers()
        {
            if (Private()) yield return "private";
            if (Static()) yield return "static";
            if (Unsafe()) yield return "unsafe";
        }
    }
}
