using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace Reinterop
{
    /// <summary>
    /// Walks the methods in the Reinterop-tagged class to discover which types, methods,
    /// and properties should be exposed to C++.
    /// </summary>
    internal class ExposeToCppSyntaxWalker : CSharpSyntaxWalker
    {
        public readonly Dictionary<ITypeSymbol, TypeToGenerate> GenerationItems = new Dictionary<ITypeSymbol, TypeToGenerate>(SymbolEqualityComparer.Default);

        public ExposeToCppSyntaxWalker(CppGenerationContext context, SemanticModel semanticModel)
        {
            this._context = context;
            this._semanticModel = semanticModel;
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            base.VisitInvocationExpression(node);
            ISymbol? symbol = this._semanticModel.GetSymbolInfo(node.Expression).Symbol;

            IMethodSymbol? methodSymbol = symbol as IMethodSymbol;
            if (methodSymbol != null)
            {
                this.AddMethod(methodSymbol);
            }

            IPropertySymbol? propertySymbol = symbol as IPropertySymbol;
            if (propertySymbol != null)
            {
                this.AddProperty(propertySymbol);
            }
        }

        public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            base.VisitMemberAccessExpression(node);

            ISymbol? symbol = this._semanticModel.GetSymbolInfo(node.Name).Symbol;

            IPropertySymbol? propertySymbol = symbol as IPropertySymbol;
            if (propertySymbol != null)
            {
                this.AddProperty(propertySymbol);
            }

            IEventSymbol? eventSymbol = symbol as IEventSymbol;
            if (eventSymbol != null)
            {
                this.AddEvent(eventSymbol);
            }

            IFieldSymbol? fieldSymbol = symbol as IFieldSymbol;
            if (fieldSymbol != null)
            {
                this.AddField(fieldSymbol);
            }
        }

        public override void VisitGenericName(GenericNameSyntax node)
        {
            base.VisitGenericName(node);

            ITypeSymbol? type = this._semanticModel.GetTypeInfo(node).Type;
            if (type == null)
                return;

            this.AddType(type);
        }

        public override void VisitIdentifierName(IdentifierNameSyntax node)
        {
            base.VisitIdentifierName(node);

            ITypeSymbol? type = this._semanticModel.GetTypeInfo(node).Type;
            if (type == null)
                return;

            this.AddType(type);
        }

        public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            base.VisitObjectCreationExpression(node);

            ISymbol? symbol = this._semanticModel.GetSymbolInfo(node).Symbol;
            IMethodSymbol? methodSymbol = symbol as IMethodSymbol;
            if (methodSymbol == null)
                return;

            this.AddConstructor(methodSymbol);
        }

        public override void VisitElementAccessExpression(ElementAccessExpressionSyntax node)
        {
            base.VisitElementAccessExpression(node);

            // Look for the property for accessing elements of a list (or similar) by index.
            ITypeSymbol? containingType = null;
            ISymbol? symbol = this._semanticModel.GetSymbolInfo(node).Symbol;
            IPropertySymbol? property = symbol as IPropertySymbol;
            if (property == null)
            {
                // Arrays don't have a proper property. Find one on the IList<T> interface instead.
                ITypeSymbol? expressionType = this._semanticModel.GetTypeInfo(node.Expression).Type as ITypeSymbol;
                if (expressionType == null)
                    return;

                containingType = expressionType;
                INamedTypeSymbol? listInterface = expressionType.AllInterfaces.Where(ifc => ifc.Name == "IList" && ifc.IsGenericType).FirstOrDefault();
                property = CSharpTypeUtility.FindMember(listInterface, "this[]") as IPropertySymbol;

                if (property == null)
                    return;
            }

            this.AddProperty(property, containingType);
        }

        public TypeToGenerate AddType(ITypeSymbol type)
        {
            // Drop the nullability ("?") from the type if present.
            INamedTypeSymbol? named = type as INamedTypeSymbol;
            if (named != null && named.Name == "Nullable" && named.IsGenericType && named.TypeArguments.Length == 1)
            {
                type = named.TypeArguments[0];
            }

            // Don't add error types.
            if (type.TypeKind == TypeKind.Error)
                return new TypeToGenerate(type);

            // Don't add generic type parameters
            if (type.Kind == SymbolKind.TypeParameter)
                return new TypeToGenerate(type);

            // Don't add "void"
            if (type.SpecialType == SpecialType.System_Void)
                return new TypeToGenerate(type);

            // Don't add types without a name, except for arrays.
            if (type.TypeKind != TypeKind.Array && type.Name == "")
                return new TypeToGenerate(type);

            TypeToGenerate generationItem;
            if (!this.GenerationItems.TryGetValue(type, out generationItem))
            {
                generationItem = new TypeToGenerate(type);
                this.GenerationItems.Add(type, generationItem);

                // If this is a delegate, be sure to add the Invoke method.
                if (type.TypeKind == TypeKind.Delegate)
                {
                    IMethodSymbol? invokeMethod = CSharpTypeUtility.FindMember(type, "Invoke") as IMethodSymbol;
                    if (invokeMethod != null)
                        this.AddMethod(invokeMethod);
                }

                bool isBlittableStruct = Interop.IsBlittableStruct(this._context, type);

                // If this type has overloaded operator==, generate wrappers for it, because we need it
                // to even compare to null. But we don't need to compare blittable structs to null.
                if (!isBlittableStruct)
                {
                    IEnumerable<ISymbol> equalityOperators = CSharpTypeUtility.FindMembers(type, "op_Equality");
                    foreach (ISymbol equalityOperator in equalityOperators)
                    {
                        if (equalityOperator is IMethodSymbol method)
                            AddMethod(method);
                    }
                    IEnumerable<ISymbol> inequalityOperators = CSharpTypeUtility.FindMembers(type, "op_Inequality");
                    foreach (ISymbol inequalityOperator in inequalityOperators)
                    {
                        if (inequalityOperator is IMethodSymbol method)
                            AddMethod(method);
                    }
                }

                // If this is a blittable struct, we need to generate all the field types, too.
                if (type.TypeKind != TypeKind.Enum && isBlittableStruct)
                {
                    ImmutableArray<ISymbol> members = type.GetMembers();
                    foreach (ISymbol member in members)
                    {
                        IFieldSymbol? field = member as IFieldSymbol;
                        if (field == null)
                            continue;
                        AddType(field.Type);
                    }
                }
            }

            // If this type is an enumeration, add all of the enum values if we haven't already.
            if (generationItem.EnumValues.Count == 0 &&
                SymbolEqualityComparer.Default.Equals(type.BaseType, _semanticModel.Compilation.GetSpecialType(SpecialType.System_Enum)))
            {
                ImmutableArray<ISymbol> members = type.GetMembers();
                foreach (ISymbol symbol in members)
                {
                    IFieldSymbol? field = symbol as IFieldSymbol;
                    if (field == null || !field.IsConst)
                        continue;

                    generationItem.EnumValues.Add(field);
                }
            }

            // If this is an instantiated generic, we also need the types it's instantiated with.
            if (named != null && named.IsGenericType)
            {
                foreach (ITypeSymbol arg in named.TypeArguments)
                {
                    this.AddType(arg);
                }
            }

            // If this is an array, we also need the element type.
            IArrayTypeSymbol? arrayType = type as IArrayTypeSymbol;
            if (arrayType != null)
            {
                this.AddType(arrayType.ElementType);
            }

            return generationItem;
        }

        public TypeToGenerate AddMethod(IMethodSymbol symbol)
        {
            TypeToGenerate item = this.AddType(symbol.ContainingType);
            item.Methods.Add(symbol);

            // We also need to generate the parameter and return value types
            this.AddType(symbol.ReturnType);

            foreach (IParameterSymbol parameter in symbol.Parameters)
            {
                this.AddType(parameter.Type);
            }

            return item;
        }

        public TypeToGenerate AddProperty(IPropertySymbol symbol, ITypeSymbol? containingType = null)
        {
            if (containingType == null)
                containingType = symbol.ContainingType;

            TypeToGenerate item = this.AddType(containingType);
            item.Properties.Add(symbol);

            // We also need to generate the property type.
            this.AddType(symbol.Type);

            return item;
        }

        public void AddEvent(IEventSymbol symbol)
        {
            TypeToGenerate item = this.AddType(symbol.ContainingType);
            item.Events.Add(symbol);

            // We also need to generate the event type.
            this.AddType(symbol.Type);
        }

        public TypeToGenerate AddField(IFieldSymbol symbol)
        {
            TypeToGenerate item = this.AddType(symbol.ContainingType);
            item.Fields.Add(symbol);

            // We also need to generate the field type.
            this.AddType(symbol.Type);

            return item;
        }

        public TypeToGenerate AddConstructor(IMethodSymbol symbol)
        {
            TypeToGenerate item = this.AddType(symbol.ContainingType);
            item.Constructors.Add(symbol);

            // We also need to generate the parameter types
            foreach (IParameterSymbol parameter in symbol.Parameters)
            {
                this.AddType(parameter.Type);
            }

            return item;
        }

        private CppGenerationContext _context;
        private SemanticModel _semanticModel;
    }
}
