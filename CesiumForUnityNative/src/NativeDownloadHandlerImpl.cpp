#include "NativeDownloadHandlerImpl.h"

using namespace DotNet::CesiumForUnity;

namespace CesiumForUnityNative {

NativeDownloadHandlerImpl::NativeDownloadHandlerImpl(
    const NativeDownloadHandler& handler) {}

void NativeDownloadHandlerImpl::JustBeforeDelete(
    const NativeDownloadHandler& handler) {}

bool NativeDownloadHandlerImpl::ReceiveDataNative(
    const NativeDownloadHandler& handler,
    void* data,
    std::int32_t dataLength) {
  std::byte* p = static_cast<std::byte*>(data);
  this->_data.insert(this->_data.end(), p, p + dataLength);
  return true;
}

gsl::span<const std::byte> NativeDownloadHandlerImpl::getData() const {
  return this->_data;
}

} // namespace CesiumForUnityNative
