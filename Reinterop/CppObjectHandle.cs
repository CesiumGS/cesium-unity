namespace Reinterop
{
    internal class CppObjectHandle
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

            return new CppType(InteropTypeKind.ClassWrapper, ns, "ObjectHandle", null, 0);
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
                class ObjectHandle {
                public:
                  ObjectHandle() noexcept;
                  explicit ObjectHandle(void* handle) noexcept;
                  ObjectHandle(const ObjectHandle& rhs) noexcept;
                  ObjectHandle(ObjectHandle&& rhs) noexcept;
                  ~ObjectHandle() noexcept;
                
                  ObjectHandle& operator=(const ObjectHandle& rhs) noexcept;
                  ObjectHandle& operator=(ObjectHandle&& rhs) noexcept;
                
                  void* GetRaw() const;

                  // Return the underlying raw handle and set the object's to nullptr
                  // so it will not be released when this object is destroyed.
                  void* Release();
                
                private:
                  void* _handle;
                };
                """);

            CppType utilityType = new CppType(InteropTypeKind.ClassWrapper, type.Namespaces, "ObjectHandleUtility", null, 0);

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
            utilityType.AddSourceIncludesToSet(sourceFile.Includes);

            var sourceNamespace = sourceFile.GetNamespace(type.GetFullyQualifiedNamespace(false));
            sourceNamespace.Members.Add(
                $$"""
                ObjectHandle::ObjectHandle() noexcept : _handle(nullptr) {}

                ObjectHandle::ObjectHandle(void* handle) noexcept : _handle(handle) {}

                ObjectHandle::ObjectHandle(const ObjectHandle& rhs) noexcept
                    : _handle(ObjectHandleUtility::CopyHandle(rhs._handle)) {}

                ObjectHandle::ObjectHandle(ObjectHandle&& rhs) noexcept : _handle(rhs._handle) {
                  rhs._handle = nullptr;
                }

                ObjectHandle::~ObjectHandle() noexcept {
                  ObjectHandleUtility::FreeHandle(this->_handle);
                }

                ObjectHandle& ObjectHandle::operator=(const ObjectHandle& rhs) noexcept {
                  if (&rhs != this) {
                    ObjectHandleUtility::FreeHandle(this->_handle);
                    this->_handle = ObjectHandleUtility::CopyHandle(rhs._handle);
                  }

                  return *this;
                }

                ObjectHandle& ObjectHandle::operator=(ObjectHandle&& rhs) noexcept {
                  if (&rhs != this) {
                    ObjectHandleUtility::FreeHandle(this->_handle);
                    this->_handle = rhs._handle;
                    rhs._handle = nullptr;
                  }

                  return *this;
                }

                void* ObjectHandle::GetRaw() const { return this->_handle; }

                void* ObjectHandle::Release() {
                  void* handle = this->_handle;
                  this->_handle = nullptr;
                  return handle;
                }
                """);
        }
    }
}
