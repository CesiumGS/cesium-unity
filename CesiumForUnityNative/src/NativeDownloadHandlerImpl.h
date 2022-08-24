#pragma once

#include <gsl/span>

#include <cstddef>
#include <cstdint>
#include <vector>

namespace DotNet::CesiumForUnity {
class NativeDownloadHandler;
}

namespace CesiumForUnityNative {

class NativeDownloadHandlerImpl {
public:
  NativeDownloadHandlerImpl(
      const ::DotNet::CesiumForUnity::NativeDownloadHandler& handler);
  void JustBeforeDelete(
      const ::DotNet::CesiumForUnity::NativeDownloadHandler& handler);
  bool ReceiveDataNative(
      const ::DotNet::CesiumForUnity::NativeDownloadHandler& handler,
      void* data,
      std::int32_t dataLength);

  gsl::span<const std::byte> getData() const;

private:
  std::vector<std::byte> _data;
};

} // namespace CesiumForUnityNative
