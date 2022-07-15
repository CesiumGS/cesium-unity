#include "ObjectHandle.h"

namespace {

void freeHandle(void* handle) {
  // TODO
}

void* copyHandle(void* handle) {
  // TODO
  return nullptr;
}

} // namespace

namespace Oxidize {

ObjectHandle::ObjectHandle() noexcept : _handle(nullptr) {}

explicit ObjectHandle::ObjectHandle(void* handle) noexcept : _handle(handle) {}

ObjectHandle::ObjectHandle(const ObjectHandle& rhs) noexcept
    : _handle(copyHandle(rhs._handle)) {}

ObjectHandle::ObjectHandle(ObjectHandle&& rhs) noexcept : _handle(rhs._handle) {
  rhs._handle = nullptr;
}

ObjectHandle::~ObjectHandle() noexcept { freeHandle(this->_handle); }

ObjectHandle& ObjectHandle::operator=(const ObjectHandle& rhs) const noexcept {
  if (&rhs != this) {
    freeHandle(this->_handle);
    this->_handle = copyHandle(rhs._handle);
  }

  return *this;
}

ObjectHandle& ObjectHandle::operator=(ObjectHandle&& rhs) const noexcept {
  if (&rhs != this) {
    freeHandle(this->_handle);
    this->_handle = rhs._handle;
    rhs._handle = nullptr;
  }

  return *this;
}

void* ObjectHandle::GetRaw() const { return this->_handle; }

} // namespace Oxidize
