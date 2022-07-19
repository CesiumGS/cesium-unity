using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Oxidize
{
    internal class CppObjectHandle
    {
        public static CppType GetCppType(CppGenerationContext context)
        {
            List<string> ns = new List<string>();
            if (context.BaseNamespace.Length > 0)
                ns.Add(context.BaseNamespace);
            ns.Add("Oxidize");

            return new CppType(CppTypeKind.ClassWrapper, ns, "ObjectHandle", null, 0);
        }

        public static void Generate(CppGenerationContext context)
        {
            CppType type = GetCppType(context);

            string header =
                $$"""
                #pragma once

                namespace {{type.GetFullyQualifiedNamespace(false)}} {

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

                private:
                  void* _handle;
                };

                } // namespace {{type.GetFullyQualifiedNamespace(false)}}
                """;

            string headerPath = Path.Combine(new string[] { context.OutputHeaderDirectory }.Concat(type.Namespaces).ToArray());
            Directory.CreateDirectory(headerPath);
            File.WriteAllText(Path.Combine(headerPath, type.Name + ".h"), header, Encoding.UTF8);

            CppType utilityType = new CppType(CppTypeKind.ClassWrapper, type.Namespaces, "ObjectHandleUtility", null, 0);
            
            HashSet<string> includes = new HashSet<string>();
            type.AddSourceIncludesToSet(includes);
            utilityType.AddSourceIncludesToSet(includes);

            string source =
                $$"""
                {{string.Join(Environment.NewLine, includes.Select(include => $"#include {include}"))}}

                namespace {{type.GetFullyQualifiedNamespace(false)}} {

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

                } // namespace {{type.GetFullyQualifiedNamespace(false)}}
                """;

            Directory.CreateDirectory(context.OutputSourceDirectory);
            File.WriteAllText(Path.Combine(context.OutputSourceDirectory, type.Name + ".cpp"), source, Encoding.UTF8);
        }
    }
}
