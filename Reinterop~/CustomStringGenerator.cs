using Microsoft.CodeAnalysis;
using System.Diagnostics;

namespace Reinterop
{
    internal class CustomStringGenerator : ICustomGenerator
    {
        public IEnumerable<TypeToGenerate> GetDependencies(CppGenerationContext context)
        {
            INamedTypeSymbol? encoding = context.Compilation.GetTypeByMetadataName("System.Text.Encoding");
            if (encoding == null)
                yield break;

            IPropertySymbol? utf8 = CSharpTypeUtility.FindMembers(encoding, "UTF8").Where(
                member => member is IPropertySymbol
            ).FirstOrDefault() as IPropertySymbol;
            IMethodSymbol? getString = CSharpTypeUtility.FindMembers(encoding, "GetString").Where(
                member => member is IMethodSymbol method &&
                method.Parameters.Length == 2 &&
                method.Parameters[0].Type.TypeKind == TypeKind.Pointer &&
                method.Parameters[0].Type is IPointerTypeSymbol pointer &&
                pointer.PointedAtType.SpecialType == SpecialType.System_Byte &&
                method.Parameters[1].Type.SpecialType == SpecialType.System_Int32
            ).FirstOrDefault() as IMethodSymbol;
            if (utf8 == null || getString == null)
                yield break;

            INamedTypeSymbol? marshal = context.Compilation.GetTypeByMetadataName("System.Runtime.InteropServices.Marshal");
            if (marshal == null)
                yield break;

            IMethodSymbol? stringToCoTaskMemUTF8 = CSharpTypeUtility.FindMembers(marshal, "StringToCoTaskMemUTF8").Where(
                member => member is IMethodSymbol method &&
                method.Parameters.Length == 1 &&
                method.Parameters[0].Type.SpecialType == SpecialType.System_String
            ).FirstOrDefault() as IMethodSymbol;
            IMethodSymbol? freeCoTaskMem = CSharpTypeUtility.FindMembers(marshal, "FreeCoTaskMem").Where(
                member => member is IMethodSymbol method &&
                method.Parameters.Length == 1 &&
                method.Parameters[0].Type.SpecialType == SpecialType.System_IntPtr
            ).FirstOrDefault() as IMethodSymbol;
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

        public GeneratedResult? Generate(CppGenerationContext context, TypeToGenerate type, GeneratedResult? generated)
        {
            // This generator only operates on strings.
            if (generated == null || type.Type.SpecialType != SpecialType.System_String)
                return generated;

            // If the dependencies list is empty, some dependencies failed to resolve so don't try to generate.
            if (!this.GetDependencies(context).Any())
                return generated;

            // Add a constructor taking std::string.
            generated.CppDeclaration.Elements.Add(new(
                Content: $"String(const ::std::string& s);",
                AdditionalIncludes: new[] { "<string>" }));

            CppType stringWrapper = CppType.FromCSharp(context, context.Compilation.GetSpecialType(SpecialType.System_String));

            INamedTypeSymbol? encoding = context.Compilation.GetTypeByMetadataName("System.Text.Encoding");
            if (encoding == null)
                return generated;

            CppType encodingWrapper = CppType.FromCSharp(context, encoding);

            INamedTypeSymbol? marshal = context.Compilation.GetTypeByMetadataName("System.Runtime.InteropServices.Marshal");
            if (marshal == null)
                return generated;

            CppType marshalWrapper = CppType.FromCSharp(context, marshal);

            generated.CppDefinition.Elements.Add(new(
                Content:
                    $$"""
                    String::String(const ::std::string& s) : _handle() {
                      String result = {{encodingWrapper.GetFullyQualifiedName()}}::UTF8().GetString(
                        const_cast<std::uint8_t*>(reinterpret_cast<const std::uint8_t*>(s.data())),
                        std::int32_t(s.size()));
                      this->_handle = std::move(result._handle);
                    }
                    """,
                TypeDefinitionsReferenced: new[] { encodingWrapper }));

            // Add a ToStlString method
            generated.CppDeclaration.Elements.Add(new(
                Content: $"std::string ToStlString() const;",
                AdditionalIncludes: new[] { "<string>" }));

            generated.CppDefinition.Elements.Add(new(
                Content:
                    $$"""
                    std::string String::ToStlString() const {
                      if (*this == nullptr)
                        return std::string();

                      void* p = {{marshalWrapper.GetFullyQualifiedName()}}::StringToCoTaskMemUTF8(*this);
                      try {
                        std::string result = static_cast<char*>(p);
                        {{marshalWrapper.GetFullyQualifiedName()}}::FreeCoTaskMem(p);
                        return result;
                      } catch (...) {
                        {{marshalWrapper.GetFullyQualifiedName()}}::FreeCoTaskMem(p);
                        throw;
                      }
                    }
                    """,
                TypeDefinitionsReferenced: new[] { marshalWrapper }
            ));

            return generated;
        }
    }
}
