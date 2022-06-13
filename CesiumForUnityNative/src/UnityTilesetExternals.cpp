#include "UnityTilesetExternals.h"

#include "UnityAssetAccessor.h"
#include "UnityPrepareRendererResources.h"
#include "UnityTaskProcessor.h"

#include <Cesium3DTilesSelection/CreditSystem.h>
#include <Cesium3DTilesSelection/TilesetExternals.h>

#include <memory>

using namespace Cesium3DTilesSelection;
using namespace CesiumAsync;

namespace CesiumForUnity {

namespace {

std::unique_ptr<TilesetExternals> pExternals = nullptr;
std::shared_ptr<UnityAssetAccessor> pAccessor = nullptr;
std::shared_ptr<UnityPrepareRendererResources> pPrepareRendererResources =
    nullptr;
std::shared_ptr<UnityTaskProcessor> pTaskProcessor = nullptr;
std::shared_ptr<CreditSystem> pCreditSystem = nullptr;

const std::shared_ptr<UnityAssetAccessor>& getAssetAccessor() {
  if (!pAccessor) {
    pAccessor = std::make_shared<UnityAssetAccessor>();
  }
  return pAccessor;
}

const std::shared_ptr<UnityPrepareRendererResources>&
getPrepareRendererResources() {
  if (!pPrepareRendererResources) {
    pPrepareRendererResources =
        std::make_shared<UnityPrepareRendererResources>();
  }
  return pPrepareRendererResources;
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

const TilesetExternals& getUnityTilesetExternals() {
  if (!pExternals) {
    pExternals = std::make_unique<TilesetExternals>(TilesetExternals{
        getAssetAccessor(),
        getPrepareRendererResources(),
        AsyncSystem(getTaskProcessor()),
        getCreditSystem(),
        spdlog::default_logger()});
  }
  return *pExternals;
}

} // namespace CesiumForUnity
