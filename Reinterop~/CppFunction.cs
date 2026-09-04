using Microsoft.CodeAnalysis;

namespace Reinterop
{
    /// <summary>
    /// Describes a plain C++ function - a method, property accessor, constructor, or field
    /// accessor - purely in terms of its parameters and return type, and generates its
    /// declaration and (given a body) its definition.
    /// </summary>
    internal class CppFunction
    {
        public CppGenerationContext Context { get; }

        public string Name { get; }

        /// <summary>
        /// The C++ type that owns this function.
        /// </summary>
        public CppType Owner { get; }

        /// <summary>
        /// Determines if this function is a C++ constructor, i.e., its name is the same as its owner type's name.
        /// </summary>
        public bool IsConstructor => Name == Owner.Name;

        /// <summary>
        /// Determines if this function is a C++ type conversion operator, i.e., its name is "operator X" where X is not one of the built-in C++ operators.
        /// </summary>
        public bool IsConversionOperator
        {
            get
            {
                if (!Name.StartsWith("operator "))
                    return false;

                string op = Name.Substring("operator ".Length).Replace(" ", "").Trim();

                string[] operators = [
                    "+", "-", "*", "/", "%", "^", "&", "|", "~", "!", "=", "<", ">",
                    "+=", "-=", "*=", "/=", "%=", "^=", "&=", "|=", "<<", ">>",">>=", "<<=",
                    "==", "!=", "<=", ">=", "<=>", "&&", "||", "++", "--", ",", "->*", "->", "()", "[]",
                    "new", "new[]", "delete", "delete[]", "co_await"
                ];

                return !operators.Contains(op);
            }
        }

        /// <summary>
        /// Determines if this function has no declared return type, i.e., it is a constructor or a conversion operator. In C++, constructors and
        /// conversion operators don't have a return type declaration, so this is used to avoid generating one for them.
        /// </summary>
        public bool HasNoReturnTypeDeclaration => IsConstructor || IsConversionOperator;

        private List<CppParameter> _typeParameters = new List<CppParameter>();

        /// <summary>
        /// Gets or sets the generic type parameters to this function. If this is not a generic function, the list is empty.
        /// </summary>
        public List<CppParameter> TypeParameters() { return _typeParameters; }

        /// <summary>
        /// Sets the generic type parameters to this function. If this is not a generic function, the list should be empty.
        /// </summary>
        public CppFunction TypeParameters(IEnumerable<CppParameter> typeParameters)
        {
            _typeParameters = typeParameters.ToList();
            return this;
        }

        /// <summary>
        /// Sets the generic type parameters to this function from a list of Roslyn type parameters. If this is not a generic
        /// function, the list should be empty.
        /// </summary>
        public CppFunction TypeParameters(IEnumerable<ITypeParameterSymbol> typeParameters)
        {
            return TypeParameters(typeParameters.Select(parameter => new CppParameter(CppType.FromCSharp(Context, CSharpType.FromSymbol(Context, parameter)), parameter.Name)));
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
        public CppFunction TypeArguments(IEnumerable<CppType>? typeArguments)
        {
            if (typeArguments == null)
                _typeArguments = new List<CppType>();
            else
                _typeArguments = typeArguments.ToList();
            return this;
        }

        /// <summary>
        /// Sets the generic type arguments to this function from a list of C# type arguments. These are the types
        /// that fill in the <see cref="TypeParameters"/> in the instantiated generic.
        /// </summary>
        public CppFunction TypeArguments(IEnumerable<CSharpType> typeArguments)
        {
            return TypeArguments(typeArguments.Select(t => CppType.FromCSharp(Context, t)));
        }

        /// <summary>
        /// Sets the generic type arguments to this function from a list of Roslyn type arguments. These are the types
        /// that fill in the <see cref="TypeParameters"/> in the instantiated generic.
        /// </summary>
        public CppFunction TypeArguments(IEnumerable<ITypeSymbol> typeArguments)
        {
            return TypeArguments(typeArguments.Select(t => CppType.FromCSharp(Context, CSharpType.FromSymbol(Context, t))));
        }

        private List<CppParameter> _parameters = new List<CppParameter>();

        /// <summary>
        /// Gets the parameters to the function.
        /// </summary>
        public List<CppParameter> Parameters() { return _parameters; }

        /// <summary>
        /// Sets the parameters to the function. The implicit "this" parameter should _not_ be included.
        /// </summary>
        public CppFunction Parameters(IEnumerable<CppParameter> parameters)
        {
            _parameters = parameters.ToList();
            return this;
        }

        /// <summary>
        /// Sets the parameters to the function from a list of C# parameters. The implicit "this" parameter should
        /// _not_ be included.
        /// </summary>
        public CppFunction Parameters(IEnumerable<CSharpParameter> parameters)
        {
            return Parameters(parameters.Select(parameter => new CppParameter(CppType.FromCSharp(Context, parameter.Type).AsParameterType(), parameter.Name)));
        }

        /// <summary>
        /// Sets the parameters to the function from a list of Roslyn parameters. The implicit "this" parameter should
        /// _not_ be included.
        /// </summary>
        public CppFunction Parameters(IEnumerable<IParameterSymbol> parameters)
        {
            return Parameters(parameters.Select(parameter => new CSharpParameter(CSharpType.FromSymbol(Context, parameter.Type), parameter.Name)));
        }

        private CppType _returnType = CppType.Void;

        /// <summary>
        /// Gets the function's return type.
        /// </summary>
        public CppType ReturnType() { return _returnType; }

        /// <summary>
        /// Sets the function's return type.
        /// </summary>
        public CppFunction ReturnType(CppType returnType)
        {
            _returnType = returnType;
            return this;
        }

        /// <summary>
        /// Sets the function's return type from a C# type.
        /// </summary>
        public CppFunction ReturnType(CSharpType returnType)
        {
            return ReturnType(CppType.FromCSharp(Context, returnType).AsReturnType());
        }

        /// <summary>
        /// Sets the function's return type from a Roslyn type.
        /// </summary>
        public CppFunction ReturnType(ITypeSymbol returnType)
        {
            return ReturnType(CSharpType.FromSymbol(Context, returnType));
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
        public CppFunction Static(bool isStatic)
        {
            _static = isStatic;
            return this;
        }

        private bool _const = true;

        /// <summary>
        /// Gets a value indicating whether this function is marked as `const` in the generated C++ code.
        /// See <see cref="Const(bool)"/> for more information.
        /// </summary>
        public bool Const() { return _const; }

        /// <summary>
        /// Sets a value indicating whether this function is marked as `const` in the generated C++ code. This is ignored for static functions.
        /// It defaults to true.
        /// </summary>
        public CppFunction Const(bool isConst)
        {
            _const = isConst;
            return this;
        }

        private bool _private = false;

        /// <summary>
        /// Gets a value indicating whether this function is private.
        /// </summary>
        public bool Private() { return _private; }

        /// <summary>
        /// Sets a value indicating whether this function is private.
        /// </summary>
        public CppFunction Private(bool isPrivate)
        {
            _private = isPrivate;
            return this;
        }

        private bool _deleted = false;

        /// <summary>
        /// Gets a value indicating whether this function is deleted (` = delete`) in the generated C++ code.
        /// </summary>
        public bool Deleted() { return _deleted; }

        /// <summary>
        /// Sets a value indicating whether this function is deleted (` = delete`) in the generated C++ code.
        /// </summary>
        public CppFunction Deleted(bool isDeleted)
        {
            _deleted = isDeleted;
            return this;
        }

        private bool _explicit = false;

        /// <summary>
        /// Gets a value indicating whether this function is marked as `explicit` in the generated C++ code.
        /// This is only used for constructors and conversion operators.
        /// </summary>
        public bool Explicit() { return _explicit; }

        /// <summary>
        /// Sets a value indicating whether this function is marked as `explicit` in the generated C++ code.
        /// This is only used for constructors and conversion operators.
        /// </summary>
        public CppFunction Explicit(bool isExplicit)
        {
            _explicit = isExplicit;
            return this;
        }

        private bool _noExcept = false;

        /// <summary>
        /// Gets a value indicating whether this function is marked as `noexcept` in the generated C++ code.
        /// </summary>
        public bool NoExcept() { return _noExcept; }

        /// <summary>
        /// Sets a value indicating whether this function is marked as `noexcept` in the generated C++ code.
        /// </summary>
        public CppFunction NoExcept(bool noExcept)
        {
            _noExcept = noExcept;
            return this;
        }

        private CppFunction? _specializes = null;

        /// <summary>
        /// Gets the generic function that this function specializes, if any. Specializations don't get their own declaration, but instead
        /// reuse the template declaration of the generic function.
        /// </summary>
        public CppFunction? Specializes() { return _specializes; }

        /// <summary>
        /// Sets the generic function that this function specializes, if any. Specializations don't get their own declaration, but instead
        /// reuse the template declaration of the generic function.
        /// </summary>
        public CppFunction Specializes(CppFunction? specializes)
        {
            _specializes = specializes;
            return this;
        }

        private List<CppStatement>? _definitionBody = null;

        /// <summary>
        /// Gets the statements of this function's definition. See <see cref="DefinitionBody(IEnumerable{CppStatement}?)"/>
        /// for details.
        /// </summary>
        public List<CppStatement>? DefinitionBody() { return _definitionBody; }

        /// <summary>
        /// Sets the statements of this function's definition, used by <see cref="AddToGeneration"/>. If left
        /// unset (or set to null), <see cref="AddToGeneration"/> won't add a definition at all - only a
        /// declaration - which is useful for an unspecialized generic function template, or any other
        /// function whose declaration alone needs to be generated.
        /// </summary>
        public CppFunction DefinitionBody(IEnumerable<CppStatement>? body)
        {
            if (body != null)
                _definitionBody = body.ToList();
            else
                _definitionBody = null;
            return this;
        }

        private List<CppMemberInitializer>? _memberInitializers = null;

        /// <summary>
        /// Gets the constructor member initializers. This is only used for constructors, and it is ignored for non-constructors.
        /// For constructors, this is the list of member initializers to use in the constructor's definition.
        /// </summary>
        public List<CppMemberInitializer>? MemberInitializers() { return _memberInitializers; }

        /// <summary>
        /// Sets the constructor member initializers. This is only used for constructors, and it is ignored for non-constructors.
        /// For constructors, this is the list of member initializers to use in the constructor's definition.
        /// </summary>
        public CppFunction MemberInitializers(IEnumerable<CppMemberInitializer>? memberInitializers)
        {
            if (memberInitializers != null)
                _memberInitializers = memberInitializers.ToList();
            else
                _memberInitializers = null;
            return this;
        }

        /// <summary>The wrapped function's own parameter list, e.g. "int x, float y".</summary>
        public string ParameterListDeclaration() =>
            string.Join(", ", Parameters().Select(parameter => $"{parameter.Type.GetFullyQualifiedName()} {parameter.Name}"));

        /// <summary>The types referenced by <see cref="Parameters"/>, for TypeDeclarationsReferenced.</summary>
        public IEnumerable<CppType> ParameterTypes => Parameters().Select(parameter => parameter.Type);

        /// <summary>
        /// Gets whether this function is an unspecialized generic, i.e., it has generic <see cref="TypeParameters"/>
        /// but any of the corresponding <see cref="TypeArguments"/> are just a
        /// <see cref="InteropTypeKind.GenericParameter"/>. We can't generate a definition for an unspecialized generic
        /// (there's no concrete type to fill in its template parameters), but we can still add its declaration to the
        /// generation, as well as the definitions of any of its specializations.
        /// </summary>
        public bool IsUnspecializedGeneric => TypeArguments().Any(t => t.Kind == InteropTypeKind.GenericParameter);

        public CppFunction(
            CppGenerationContext context,
            CppType owner,
            string name)
        {
            this.Context = context;
            this.Owner = owner;
            this.Name = name;
        }

        /// <summary>
        /// Adds everything needed to expose this function: its own declaration, and (if
        /// <see cref="DefinitionBody"/> was set) its definition.
        /// </summary>
        public void AddToGeneration(GeneratedResult result)
        {
            // Don't add a declaration for a specialization. Use the generic template declaration instead.
            if (Specializes() == null)
                AddDeclaration(result);

            if (!Deleted() && !IsUnspecializedGeneric && _definitionBody != null)
                AddDefinition(result, _definitionBody);
        }

        private void AddDeclaration(GeneratedResult result)
        {
            // Constructors are neither static nor const.
            // All other instance functions are const, because C# does not play by C++ const-correctness patterns.
            string modifiers = Static() && !IsConstructor ? "static " : "";
            string afterModifiers = !Const() || Static() || IsConstructor ? "" : " const";

            if (NoExcept())
                afterModifiers += " noexcept";

            if (Deleted())
                afterModifiers += " = delete";

            if (Explicit() && (IsConstructor || IsConversionOperator))
                modifiers = "explicit " + modifiers;

            if (IsUnspecializedGeneric)
                modifiers = "template <" + string.Join(", ", TypeParameters().Select(t => "typename " + t.Type.GetFullyQualifiedName())) + ">\n" + modifiers;

            // Constructors do not have return types.
            string returnType = HasNoReturnTypeDeclaration ? "" : $"{ReturnType().GetFullyQualifiedName()} ";

            result.CppDeclaration.Elements.Add(new(
                Content: $"{modifiers}{returnType}{Name}({ParameterListDeclaration()}){afterModifiers};",
                TypeDeclarationsReferenced: new[] { ReturnType() }.Concat(ParameterTypes),
                IsPrivate: Private()
            ));
        }

        private void AddDefinition(GeneratedResult result, IReadOnlyList<CppStatement> body)
        {
            GeneratedCppDefinition definition = result.CppDefinition;
            string afterModifiers = !Const() || Static() || IsConstructor ? "" : " const";
            string typeTemplateSpecialization = GetTypeTemplateSpecialization(definition.Type);
            string templatePrefix = "";
            string templateSpecialization = "";
            string parameters;

            if (NoExcept())
                afterModifiers += " noexcept";

            // Constructors do not have return types.
            string returnType = HasNoReturnTypeDeclaration ? "" : $"{ReturnType().GetFullyQualifiedName()} ";

            CppFunction? specializes = Specializes();
            if (specializes != null)
            {
                // This is a specialization of a generic. We need to declare it as such.
                templatePrefix = "template <> ";
                templateSpecialization = $"<{string.Join(", ", TypeArguments().Select(t => t.GetFullyQualifiedName()))}>";

                // We also need to pass every generic parameter as a const reference in order to match the unspecialized template.
                // This isn't ideal (passing a parameter as `const int&`, for example, is a bit weird), but it works and
                // isn't a big enough problem to bother fixing right now.
                List<CppParameter> genericFunctionParameters = specializes.Parameters();
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

            List<CppMemberInitializer>? memberInitializers = IsConstructor ? MemberInitializers() : null;
            string memberInitialization = memberInitializers != null && memberInitializers.Count > 0
                ? " : " + string.Join(", ", memberInitializers.Select(initializer => $"{initializer.MemberName}({CppPrinter.Print(initializer.Value)})"))
                : "";

            HashSet<string> requiredIncludes = CppPrinter.GetRequiredIncludes(body);
            if (memberInitializers != null)
                requiredIncludes.UnionWith(CppPrinter.GetRequiredIncludes(memberInitializers.Select(i => i.Value)));

            definition.Elements.Add(new(
                Content:
                    $$"""
                    {{templatePrefix}}{{returnType}}{{definition.Type.Name}}{{typeTemplateSpecialization}}::{{Name}}{{templateSpecialization}}({{parameters}}){{afterModifiers}}{{memberInitialization}} {
                        {{new[] { CppPrinter.Print(body) }.JoinAndIndent("    ")}}
                    }
                    """,
                TypeDefinitionsReferenced: new[]
                {
                    definition.Type,
                    ReturnType(),
                }.Concat(ParameterTypes),
                AdditionalIncludes: requiredIncludes
            ));
        }

        /// <summary>
        /// Gets the declaration of a function pointer that can point to this C++ function.
        /// </summary>
        /// <param name="variableName">The name of the function pointer variable. This can be ommitted if a name isn't needed, such as when casting.</param>
        public string GetFunctionPointerDeclaration(string variableName = "")
        {
            return $"{ReturnType().GetFullyQualifiedName()} (*{variableName})({string.Join(", ", ParameterTypes.Select(t => t.GetFullyQualifiedName()))})";
        }

        public CppFunction Clone()
        {
            return new CppFunction(Context, Owner, Name)
                .TypeParameters(TypeParameters())
                .TypeArguments(TypeArguments())
                .ReturnType(ReturnType())
                .Parameters(Parameters())
                .Static(Static())
                .Private(Private())
                .Specializes(Specializes())
                .Const(Const())
                .NoExcept(NoExcept())
                .Deleted(Deleted())
                .Explicit(Explicit())
                .MemberInitializers(MemberInitializers())
                .DefinitionBody(DefinitionBody());
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
