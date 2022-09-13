#include "UnityTilesetExternals.h"

#include "UnityAssetAccessor.h"
#include "UnityPrepareRendererResources.h"
#include "UnityTaskProcessor.h"

#include <Cesium3DTilesSelection/CreditSystem.h>
#include <Cesium3DTilesSelection/TilesetExternals.h>
#include <CesiumAsync/CachingAssetAccessor.h>
#include <CesiumAsync/SqliteCache.h>

#include <DotNet/System/String.h>
#include <DotNet/UnityEngine/Application.h>

#include <memory>

using namespace Cesium3DTilesSelection;
using namespace CesiumAsync;
using namespace DotNet;

namespace CesiumForUnityNative {

namespace {

std::shared_ptr<CachingAssetAccessor> pAccessor = nullptr;
std::shared_ptr<UnityTaskProcessor> pTaskProcessor = nullptr;
std::shared_ptr<CreditSystem> pCreditSystem = nullptr;

const std::shared_ptr<CachingAssetAccessor>& getAssetAccessor() {
  if (!pAccessor) {
    std::string tempPath =
        UnityEngine::Application::temporaryCachePath().ToStlString();
    std::string cacheDBPath = tempPath + "/cesium-request-cache.sqlite";

    pAccessor = std::make_shared<CachingAssetAccessor>(
        spdlog::default_logger(),
        std::make_shared<UnityAssetAccessor>(),
        std::make_shared<SqliteCache>(spdlog::default_logger(), cacheDBPath));
  }
  return pAccessor;
}

const std::shared_ptr<UnityTaskProcessor>& getTaskProcessor() {
  if (!pTaskProcessor) {
    pTaskProcessor = std::make_shared<UnityTaskProcessor>();
  }
  return pTaskProcessor;
}

const std::shared_ptr<CreditSystem>& getCreditSystem() {
  if (!pCreditSystem) {
    pCreditSystem = std::make_shared<CreditSystem>();
  }
  return pCreditSystem;
}

} // namespace

Cesium3DTilesSelection::TilesetExternals
createTilesetExternals(const ::DotNet::UnityEngine::GameObject& tileset) {
  return TilesetExternals{
      getAssetAccessor(),
      std::make_shared<UnityPrepareRendererResources>(tileset),
      AsyncSystem(getTaskProcessor()),
      getCreditSystem(),
      spdlog::default_logger()};
}

} // namespace CesiumForUnityNative
