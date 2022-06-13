#pragma once

#include "Bindings.h"

#include <gsl/span>

#include <cstddef>
#include <vector>

namespace CesiumForUnity {

class NativeDownloadHandler : public BaseNativeDownloadHandler {
public:
  CESIUM_FOR_UNITY_NATIVE_DOWNLOAD_HANDLER_DEFAULT_CONTENTS_DECLARATION
  CESIUM_FOR_UNITY_NATIVE_DOWNLOAD_HANDLER_DEFAULT_CONSTRUCTOR_DECLARATION

  NativeDownloadHandler();

  virtual System::Boolean
  ReceiveDataNative(void* data, System::Int32 dataLength) override;

  gsl::span<const std::byte> getData() const;

private:
  std::vector<std::byte> _data;
};

} // namespace CesiumForUnity
