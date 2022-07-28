using Microsoft.CodeAnalysis;

namespace Oxidize
{
    internal class Interop
    {
        /// <summary>
        /// Creates a C# delegate, field of delegate type, and method/constructor/property to be invoked
        /// by the delegate in order to provide a way for C++ code to call the method/constructor/property.
        /// </summary>
        /// <param name="compilation">The compilation context.</param>
        /// <param name="ownerType">The type that owns the method, constructor, or property.</param>
        /// <param name="name">The name of the method, constructor, or property. This is used to name the delegate, field, and method, and need not match the target exactly.</param>
        /// <param name="invocationTarget">The C# code that is the target of the invocation. For a static method, sh</param>
        /// <param name="returnType"></param>
        /// <param name="otherParameters"></param>
        /// <returns></returns>
        public static GeneratedCSharpDelegateInit CreateCSharpDelegateInit(
            Compilation compilation,
            ITypeSymbol ownerType,
            IMethodSymbol method)
        {
            CSharpType csType = CSharpType.FromSymbol(compilation, ownerType);
            var parameters = method.Parameters;
            var returnType = method.ReturnType;

            IPropertySymbol? property = method.AssociatedSymbol as IPropertySymbol;

            string descriptiveName;
            string accessName;
            if (method.Name == ".ctor")
            {
                descriptiveName = "Constructor";
                accessName = $"new {csType.GetFullyQualifiedName()}";
            }
            else if (property != null && ReferenceEquals(property.GetMethod, method))
            {
                descriptiveName = $"Get{property.Name}";
                accessName = property.Name;
            }
            else if (property != null && ReferenceEquals(property.SetMethod, method))
            {
                descriptiveName = $"Set{property.Name}";
                accessName = property.Name;
            }
            else
            {
                descriptiveName = method.Name;
                accessName = method.Name;
            }

            var callParameterDetails = parameters.Select(parameter => (Name: parameter.Name, Type: CSharpType.FromSymbol(compilation, parameter.Type), InteropType: CSharpType.FromSymbol(compilation, parameter.Type).AsInteropType()));
            var interopParameterDetails = callParameterDetails;

            string invocationTarget;
            if (method.Name == ".ctor")
            {
                // New object construction
                invocationTarget = $"new {csType.GetFullyQualifiedName()}";
                returnType = ownerType;
            }
            else if (!method.IsStatic)
            {
                // Instance method or property
                interopParameterDetails = new[] { (Name: "thiz", Type: csType, InteropType: csType.AsInteropType()) }.Concat(interopParameterDetails);
                invocationTarget = $"(({csType.GetFullyQualifiedName()})ObjectHandleUtility.GetObjectFromHandle(thiz)!).{accessName}";
            }
            else
            {
                // Static method or property
                invocationTarget = $"{csType.GetFullyQualifiedName()}.{accessName}";
            }

            CSharpType csReturnType = CSharpType.FromSymbol(compilation, returnType);
            CSharpType csInteropReturnType = csReturnType.AsInteropType();
            string interopReturnTypeString = csInteropReturnType.GetFullyQualifiedName();

            string callParameterList = string.Join(", ", callParameterDetails.Select(parameter => parameter.Type.GetConversionFromInteropType(parameter.Name)));
            string interopParameterList = string.Join(", ", interopParameterDetails.Select(parameter => $"{parameter.InteropType.GetFullyQualifiedName()} {parameter.Name}"));

            string implementation;
            if (property != null && ReferenceEquals(property.GetMethod, method))
            {
                // A property getter.
                implementation =
                    $$"""
                    var result = {{invocationTarget}};
                    return {{csReturnType.GetConversionToInteropType("result")}};
                    """;
            }
            else if (property != null && ReferenceEquals(property.SetMethod, method))
            {
                // A property setter.
                implementation =
                    $$"""
                    {{invocationTarget}} = {{callParameterList}};
                    """;
            }
            else if (returnType.SpecialType == SpecialType.System_Void)
            {
                // Regular method returning void.
                implementation =
                    $$"""
                    {{invocationTarget}}({{callParameterList}});
                    """;
            }
            else
            {
                // Either a constructor or a regular method with a return value.
                implementation =
                    $$"""
                    var result = {{invocationTarget}}({{callParameterList}});
                    return {{csReturnType.GetConversionToInteropType("result")}};
                    """;
            }

            string baseName = $"{csType.GetFullyQualifiedName().Replace(".", "_")}_{descriptiveName}";
            return new GeneratedCSharpDelegateInit(
                // TODO: incorporate parameter types into delegate name to support overloading.
                name: $"{baseName}Delegate",
                content:
                    $$"""
                    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
                    private delegate {{interopReturnTypeString}} {{baseName}}Type({{interopParameterList}});
                    private static readonly {{baseName}}Type {{baseName}}Delegate = new {{baseName}}Type({{baseName}});
                    private static {{interopReturnTypeString}} {{baseName}}({{interopParameterList}})
                    {
                      {{implementation.Replace(Environment.NewLine, Environment.NewLine + "  ")}}
                    }
                    """);
        }

        internal static void GenerateForType(CppGenerationContext context, GenerationItem item, GeneratedResult result)
        {
            string initializeOxidizeHeader = context.BaseNamespace == null ? "<initializeOxidize.h>" : $"<{context.BaseNamespace.Replace("::", "/")}/initializeOxidize.h>";
            result.CppDeclaration.Elements.Add(new(
                Content: "friend void ::initializeOxidize(void** functionPointers, std::int32_t count);",
                IsPrivate: true,
                TypeDeclarationsReferenced: new[] { CppType.Int32 },
                AdditionalIncludes: new[] { initializeOxidizeHeader }));
        }
    }
}
