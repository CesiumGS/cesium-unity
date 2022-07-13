using System;
using System.Collections.Generic;
using System.Text;

namespace Oxidize
{
    internal enum CppTranslatedTypeKind
    {
        Unknown,
        Primitive,
        BlittableStruct,
        NonBlittableStruct,
        Class,
        Pointer
    }

    internal struct CppTranslatedType
    {
        public string? Type;
        public List<string>? DeclarationHeaders;
        public List<string>? DefinitionHeaders;
        public List<string>? ForwardDeclarations;

        public CppTranslatedTypeKind Kind;

        public static CppTranslatedType Unknown
        {
            get
            {
                return new CppTranslatedType {
                    Kind = CppTranslatedTypeKind.Unknown
                };
            }
        }
    }
}
