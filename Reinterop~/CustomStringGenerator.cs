using Microsoft.CodeAnalysis;
using System.Diagnostics;

namespace Reinterop
{
    internal class CustomStringGenerator : ICustomGenerator
    {
        public IEnumerable<TypeToGenerate> GetDependencies(ReinteropGenerationContext context)
        {
            INamedTypeSymbol? encoding = context.Compilation.GetTypeByMetadataName("System.Text.Encoding");
            if (encoding == null)
                yield break;

            IPropertySymbol? utf8 = CSharpTypeUtility.FindMembers(encoding, "UTF8").FirstOrDefault(
                member => member is IPropertySymbol
            ) as IPropertySymbol;
            IMethodSymbol? getString = CSharpTypeUtility.FindMembers(encoding, "GetString").FirstOrDefault(
                member => member is IMethodSymbol method &&
                method.Parameters.Length == 2 &&
                method.Parameters[0].Type.TypeKind == TypeKind.Pointer &&
                method.Parameters[0].Type is IPointerTypeSymbol pointer &&
                pointer.PointedAtType.SpecialType == SpecialType.System_Byte &&
                method.Parameters[1].Type.SpecialType == SpecialType.System_Int32
            ) as IMethodSymbol;
            if (utf8 == null || getString == null)
                yield break;

            INamedTypeSymbol? marshal = context.Compilation.GetTypeByMetadataName("System.Runtime.InteropServices.Marshal");
            if (marshal == null)
                yield break;

            IMethodSymbol? stringToCoTaskMemUTF8 = CSharpTypeUtility.FindMembers(marshal, "StringToCoTaskMemUTF8").FirstOrDefault(
                member => member is IMethodSymbol method &&
                method.Parameters.Length == 1 &&
                method.Parameters[0].Type.SpecialType == SpecialType.System_String
            ) as IMethodSymbol;
            IMethodSymbol? freeCoTaskMem = CSharpTypeUtility.FindMembers(marshal, "FreeCoTaskMem").FirstOrDefault(
                member => member is IMethodSymbol method &&
                method.Parameters.Length == 1 &&
                method.Parameters[0].Type.SpecialType == SpecialType.System_IntPtr
            ) as IMethodSymbol;
            if (stringToCoTaskMemUTF8 == null || freeCoTaskMem == null)
                yield break;

            TypeToGenerate generateEncoding = new TypeToGenerate(encoding);
            generateEncoding.Properties.Add(utf8);
            generateEncoding.Methods.Add(getString);
            yield return generateEncoding;

            TypeToGenerate generateMarshal = new TypeToGenerate(marshal);
            generateMarshal.Methods.Add(stringToCoTaskMemUTF8);
            generateMarshal.Methods.Add(freeCoTaskMem);
            yield return generateMarshal;
        }

        public GeneratedResult? Generate(ReinteropGenerationContext context, TypeToGenerate type, GeneratedResult? generated)
        {
            // This generator only operates on strings.
            if (generated == null || type.Type.SpecialType != SpecialType.System_String)
                return generated;

            // If the dependencies list is empty, some dependencies failed to resolve so don't try to generate.
            if (!this.GetDependencies(context).Any())
                return generated;

            INamedTypeSymbol? encoding = context.Compilation.GetTypeByMetadataName("System.Text.Encoding");
            if (encoding == null)
                return generated;

            CppType encodingWrapper = CppType.FromCSharp(context, CSharpType.FromSymbol(context, encoding));

            INamedTypeSymbol? marshal = context.Compilation.GetTypeByMetadataName("System.Runtime.InteropServices.Marshal");
            if (marshal == null)
                return generated;

            CppType marshalWrapper = CppType.FromCSharp(context, CSharpType.FromSymbol(context, marshal));
            static string GetHeaderInclude(CppType cppType) => cppType.HeaderOverride ?? $"<{string.Join("/", cppType.Namespaces.Append(cppType.Name))}.h>";

            // String result = Encoding::UTF8().GetString(
            //     const_cast<std::uint8_t*>(reinterpret_cast<const std::uint8_t*>(s.data())),
            //     std::int32_t(s.size()));
            CppExpression getString = new CppCall(
                new CppMemberAccess(
                    new CppCall(new CppIdentifier(encodingWrapper.GetFullyQualifiedName() + "::UTF8"), []),
                    "GetString"),
                [
                    CppCast.Const(
                        CppType.UInt8.AsPointer(),
                        CppCast.Reinterpret(
                            CppType.UInt8.AsConstPointer(),
                            new CppCall(new CppMemberAccess(new CppIdentifier("s"), "data"), []))),
                    new CppCall(
                        new CppIdentifier("std::int32_t"),
                        [new CppCall(new CppMemberAccess(new CppIdentifier("s"), "size"), [])])
                ])
            {
                RequiredIncludes = ["<string>", "<cstdint>", GetHeaderInclude(encodingWrapper)]
            };

            CppFunction stringConstructorWrapper = new CppFunction(context, generated.Type, generated.Type.Name)
                .Parameters([new CppParameter(CppType.StlString.AsConstReference(), "s")])
                .MemberInitializers([new CppMemberInitializer("_handle")])
                .DefinitionBody([
                    new CppVariableDeclaration(generated.Type, "result", getString),
                    new CppAssignment(
                        new CppPointerMemberAccess(CppIdentifier.This, "_handle"),
                        new CppMove(new CppMemberAccess(new CppIdentifier("result"), "_handle")))
                ]);
            generated.ExtraCppFunctions.Add(stringConstructorWrapper);

            // Add a ToStlString method
            CppExpression emptyStdString = new CppCall(new CppIdentifier("std::string"), [])
            {
                RequiredIncludes = ["<string>"]
            };
            CppExpression freeCoTaskMemCall = new CppCall(
                new CppIdentifier($"{marshalWrapper.GetFullyQualifiedName()}::FreeCoTaskMem"),
                [new CppIdentifier("p")])
            {
                RequiredIncludes = [GetHeaderInclude(marshalWrapper)]
            };

            CppFunction toStlString = new CppFunction(context, generated.Type, "ToStlString")
                .ReturnType(CppType.StlString)
                .DefinitionBody([
                    new CppIf(
                        new CppBinary("==", new CppUnary("*", CppIdentifier.This), CppLiteral.Nullptr),
                        [new CppReturn(emptyStdString)]),
                    new CppVariableDeclaration(
                        CppType.VoidPointer,
                        "p",
                        new CppCall(
                            new CppIdentifier($"{marshalWrapper.GetFullyQualifiedName()}::StringToCoTaskMemUTF8"),
                            [new CppUnary("*", CppIdentifier.This)])
                        {
                            RequiredIncludes = [GetHeaderInclude(marshalWrapper)]
                        }),
                    new CppTry(
                        [
                            new CppVariableDeclaration(CppType.StlString, "result", CppCast.Static(CppType.Char.AsPointer(), new CppIdentifier("p"))),
                            new CppExpressionStatement(freeCoTaskMemCall),
                            new CppReturn(new CppIdentifier("result"))
                        ],
                        [
                            new CppCatch(null, null, [
                                new CppExpressionStatement(freeCoTaskMemCall),
                                new CppThrow()
                            ])
                        ])
                ]);
            generated.ExtraCppFunctions.Add(toStlString);

            return generated;
        }
    }
}
