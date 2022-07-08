using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Oxidize
{
    internal class OxidizeWalker : CSharpSyntaxWalker
    {
        public readonly Dictionary<ITypeSymbol, GenerationItem> GenerationItems = new Dictionary<ITypeSymbol, GenerationItem>(SymbolEqualityComparer.Default);

        public OxidizeWalker(SemanticModel semanticModel)
        {
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
        }

        public override void VisitIdentifierName(IdentifierNameSyntax node)
        {
            base.VisitIdentifierName(node);

            ITypeSymbol? type = this._semanticModel.GetTypeInfo(node).Type;
            if (type == null)
                return;

            this.AddType(type);
        }

        private GenerationItem AddType(ITypeSymbol type)
        {
            GenerationItem generationItem;
            if (!this.GenerationItems.TryGetValue(type, out generationItem))
            {
                generationItem = new GenerationItem(type);
                this.GenerationItems.Add(type, generationItem);
            }

            return generationItem;
        }

        private GenerationItem AddMethod(IMethodSymbol symbol)
        {
            GenerationItem item = this.AddType(symbol.ContainingType);
            item.methods.Add(symbol);

            // We also need to generate the parameter and return value types
            this.AddType(symbol.ReturnType);
            
            foreach (IParameterSymbol parameter in symbol.Parameters)
            {
                this.AddType(parameter.Type);
            }

            return item;
        }

        private GenerationItem AddProperty(IPropertySymbol symbol)
        {
            GenerationItem item = this.AddType(symbol.ContainingType);
            item.properties.Add(symbol);

            // We also need to generate the property type.
            this.AddType(symbol.Type);

            return item;
        }

        private SemanticModel _semanticModel;
    }
}
