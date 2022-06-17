#include "UnityTilesetExternals.h"

#include "Interop.h"
#include "UnityAssetAccessor.h"
#include "UnityPrepareRendererResources.h"
#include "UnityTaskProcessor.h"

#include <Cesium3DTilesSelection/CreditSystem.h>
#include <Cesium3DTilesSelection/TilesetExternals.h>
#include <CesiumAsync/CachingAssetAccessor.h>
#include <CesiumAsync/SqliteCache.h>

#include <memory>

using namespace Cesium3DTilesSelection;
using namespace CesiumAsync;

namespace CesiumForUnity {

namespace {

std::unique_ptr<TilesetExternals> pExternals = nullptr;
std::shared_ptr<UnityAssetAccessor> pAccessor = nullptr;
std::shared_ptr<UnityTaskProcessor> pTaskProcessor = nullptr;
std::shared_ptr<CreditSystem> pCreditSystem = nullptr;

const std::shared_ptr<UnityAssetAccessor>& getAssetAccessor() {
  if (!pAccessor) {
    pAccessor = std::make_shared<UnityAssetAccessor>();
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
createTilesetExternals(UnityEngine::GameObject& tileset) {
  std::string tempPath =
      Interop::convert(UnityEngine::Application::GetTemporaryCachePath());
  std::string cacheDBPath = tempPath + "/cesium-request-cache.sqlite";

  return TilesetExternals{
      std::make_shared<CachingAssetAccessor>(
          spdlog::default_logger(),
          getAssetAccessor(),
          std::make_shared<SqliteCache>(spdlog::default_logger(), cacheDBPath)),
      std::make_shared<UnityPrepareRendererResources>(tileset),
      AsyncSystem(getTaskProcessor()),
      getCreditSystem(),
      spdlog::default_logger()};
}

} // namespace CesiumForUnity
