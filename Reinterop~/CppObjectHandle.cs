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

                  static void startNewAppDomain();
                  static void endCurrentAppDomain();
                
                private:
                  void* _handle;
                  int32_t _appDomainIndex;
                  
                  static std::atomic<int32_t> _runningAppDomainIndex;
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
                // When this is negative, it indicates either:
                // - initializeReinterop hasn't been called yet at all, or
                // - The current (previous) AppDomain is unloading or unloaded, and initializeReinterop
                //   hasn't been called for the new one yet.
                std::atomic<int32_t> ObjectHandle::_runningAppDomainIndex = -1;

                ObjectHandle::ObjectHandle() noexcept : _handle(nullptr), _appDomainIndex(0) {}

                ObjectHandle::ObjectHandle(void* handle) noexcept : _handle(handle), _appDomainIndex(_runningAppDomainIndex) {
                  // We shouldn't be creating new references to managed objects when the _runningAppDomainIndex is negative.
                  CESIUM_ASSERT(this->_handle == nullptr || this->_appDomainIndex > 0);
                }

                ObjectHandle::ObjectHandle(const ObjectHandle& rhs) noexcept
                    : _handle(nullptr), _appDomainIndex(_runningAppDomainIndex) {
                  void* handle = rhs.GetRaw();
                  if (handle != nullptr)
                    this->_handle = ObjectHandleUtility::CopyHandle(handle);
                }

                ObjectHandle::ObjectHandle(ObjectHandle&& rhs) noexcept : _handle(rhs._handle), _appDomainIndex(rhs._appDomainIndex) {
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
                    if (handle != nullptr) {
                      ObjectHandleUtility::FreeHandle(handle);
                    }
                    
                    void* rhsHandle = rhs.GetRaw();
                    if (rhsHandle != nullptr) {
                      this->_handle = ObjectHandleUtility::CopyHandle(rhsHandle);
                    } else {
                      this->_handle = nullptr;
                    }

                    this->_appDomainIndex = _runningAppDomainIndex;
                  }

                  return *this;
                }

                ObjectHandle& ObjectHandle::operator=(ObjectHandle&& rhs) noexcept {
                  if (&rhs != this) {
                    void* handle = this->GetRaw();
                    if (handle != nullptr)
                        ObjectHandleUtility::FreeHandle(handle);
                    this->_handle = rhs.GetRaw();
                    this->_appDomainIndex = rhs._appDomainIndex;
                    rhs._handle = nullptr;
                  }

                  return *this;
                }

                void* ObjectHandle::GetRaw() const {
                  if (this->_handle == nullptr || this->_appDomainIndex == _runningAppDomainIndex)
                    return this->_handle;

                  // Handle is from the "wrong" AppDomain. This is not an error if the current AppDomain is still
                  // finalizing, but in that case we do _not_ want to access the managed object. Treat it as if
                  // it's null.
                  CESIUM_ASSERT(ObjectHandleUtility::IsAppDomainFinalizingForUnload());
                  return nullptr;
                }

                void* ObjectHandle::Release() {
                  void* handle = this->GetRaw();
                  this->_handle = nullptr;
                  return handle;
                }

                /*static*/ void ObjectHandle::startNewAppDomain() {
                    _runningAppDomainIndex = -_runningAppDomainIndex;
                }

                /*static*/ void ObjectHandle::endCurrentAppDomain() {
                    _runningAppDomainIndex = -(_runningAppDomainIndex + 1);
                }                
                """);
        }
    }
}
