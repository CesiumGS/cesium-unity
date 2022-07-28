using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection.Metadata;
using System.Text;

namespace Oxidize
{
    internal class CSharpCodeGenerator
    {
        struct InteropItem
        {
            public string Name;
            public string Content;
        }

        public static void Generate(SourceProductionContext context, Compilation compilation, ImmutableArray<GeneratedResult?> results)
        {
            GeneratedCSharpInit combined = GeneratedCSharpInit.Merge(results.Select(result => result == null ? new GeneratedCSharpInit() : result.CSharpInit));
            Console.WriteLine(combined.ToSourceFileString());
            context.AddSource("OxidizeInitializer", combined.ToSourceFileString());
        }

        private static InteropItem CreateMethodInterop(Compilation compilation, InteropMethod interop)
        {
            CSharpType type = CSharpType.FromSymbol(compilation, interop.Type);

            string delegateName = $"{type.GetFullyQualifiedName().Replace(".", "_")}_{interop.Method.Name}";

            CSharpType returnType = CSharpType.FromSymbol(compilation, interop.Method.ReturnType);
            CSharpType interopReturnType = returnType.AsInteropType();
            string interopReturnTypeString = interopReturnType.GetFullyQualifiedName();

            var parameters = interop.Method.Parameters.Select(parameter => (Name: parameter.Name, Type: CSharpType.FromSymbol(compilation, parameter.Type)));
            var interopParameters = parameters.Select(parameter => (Name: parameter.Name, Type: parameter.Type, InteropType: parameter.Type.AsInteropType()));

            string callParameterList = string.Join(", ", interopParameters.Select(parameter => parameter.Type.GetConversionFromInteropType(parameter.Name)));

            string invocationTarget = type.GetFullyQualifiedName();
            if (!interop.Method.IsStatic)
            {
                interopParameters = new[] { (Name: "thiz", Type: type, InteropType: type.AsInteropType()) }.Concat(interopParameters);
                invocationTarget = $"(({type.GetFullyQualifiedName()})ObjectHandleUtility.GetObjectFromHandle(thiz)!)";
            }

            string interopParameterList = string.Join(", ", interopParameters.Select(parameter => $"{parameter.InteropType.GetFullyQualifiedName()} {parameter.Name}"));


            string implementation;
            IPropertySymbol? property = interop.Method.AssociatedSymbol as IPropertySymbol;
            if (property != null && ReferenceEquals(property.GetMethod, interop.Method))
            {
                // A property getter.
                implementation =
                    $$"""
                    var result = {{invocationTarget}}.{{property.Name}};
                    return {{returnType.GetConversionToInteropType("result")}};
                    """;
            }
            else if (property != null && ReferenceEquals(property.SetMethod, interop.Method))
            {
                // A property setter.
                implementation =
                    $$"""
                    {{invocationTarget}}.{{property.Name}} = {{callParameterList}};
                    """;
            }
            else if (returnType.Symbol.SpecialType == SpecialType.System_Void)
            {
                // Regular method returning void.
                implementation =
                    $$"""
                    {{invocationTarget}}.{{interop.Method.Name}}({{callParameterList}});
                    """;
            }
            else
            {
                // Regular method with a return value.
                implementation =
                    $$"""
                    var result = {{invocationTarget}}.{{interop.Method.Name}}({{callParameterList}});
                    return {{returnType.GetConversionToInteropType("result")}};
                    """;
            }

            return new InteropItem
            {
                Name = delegateName,
                Content =
                    $$"""
                    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
                    private delegate {{interopReturnTypeString}} {{delegateName}}Type({{interopParameterList}});
                    private static readonly {{delegateName}}Type {{delegateName}}Delegate = new {{delegateName}}Type({{delegateName}});
                    private static {{interopReturnTypeString}} {{delegateName}}({{interopParameterList}})
                    {
                      {{implementation.Replace(Environment.NewLine, Environment.NewLine + "  ")}}
                    }
                    """
            };
        }

        private static InteropItem CreateConstructorInterop(Compilation compilation, InteropConstructor interop)
        {
            CSharpType type = CSharpType.FromSymbol(compilation, interop.Type);

            string delegateName = $"{type.GetFullyQualifiedName().Replace(".", "_")}_Constructor";

            CSharpType interopReturnType = type.AsInteropType();
            string interopReturnTypeString = interopReturnType.GetFullyQualifiedName();

            var parameters = interop.Constructor.Parameters.Select(parameter => (Name: parameter.Name, Type: CSharpType.FromSymbol(compilation, parameter.Type)));
            var interopParameters = parameters.Select(parameter => (Name: parameter.Name, Type: parameter.Type, InteropType: parameter.Type.AsInteropType()));

            string callParameterList = string.Join(", ", interopParameters.Select(parameter => parameter.Type.GetConversionFromInteropType(parameter.Name)));

            string invocationTarget = type.GetFullyQualifiedName();
            string interopParameterList = string.Join(", ", interopParameters.Select(parameter => $"{parameter.InteropType.GetFullyQualifiedName()} {parameter.Name}"));


            string implementation =
                $$"""
                var result = new {{invocationTarget}}({{callParameterList}});
                return {{type.GetConversionToInteropType("result")}};
                """;

            return new InteropItem
            {
                Name = delegateName,
                Content =
                    $$"""
                    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
                    private delegate {{interopReturnTypeString}} {{delegateName}}Type({{interopParameterList}});
                    private static readonly {{delegateName}}Type {{delegateName}}Delegate = new {{delegateName}}Type({{delegateName}});
                    private static {{interopReturnTypeString}} {{delegateName}}({{interopParameterList}})
                    {
                      {{implementation.Replace(Environment.NewLine, Environment.NewLine + "  ")}}
                    }
                    """
            };
        }
    }
}
