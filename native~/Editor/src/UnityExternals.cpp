#include "UnityExternals.h"

#include "UnityEmscriptenAssetAccessor.h"
#include "UnityTaskProcessor.h"
#include "UnityWebRequestAssetAccessor.h"

#include <CesiumAsync/AsyncSystem.h>
#include <CesiumAsync/CachingAssetAccessor.h>
#include <CesiumAsync/GunzipAssetAccessor.h>
#include <CesiumAsync/SqliteCache.h>

#include <DotNet/CesiumForUnity/CesiumRuntimeSettings.h>
#include <DotNet/System/String.h>
#include <DotNet/UnityEngine/Application.h>

#include <memory>

using namespace CesiumAsync;
using namespace CesiumForUnityNative;
using namespace DotNet;

namespace {

std::shared_ptr<IAssetAccessor> pAccessor = nullptr;
std::shared_ptr<ITaskProcessor> pTaskProcessor = nullptr;
std::optional<AsyncSystem> asyncSystem;

} // namespace

namespace CesiumForUnityNative {

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
#ifdef __EMSCRIPTEN__
            std::make_shared<UnityEmscriptenAssetAccessor>(),
#else
            std::make_shared<UnityWebRequestAssetAccessor>(),
#endif
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

} // namespace CesiumForUnityNative
