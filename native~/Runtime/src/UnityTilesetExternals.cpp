#include "UnityTilesetExternals.h"

#include "UnityAssetAccessor.h"
#include "UnityPrepareRendererResources.h"
#include "UnityTaskProcessor.h"

#include <CesiumAsync/CachingAssetAccessor.h>
#include <CesiumAsync/GunzipAssetAccessor.h>
#include <CesiumAsync/SqliteCache.h>
#include <CesiumUtility/CreditSystem.h>

#include <DotNet/CesiumForUnity/CesiumCreditSystem.h>
#include <DotNet/CesiumForUnity/CesiumRuntimeSettings.h>
#include <DotNet/System/String.h>
#include <DotNet/UnityEngine/Application.h>

using namespace CesiumUtility;
using namespace Cesium3DTilesSelection;
using namespace CesiumAsync;
using namespace DotNet;

namespace CesiumForUnityNative {

namespace {

std::shared_ptr<IAssetAccessor> pAccessor = nullptr;
std::shared_ptr<ITaskProcessor> pTaskProcessor = nullptr;
std::shared_ptr<CreditSystem> pCreditSystem = nullptr;
std::optional<AsyncSystem> asyncSystem;

} // namespace

const std::shared_ptr<IAssetAccessor>& getAssetAccessor() {
  if (!pAccessor) {
    std::string tempPath =
        UnityEngine::Application::temporaryCachePath().ToStlString();
    std::string cacheDBPath = tempPath + "/cesium-request-cache.sqlite";

    int32_t requestsPerCachePrune =
        CesiumForUnity::CesiumRuntimeSettings::requestsPerCachePrune();
    uint64_t maxItems = CesiumForUnity::CesiumRuntimeSettings::maxItems();

    pAccessor = std::make_shared<GunzipAssetAccessor>(
        std::make_shared<CachingAssetAccessor>(
            spdlog::default_logger(),
            std::make_shared<UnityAssetAccessor>(),
            std::make_shared<SqliteCache>(
                spdlog::default_logger(),
                cacheDBPath,
                maxItems),
            requestsPerCachePrune));
  }
  return pAccessor;
}

const std::shared_ptr<ITaskProcessor>& getTaskProcessor() {
  if (!pTaskProcessor) {
    pTaskProcessor = std::make_shared<UnityTaskProcessor>();
  }
  return pTaskProcessor;
}

AsyncSystem getAsyncSystem() {
  if (!asyncSystem) {
    asyncSystem.emplace(getTaskProcessor());
  }
  return *asyncSystem;
}

const std::shared_ptr<CreditSystem>&
getOrCreateCreditSystem(const CesiumForUnity::Cesium3DTileset& tileset) {
  // First, get the existing credit system associated with the tileset.
  // (This happens when the existing tileset is recreated.)
  Cesium3DTilesetImpl& tilesetImpl = tileset.NativeImplementation();
  CesiumForUnity::CesiumCreditSystem creditSystem =
      tilesetImpl.getCreditSystem();

  // If the tileset does not reference a credit system, get the default one.
  if (creditSystem == nullptr) {
    creditSystem = CesiumForUnity::CesiumCreditSystem::GetDefaultCreditSystem();
    // This is necessary for the tileset to track the Unity credit system's
    // lifetime.
    tilesetImpl.setCreditSystem(creditSystem);
  }

  CesiumCreditSystemImpl& creditSystemImpl =
      creditSystem.NativeImplementation();
  pCreditSystem = creditSystemImpl.getNativeCreditSystem();

  return pCreditSystem;
}

Cesium3DTilesSelection::TilesetExternals
createTilesetExternals(const CesiumForUnity::Cesium3DTileset& tileset) {
  return TilesetExternals{
      getAssetAccessor(),
      std::make_shared<UnityPrepareRendererResources>(tileset.gameObject()),
      AsyncSystem(getTaskProcessor()),
      getOrCreateCreditSystem(tileset),
      spdlog::default_logger()};
}

} // namespace CesiumForUnityNative
