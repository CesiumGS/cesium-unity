#include "NativeDownloadHandler.h"

namespace CesiumForUnity {

CESIUM_FOR_UNITY_NATIVE_DOWNLOAD_HANDLER_DEFAULT_CONTENTS_DEFINITION
CESIUM_FOR_UNITY_NATIVE_DOWNLOAD_HANDLER_DEFAULT_CONSTRUCTOR_DEFINITION

NativeDownloadHandler::NativeDownloadHandler()
    : System::IDisposable(nullptr),
      UnityEngine::Networking::DownloadHandler(nullptr),
      UnityEngine::Networking::DownloadHandlerScript(nullptr),
      AbstractBaseNativeDownloadHandler(nullptr),
      BaseNativeDownloadHandler() {}

System::Boolean
NativeDownloadHandler::ReceiveDataNative(void* data, System::Int32 dataLength) {
  std::byte* p = static_cast<std::byte*>(data);
  this->_data.insert(this->_data.end(), p, p + dataLength);
  return true;
}

gsl::span<const std::byte> NativeDownloadHandler::getData() const {
  return this->_data;
}

} // namespace CesiumForUnity
