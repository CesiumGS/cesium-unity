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
            headerFile.Includes.Add("<cstdint>");
            headerFile.Includes.Add("<atomic>");
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
                  int32_t _appDomainId;
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
            sourceFile.Includes.Add("<cstdint>");
            sourceFile.Includes.Add("<atomic>");
            sourceFile.Includes.Add("<CesiumUtility/Assert.h>");
            sourceNamespace.Members.Add(
                $$"""
                ObjectHandle::ObjectHandle() noexcept : _handle(nullptr), _appDomainId(ObjectHandleUtility::GetCurrentAppDomainId()) {}

                ObjectHandle::ObjectHandle(void* handle) noexcept : _handle(handle), _appDomainId(ObjectHandleUtility::GetCurrentAppDomainId()) {
                }

                ObjectHandle::ObjectHandle(const ObjectHandle& rhs) noexcept
                    : _handle(nullptr), _appDomainId(ObjectHandleUtility::GetCurrentAppDomainId()) {
                  void* handle = rhs.GetRaw();
                  if (handle != nullptr)
                    this->_handle = ObjectHandleUtility::CopyHandle(handle);
                }

                ObjectHandle::ObjectHandle(ObjectHandle&& rhs) noexcept : _handle(rhs._handle), _appDomainId(rhs._appDomainId) {
                  rhs._handle = nullptr;
                }

                ObjectHandle::~ObjectHandle() noexcept {
                  void* handle = this->GetRaw();
                  if (handle != nullptr)
                    ObjectHandleUtility::FreeHandle(handle);
                }

                ObjectHandle& ObjectHandle::operator=(const ObjectHandle& rhs) noexcept {
                  if (&rhs != this) {
                    void* handle = this->GetRaw();
                    if (handle != nullptr)
                      ObjectHandleUtility::FreeHandle(handle);
                    
                    void* rhsHandle = rhs.GetRaw();
                    if (rhsHandle != nullptr) {
                      this->_handle = ObjectHandleUtility::CopyHandle(rhsHandle);
                    } else {
                      this->_handle = nullptr;
                    }

                    this->_appDomainId = ObjectHandleUtility::GetCurrentAppDomainId();
                  }

                  return *this;
                }

                ObjectHandle& ObjectHandle::operator=(ObjectHandle&& rhs) noexcept {
                  if (&rhs != this) {
                    void* handle = this->GetRaw();
                    if (handle != nullptr)
                        ObjectHandleUtility::FreeHandle(handle);
                    this->_handle = rhs.GetRaw();
                    rhs._handle = nullptr;
                  }

                  return *this;
                }

                void* ObjectHandle::GetRaw() const {
                  CESIUM_ASSERT(this->_handle == nullptr || this->_appDomainId == ObjectHandleUtility::GetCurrentAppDomainId());
                  return this->_handle;
                }

                void* ObjectHandle::Release() {
                  void* handle = this->GetRaw();
                  this->_handle = nullptr;
                  return handle;
                }
                """);
        }
    }
}
