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
        public static void Generate(SourceProductionContext context, Compilation compilation, ImmutableArray<TypeDefinition?> typeDefinitions)
        {
            var interopFunctions = typeDefinitions.SelectMany(typeDefinition => typeDefinition == null ? new List<InteropFunction>() : typeDefinition.interopFunctions);
            var items = interopFunctions.Select(function =>
            {
                CSharpType type = CSharpType.FromSymbol(compilation, function.Type);

                string delegateName = $"{type.GetFullyQualifiedName().Replace(".", "_")}_{function.Method.Name}";

                CSharpType returnType = CSharpType.FromSymbol(compilation, function.Method.ReturnType);
                CSharpType interopReturnType = returnType.AsInteropType();
                string interopReturnTypeString = interopReturnType.GetFullyQualifiedName();

                var parameters = function.Method.Parameters.Select(parameter => (Name: parameter.Name, Type: CSharpType.FromSymbol(compilation, parameter.Type)));
                var interopParameters = parameters.Select(parameter => (Name: parameter.Name, Type: parameter.Type, InteropType: parameter.Type.AsInteropType()));

                string callParameterList = string.Join(", ", interopParameters.Select(parameter => parameter.Type.GetConversionFromInteropType(parameter.Name)));

                string invocationTarget = type.GetFullyQualifiedName();
                if (!function.Method.IsStatic)
                {
                    interopParameters = new[] { (Name: "thiz", Type: type, InteropType: type.AsInteropType()) }.Concat(interopParameters);
                    invocationTarget = $"(({type.GetFullyQualifiedName()}?)ObjectHandleUtility.GetObjectFromHandle(thiz))";
                }

                string interopParameterList = string.Join(", ", interopParameters.Select(parameter => $"{parameter.InteropType.GetFullyQualifiedName()} {parameter.Name}"));

                string implementation;
                if (returnType.Symbol.SpecialType == SpecialType.System_Void)
                {
                    implementation =
                        $$"""
                        {{invocationTarget}}.{{function.Method.Name}}({{callParameterList}});
                        """;
                }
                else
                {
                    implementation =
                        $$"""
                        var result = {{invocationTarget}}.{{function.Method.Name}}({{callParameterList}});
                        return {{returnType.GetConversionToInteropType("result")}};
                        """;
                }

                return (
                    DelegateName: delegateName,
                    Delegates: 
                        $$"""
                        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
                        private delegate {{interopReturnTypeString}} {{delegateName}}Type({{interopParameterList}});
                        private static readonly {{delegateName}}Type {{delegateName}}Delegate = new {{delegateName}}Type({{delegateName}});
                        private static {{interopReturnTypeString}} {{delegateName}}({{interopParameterList}})
                        {
                          {{implementation.Replace(Environment.NewLine, Environment.NewLine + "  ")}}
                        }
                        """);
            });

            int count = items.Count();

            string source =
                $$"""
                using System.Runtime.InteropServices;

                namespace Oxidize
                {
                    public class OxidizeInitializer
                    {
                        public static void InitializeOxidize()
                        {
                            unsafe
                            {
                                IntPtr memory = Marshal.AllocHGlobal(sizeof(IntPtr) * {{count}});
                                int i = 0;
                                {{string.Join(Environment.NewLine + "                ", items.Select(items => $"Marshal.WriteIntPtr(memory, (i++) * sizeof(IntPtr), Marshal.GetFunctionPointerForDelegate({items.DelegateName}Delegate));"))}}
                                initializeOxidize(memory, {{count}});
                            }
                        }

                        [DllImport("TestOxidizeNative.dll", CallingConvention=CallingConvention.Cdecl)]
                        private static extern void initializeOxidize(IntPtr functionPointers, int count);

                        {{string.Join(Environment.NewLine + "        ", items.Select(item => item.Delegates.Replace(Environment.NewLine, Environment.NewLine + "        ")))}}
                    }
                }
                """;

            Console.WriteLine(source);
            context.AddSource("OxidizeInitializer", source);
         }
    }
}
