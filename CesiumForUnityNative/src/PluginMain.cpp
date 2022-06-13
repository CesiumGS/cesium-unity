#include "Bindings.h"
#include "UnityLoggerSink.h"

#include <Cesium3DTilesSelection/registerAllTileContentTypes.h>

#include <spdlog/spdlog.h>

using namespace Cesium3DTilesSelection;
using namespace CesiumForUnity;

void PluginMain(void* memory, int32_t memorySize, bool isFirstBoot) {
  registerAllTileContentTypes();
  spdlog::default_logger()->sinks() = {std::make_shared<UnityLoggerSink>()};
}
