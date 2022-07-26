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

            string name = method.Name;
            if (name == ".ctor")
            {
                name = "Constructor";
            }
            else if (property != null && ReferenceEquals(property.GetMethod, method))
            {
                name = $"Get{property.Name}";
            }
            else if (property != null && ReferenceEquals(property.SetMethod, method))
            {
                name = $"Set{property.Name}";
            }
            else
            {
                name = method.Name;
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
                invocationTarget = $"(({csType.GetFullyQualifiedName()})ObjectHandleUtility.GetObjectFromHandle(thiz)!).${name}";
            }
            else
            {
                // Static method or property
                invocationTarget = $"{csType.GetFullyQualifiedName()}.{name}";
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
                    {{invocationTarget}}.{{property.Name}} = {{callParameterList}};
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
                // Constructor, or regular method with a return value.
                implementation =
                    $$"""
                    var result = {{invocationTarget}}({{callParameterList}});
                    return {{csReturnType.GetConversionToInteropType("result")}};
                    """;
            }

            string baseName = $"{csType.GetFullyQualifiedName().Replace(".", "_")}_{name}";
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
    }
}
