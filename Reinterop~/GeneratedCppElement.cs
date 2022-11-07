namespace Reinterop
{
    internal abstract record GeneratedCppElement(
        IEnumerable<CppType>? TypeDeclarationsReferenced = null,
        IEnumerable<CppType>? TypeDefinitionsReferenced = null,
        IEnumerable<string>? AdditionalIncludes = null)
    {
        public IEnumerable<string> AdditionalIncludes { get; init; } =
            AdditionalIncludes == null ? Array.Empty<string>() : AdditionalIncludes;
        public IEnumerable<CppType> TypeDeclarationsReferenced { get; init; } =
            TypeDeclarationsReferenced == null ? Array.Empty<CppType>() : TypeDeclarationsReferenced;
        public IEnumerable<CppType> TypeDefinitionsReferenced { get; init; } =
            TypeDefinitionsReferenced == null ? Array.Empty<CppType>() : TypeDefinitionsReferenced;

        public void AddIncludesToSet(ISet<string> set)
        {
            set.UnionWith(this.AdditionalIncludes);

            foreach (CppType type in this.TypeDeclarationsReferenced)
            {
                if (!type.CanBeForwardDeclared)
                    type.AddSourceIncludesToSet(set);
            }

            foreach (CppType type in this.TypeDefinitionsReferenced)
            {
                type.AddSourceIncludesToSet(set);
            }
        }

        public void AddForwardDeclarationsToSet(ISet<string> set)
        {
            foreach (CppType type in this.TypeDeclarationsReferenced)
            {
                if (type.CanBeForwardDeclared)
                    type.AddForwardDeclarationsToSet(set);
                else if (type.GenericArguments != null && type.GenericArguments.Count > 0)
                {
                    foreach (CppType genericType in type.GenericArguments)
                    {
                      genericType.AddForwardDeclarationsToSet(set);
                    }
                }
            }
        }
    }
}
