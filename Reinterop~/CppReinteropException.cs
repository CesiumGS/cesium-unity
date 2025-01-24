namespace Reinterop
{
    internal class CppReinteropException
    {
        public static CppType GetCppType(CppGenerationContext context)
        {
            List<string> ns = new List<string>();
            if (context.BaseNamespace.Length > 0)
                ns.Add(context.BaseNamespace);
            ns.Add("Reinterop");

            // If the first two namespaces are identical, remove the duplication.
            // This is to avoid `Reinterop::Reinterop`.
            if (ns.Count >= 2 && ns[0] == ns[1])
                ns.RemoveAt(0);

            return new CppType(InteropTypeKind.ClassWrapper, ns, "ReinteropException", null, 0);
        }

        public static void Generate(CppGenerationContext context, IDictionary<string, CppSourceFile> sourceFiles)
        {
            CppType type = GetCppType(context);

            string headerPath = Path.Combine(new[] { "include" }.Concat(type.Namespaces).Concat(new[] { type.Name + ".h" }).ToArray());

            CppSourceFile? headerFile = null;
            if (!sourceFiles.TryGetValue(headerPath, out headerFile))
            {
                headerFile = new CppSourceFile();
                headerFile.IsHeaderFile = true;
                headerFile.Filename = headerPath;
                sourceFiles.Add(headerPath, headerFile);
            }

            var headerNamespace = headerFile.GetNamespace(type.GetFullyQualifiedNamespace(false));
            headerNamespace.Members.Add(
                $$"""
                class ReinteropException : public std::runtime_error {
                public:
                  ReinteropException(const DotNet::System::Exception& exception);
                  const ::DotNet::System::Exception& GetDotNetException() const;
                
                private:
                  ::DotNet::System::Exception _exception;
                };
                """);

            headerFile.Includes.Add("<stdexcept>");
            
            CppType exceptionType = new CppType(InteropTypeKind.ClassWrapper, new[] { "DotNet", "System" }, "Exception", null, 0);
            exceptionType.AddSourceIncludesToSet(headerFile.Includes);

            string sourcePath = Path.Combine("src", type.Name + ".cpp");

            CppSourceFile? sourceFile = null;
            if (!sourceFiles.TryGetValue(sourcePath, out sourceFile))
            {
                sourceFile = new CppSourceFile();
                sourceFile.IsHeaderFile = false;
                sourceFile.Filename = sourcePath;
                sourceFiles.Add(sourcePath, sourceFile);
            }

            type.AddSourceIncludesToSet(sourceFile.Includes);

            CppType stringType = new CppType(InteropTypeKind.ClassWrapper, new[] { "DotNet", "System" }, "String", null, 0);
            stringType.AddSourceIncludesToSet(sourceFile.Includes);

            var sourceNamespace = sourceFile.GetNamespace(type.GetFullyQualifiedNamespace(false));
            sourceNamespace.Members.Add(
                $$"""
                ReinteropException::ReinteropException(const DotNet::System::Exception& exception)
                    : std::runtime_error(exception.Message().ToStlString()),
                    _exception(exception) {}
                
                const ::DotNet::System::Exception& ReinteropException::GetDotNetException() const {
                    return this->_exception;
                }
                """);
        }
    }
}
