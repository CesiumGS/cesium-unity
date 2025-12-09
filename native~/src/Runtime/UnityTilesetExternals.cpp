#include "UnityTilesetExternals.h"

#include "UnityEmscriptenAssetAccessor.h"
#include "UnityPrepareRendererResources.h"
#include "UnityTaskProcessor.h"
#include "UnityWebRequestAssetAccessor.h"

#include <Cesium3DTilesSelection/Tileset.h>
#include <CesiumAsync/AsyncSystem.h>
#include <CesiumAsync/CachingAssetAccessor.h>
#include <CesiumAsync/GunzipAssetAccessor.h>
#include <CesiumAsync/SqliteCache.h>
#include <CesiumUtility/CreditSystem.h>

#include <DotNet/CesiumForUnity/CesiumCreditSystem.h>
#include <DotNet/CesiumForUnity/CesiumRuntimeSettings.h>
#include <DotNet/System/Array1.h>
#include <DotNet/System/Object.h>
#include <DotNet/System/String.h>
#include <DotNet/UnityEngine/Application.h>
#include <DotNet/UnityEngine/Debug.h>
#include <DotNet/UnityEngine/GameObject.h>
#include <DotNet/UnityEngine/SceneManagement/Scene.h>
#include <DotNet/UnityEngine/SceneManagement/SceneManager.h>

#if UNITY_EDITOR
#include <DotNet/UnityEditor/AssemblyReloadCallback.h>
#include <DotNet/UnityEditor/AssemblyReloadEvents.h>
#endif

using namespace CesiumUtility;
using namespace Cesium3DTilesSelection;
using namespace CesiumAsync;
using namespace DotNet;

namespace CesiumForUnityNative {

std::shared_ptr<CreditSystem>
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
  std::shared_ptr<CreditSystem> pCreditSystem =
      creditSystemImpl.getNativeCreditSystem();

  return pCreditSystem;
}

Cesium3DTilesSelection::TilesetExternals
createTilesetExternals(const CesiumForUnity::Cesium3DTileset& tileset) {
  return TilesetExternals{
      getAssetAccessor(),
      std::make_shared<UnityPrepareRendererResources>(tileset.gameObject()),
      getAsyncSystem(),
      getOrCreateCreditSystem(tileset),
      spdlog::default_logger()};
}

} // namespace CesiumForUnityNative
