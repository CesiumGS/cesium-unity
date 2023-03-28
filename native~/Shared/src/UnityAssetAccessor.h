#pragma once

#include <CesiumAsync/IAssetAccessor.h>

#include <DotNet/System/EventHandler.h>
#include <DotNet/System/String.h>

#include <utility>
#include <vector>

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
  void init() noexcept;
  void onDomainUnload() noexcept;

  DotNet::System::EventHandler _domainUnloadHandler;
  std::vector<std::pair<DotNet::System::String, DotNet::System::String>>
      _cesiumRequestHeaders;
};

} // namespace CesiumForUnityNative
