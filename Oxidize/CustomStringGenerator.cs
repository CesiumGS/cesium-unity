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

            return generated;
        }
    }
}
