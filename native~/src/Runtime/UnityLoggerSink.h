#pragma once

#include <spdlog/sinks/base_sink.h>

#include <mutex>

namespace CesiumForUnity {

class UnityLoggerSink : public spdlog::sinks::base_sink<std::mutex> {
protected:
  virtual void sink_it_(const spdlog::details::log_msg& msg) override;
  virtual void flush_() override;
};

} // namespace CesiumForUnity
