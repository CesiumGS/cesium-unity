#include "UnityTilesetExternals.h"

#include "UnityAssetAccessor.h"
#include "UnityPrepareRendererResources.h"
#include "UnityTaskProcessor.h"

#include <Cesium3DTilesSelection/CreditSystem.h>
#include <CesiumAsync/CachingAssetAccessor.h>
#include <CesiumAsync/SqliteCache.h>

#include <DotNet/CesiumForUnity/CesiumCreditSystem.h>
#include <DotNet/System/String.h>
#include <DotNet/UnityEngine/Application.h>

#include <memory>

#if UNITY_EDITOR
#include <DotNet/UnityEditor/EditorApplication.h>
#endif

using namespace Cesium3DTilesSelection;
using namespace CesiumAsync;
using namespace DotNet;

namespace CesiumForUnityNative {

namespace {

std::shared_ptr<CachingAssetAccessor> pAccessor = nullptr;
std::shared_ptr<UnityTaskProcessor> pTaskProcessor = nullptr;
std::shared_ptr<CreditSystem> pCreditSystem = nullptr;
#if UNITY_EDITOR
// If a tileset is loading in the editor, it won't instantiate the
// credit system prefab. A CreditSystem will be manually constructed
// so that the TilesetExternals won't contain a nullptr.
std::shared_ptr<CreditSystem> pEditorCreditSystem = nullptr;
#endif

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

const std::shared_ptr<CreditSystem>&
getCreditSystem(const CesiumForUnity::Cesium3DTileset& tileset) {
#if UNITY_EDITOR
  if (UnityEngine::Application::isEditor() &&
      !UnityEditor::EditorApplication::isPlaying()) {
    if (!pEditorCreditSystem) {
      pEditorCreditSystem = std::make_shared<CreditSystem>();
    }
    return pEditorCreditSystem;
  }
#endif

  // Get the credit system associated with the tileset.
  Cesium3DTilesetImpl& tilesetImpl = tileset.NativeImplementation();
  CesiumForUnity::CesiumCreditSystem creditSystem =
      tilesetImpl.getCreditSystem();

  // If the tileset does not already reference a credit system,
  // get the default one.
  if (creditSystem == nullptr) {
    creditSystem = CesiumCreditSystemImpl::getDefaultCreditSystem();
    tilesetImpl.setCreditSystem(creditSystem);
  }

  CesiumCreditSystemImpl& creditSystemImpl =
      creditSystem.NativeImplementation();
  pCreditSystem = creditSystemImpl.getExternalCreditSystem();

  return pCreditSystem;
}

} // namespace

Cesium3DTilesSelection::TilesetExternals
createTilesetExternals(const CesiumForUnity::Cesium3DTileset& tileset) {
  return TilesetExternals{
      getAssetAccessor(),
      std::make_shared<UnityPrepareRendererResources>(tileset.gameObject()),
      AsyncSystem(getTaskProcessor()),
      getCreditSystem(tileset),
      spdlog::default_logger()};
}

} // namespace CesiumForUnityNative
