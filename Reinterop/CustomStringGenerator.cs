using Microsoft.CodeAnalysis;

namespace Oxidize
{
    internal class CustomStringGenerator : ICustomGenerator
    {
        public GeneratedResult? Generate(CppGenerationContext context, TypeToGenerate type, GeneratedResult? generated)
        {
            if (generated == null)
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
