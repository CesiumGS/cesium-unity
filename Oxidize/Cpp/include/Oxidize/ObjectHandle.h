#pragma once

namespace Oxidize {

class ObjectHandle {
public:
  ObjectHandle() noexcept;
  explicit ObjectHandle(void* handle) noexcept;
  ObjectHandle(const ObjectHandle& rhs) noexcept;
  ObjectHandle(ObjectHandle&& rhs) noexcept;
  ~ObjectHandle() noexcept;

  ObjectHandle& operator=(const ObjectHandle& rhs) const noexcept;
  ObjectHandle& operator=(ObjectHandle&& rhs) const noexcept;

  void* GetRaw() const;

private:
  void* _handle;
};

} // namespace Oxidize
