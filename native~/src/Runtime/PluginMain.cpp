#include "CesiumUtility/Tracing.h"
#include "UnityLoggerSink.h"

#include <CesiumGltfConverters/registerAllConverterTypes.h>

#include <spdlog/spdlog.h>

#if CESIUM_TRACING_ENABLED
#include <chrono>
#endif

using namespace CesiumForUnity;

void start() {
  CesiumGltfConverters::registerAllConverterTypes();
  spdlog::default_logger()->sinks() = {std::make_shared<UnityLoggerSink>()};

  CESIUM_TRACE_INIT(
      "cesium-trace-" +
      std::to_string(std::chrono::time_point_cast<std::chrono::microseconds>(
                         std::chrono::steady_clock::now())
                         .time_since_epoch()
                         .count()) +
      ".json");
}

void stop() {}
