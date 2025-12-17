#include "UnityExternals.h"

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
#include <DotNet/CesiumForUnity/CesiumIonServer.h>
#include <DotNet/CesiumForUnity/CesiumRuntimeSettings.h>
#include <DotNet/System/AppDomain.h>
#include <DotNet/System/Array1.h>
#include <DotNet/System/Collections/Generic/IReadOnlyCollection1.h>
#include <DotNet/System/Collections/Generic/IReadOnlyList1.h>
#include <DotNet/System/EventArgs.h>
#include <DotNet/System/EventHandler.h>
#include <DotNet/System/Object.h>
#include <DotNet/System/String.h>
#include <DotNet/UnityEngine/Application.h>
#include <DotNet/UnityEngine/Debug.h>
#include <DotNet/UnityEngine/GameObject.h>
#include <DotNet/UnityEngine/SceneManagement/Scene.h>
#include <DotNet/UnityEngine/SceneManagement/SceneManager.h>

#if UNITY_EDITOR
#include <DotNet/CesiumForUnity/CesiumIonServerManager.h>
#include <DotNet/CesiumForUnity/CesiumIonSession.h>
#include <DotNet/UnityEditor/AssemblyReloadCallback.h>
#include <DotNet/UnityEditor/AssemblyReloadEvents.h>
#endif

using namespace CesiumUtility;
using namespace Cesium3DTilesSelection;
using namespace CesiumAsync;
using namespace DotNet;

namespace CesiumForUnityNative {

namespace {

std::shared_ptr<IAssetAccessor> pAccessor = nullptr;
std::shared_ptr<ITaskProcessor> pTaskProcessor = nullptr;
std::optional<AsyncSystem> asyncSystem;

#ifdef __EMSCRIPTEN__
std::shared_ptr<UnityEmscriptenAssetAccessor> pWebRequestAccessor = nullptr;
#else
std::shared_ptr<UnityWebRequestAssetAccessor> pWebRequestAccessor = nullptr;
#endif

void shutdownExternals();

} // namespace

void initializeExternals() {
  CESIUM_ASSERT(pAccessor == nullptr);
  CESIUM_ASSERT(pTaskProcessor == nullptr);
  CESIUM_ASSERT(!asyncSystem.has_value());
  CESIUM_ASSERT(pWebRequestAccessor == nullptr);

  try {
    std::string tempPath =
        UnityEngine::Application::temporaryCachePath().ToStlString();
    std::string cacheDBPath = tempPath + "/cesium-request-cache.sqlite";

    int32_t requestsPerCachePrune =
        CesiumForUnity::CesiumRuntimeSettings::requestsPerCachePrune();
    uint64_t maxItems = CesiumForUnity::CesiumRuntimeSettings::maxItems();

    pWebRequestAccessor =
#ifdef __EMSCRIPTEN__
        std::make_shared<UnityEmscriptenAssetAccessor>();
#else
        std::make_shared<UnityWebRequestAssetAccessor>();
#endif

    pAccessor = std::make_shared<GunzipAssetAccessor>(
        std::make_shared<CachingAssetAccessor>(
            spdlog::default_logger(),
            pWebRequestAccessor,
            std::make_shared<SqliteCache>(
                spdlog::default_logger(),
                cacheDBPath,
                maxItems),
            requestsPerCachePrune));

    pTaskProcessor = std::make_shared<UnityTaskProcessor>();
    asyncSystem.emplace(pTaskProcessor);
  } catch (const std::exception& e) {
    spdlog::error("Failed to initialize Cesium externals: {}", e.what());
    throw;
  }

#if UNITY_EDITOR
  DotNet::UnityEditor::AssemblyReloadEvents::add_beforeAssemblyReload(
      DotNet::UnityEditor::AssemblyReloadCallback(
          []() { shutdownExternals(); }));
  DotNet::System::AppDomain::CurrentDomain().add_DomainUnload(
      DotNet::System::EventHandler([](const DotNet::System::Object& sender,
                                      const DotNet::System::EventArgs& e) {
        Reinterop::ObjectHandle::endCurrentAppDomain();
      }));
#endif
}

namespace {

void shutdownExternals() {
#ifndef __EMSCRIPTEN__
  if (pWebRequestAccessor) {
    pWebRequestAccessor->failAllFutureRequests();
    pWebRequestAccessor->cancelActiveRequests();
  }
#endif

  // Before destroying the rest of the externals, we must ensure all async work
  // has completed. This isn't extremely easy to determine. We need to find all
  // the Tilesets and ActivatedRasterOverlays and ask them if any loads are in
  // progress.
  int32_t sceneCount = UnityEngine::SceneManagement::SceneManager::sceneCount();
  for (int32_t i = 0; i < sceneCount; ++i) {
    System::Array1<UnityEngine::GameObject> gameObjects =
        UnityEngine::SceneManagement::SceneManager::GetSceneAt(i)
            .GetRootGameObjects();
    for (int32_t j = 0; j < gameObjects.Length(); ++j) {
      System::Array1<CesiumForUnity::Cesium3DTileset> tilesets =
          gameObjects[j]
              .GetComponentsInChildren<CesiumForUnity::Cesium3DTileset>();
      for (int32_t k = 0; k < tilesets.Length(); ++k) {
        Tileset* pTileset = tilesets[k].NativeImplementation().getTileset();
        if (pTileset) {
          if (!pTileset->waitForAllLoadsToComplete(1000.0)) {
            UnityEngine::Debug::LogWarning(System::String(fmt::format(
                "Waiting up to 30 seconds for '{}' loads to "
                "complete so that the AppDomain can be unloaded.",
                tilesets[k].name().ToStlString())));
            if (!pTileset->waitForAllLoadsToComplete(30000.0)) {
              UnityEngine::Debug::LogError(System::String(fmt::format(
                  "Timed out waiting for '{}' loads to complete.",
                  tilesets[k].name().ToStlString())));
            }
          }
        }
      }
    }
  }

#if UNITY_EDITOR
  // We also need to wait for in-progress CesiumIonSessions to finish.
  System::Collections::Generic::IReadOnlyList1<CesiumForUnity::CesiumIonServer>
      serverList = CesiumForUnity::CesiumIonServerManager::instance().servers();
  System::Collections::Generic::IReadOnlyCollection1<
      CesiumForUnity::CesiumIonServer>
      serverCollection = serverList;
  for (int32_t i = 0; i < serverCollection.Count(); ++i) {
    CesiumForUnity::CesiumIonSession session =
        CesiumForUnity::CesiumIonServerManager::instance().GetSession(
            serverList[i]);
    while (session.IsBusy()) {
      pAccessor->tick();
      asyncSystem->dispatchMainThreadTasks();
    }
  }
#endif

  pWebRequestAccessor.reset();
  pAccessor.reset();
  pTaskProcessor.reset();
  asyncSystem.reset();
}

} // namespace

const std::shared_ptr<IAssetAccessor>& getAssetAccessor() {
  if (pAccessor == nullptr) {
    initializeExternals();
  }
  return pAccessor;
}

const std::shared_ptr<ITaskProcessor>& getTaskProcessor() {
  if (pTaskProcessor == nullptr) {
    initializeExternals();
  }
  return pTaskProcessor;
}

AsyncSystem& getAsyncSystem() {
  if (!asyncSystem) {
    initializeExternals();
  }
  return *asyncSystem;
}

} // namespace CesiumForUnityNative
