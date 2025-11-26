#pragma once

#include <CesiumAsync/IAssetAccessor.h>

namespace CesiumForUnityNative {

class UnityWebRequestAssetAccessor : public CesiumAsync::IAssetAccessor {
public:
  UnityWebRequestAssetAccessor();

  virtual CesiumAsync::Future<std::shared_ptr<CesiumAsync::IAssetRequest>>
  get(const CesiumAsync::AsyncSystem& asyncSystem,
      const std::string& url,
      const std::vector<THeader>& headers = {}) override;

  virtual CesiumAsync::Future<std::shared_ptr<CesiumAsync::IAssetRequest>>
  request(
      const CesiumAsync::AsyncSystem& asyncSystem,
      const std::string& verb,
      const std::string& url,
      const std::vector<THeader>& headers = std::vector<THeader>(),
      const std::span<const std::byte>& contentPayload = {}) override;

  virtual void tick() noexcept override;

private:
  CesiumAsync::HttpHeaders _cesiumRequestHeaders;
};

} // namespace CesiumForUnityNative
