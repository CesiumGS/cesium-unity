#include "CesiumUtility/Tracing.h"
#include "UnityLoggerSink.h"
#include "UnityTilesetExternals.h"

#include <Cesium3DTilesContent/registerAllTileContentTypes.h>

#include <spdlog/spdlog.h>

#if CESIUM_TRACING_ENABLED
#include <chrono>
#endif

using namespace Cesium3DTilesContent;
using namespace CesiumForUnity;

void start() {
  registerAllTileContentTypes();
  spdlog::default_logger()->sinks() = {std::make_shared<UnityLoggerSink>()};

  CESIUM_TRACE_INIT(
      "cesium-trace-" +
      std::to_string(std::chrono::time_point_cast<std::chrono::microseconds>(
                         std::chrono::steady_clock::now())
                         .time_since_epoch()
                         .count()) +
      ".json");

//   CesiumForUnityNative::initializeExternals();

// #if UNITY_EDITOR
//   DotNet::UnityEditor::AssemblyReloadEvents::add_beforeAssemblyReload(
//       DotNet::UnityEditor::AssemblyReloadCallback(
//           []() { CesiumForUnityNative::shutdownExternals(); }));
// #endif
}

void stop() {}
