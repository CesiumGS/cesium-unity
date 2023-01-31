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
  bool ReceiveDataNative(
      const ::DotNet::CesiumForUnity::NativeDownloadHandler& handler,
      void* data,
      std::int32_t dataLength);

  const std::vector<std::byte>& getData() const noexcept;
  std::vector<std::byte>& getData() noexcept;

private:
  std::vector<std::byte> _data;
};

} // namespace CesiumForUnityNative
