#pragma once

#include <gsl/span>

#include <cstddef>
#include <cstdint>
#include <vector>

namespace Oxidize::CesiumForUnity {
class NativeDownloadHandler;
}

namespace CesiumForUnity {

class NativeDownloadHandlerImpl {
public:
  NativeDownloadHandlerImpl(
      const ::Oxidize::CesiumForUnity::NativeDownloadHandler& handler);
  void JustBeforeDelete(
      const ::Oxidize::CesiumForUnity::NativeDownloadHandler& handler);
  bool ReceiveDataNative(
      const ::Oxidize::CesiumForUnity::NativeDownloadHandler& handler,
      void* data,
      std::int32_t dataLength);

  gsl::span<const std::byte> getData() const;

private:
  std::vector<std::byte> _data;
};

} // namespace CesiumForUnity
