#pragma once

#include <CesiumAsync/IAssetAccessor.h>

namespace CesiumForUnityNative {

class UnityAssetAccessor : public CesiumAsync::IAssetAccessor {
public:
  UnityAssetAccessor();

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
      const gsl::span<const std::byte>& contentPayload = {}) override;

  virtual void tick() noexcept override;

private:
  const std::string _cesiumPlatformHeader;
  const std::string _cesiumVersionHeader;
};

} // namespace CesiumForUnityNative
