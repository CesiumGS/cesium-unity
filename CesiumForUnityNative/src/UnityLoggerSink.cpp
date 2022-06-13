#include "Bindings.h"
#include "UnityLoggerSink.h"

using namespace CesiumForUnity;
using namespace UnityEngine;

void UnityLoggerSink::sink_it_(const spdlog::details::log_msg& msg) {
  spdlog::memory_buf_t formatted;
  this->formatter_->format(msg, formatted);
  Debug::Log(System::String(fmt::to_string(formatted).c_str()));
}

void UnityLoggerSink::flush_() {}
