#include "UnityLoggerSink.h"

#include <spdlog/spdlog.h>

using namespace CesiumForUnity;

void start() {
  spdlog::default_logger()->sinks() = {std::make_shared<UnityLoggerSink>()};
}

void stop() {}
